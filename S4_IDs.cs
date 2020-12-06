using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using S_IDs;
using SkladData;

namespace S4DataObjs {
    static class S4_IDs {
        private static S5Data _data;

        public static void Deserialize(string input) {
            var serializer = new XmlSerializer(typeof(S5Data));

            using (var stringReader = new StringReader(input)) {
                _data = (S5Data)serializer.Deserialize(stringReader);
            }
        }

        public static string GetArtiklID(string katalog) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataArtikl artikl in _data.ArtiklList) {
                if (artikl.Katalog == katalog) {
                    return artikl.ID;
                }
            }

            return null;
        }

        public static string GetFirmaID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataFirma firma in _data.FirmaList) {
                if (firma.Kod == kod) {
                    return firma.ID;
                }
            }

            return null;
        }

        public static string GetSkladID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataSklad sklad in _data.SkladList) {
                if (sklad.Kod == kod) {
                    return sklad.ID;
                }
            }

            return null;
        }

        public static string GetSazbaDPHID(string sazba) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataSazbaDPH dph in _data.SazbaDPHList) {
                if (dph.Sazba.StartsWith(sazba) || dph.Sazba == sazba) {
                    return dph.ID;
                }
            }

            return null;
        }
    }
}