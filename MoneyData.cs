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

        public MoneyData() { }

        public S5Data GetS5Data() {
            data.ArtiklList = _artikly.ToArray();
            data.FirmaList = _firmy.ToArray();
            data.KategorieArtikluList = _kategorie.ToArray();
            data.ZasobaList = _zasoby.ToArray();

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

            if (file is SkladDataFileKarty) {
                _artikly.AddRange(MoneyDataArtikl.GetData(file));
                _zasoby.AddRange(MoneyDataZasoba.GetData(file));
                return;
            }

            if (file is SkladDataFileKod) {
                _kategorie.AddRange(MoneyDataKategorie.GetData(file));
                return;
            }

            if (file is SkladDataFilePodKod) {
                _kategorie.AddRange(MoneyDataKategorie.GetData(file));
                return;
            }
        }
    }
}