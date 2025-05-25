using MelonLoader;

namespace OBS_Control_API
{
    public static class BuildInfo
    {
        public const string ModName = "OBS_Control_API";
        public const string ModVersion = "1.0.0";
        public const string Description = "Manages a websocket connection to OBS";
        public const string Author = "Kalamart";
        public const string Company = "";
    }
    public partial class OBS : MelonMod
    {
        //constants
        private bool forceReplayBuffer = true;
        private const string ip = "localhost";
        private const int port = 4455;
        private const string password = "your_password_here";

        // variables
        private ConnectionManager connectionManager;
        private static RequestManager requestManager;

        private bool isReplayBufferActive = false;
        private bool isRecordingActive = false;
        private bool isStreamActive = false;
        private bool stopReplayBufferAtShutdown = false;

        /**
         * <summary>
         * Log to console.
         * </summary>
         */
        private static void Log(string msg)
        {
            MelonLogger.Msg(msg);
        }
        /**
         * <summary>
         * Log to console but in yellow.
         * </summary>
         */
        private static void LogWarn(string msg)
        {
            MelonLogger.Warning(msg);
        }
        /**
         * <summary>
         * Log to console but in red.
         * </summary>
         */
        private static void LogError(string msg)
        {
            MelonLogger.Error(msg);
        }

        /**
         * <summary>
         * Called when the mod is initialized at the start of the game.
         * </summary>
         */
        public override void OnLateInitializeMelon()
        {
            requestManager = new RequestManager();
            connectionManager = new ConnectionManager(requestManager, ip, port, password);
            InitEvents();
            onConnect += OnConnect;
            onDisconnect += OnDisconnect;
            connectionManager.Start();
        }

        /**
         * <summary>
         * (Re)start connection thread.
         * </summary>
         */
        public void Connect()
        {
            if (!IsConnected())
            {
                connectionManager.UpdateWebsocketConfig(ip, port, password);
                connectionManager.Start();
            }
        }

        /**
         * <summary>
         * Close the connection and don't try to reconnect.
         * </summary>
         */
        public void Disconnect()
        {
            connectionManager.Stop();
        }

        /**
         * <summary>
         * Called when the connection with OBS is 100% established and authentication is complete.
         * </summary>
         */
        private void OnConnect()
        {
            SetMainStatus();
            if (!isReplayBufferActive && forceReplayBuffer)
            {
                Log($"Replay Buffer was inactive, starting it...");
                stopReplayBufferAtShutdown = true;
                StartReplayBuffer();
            }
        }

        /**
         * <summary>
         * Called when the connection with OBS is closed.
         * </summary>
         */
        private void OnDisconnect()
        {
            isReplayBufferActive = false;
            isRecordingActive = false;
            isStreamActive = false;
        }

        /**
         * <summary>
         * Called 50 times per second, used for frequent updates.
         * </summary>
         */
        public override void OnFixedUpdate()
        {
        }

        /**
         * <summary>
         * Called when the game is closed cleanly (by closing the window).
         * </summary>
         */
        public override void OnApplicationQuit()
        {
            if (connectionManager.IsConnected() && isReplayBufferActive && stopReplayBufferAtShutdown)
            {
                Log("Stopping replay buffer");
                StopReplayBuffer();
            }
            connectionManager.Stop();
        }

        /**
         * <summary>
         * Returns true if the client is connected to OBS, ready to send requests and receive events.
         * </summary>
         */
        public bool IsConnected()
        {
            return connectionManager.IsConnected();
        }

        /**
         * <summary>
         * Returns true if the replay buffer is active.
         * </summary>
         */
        public bool IsReplayBufferActive()
        {
            return isReplayBufferActive;
        }

        /**
         * <summary>
         * Returns true if recording is active.
         * </summary>
         */
        public bool IsRecordingActive()
        {
            return isRecordingActive;
        }

        /**
         * <summary>
         * Returns true if streaming is active.
         * </summary>
         */
        public bool IsStreamActive()
        {
            return isStreamActive;
        }
    }
}