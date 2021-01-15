using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S8_ObjVyd;
using SkladData;

namespace SDataObjs {
    class S8_ObjVyd : S0_Generic<S5Data> {

        private List<S5DataObjednavkaVydana> _objednavky = new List<S5DataObjednavkaVydana>();

        public new static string GetID(string id) {
            return "OV" + id;
        }
        public new static string GetNazev(string id) {
            return "Objednávka vydaná " + id;
        }

        public S8_ObjVyd(string dir, Encoding enc) {
            convert(
                new SkladDataFile(dir, SFile.CPOHYBOB, enc),
                new SkladDataFile(dir, SFile.POHYBOB, enc)
            );
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                ObjednavkaVydanaList = _objednavky.ToArray()
            };
        }

        private void convert(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataObjednavkaVydana();
                doc.Jmeno = id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = GetNazev(data["CisloVydejky"].GetNum());
                doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + Environment.NewLine + Environment.NewLine + header.ToString();
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.ZapornyPohyb = "False";
                doc.Polozky = new S5DataObjednavkaVydanaPolozky();
                _objednavky.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            List<S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                katalog = id = GetID(data["CisloVydejky"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["NakupCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.SazbaDPH_ID = S0_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.Vyrizeno = "True";
                pol.ObsahPolozky = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydaneObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                };
                polozky.Add(pol);
            }
            addRows(polozky, id);
        }

        private void addRows(List<S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane> items, string id) {
            var found = _objednavky.Find(delegate (S5DataObjednavkaVydana doc) {
                return doc.Jmeno == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataObjednavkaVydanaPolozky() {
                PolozkaObjednavkyVydane = items.ToArray()
            };
        }
    }
}
