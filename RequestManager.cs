using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;

namespace OBS_Control_API
{
    public partial class OBS
    {
        /**
         * <summary>
         * Manages the sending of requests and the reception of the responses, by generating usique requetsId strings.
         * </summary>
         */
        private partial class RequestManager
        {
            // variables
            private ConnectionManager connectionManager;
            private Dictionary<string, RequestResponse> currentRequests;

            /**
             * <summary>
             * Initialize request manager.
             * </summary>
             */
            public RequestManager()
            {
                currentRequests = new Dictionary<string, RequestResponse>();
            }

            /**
             * <summary>
             * Set the instance of the connection manager that is to be used for the requests
             * </summary>
             */
            public void SetConnectionManager(ConnectionManager manager)
            {
                connectionManager = manager;
            }

            /**
             * <summary>
             * Sends a request with or without parameters, and waits for the response.
             * Returned value is a string representation of responseData.
             * </summary>
             */
            public string SendRequest(string requestType, object parameters)
            {
                RequestResponse resp = null;
                int attempts = 0;
                while (resp is null && attempts<100)
                {
                    string requestId = NewRequest(requestType, parameters);
                    if (requestId is null)
                    {
                        return null;
                    }
                    while (currentRequests[requestId] is null)
                    {
                        Thread.Sleep(10);
                        attempts++;
                    }
                    if (!(currentRequests[requestId] is null))
                    {
                        resp = currentRequests[requestId];
                        if (resp.requestStatus.code == (int)RequestResponse.Code.NotReady)
                        {
                            resp = null; //try again
                            Thread.Sleep(10);
                            attempts++;
                        }
                        else if (!resp.requestStatus.result)
                        {
                            if (resp.requestStatus.comment is null)
                            {
                                LogError($"Request {requestType} returned error code {resp.requestStatus.code}");
                            }
                            else
                            {
                                LogError($"Request {requestType} returned error code {resp.requestStatus.code}: {resp.requestStatus.comment}");
                            }
                            return null;
                        }
                    }
                }
                if (resp is null)
                {
                    LogError($"Request {requestType} timed out");
                    return null;
                }
                return resp.responseData;
            }

            /**
             * <summary>
             * Generates a new request message with a unique uuid for the requestId, and sends it to the server.
             * </summary>
             */
            private string NewRequest(string requestType, object parameters)
            {
                if (!connectionManager.IsConnected())
                {
                    LogError($"Request {requestType} cannot be executed because the client is not connected to OBS");
                    return null;
                }
                string requestId = Guid.NewGuid().ToString();
                var d = new Dictionary<string, object>
                {
                    ["requestType"] = requestType,
                    ["requestId"] = requestId
                };
                if (parameters != null)
                {
                    // add request parameters if there are any
                    d["requestData"] = parameters;
                }

                // Ask replay buffer status
                var req = new Dictionary<string, object>
                {
                    ["op"] = OpCode.Request,
                    ["d"] = d
                };
                connectionManager.SendMsg(req);
                currentRequests.Add(requestId, null);
                return requestId;
            }

            /**
             * <summary>
             * Called when a new request response has been received by the connection manager.
             * Uses the requestId in order to update the pending request.
             * </summary>
             */
            public void ReceiveResponse(string data_str)
            {
                var data = JsonConvert.DeserializeObject<RequestResponseRaw>(data_str);
                var requestId = data.requestId;
                if (currentRequests.ContainsKey(requestId))
                {
                    RequestResponse resp = new RequestResponse();
                    resp.requestStatus = data.requestStatus;
                    if (data.responseData!=null)
                    {
                        resp.responseData = data.responseData.ToString();
                    }
                    else
                    {
                        resp.responseData = "{}";
                    }
                    currentRequests[requestId] = resp;
                }
            }
        }
    }
}
