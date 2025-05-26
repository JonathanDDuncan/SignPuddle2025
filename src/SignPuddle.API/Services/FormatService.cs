namespace SignPuddle.API.Services
{
    public interface IFormatService
    {
        string ConvertFswToSvg(string fsw);
        string ConvertFswToPng(string fsw);
        string ConvertFswToKsw(string fsw);
        string ConvertFswToBsw(string fsw);
        string ConvertKswToFsw(string ksw);
        string ConvertBswToFsw(string bsw);
        bool ValidateFormat(string text, string format);
        bool IsVertical(string text);
    }

    public class FormatService : IFormatService
    {
        public string ConvertFswToSvg(string fsw)
        {
            // Implementation would convert FSW to SVG
            // This would use the logic from the current PHP implementation in msw.php
            return GenerateSvgFromFsw(fsw);
        }

        public string ConvertFswToPng(string fsw)
        {
            // Implementation would convert FSW to PNG
            // This would likely call the SVG conversion and then render to PNG
            return ConvertSvgToPng(ConvertFswToSvg(fsw));
        }

        public string ConvertFswToKsw(string fsw)
        {
            // Implementation would convert FSW to KSW format
            // This would use the logic from the current PHP implementation in ksw.php
            return TransformFswToKsw(fsw);
        }

        public string ConvertFswToBsw(string fsw)
        {
            // Implementation would convert FSW to BSW format
            // This would use the logic from the current PHP implementation in bsw.php
            return TransformFswToBsw(fsw);
        }

        public string ConvertKswToFsw(string ksw)
        {
            // Implementation would convert KSW to FSW format
            return TransformKswToFsw(ksw);
        }

        public string ConvertBswToFsw(string bsw)
        {
            // Implementation would convert BSW to FSW format
            return TransformBswToFsw(bsw);
        }

        public bool ValidateFormat(string text, string format)
        {
            return format.ToLower() switch
            {
                "fsw" => ValidateFsw(text),
                "ksw" => ValidateKsw(text),
                "bsw" => ValidateBsw(text),
                _ => false
            };
        }

        public bool IsVertical(string text)
        {
            // Implementation would check if the text is vertical
            // This would use the logic from isVert() in msw.php
            return text.Contains("L") && !text.Contains("M");
        }

        #region Helper Methods
        private string GenerateSvgFromFsw(string fsw)
        {
            // Implementation details here
            return $"<svg>{fsw}</svg>";
        }

        private string ConvertSvgToPng(string svg)
        {
            // Implementation details here
            return "data:image/png;base64,...";
        }

        private string TransformFswToKsw(string fsw)
        {
            // Implementation details here
            return fsw.Replace("S", "K");
        }

        private string TransformFswToBsw(string fsw)
        {
            // Implementation details here
            return fsw.Replace("S", "B");
        }

        private string TransformKswToFsw(string ksw)
        {
            // Implementation details here
            return ksw.Replace("K", "S");
        }

        private string TransformBswToFsw(string bsw)
        {
            // Implementation details here
            return bsw.Replace("B", "S");
        }

        private bool ValidateFsw(string fsw)
        {
            // Implementation details here
            return fsw.StartsWith("S");
        }

        private bool ValidateKsw(string ksw)
        {
            // Implementation details here
            return ksw.StartsWith("K");
        }

        private bool ValidateBsw(string bsw)
        {
            // Implementation details here
            return bsw.StartsWith("B");
        }
        #endregion
    }
}