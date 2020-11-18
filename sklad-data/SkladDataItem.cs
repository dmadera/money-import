using System;
using System.Text.RegularExpressions;

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
            Regex r = new Regex(@"(\d{2}).(\d{2}).(\d{2})", RegexOptions.IgnoreCase);
            Match m = r.Match(value);
            if (m.Groups.Count == 4) {
                return new DateTime(
                    int.Parse("20" + m.Groups[3].Value),
                    int.Parse(m.Groups[2].Value),
                    int.Parse(m.Groups[1].Value));
            }

            throw new InvalidCastException("Není validní formát datumu: " + value);
        }

        public float GetFloat() {
            var v = GetDecimal().Replace(",", ".");
            return float.Parse(v);
        }

        static public bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }

    }
}