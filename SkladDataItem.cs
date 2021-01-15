using System;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace SkladData {
    class SkladDataItem {
        private string value;

        public SkladDataItem(string value) {
            this.value = value.Trim();
        }

        public string GetRaw() {
            return value;
        }

        public string GetNum() {
            return Regex.Replace(value, @"[^0-9\-]+", "");
        }

        public string GetBoolean() {
            var r = new Regex(@"^[1aAYT]");
            return r.IsMatch(value) ? "True" : "False";
        }

        public string GetAlfaNum() {
            return Regex.Replace(value, @"[^0-9a-zA-Z]+", "");

        }

        public string GetText() {
            return Regex.Replace(value, @"\s{2,}", " ");
        }

        public string GetTextNoDiacritics() {
            return RemoveDiacritics(Regex.Replace(value, @"\s{2,}", " "));
        }

        public string GetDecimal() {
            return Regex.Replace(value.Replace(".", ","), @"[^0-9,\-]+", "");
        }

        public bool IsEmpty() {
            return Regex.Replace(value, @"\s", "") == "";
        }

        public string GetNoSpaces() {
            return Regex.Replace(value, @"\s", "");
        }

        public string GetDecimalNegative() {
            var v = GetDecimal();
            return v.StartsWith("-") ? v.Substring(1) : "-" + v;
        }

        public string GetNumNegative() {
            var v = GetNum();
            return v.StartsWith("-") ? v.Substring(1) : "-" + v;
        }

        public DateTime GetDate() {
            double val = double.Parse(value);
            if(val <= 719163) return new DateTime(); 
            return DateTime.UnixEpoch.AddDays(val - 719163);
        }

        public float GetFloat() {
            var v = GetDecimal().Replace(",", ".");
            return float.Parse(v);
        }

        public static bool IsValidEmail(string address) => address != null && new EmailAddressAttribute().IsValid(address);

        public static string RemoveDiacritics(string text) {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString) {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

    }
}