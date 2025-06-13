namespace LingoEngine.Tools
{
    public static class MimeHelper
    {
        /// <summary>
        /// Return MIME type for common image extensions.
        /// Fallback to "application/octet-stream" when extension unknown.
        /// </summary>
        public static string GetMimeType(string filename)
        {
            var ext = System.IO.Path.GetExtension(filename).ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}
