using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace sklad_data_parser {
    class SKLOdberatel {
        private static string prefix = "AD";
        private static string sufix = "ODB";

        public string kod, nazev, splatnost, sleva, ico, dic, poznamka, sdph;
        public string objednavajici_prijmeni, prebirajici_prijmeni, telefon, email, email2, emailFA, emailFA2;
        public string ulice, mesto, psc;
        public string nazevp, ulicep, mestop, pscp;
        public string kupnismlouva, sumfa, kodsumfa, davatsek, np, kododb, cisloskupiny, odesilat;
        public string datumporizeni, prabato, prirazka, cislodod;

        public SKLOdberatel(string radek) {

            Regex r = new Regex(@"^\s{5}\p{N}{5}\s{5}");
            if (!r.IsMatch(radek)) {
                throw new ArgumentException("Datový řádek neodpovídá formátu.");
            }

            int[] splits = { 16, 89, 97, 105, 126, 147, 153, 163, 167, 196, 199, 220, 241, 303, 311, 373,
                386, 392, 400, 461, 531, 583, 644, 706, 713, 728, 734, 743, 757, 766, 828, 849, 871,
                883, 891, 900 };

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
            sleva = Str.Decimal(String.Format("{0:0.00#}", float.Parse(h[2]) * -1));
            splatnost = Str.NumsOnly(h[3]);
            ulice = h[4];
            mesto = h[5];
            psc = Str.NumsOnly(h[6]);
            ico = Str.NumsOnly(h[7]);
            dic = h[9] == "" ? "" : "CZ" + Str.NumsOnly(h[9]);
            objednavajici_prijmeni = h[10];
            prebirajici_prijmeni = h[11];
            telefon = Str.NumsOnly(h[12]);
            var emaily = Str.ParseEmaily(h[13]);
            email = emaily[0];
            email2 = emaily[1];
            odesilat = Str.TrueFalse(h[14]);
            emaily = Str.ParseEmaily(h[15]);
            emailFA = emaily[0];
            emailFA2 = emaily[1];
            kupnismlouva = Str.TrueFalse(h[16]);
            sumfa = Str.TrueFalse(h[17]);
            kodsumfa = Str.AlfaOnly(h[18]);
            poznamka = String.Format("{0}\n{1}\n{2}\n{3}\n{4}", h[19..24]);
            kododb = Str.AlfaOnly(h[24]);
            np = Str.TrueFalse(h[25]);
            sdph = Str.TrueFalse(h[26]);
            davatsek = Str.TrueFalse(h[27]);
            datumporizeni = h[28];
            cisloskupiny = Str.NumsOnly(h[29]);
            nazevp = h[30];
            ulicep = h[31];
            mestop = h[32];
            pscp = Str.NumsOnly(h[33]);
            prabato = Str.NumsOnly(h[36]);
            prirazka = Str.TrueFalse(h[35]);
            cislodod = Str.NumsOnly(h[36]);

            r = new Regex(@"^([\|\*\\]+|a\p{N})+(.*)$");
            if (r.IsMatch(nazev)) {
                throw new ArgumentException("Hodnota názvu neodpovídá odběrateli.");
            }
        }

        public static string GetKod(string cislo) {
            return prefix + sufix + cislo;
        }

        public static List<SKLOdberatel> NactiSoubor(string soubor) {
            string[] radky = System.IO.File.ReadAllLines(soubor,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            var odberatele = new List<SKLOdberatel>();

            foreach (string radek in radky) {
                try {
                    odberatele.Add(new SKLOdberatel(radek));
                } catch (ArgumentException e) {
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine("Detail řádku: " + radek);
                }
            }

            return odberatele;
        }
    }

}