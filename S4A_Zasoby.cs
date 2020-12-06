using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using SA_Zasoby;
using SkladData;

namespace S4DataObjs {
    class S4A_Zasoby {

        private List<S5DataZasoba> _data = new List<S5DataZasoba>();

        private Predicate<S5DataZasoba> _filter = delegate (S5DataZasoba a) {
            return true;
        };

        public static string GetID(string id) {
            return "ZAS" + id;
        }

        public S4A_Zasoby(string kartyFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(kartyFile, encoding);
            convert(new SkladDataFile(lines));
        }

        public S5Data GetS5Data() {
            return new S5Data() {
                ZasobaList = _data.FindAll(_filter).ToArray()
            };
        }

        public void serialize(string output) {
            var serializer = new XmlSerializer(typeof(S5Data));
            using (var stream = new StreamWriter(output)) {
                serializer.Serialize(stream, GetS5Data());
            }
        }

        private void convert(SkladDataFile file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                var zasoba = new S5DataZasoba() {
                    Nazev = d["NazevZbozi"].GetText(),
                    NastaveniZasoby = new S5DataZasobaNastaveniZasoby() {
                        VydejDoMinusu = new enum_VydejDoMinusu() {
                            EnumValueName = enum_VydejDoMinusuEnumValueName.Nekontrolovat
                        }
                    },
                    Sklad = new S5DataZasobaSklad() {
                        Kod = "HL"
                    },
                    Artikl = new S5DataZasobaArtikl() {
                        Katalog = S4A_Katalog.GetID(d["CisloKarty"].GetNum())
                    },
                    Kod = GetID(d["CisloKarty"].GetNum()),
                    HistorickaCena = d["NakupCena"].GetDecimal()
                };

                _data.Add(zasoba);
            }
        }
    }
}
