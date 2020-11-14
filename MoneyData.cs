using Schemas;
using System.Xml.Serialization;
using System.IO;

namespace MoneyDataObjects {
    class MoneyData {
        private S5Data data;

        public MoneyData(string path) {
            var serializer = new XmlSerializer(typeof(S5Data));
            using (var stream = new StreamReader(path)) {
                data = (S5Data)serializer.Deserialize(stream);

            }
        }
        public string GetKategorieID(string kod) {
            foreach (S5DataKategorieArtiklu k in data.KategorieArtikluList) {
                if (k.Kod == kod) {
                    return k.ID;
                }
            }
            return null;
        }
    }
}