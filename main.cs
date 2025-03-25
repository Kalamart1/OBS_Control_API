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

    public class MyMod : MelonMod
    {
        public static void Log(string msg)
        {
            MelonLogger.Msg(msg);
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
        }

        public override void OnFixedUpdate()
        {
        }
    }
}
