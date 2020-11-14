using System.Collections.Generic;

using SkladData;
using Schemas;

namespace MoneyDataObjects {

    class MoneyDataZasoba {
        public static List<S5DataZasoba> GetData(SkladDataFile kar) {
            var data = new List<S5DataZasoba>();

            foreach (SkladDataObj obj in kar.Data) {
                var d = obj.Items;

                var zasoba = new S5DataZasoba() {
                    Sklad_ID = "HL",
                    SkladovaPozice_ID = d["Pozice"].GetAlfaNum(),
                    Artikl_ID = kar.GetID(d["CisloKarty"].GetNum()),
                    Kod = kar.GetID(d["CisloKarty"].GetNum()),
                    HistorickaCena = d["NakupCena"].GetDecimal()
                };

                data.Add(zasoba);
            }

            return data;
        }
    }
}