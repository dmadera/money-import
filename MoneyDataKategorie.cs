using System.Collections.Generic;
using SkladData;
using Schemas;

namespace MoneyDataObjects {

    class MoneyDataKategorie {
        private List<S5DataKategorieArtiklu> _s5DataKats = new List<S5DataKategorieArtiklu>();
        public void Add(SkladDataFileKod kat, SkladDataFilePodKod sub) {
            foreach (SkladDataObj obj in kat.Data) {
                var d = obj.Items;

                var kategorie = new S5DataKategorieArtiklu() {
                    Kod = d["KodZbozi"].GetNum(),
                    Nazev = d["NazevKodu"].GetText(),
                };

                _s5DataKats.Add(kategorie);
            }

            foreach (SkladDataObj obj in sub.Data) {
                var d = obj.Items;

                var kategorie = new S5DataKategorieArtiklu() {
                    Kod = d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum(),
                    Nazev = d["NazevKodu"].GetText(),
                    ParentObject_ID = d["KodZbozi"].GetNum()
                };

                _s5DataKats.Add(kategorie);
            }
        }
    }
}