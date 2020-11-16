using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using SKDZasoby;
using SkladData;

namespace S5DataObj {
    class S5Zasoby {

        private List<S5DataZasoba> _data = new List<S5DataZasoba>();

        private Predicate<S5DataZasoba> _filter = delegate (S5DataZasoba a) {
            return true;
        };

        public static string GetID(string id) {
            return "ART0" + id;
        }

        public S5Zasoby(string kartyFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(kartyFile, encoding);
            convert(new SkladDataFileKarty(lines));
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

        private void convert(SkladDataFileKarty file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                var zasoba = new S5DataZasoba() {
                    Sklad_ID = "HL",
                    Sklad = new S5DataZasobaSklad() {
                        Kod = "HL"
                    },
                    SkladovaPozice_ID = d["Pozice"].GetAlfaNum(),
                    Artikl_ID = S5Katalog.GetID(d["CisloKarty"].GetNum()),
                    Kod = S5Katalog.GetID(d["CisloKarty"].GetNum()),
                    HistorickaCena = d["NakupCena"].GetDecimal(),
                    Group = new group() {
                        Kod = "HL"
                    }
                };

                _data.Add(zasoba);
            }
        }
    }
}