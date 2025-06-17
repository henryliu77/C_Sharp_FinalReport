public static class EmojiHelper
{
    private static Dictionary<string, string> emojiDict;

    /*static EmojiHelper()
    {
        string emojiFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Emojis");
        emojiDict = LoadEmojiDict(emojiFolder);
    }

    public static bool IsEmojiCode(string msg)
    {
        return !string.IsNullOrEmpty(msg) && msg.StartsWith(":") && msg.EndsWith(":") && emojiDict.ContainsKey(msg);
    }

    public static string GetEmojiFilePath(string code)
    {
        if (emojiDict.ContainsKey(code))
            return emojiDict[code];
        return null;
    }*/

    // 你可以直接用前面提供的這個方法
    public static Dictionary<string, string> LoadEmojiDict(string emojiFolder)
    {
        var dict = new Dictionary<string, string>();
        if (!Directory.Exists(emojiFolder))
            return dict;
        var files = Directory.GetFiles(emojiFolder, "*.*")
                    .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase));
        foreach (var file in files)
        {
            string filename = Path.GetFileNameWithoutExtension(file);  // e.g. "smile"
            string code = $":{filename}:";                             // e.g. ":smile:"
            dict[code] = file;
        }
        return dict;
    }
}
