using System;
using System.Collections.Generic;

using SkladData;
using Schemas;

namespace MoneyDataObjects {

    class MoneyDataKategorie {
        public static List<S5DataKategorieArtiklu> GetData(SkladDataFile file,
                List<S5DataKategorieArtiklu> kategorie = null) {

            var data = new List<S5DataKategorieArtiklu>();
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                string kod, nazev, parent = null;

                if (!d.ContainsKey("PodKodZbozi")) {
                    kod = d["KodZbozi"].GetNum();
                    nazev = d["NazevKodu"].GetText();
                } else {
                    kod = d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum();
                    nazev = d["NazevPOdKodu"].GetText();
                    parent = kategorie.Find((a) => { return a.Kod == d["KodZbozi"].GetNum(); }).ID;
                }

                var kat = new S5DataKategorieArtiklu() {
                    ID = Guid.NewGuid().ToString(),
                    Kod = kod,
                    Nazev = nazev,
                    ParentObject_ID = parent,

                };

                data.Add(kat);
            }

            return data;
        }
    }
}