using System;
using System.Collections.Generic;
using System.Text;

namespace DMS_BAPL_Utils.ViewModels
{
    public class ProformaInvoicePdfModel
    {
        // Dealer
        public string? DealerName { get; set; }
        public string? DealerAddress { get; set; }
        public string? DealerPhone { get; set; }
        public string? DealerEmail { get; set; }
        public string? DealerGstin { get; set; }
        public string? DealerPan { get; set; }
        public string? DealerTradeCertNo { get; set; }

        // Invoice
        public string? ProformaNo { get; set; }
        public string? SaleBillNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? SaleType { get; set; }
        public string? BillTypeText { get; set; }
        public string? CustomerType { get; set; }

        // Customer
        public string? CustomerName { get; set; }
        public string? BillingName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }
        public string? CustomerCountry { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerGstin { get; set; }
        public string? FinancedBy { get; set; }

        // Lines
        public List<ProformaInvoiceLine> Lines { get; set; } = new();

        // Totals
        public bool IsIgst { get; set; }

        public decimal TaxableTotal { get; set; }

        public decimal IgstPer { get; set; }
        public decimal IgstTotal { get; set; }

        public decimal CgstPer { get; set; }
        public decimal CgstTotal { get; set; }

        public decimal SgstPer { get; set; }
        public decimal SgstTotal { get; set; }

        public decimal SubsidyTotal { get; set; }
        public decimal RegTotal { get; set; }
        public decimal InsuranceTotal { get; set; }
        public decimal GrandTotal { get; set; }

        public string? AmountInWords { get; set; }

        public static string IndianCurrencyWords(decimal amount)
        {
            long rupees = (long)decimal.Truncate(amount);
            int paise = (int)Math.Round((amount - rupees) * 100m, MidpointRounding.AwayFromZero);

            if (paise == 100)
            {
                rupees++;
                paise = 0;
            }

            var sb = new StringBuilder();

            sb.Append(rupees == 0
                ? "Zero"
                : NumberToWords(rupees));

            sb.Append(" Rupees");

            if (paise > 0)
            {
                sb.Append(" and ");
                sb.Append(NumberToWords(paise));
                sb.Append(" Paise");
            }

            sb.Append(" Only");

            return sb.ToString();
        }

        private static string NumberToWords(long n)
        {
            if (n == 0)
                return "Zero";

            if (n < 0)
                return "Minus " + NumberToWords(-n);

            string[] ones =
            {
                "", "One", "Two", "Three", "Four", "Five", "Six",
                "Seven", "Eight", "Nine", "Ten", "Eleven",
                "Twelve", "Thirteen", "Fourteen", "Fifteen",
                "Sixteen", "Seventeen", "Eighteen", "Nineteen"
            };

            string[] tens =
            {
                "", "", "Twenty", "Thirty", "Forty",
                "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
            };

            string words = "";

            long crore = n / 10000000;
            n %= 10000000;

            long lakh = n / 100000;
            n %= 100000;

            long thousand = n / 1000;
            n %= 1000;

            long hundred = n / 100;
            n %= 100;

            if (crore > 0)
                words += NumberToWords(crore) + " Crore ";

            if (lakh > 0)
                words += NumberToWords(lakh) + " Lakh ";

            if (thousand > 0)
                words += NumberToWords(thousand) + " Thousand ";

            if (hundred > 0)
                words += ones[hundred] + " Hundred ";

            if (n > 0)
            {
                if (words != "")
                    words += "and ";

                if (n < 20)
                    words += ones[n];
                else
                {
                    words += tens[n / 10];

                    if (n % 10 > 0)
                        words += " " + ones[n % 10];
                }
            }

            return words.Trim();
        }
    }

    public class ProformaInvoiceLine
    {
        public int SrNo { get; set; }

        public string? Description { get; set; }
        public string? ProductCode { get; set; }
        public string? Hsn { get; set; }
        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? Colour { get; set; }

        public int Qty { get; set; } = 1;

        public decimal ItemRate { get; set; }
        public decimal PreGstDiscount { get; set; }
        public decimal TaxableValue { get; set; }

        public decimal IgstPer { get; set; }
        public decimal IgstAmt { get; set; }

        public decimal CgstPer { get; set; }
        public decimal CgstAmt { get; set; }

        public decimal SgstPer { get; set; }
        public decimal SgstAmt { get; set; }

        public decimal FameII { get; set; }
        public decimal RegAmount { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal FinalAmount { get; set; }

        public string? BatteryNo { get; set; }
        public string? Chemistry { get; set; }
        public string? BatteryMake { get; set; }
        public string? BatteryCapacity { get; set; }

        public string? CouponNo { get; set; }

        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? Vcu { get; set; }

        public int? MfgYear { get; set; }
    }
}