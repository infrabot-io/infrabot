namespace Infrabot.WebUI.Utils
{
    public static class PathNormalizer
    {
        public static string NormalizePath(string path)
        {
            return Path.IsPathRooted(path)
                ? path
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        }
    }
}
