using System.Collections.Generic;

using SkladData;
using Schemas;

namespace MoneyDataObjects {

    class MoneyDataKategorie {
        public static List<S5DataKategorieArtiklu> GetData(SkladDataFile kat) {
            var data = new List<S5DataKategorieArtiklu>();
            foreach (SkladDataObj obj in kat.Data) {
                var d = obj.Items;
                string kod, nazev, parent = null;

                if (!d.ContainsKey("PodKodZbozi")) {
                    kod = d["KodZbozi"].GetNum();
                    nazev = d["NazevKodu"].GetText();
                } else {
                    kod = d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum();
                    nazev = d["NazevPOdKodu"].GetText();
                    parent = d["KodZbozi"].GetNum();
                }

                var kategorie = new S5DataKategorieArtiklu() {
                    Kod = kod,
                    Nazev = nazev,
                    ParentObject_ID = parent
                };

                data.Add(kategorie);
            }

            return data;
        }
    }
}