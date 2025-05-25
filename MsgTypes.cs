namespace OBS_Control_API
{
    /**
     * <summary>
     * Exhaustive list of Op codes in the OBS websocket API.
     * </summary>
     */
    public enum OpCode
    {
        Hello = 0,
        Identify = 1,
        Identified = 2,
        Reidentify = 3,
        Event = 5,
        Request = 6,
        RequestResponse = 7,
        RequestBatch = 8,
        RequestBatchResponse = 9
    }

    /**
     * <summary>
     * Structure of received messages.
     * </summary>
     */
    public class Msg
    {
        public object d { get; set; }
        public int op { get; set; }
    }

    /**
     * <summary>
     * Structure of the data received with OpCode Hello.
     * </summary>
     */
    public class Hello
    {
        public Authentication authentication { get; set; }
        public string obsWebSocketVersion { get; set; }
        public string rpcVersion { get; set; }
        public class Authentication
        {
            public string challenge { get; set; }
            public string salt { get; set; }
        }
    }

    /**
     * <summary>
     * Structure of the data received with OpCode RequestResponse.
     * This is for parsing purposes only, and will not be directly transmitted to the caller.
     * </summary>
     */
    public class RequestResponseRaw
    {
        public string requestId { get; set; }
        public RequestResponse.RequestStatus requestStatus { get; set; }
        public object responseData { get; set; } // optional, depends on request
    }

    /**
     * <summary>
     * Structure of the data of the request response that we want to keep.
     * </summary>
     */
    public partial class RequestResponse
    {
        public RequestStatus requestStatus { get; set; }
        public string responseData { get; set; } // optional, depends on request
        public class RequestStatus
        {
            public bool result { get; set; }
            public int code { get; set; }
            public string comment { get; set; } // optional
        }

        /**
         * <summary>
         * Non-exhaustive list of codes that can be returned in RequestStatus
         * </summary>
         */
        public enum Code
        {
            Success = 100,
            NotReady = 207
        }
    }

    /**
     * <summary>
     * Structure of the data received with OpCode Event.
     * </summary>
     */
    public partial class Event
    {
        public object eventData { get; set; } // depends on event
        public int eventIntent { get; set; }
        public string eventType { get; set; }
    }
}
