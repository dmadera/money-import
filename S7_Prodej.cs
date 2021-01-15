using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S7_Prodej;
using SkladData;

namespace SDataObjs {
     class S7_Prodej : S0_Generic<S5Data> {
        public new static string GetID(string id) {
            return "S_PV" + id;
        }
        public new static string GetNazev(string id) {
            return "Prodejka ze SKLADU " + id;
        }
        private List<S5DataProdejkaVydana> _doklady = new List<S5DataProdejkaVydana>();

        public S7_Prodej(string dir, Encoding enc) {
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBVP, enc), new SkladDataFile(dir, SFile.POHYBVP, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                ProdejkaVydanaList = _doklady.ToArray(),
            };
        }

        private void convertPohybV(SkladDataFile headers, SkladDataFile rows) {
            string id = "";

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataProdejkaVydana();
                id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = GetNazev(data["CisloVydejky"].GetNum());
                doc.CisloDokladu = id;
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + Env.XMLNewLine + Environment.NewLine + header.ToString();
                doc.Zauctovano = doc.Vyrizeno = doc.ZauctovaniZpracovano = "True";                
                doc.Polozky = new S5DataProdejkaVydanaPolozky();
                doc.ProcentniZisk = header.GetProcentniZisk();             
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetOdbID(header.Items["CisloOdberatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.TypDokladu = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item2 };
                _doklady.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            var polozky = new List<S5DataProdejkaVydanaPolozkyPolozkaProdejkyVydane>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum());
                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataProdejkaVydanaPolozkyPolozkaProdejkyVydane>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataProdejkaVydanaPolozkyPolozkaProdejkyVydane();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCenaBezDPH = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.Vyrizeno = "True";
                pol.SazbaDPH_ID = S0_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.ObsahPolozky = new S5DataProdejkaVydanaPolozkyPolozkaProdejkyVydaneObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID
                };
                polozky.Add(pol);
            }
            addRows(polozky, id);
        }

        private void addRows(List<S5DataProdejkaVydanaPolozkyPolozkaProdejkyVydane> items, string id) {
            var found = _doklady.Find(delegate (S5DataProdejkaVydana doc) {
                return doc.CisloDokladu == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataProdejkaVydanaPolozky() {
                PolozkaProdejkyVydane = items.ToArray()
            };
        }
    }
}