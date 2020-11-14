using System.Collections.Generic;
using SkladData;
using Schemas;

namespace MoneyDataObjects {

    class MoneyDataZasoba {
        private List<S5DataZasoba> _s5DataZasobas = new List<S5DataZasoba>();
        public void Add(SkladDataFileKarty kar) {
            foreach (SkladDataObj obj in kar.Data) {
                var d = obj.Items;

                var zasoba = new S5DataZasoba() {
                    Sklad_ID = "HL",
                    SkladovaPozice_ID = d["Pozice"].GetAlfaNum(),
                    Artikl_ID = kar.GetID(d["CisloKarty"].GetNum()),
                    Kod = kar.GetID(d["CisloKarty"].GetNum()),
                    HistorickaCena = d["NakupCena"].GetDecimal()
                };

                _s5DataZasobas.Add(zasoba);
            }
        }
    }
}