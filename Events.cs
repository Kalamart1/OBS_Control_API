using Newtonsoft.Json;
using System.Threading.Tasks;
using System;

namespace OBS_Control_API
{
    public partial class OBS
    {
        public static event Action onConnect;
        public static event Action onDisconnect;
        public static event Action<Event> onEvent;
        public static event Action<string> onReplayBufferSaved;
        public static event Action onReplayBufferStarted;
        public static event Action onReplayBufferStopped;
        public static event Action<string> onRecordingStarted;
        public static event Action<string> onRecordingStopping;
        public static event Action<string> onRecordingStopped;
        public static event Action onRecordingPaused;
        public static event Action onRecordingResumed;
        public static event Action onStreamStarted;
        public static event Action onStreamStopped;
        public static event Action<string> onRecordFileChanged;

        /**
         * <summary>
         * Enable the triggering of the events.
         * </summary>
         */
        private void InitEvents()
        {
            connectionManager.onEvent += OnEvent;
        }

        /**
         * <summary>
         * Called every time an event is received.
         * Performs the parsing of a few main types of events.
         * </summary>
         */
        private void OnEvent(Event data)
        {
            Task.Run(() => onEvent(data));
            //Log($"OnEvent {data.eventType}");
            string eventDataStr = data.eventData.ToString();
            if (data.eventType == "ReplayBufferSaved")
            {
                var eventData = JsonConvert.DeserializeObject<Event.ReplayBufferSaved>(eventDataStr);
                Task.Run(() => onReplayBufferSaved(eventData.savedReplayPath));
            }
            if (data.eventType == "ReplayBufferStateChanged")
            {
                var eventData = JsonConvert.DeserializeObject<Event.ReplayBufferStateChanged>(eventDataStr);
                isReplayBufferActive = eventData.outputActive;
                if (eventData.outputState== "OBS_WEBSOCKET_OUTPUT_STARTED")
                {
                    Task.Run(() => onReplayBufferStarted());
                }
                else if (eventData.outputState == "OBS_WEBSOCKET_OUTPUT_STOPPED")
                {
                    Task.Run(() => onReplayBufferStopped());
                }
            }
            else if (data.eventType == "RecordStateChanged")
            {
                var eventData = JsonConvert.DeserializeObject<Event.RecordStateChanged>(eventDataStr);
                isRecordingActive = eventData.outputActive;
                if (eventData.outputState == "OBS_WEBSOCKET_OUTPUT_STARTED")
                {
                    Task.Run(() => onRecordingStarted(eventData.outputPath));
                }
                else if (eventData.outputState == "OBS_WEBSOCKET_OUTPUT_STOPPING")
                {
                    Task.Run(() => onRecordingStopping(eventData.outputPath));
                }
                else if (eventData.outputState == "OBS_WEBSOCKET_OUTPUT_STOPPED")
                {
                    Task.Run(() => onRecordingStopped(eventData.outputPath));
                }
                else if (eventData.outputState == "OBS_WEBSOCKET_OUTPUT_PAUSED")
                {
                    Task.Run(() => onRecordingPaused());
                }
                else if (eventData.outputState == "OBS_WEBSOCKET_OUTPUT_RESUMED")
                {
                    Task.Run(() => onRecordingResumed());
                }
            }
            else if (data.eventType == "StreamStateChanged")
            {
                var eventData = JsonConvert.DeserializeObject<Event.ReplayBufferStateChanged>(eventDataStr);
                isStreamActive = eventData.outputActive;
                if (eventData.outputState == "OBS_WEBSOCKET_OUTPUT_STARTED")
                {
                    Task.Run(() => onStreamStarted());
                }
                else if (eventData.outputState == "OBS_WEBSOCKET_OUTPUT_STOPPED")
                {
                    Task.Run(() => onStreamStopped());
                }
            }
            else if (data.eventType == "RecordFileChanged")
            {
                var eventData = JsonConvert.DeserializeObject<Event.RecordFileChanged>(eventDataStr);
                Task.Run(() => onRecordFileChanged(eventData.newOutputPath));
            }
        }
    }
}

namespace OBS_Control_API
{
    // We define the structure of event data as nested classes
    public partial class Event
    {
        public class ReplayBufferStateChanged
        {
            public bool outputActive { get; set; }
            public string outputState { get; set; }
        }

        public class ReplayBufferSaved
        {
            public string savedReplayPath { get; set; }
        }

        public class RecordStateChanged
        {
            public bool outputActive { get; set; }
            public string outputPath { get; set; }
            public string outputState { get; set; }
        }
        public class RecordFileChanged
        {
            public string newOutputPath { get; set; }
        }
    }

}