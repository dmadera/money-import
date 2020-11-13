using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace sklad_data_parser {
    class SKLDodavatel {

        private static string prefix = "AD";
        private static string sufix = "DOD";

        public string kod, nazev, ico, dic, poznamka;
        public string zastupujici, telefon, email, email2, zastupujici_oz, telefon_oz, email_oz, email2_oz;
        public string ulice, mesto, psc;
        public string datumporizeni, dodani, zavozden;

        public SKLDodavatel(string radek) {

            Regex r = new Regex(@"^\s{5}\p{N}{5}\s{5}");
            if (!r.IsMatch(radek)) {
                throw new ArgumentException("Datový řádek neodpovídá formátu.");
            }

            int[] splits = { 16, 89, 110, 131, 137, 147, 151, 162, 183, 204, 225, 246,
                307, 368, 427, 486, 545, 604, 664, 678, 689};

            string[] h = new string[splits.Length + 1];
            int start = 0;
            int i = 0;
            foreach (int split in splits) {
                h[i] = radek.Substring(start, split - start).Trim();
                start = split;
                i++;
            }
            h[i] = radek.Substring(start);

            kod = GetKod(Str.NumsOnly(h[0]));
            nazev = Str.Text(h[1]);
            ulice = h[4];
            mesto = h[5];
            psc = Str.NumsOnly(h[6]);
            ico = Str.NumsOnly(h[7]);
            dic = h[9] == "" ? "" : "CZ" + Str.NumsOnly(h[9]);
            zastupujici = h[10];
            zastupujici_oz = h[11];
            telefon = Str.NumsOnly(h[12]);
            var emaily = Str.ParseEmaily(h[13]);
            email = emaily[0];
            email2 = emaily[1];
            emaily = Str.ParseEmaily(h[14]);
            email_oz = emaily[0];
            email2_oz = emaily[1];
            poznamka = String.Format("{0}\n{1}\n{2}\n{3}\n{4}", h[15..20]);
            datumporizeni = h[20];
            dodani = Str.NumsOnly(h[21]);
            zavozden = h[22];

            r = new Regex(@"^([\|\*\\]+|a\p{N})+(.*)$");
            if (r.IsMatch(nazev)) {
                throw new ArgumentException("Hodnota názvu neodpovídá dodavateli.");
            }
        }

        public static string GetKod(string cislo) {
            return prefix + sufix + cislo;
        }

        public static List<SKLDodavatel> NactiSoubor(string soubor) {
            string[] radky = System.IO.File.ReadAllLines(soubor,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            var odberatele = new List<SKLDodavatel>();

            foreach (string radek in radky) {
                try {
                    odberatele.Add(new SKLDodavatel(radek));
                } catch (ArgumentException e) {
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine("Detail řádku: " + radek);
                }
            }

            return odberatele;
        }
    }

}