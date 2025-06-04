namespace SignPuddle.API.Models
{
    public static class FswValidation
    {
        /// <summary>
        /// Determines if the input string is a valid Formal SignWriting (FSW) string.
        /// </summary>
        public static bool IsValidFswSign(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            var fswSignPattern = @"^(A(S[123][0-9a-f]{2}[0-5][0-9a-f])+)?[BLMR]([0-9]{3}x[0-9]{3})(S[123][0-9a-f]{2}[0-5][0-9a-f][0-9]{3}x[0-9]{3})*$";
            return System.Text.RegularExpressions.Regex.IsMatch(input, fswSignPattern);
        }
    }
}