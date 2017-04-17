namespace Dropbox
{
    internal class DropboxConstants
    {
#if UNITY_EDITOR_WIN
        public static readonly string[] DropboxInfoJsonUrl = { "%APPDATA%\\Dropbox\\info.json", "%LOCALAPPDATA%\\Dropbox\\info.json" };
#else
        public static readonly string[] DropboxInfoJsonUrl = { ".dropbox/info.json"};
#endif
        public const string UrlPref = "dropboxUrl";
    }
}
