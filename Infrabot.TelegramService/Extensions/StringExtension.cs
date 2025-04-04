namespace CustomExtensions
{
    public static class StringExtension
    {
        public static string EscapeMarkdown(this string value)
        {
            value = value.Replace("_", "\\_")
                    .Replace("[", "\\[")
                    .Replace(".", "\\.")
                    .Replace("]", "\\]")
                    .Replace("(", "\\(")
                    .Replace(")", "\\)")
                    .Replace("~", "\\~")
                    .Replace(">", "\\>")
                    .Replace("#", "\\#")
                    .Replace("+", "\\+")
                    .Replace("-", "\\-")
                    .Replace("=", "\\=")
                    .Replace("|", "\\|")
                    .Replace("{", "\\{")
                    .Replace("}", "\\}")
                    .Replace("!", "\\!");

            return value;
        }
    }
}
