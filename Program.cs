using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Schemas;

namespace sklad_data_parser {

    static class Str {
        static public bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }

        static public string NumsOnly(string input) {
            return Regex.Replace(input, @"[^0-9]+", "");
        }

        static public string[] ParseEmaily(string input) {
            var split = input.Replace(",", ";").Split(";");
            var emaily = new string[2];
            emaily[0] = IsValidEmail(split[0]) ? split[0] : "";
            emaily[1] = split.Length == 2 && IsValidEmail(split[1]) ? split[1] : "";
            return emaily;
        }

        static public string TrueFalse(string input) {
            var r = new Regex(@"^[1aAY]");
            return r.IsMatch(input) ? "True" : "False";
        }

        static public string AlfaOnly(string input) {
            return Regex.Replace(input, @"[^0-9a-zA-Z]+", "");
        }

        static public string RemMultiSpaces(string input) {
            return Regex.Replace(input, @"\s{2,}", " ");
        }

        static public string Text(string input) {
            return RemMultiSpaces(input.Trim());
        }

        static public string Decimal(string input) {
            return input.Replace(".", ",");
        }
    }

    static class S5DataIDs {
        public static Dictionary<string, string> FirmaTypSpojeni = new Dictionary<string, string>() {
            ["email"] = "dc748a02-43ed-4cc5-8264-4994b56919d0",
            ["tel"] = "999a45ed-7c85-425c-8230-d58ae08a21c6",
            ["emailfa"] = "a26c0533-f56f-49f3-a3c7-911ea1613648",
            ["emailoz"] = "02ccca64-326d-4928-bec0-fbcde09b7445"
        };
        public static Dictionary<string, string> FirmaGroup = new Dictionary<string, string>() {
            ["all"] = "d9667a95-9864-4990-b64c-06140e3b47c9",
            ["np"] = "afae82e8-afba-498c-9d70-9a2737b48b95",
            ["malobch"] = "1feb45f2-40a1-46eb-855f-fdaa080c44c3"
        };
        public static string FirmaStatCZID = "3d3f235c-df25-42ad-9cce-1b460e3a3c5f";
        public static Dictionary<string, string> ArtiklJednotka = new Dictionary<string, string>() {
            ["bal"] = "b774cf29-1165-4ddf-bc6c-08b337e43334",
            ["ks"] = "e604a0fa-c14a-40ca-97ab-c92b2ce618ef",
            ["fol"] = "93aef3ea-4a62-4e50-b0cb-b86862590596",
            ["pal"] = "87fefac8-088c-4bc4-9a24-0c6c74518236",
            ["kart"] = "f1fa221a-343b-4f4b-8851-c2bae9bd15b0"
        };
        public static Dictionary<string, string> ArtiklSazbyDPH = new Dictionary<string, string>() {
            ["21"] = "b7d9bd88-bf7e-4c11-be49-2044879c871c",
            ["15"] = "ab01bdbb-662b-422c-8835-a9270fb48eab",
            ["10"] = "ef12a972-ef84-4516-b21c-d2d3c90fe944"
        };

        public static Dictionary<string, string> ArtiklSklad = new Dictionary<string, string>() {
            ["HL"] = "cd5b9d06-843a-4728-b2b8-31ee6b21b668",
        };
    }
    class Program {

        static void Main(string[] args) {
            string souborOdb = @"/home/dmadera/projects/sklad-data-parser/assets/headODB";
            string souborDod = @"/home/dmadera/projects/sklad-data-parser/assets/headDOD";
            string souborKarty = @"/home/dmadera/projects/sklad-data-parser/assets/headKARTY";

            var odberatele = SKLOdberatel.NactiSoubor(souborOdb);
            var dodavatele = SKLDodavatel.NactiSoubor(souborDod);

            var data = new S5Data() {
                FirmaList = MONFirma.ConvertToS5Data(odberatele, dodavatele),
                ArtiklList = null,
                ZasobaList = new S5DataZasoba[] {
                    new S5DataZasoba() {
                        Artikl = new S5DataZasobaArtikl() {

                        }
                    }
                }
            };


            var serializer = new XmlSerializer(typeof(S5Data));
            using (var stream = new StreamWriter("/home/dmadera/downloads/output.xml")) {
                serializer.Serialize(stream, data);
            }
        }
    }
}
