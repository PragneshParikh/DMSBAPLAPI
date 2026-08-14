using System;

namespace DMS_BAPL_Utils.Helpers
{
    public static class NumberToWordsHelper
    {
        private static readonly string[] Ones = {
            "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
            "Seventeen", "Eighteen", "Nineteen"
        };

        private static readonly string[] Tens = {
            "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };

        public static string Convert(decimal amount)
        {
            long rounded = (long)Math.Round(amount, MidpointRounding.AwayFromZero);

            if (rounded == 0)
                return "Zero Only";

            return $"{ConvertWhole(rounded).Trim()} Only";
        }

        private static string ConvertWhole(long number)
        {
            if (number == 0) return "";

            if (number < 0)
                return "Minus " + ConvertWhole(-number);

            var crore = number / 10000000;
            number %= 10000000;
            var lakh = number / 100000;
            number %= 100000;
            var thousand = number / 1000;
            number %= 1000;
            var hundred = number / 100;
            number %= 100;

            var result = "";

            if (crore > 0) result += ConvertTwoDigit((int)crore) + " Crore ";
            if (lakh > 0) result += ConvertTwoDigit((int)lakh) + " Lakh ";
            if (thousand > 0) result += ConvertTwoDigit((int)thousand) + " Thousand ";
            if (hundred > 0) result += Ones[hundred] + " Hundred ";
            if (number > 0) result += ConvertTwoDigit((int)number) + " ";

            return result;
        }

        private static string ConvertTwoDigit(int number)
        {
            if (number < 20)
                return Ones[number];

            var tens = number / 10;
            var ones = number % 10;
            return (Tens[tens] + " " + Ones[ones]).Trim();
        }
    }
}