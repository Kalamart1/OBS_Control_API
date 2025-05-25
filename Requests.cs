using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OBS_Control_API
{
    public partial class OBS
    {
        public static string SendRequest(string requestType, object parameters)
        {
            return requestManager.SendRequest(requestType, parameters);
        }
        public static string SendRequest(string requestType)
        {
            return SendRequest(requestType, null);
        }
        public static RequestResponse.GetReplayBufferStatus GetReplayBufferStatus()
        {
            var resp = SendRequest("GetReplayBufferStatus");
            if (resp is null) return null;
            return JsonConvert.DeserializeObject<RequestResponse.GetReplayBufferStatus>(resp);
        }
        public static RequestResponse.GetRecordStatus GetRecordStatus()
        {
            var resp = SendRequest("GetRecordStatus");
            if (resp is null) return null;
            return JsonConvert.DeserializeObject<RequestResponse.GetRecordStatus>(resp);
        }
        public static RequestResponse.GetStreamStatus GetStreamStatus()
        {
            var resp = SendRequest("GetStreamStatus");
            if (resp is null) return null;
            return JsonConvert.DeserializeObject<RequestResponse.GetStreamStatus>(resp);
        }
        public static RequestResponse.GetVersion GetVersion()
        {
            var resp = SendRequest("GetVersion");
            if (resp is null) return null;
            return JsonConvert.DeserializeObject<RequestResponse.GetVersion>(resp);
        }
        public static bool StartStream()
        {
            var resp = SendRequest("StartStream");
            return !(resp is null);
        }
        public static bool StopStream()
        {
            var resp = SendRequest("StopStream");
            return !(resp is null);
        }
        public static bool StartReplayBuffer()
        {
            var resp = SendRequest("StartReplayBuffer");
            return !(resp is null);
        }
        public static bool StopReplayBuffer()
        {
            var resp = SendRequest("StopReplayBuffer");
            return !(resp is null);
        }
        public static bool SaveReplayBuffer()
        {
            var resp = SendRequest("SaveReplayBuffer");
            return !(resp is null);
        }
        public static RequestResponse.GetLastReplayBufferReplay GetLastReplayBufferReplay()
        {
            var resp = SendRequest("GetLastReplayBufferReplay");
            if (resp is null) return null;
            return JsonConvert.DeserializeObject<RequestResponse.GetLastReplayBufferReplay>(resp);
        }
        public static bool StartRecord()
        {
            var resp = SendRequest("StartRecord");
            return !(resp is null);
        }
        public static RequestResponse.StopRecord StopRecord()
        {
            var resp = SendRequest("StopRecord");
            if (resp is null) return null;
            return JsonConvert.DeserializeObject<RequestResponse.StopRecord>(resp);
        }
        public static RequestResponse.ToggleRecord ToggleRecord()
        {
            var resp = SendRequest("ToggleRecord");
            if (resp is null) return null;
            return JsonConvert.DeserializeObject<RequestResponse.ToggleRecord>(resp);
        }
        public static bool PauseRecord()
        {
            var resp = SendRequest("PauseRecord");
            return !(resp is null);
        }
        public static bool ResumeRecord()
        {
            var resp = SendRequest("ResumeRecord");
            return !(resp is null);
        }
        public static bool SplitRecordFile()
        {
            var resp = SendRequest("SplitRecordFile");
            return !(resp is null);
        }
        public static RequestResponse.GetRecordDirectory GetRecordDirectory()
        {
            var resp = SendRequest("GetRecordDirectory");
            if (resp is null) return null;
            return JsonConvert.DeserializeObject<RequestResponse.GetRecordDirectory>(resp);
        }
        public static bool SetRecordDirectory(string recordDirectory)
        {
            var parameters = new Dictionary<string, object>
            {
                ["recordDirectory"] = recordDirectory
            };
            var resp = SendRequest("SetRecordDirectory", parameters);
            return !(resp is null);
        }
        private void SetMainStatus()
        {
            try
            {
                isReplayBufferActive = GetReplayBufferStatus().outputActive;
                isRecordingActive = GetRecordStatus().outputActive;
                isStreamActive = GetStreamStatus().outputActive;
            }
            catch (Exception ex)
            {
                MelonLogger.Error(ex.Message);
            }
        }
    }
}

namespace OBS_Control_API
{
    public partial class RequestResponse
    {
        public class GetReplayBufferStatus
        {
            public bool outputActive { get; set; }
        }
        public class ToggleRecord
        {
            public bool outputActive { get; set; }
        }
        public class StopRecord
        {
            public string outputPath { get; set; }
        }
        public class GetRecordStatus
        {
            public bool outputActive { get; set; }
            public bool outputPaused { get; set; }
            public string outputTimecode { get; set; }
            public int outputDuration { get; set; }
            public long outputBytes { get; set; }
        }
        public class GetStreamStatus
        {
            public bool outputActive { get; set; }
            public bool outputReconnecting { get; set; }
            public string outputTimecode { get; set; }
            public int outputDuration { get; set; }
            public float outputCongestion { get; set; }
            public long outputBytes { get; set; }
            public int outputSkippedFrames { get; set; }
            public int outputTotalFrames { get; set; }
        }
        public class GetVersion
        {
            public string obsVersion { get; set; }
            public string obsWebSocketVersion { get; set; }
            public int rpcVersion { get; set; }
            public string[] availableRequests { get; set; }
            public string[] supportedImageFormats { get; set; }
            public string platform { get; set; }
            public string platformDescription { get; set; }
        }
        public class GetLastReplayBufferReplay
        {
            public string savedReplayPath { get; set; }
        }
        public class GetRecordDirectory
        {
            public string recordDirectory { get; set; }
        }
    }

}