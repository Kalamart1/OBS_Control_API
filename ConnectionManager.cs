using MelonLoader;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace OBS_Control_API
{
    public partial class OBS
    {
        /**
         * <summary>
         * Manages the websocket client connection and low-level functionalities such as sending/receiving text.
         * </summary>
         */
        private partial class ConnectionManager
        {
            // variables
            private RequestManager requestManager;
            private ClientWebSocket ws = new ClientWebSocket();

            private string PASSWORD = null;
            private string URL = null;

            private bool authentifying = false; // true when we started the authenticating process
            private bool waitingOnServer = false; // true if the server is unavailable
            private bool shouldReconnect = true; // becomes false if the disconnect was our fault (e.g. authentication)

            public event Action<OBS_Control_API.Event> onEvent;

            /**
             * <summary>
             * Initialize connection manager.
             * </summary>
             */
            public ConnectionManager(RequestManager manager, string OBS_ip, int OBS_port, string OBS_password)
            {
                requestManager = manager;
                manager.SetConnectionManager(this);
                UpdateWebsocketConfig(OBS_ip, OBS_port, OBS_password);
            }

            /**
             * <summary>
             * Update the URL and password used by the websocket client.
             * </summary>
             */
            public void UpdateWebsocketConfig(string OBS_ip, int OBS_port, string OBS_password)
            {
                URL = "ws://" + OBS_ip + ":" + OBS_port;
                PASSWORD = OBS_password;
            }

            /**
             * <summary>
             * Returns true if the client is connected to OBS, ready to send requests and receive events.
             * </summary>
             */
            public bool IsConnected()
            {
                return (ws.State == WebSocketState.Open);
            }

            /**
             * <summary>
             * Start the connection thread.
             * </summary>
             */
            public void Start()
            {
                Task.Run(() => ConnectAsync());
            }

            /**
             * <summary>
             * Connection thread (anychronous). Will keep reconnecting if the server is offline,
             * but will stop if there is an authentication failure or an error.
             * </summary>
             */
            private async Task ConnectAsync()
            {
                shouldReconnect = true;
                while (shouldReconnect)
                {
                    try
                    {
                        ws = new ClientWebSocket();
                        var uri = new Uri(URL);
                        await ws.ConnectAsync(uri, CancellationToken.None);
                        OnOpen();

                        var buffer = new byte[4096];
                        while (IsConnected())
                        {
                            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                OnMessage(Encoding.UTF8.GetString(buffer, 0, result.Count));
                            }
                        }
                        OnClose();
                    }
                    catch (WebSocketException wse) when (wse.WebSocketErrorCode == WebSocketError.Faulted)
                    {
                        if (!waitingOnServer)
                        {
                            Log("OBS websocket server unavailable, will attempt reconnecting every 3 seconds.");
                            waitingOnServer = true;
                        }
                    }
                    catch (WebSocketException wse)
                    {
                        LogError($"WebSocket error: {wse.WebSocketErrorCode}");
                        if (wse.WebSocketErrorCode!=WebSocketError.ConnectionClosedPrematurely)
                        {
                            shouldReconnect = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error: {ex.Message}");
                        shouldReconnect = false;
                    }
                    if (shouldReconnect)
                    {
                        await Task.Delay(3000);
                    }
                }
            }


            /**
             * <summary>
             * Called when the server has been reached but the indentification handshake hasn't been perfomed.
             * </summary>
             */
            private void OnOpen()
            {
                Log("OBS websocket server available");
                waitingOnServer = false;
            }

            /**
             * <summary>
             * Called when the websocket connection has been closed.
             * </summary>
             */
            private void OnClose()
            {
                if (authentifying)
                {
                    LogError("Authentication failed");
                    shouldReconnect = false;
                }
                Log("Connection to OBS closed");
                Task.Run(() => onDisconnect());
            }

            /**
             * <summary>
             * Close the connection and don't try to reconnect.
             * </summary>
             */
            public async void Stop()
            {
                shouldReconnect = false;
                if (IsConnected())
                {
                    Log("Closing websocket connection");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
            }

            /**
             * <summary>
             * Called every time a message is received from the OBS websocket server.
             * </summary>
             */
            private void OnMessage(string payload)
            {
                try
                {
                    //Log($"Received: {payload}");
                    var msg = JsonConvert.DeserializeObject<OBS_Control_API.Msg>(payload);
                    if (msg == null)
                        return;

                    OpCode op = (OpCode)Convert.ToInt32(msg.op);
                    string d = msg.d.ToString();
                    if (op == OpCode.Hello)
                    {
                        var data = JsonConvert.DeserializeObject<Hello>(d);
                        var auth = data.authentication;
                        string challenge = auth.challenge;
                        string salt = auth.salt;

                        string secret = Sha256Base64(PASSWORD + salt);
                        string authResp = Sha256Base64(secret + challenge);

                        var identify = new Dictionary<string, object>
                        {
                            ["op"] = OpCode.Identify,
                            ["d"] = new Dictionary<string, object>
                            {
                                ["rpcVersion"] = 1,
                                ["authentication"] = authResp
                            }
                        };

                        authentifying = true;
                        SendMsg(identify);
                    }
                    else if (op == OpCode.Identified)
                    {
                        authentifying = false;
                        Log("Connection to OBS successful");
                        Task.Run(() => onConnect());
                    }
                    else if (op == OpCode.Event)
                    {
                        var data = JsonConvert.DeserializeObject<OBS_Control_API.Event>(d);
                        Task.Run(() => onEvent(data));
                    }
                    else if (op == OpCode.RequestResponse)
                    {
                        requestManager.ReceiveResponse(d);
                    }
                    else
                    {
                        LogError($"Received invalid OpCode {op}");
                    }

                }
                catch (Exception ex)
                {
                    LogError("Error in OnMessage: " + ex.Message);
                }
            }


            /**
             * <summary>
             * Returns a Base64-encoded SHA-256 hash of the input string.
             * </summary>
             */
            private string Sha256Base64(string input)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                    return Convert.ToBase64String(hash);
                }
            }

            /**
             * <summary>
             * Send a message to the OBS websocket server.
             * </summary>
             */
            public async void SendMsg(object msg)
            {
                if (ws.State != WebSocketState.Open) return;
                var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
                var segment = new ArraySegment<byte>(buffer);
                //Log($"Sending message: {JsonConvert.SerializeObject(msg)}");
                await Task.Run(() => ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None));
            }
        }
    }
}