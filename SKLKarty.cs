using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace sklad_data_parser {
    class SKLKarta {
        private static string prefix = "ART";
        private static string sufix = "0";

        public string kod, nazev, poznamka, mernajednotka, nakupcena, sazbad, rabat, pocet;
        public string vpal, vkart, vfol, minfol;
        public string cislododavatele, nazevdodavatele, kodzbozi, podkodzbozi;
        public string prodejnost, priznak, zobrazovat, pozice, dodcislo;

        public SKLKarta(string radek) {

            Regex r = new Regex(@"^\s{3}\p{N}{4}\s{3}");
            if (!r.IsMatch(radek)) {
                throw new ArgumentException("Datový řádek neodpovídá formátu.");
            }

            int[] splits = { 12, 53, 95, 109, 121, 128, 135, 144, 150, 156, 161, 168, 183, 219, 229, 241, 253,
                             261, 272, 278 };

            var h = new string[splits.Length + 1];
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
            poznamka = Str.Text(h[2]);
            mernajednotka = Str.Text(h[3]).ToLower();
            nakupcena = Str.Decimal(h[4]);
            sazbad = Str.NumsOnly(h[5]);
            rabat = Str.Decimal(h[6]);
            pocet = Str.NumsOnly(h[7]);
            vpal = Str.NumsOnly(h[8]);
            vkart = Str.NumsOnly(h[9]);
            vfol = Str.NumsOnly(h[10]);
            minfol = Str.TrueFalse(h[11]);
            cislododavatele = Str.Decimal(h[12]);
            nazevdodavatele = Str.Text(h[13]);
            kodzbozi = Str.NumsOnly(h[14]);
            podkodzbozi = Str.NumsOnly(h[15]);
            prodejnost = Str.Decimal(h[16]);
            priznak = Str.AlfaOnly(h[17]);
            zobrazovat = Str.TrueFalse(h[18]);
            pozice = Str.AlfaOnly(h[19]);
            dodcislo = Str.Text(h[20]);

            r = new Regex(@"^([\|\*\\]+|a\p{N})+(.*)$");
            if (r.IsMatch(nazev)) {
                throw new ArgumentException("Hodnota názvu neodpovídá odběrateli.");
            }
        }

        public static string GetKod(string cislo) {
            return prefix + sufix + cislo;
        }

        public static List<SKLKarta> NactiSoubor(string soubor) {
            string[] radky = System.IO.File.ReadAllLines(soubor,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            var karty = new List<SKLKarta>();

            foreach (string radek in radky) {
                try {
                    karty.Add(new SKLKarta(radek));
                } catch (ArgumentException e) {
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine("Detail řádku: " + radek);
                }
            }

            return karty;
        }
    }

}