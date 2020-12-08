using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SC_Nabidky;
using SkladData;

namespace S4DataObjs {
    class S4O_Nabidky : S4_Generic<S5Data> {

        private List<S5DataNabidkaVydana> _nabidky = new List<S5DataNabidkaVydana>();

        public static string GetID(string id) {
            return "NAB" + id;
        }

        public S4O_Nabidky(string dir, Encoding enc) {
            convert(
                new SkladDataFile(dir, SFile.CPOHYBN, enc),
                new SkladDataFile(dir, SFile.POHYBN, enc)
            );
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                NabidkaVydanaList = _nabidky.ToArray()
            };
        }

        private void convert(SkladDataFile headers, SkladDataFile rows) {
            string id = "", firmaID;

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataNabidkaVydana();
                id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = "Nabídka č." + data["CisloVydejky"].GetNum();
                doc.Jmeno = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.PlatnostOd = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.PlatnostDo = data["DatumVydeje"].GetDate().AddDays(30);
                doc.Poznamka = data["Upozorneni"].GetText() + Environment.NewLine + Environment.NewLine + header.ToString();
                doc.ZapornyPohyb = "False";
                doc.Polozky = new S5DataNabidkaVydanaPolozky();
                doc.ZiskZaDoklad = data["Zisk"].GetDecimal();
                firmaID = S4_IDs.GetFirmaID(S4A_Adresar.GetOdbID(header.Items["CisloOdberatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.Vyrizeno = "True";
                _nabidky.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            var polozky = new List<S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                katalog = id = GetID(data["CisloVydejky"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S4_IDs.GetArtiklID(katalog);
                skladID = S4_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.DPH = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydaneDPH() { Sazba = data["SazbaD"].GetNum() };
                pol.ObsahPolozky = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydaneObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void addRows(List<S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane> items, string id) {
            var found = _nabidky.Find(delegate (S5DataNabidkaVydana doc) {
                return doc.Jmeno == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataNabidkaVydanaPolozky() {
                PolozkaNabidkyVydane = items.ToArray()
            };
        }
    }
}
