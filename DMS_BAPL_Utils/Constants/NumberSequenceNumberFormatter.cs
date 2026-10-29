using System.Globalization;
using System.Text.RegularExpressions;

namespace DMS_BAPL_Utils.Constants
{
    public static class NumberSequenceNumberFormatter
    {
        private static readonly Regex NumberPlaceholder = new("#+", RegexOptions.Compiled);

        public static string Format(string prefixTemplate, int nextNumber)
        {
            if (string.IsNullOrWhiteSpace(prefixTemplate))
                throw new ArgumentException("A sequence prefix template is required.", nameof(prefixTemplate));

            if (nextNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(nextNumber));

            var matches = NumberPlaceholder.Matches(prefixTemplate);
            if (matches.Count != 1)
                throw new ArgumentException("The sequence prefix must contain exactly one number placeholder.", nameof(prefixTemplate));

            return NumberPlaceholder.Replace(
                prefixTemplate,
                match => nextNumber.ToString($"D{match.Length}", CultureInfo.InvariantCulture));
        }
    }
}
