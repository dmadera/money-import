using System;
using System.Collections.Generic;

using Schemas;
using SkladData;

namespace MoneyDataObjects {
    class MoneyData {

        private List<S5DataArtikl> _artikly = new List<S5DataArtikl>();
        private List<S5DataKategorieArtiklu> _kategorie = new List<S5DataKategorieArtiklu>();
        private List<S5DataFirma> _firmy = new List<S5DataFirma>();
        private List<S5DataZasoba> _zasoby = new List<S5DataZasoba>();

        private S5Data data = new S5Data();

        private Predicate<S5DataArtikl> _filterArtikl = delegate (S5DataArtikl a) {
            return !a.Nazev.StartsWith("||");
        };

        private Predicate<S5DataFirma> _filterFirma = delegate (S5DataFirma a) {
            return true;
        };

        private Predicate<S5DataKategorieArtiklu> _filterKategorie = delegate (S5DataKategorieArtiklu a) {
            return true;
        };

        private Predicate<S5DataZasoba> _filterZasoby = delegate (S5DataZasoba a) {
            return true;
        };

        public MoneyData() {

        }

        public S5Data GetS5Data() {
            data.ArtiklList = _artikly.FindAll(_filterArtikl).ToArray();
            data.FirmaList = _firmy.FindAll(_filterFirma).ToArray();
            data.KategorieArtikluList = _kategorie.FindAll(_filterKategorie).ToArray();
            data.ZasobaList = _zasoby.FindAll(_filterZasoby).ToArray();

            return data;
        }

        public void Add(SkladDataFile file) {
            if (file is SkladDataFileOdb) {
                _firmy.AddRange(MoneyDataFirma.GetData((SkladDataFileOdb)file));
                return;
            }

            if (file is SkladDataFileDod) {
                _firmy.AddRange(MoneyDataFirma.GetData((SkladDataFileDod)file));
                return;
            }

            if (file is SkladDataFileKod) {
                _kategorie.AddRange(MoneyDataKategorie.GetData(file));
                return;
            }

            if (file is SkladDataFilePodKod) {
                _kategorie.AddRange(MoneyDataKategorie.GetData(file, _kategorie));

                string guid = Guid.NewGuid().ToString();
                string guid1 = Guid.NewGuid().ToString();
                _kategorie.AddRange(new S5DataKategorieArtiklu[]{
                    new S5DataKategorieArtiklu() {
                        ID = guid,
                        Kod = "0000",
                        Nazev = "Nezařazené"
                    },
                    new S5DataKategorieArtiklu() {
                        ID = guid1,
                        Kod = "00000000",
                        Nazev = "Nezařazené",
                        ParentObject_ID = guid
                    }
                });
                return;
            }

            if (file is SkladDataFileKarty) {
                _artikly.AddRange(MoneyDataArtikl.GetData(file, _kategorie));
                _zasoby.AddRange(MoneyDataZasoba.GetData(file));
                return;
            }
        }
    }
}