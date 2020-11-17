using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using SK_Prijem;
using SkladData;

namespace S4DataObjs {
    class S4K_Prijem {

        private List<S5DataDodaciListPrijaty> _data = new List<S5DataDodaciListPrijaty>();

        private Predicate<S5DataDodaciListPrijaty> _filter = delegate (S5DataDodaciListPrijaty a) {
            return true;
        };

        public static string GetID(string id) {
            return "ART0" + id;
        }

        public S4K_Prijem(string kartyFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(kartyFile, encoding);
            convert(new SkladDataFileKarty(lines));
        }

        public S5Data GetS5Data() {
            return new S5Data() {
                DodaciListPrijatyList = _data.FindAll(_filter).ToArray()
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

                var dl = new S5DataDodaciListPrijaty() {

                };

                _data.Add(dl);
            }
        }
    }
}