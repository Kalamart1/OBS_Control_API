using MelonLoader;
using RumbleModUI;

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
        private string ip = "localhost";
        private int port = 4455;
        private string password = "your_password_here";

        // variables
        Mod Mod = new Mod();
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
         * Initialize the websocket client.
         * </summary>
         */
        public void InitClient()
        {
            requestManager = new RequestManager();
            connectionManager = new ConnectionManager(requestManager, ip, port, password);
            InitEvents();
            onConnect += OnConnect;
            onDisconnect += OnDisconnect;
        }

        /**
         * <summary>
         * Called when the scene has finished loading.
         * When we are in the loader scene, starts the connection thread.
         * </summary>
         */
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Loader")
            {
                InitClient();
                SetUIOptions();
                OnUISaved();
                UI.instance.UI_Initialized += OnUIInit;
            }
        }

        /**
         * <summary>
         * (Re)start connection thread.
         * </summary>
         */
        public void Connect()
        {
            if (IsConnected())
            {
                Disconnect();
            }
            connectionManager.UpdateWebsocketConfig(ip, port, password);
            connectionManager.Start();
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
         * Specify the different options that will be used in the ModUI settings
         * </summary>
         */
        private void SetUIOptions()
        {
            Mod.ModName = BuildInfo.ModName;
            Mod.ModVersion = BuildInfo.ModVersion;

            Mod.SetFolder("OBS_Control_API");
            Mod.AddToList("Force enable replay buffer", true, 0, "Never forget to start the replay buffer again!\n" +
                "The mod will start it for you on connection, and stop it as you close the game.", new Tags { });
            Mod.AddToList("IP address", "localhost", "IP address of the OBS websocket server.", new Tags { });
            Mod.AddToList("Port", 4455, "Port used by the OBS websocket server.", new Tags { });
            Mod.AddToList("Password", "", "Password for the OBS websocket server.", new Tags { });
            Mod.GetFromFile();
        }

        /**
         * <summary>
         * Called when the actual ModUI window is initialized
         * </summary>
         */
        private void OnUIInit()
        {
            Mod.ModSaved += OnUISaved;
            UI.instance.AddMod(Mod);
        }

        /**
         * <summary>
         * Called when the user saves a configuration in ModUI
         * </summary>
         */
        private void OnUISaved()
        {
            forceReplayBuffer = (bool)Mod.Settings[0].SavedValue;
            ip = (string)Mod.Settings[1].SavedValue;
            port = (int)Mod.Settings[2].SavedValue;
            password = (string)Mod.Settings[3].SavedValue;
            Connect();
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
            Disconnect();
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