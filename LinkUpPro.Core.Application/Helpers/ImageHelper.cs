namespace LinkUpPro.Core.Application.Helpers
{
    public static class ImageHelper
    {
        public static bool IsValidImageFile(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 4) return false;

            var header = fileBytes.Take(4).ToArray();

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;

            // PNG: 89 50 4E 47
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;

            // WEBP: RIFF ... WEBP
            if (fileBytes.Length > 12)
            {
                var webpHeader = fileBytes.Take(12).ToArray();
                if (webpHeader[0] == 0x52 && webpHeader[1] == 0x49 && webpHeader[2] == 0x46 && webpHeader[3] == 0x46 &&
                    webpHeader[8] == 0x57 && webpHeader[9] == 0x45 && webpHeader[10] == 0x42 && webpHeader[11] == 0x50)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
