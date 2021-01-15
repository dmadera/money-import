using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S8_ObjPrij;
using SkladData;

namespace SDataObjs {
    class S8_ObjPrij : S0_Generic<S5Data> {

        private List<S5DataObjednavkaPrijata> _objednavky = new List<S5DataObjednavkaPrijata>();

        public new static string GetID(string id) {
            return "OP" + id;
        }

        public new static string GetNazev(string id) {
            return "Zakázka " + id;
        }

        public S8_ObjPrij(string dir, Encoding enc) {
            convertZ(new SkladDataFile(dir, SFile.CPOHYBZ, enc), new SkladDataFile(dir, SFile.POHYBZ, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                ObjednavkaPrijataList = _objednavky.ToArray()
            };
        }

        private void convertZ(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataObjednavkaPrijata();
                doc.Jmeno = doc.CisloDokladu = id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = GetNazev(data["CisloVydejky"].GetNum());
                doc.Group = new group() { Kod = "IMPORT" };
                doc.PlatnostOd = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.PlatnostDo = data["DatumVydeje"].GetDate().AddDays(20);
                doc.PlatnostDoSpecified = doc.PlatnostOdSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + Env.XMLNewLine + Env.XMLNewLine + header.ToString();
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = S0_IDs.GetFirmaID(S3_Adresar.GetDodID(header.Items["CisloOdberatele"].GetNum()));
                doc.ProcentniZisk = header.GetProcentniZisk();     
                if(data["CisloFaktury"].GetNum() == "") {
                    doc.DatumVyrizeni = data["DatumVydeje"].GetDate();
                    doc.DatumVyrizeniSpecified = true;
                    doc.Vyrizeno = "True";
                }
                doc.Polozky = new S5DataObjednavkaPrijataPolozky();
                _objednavky.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            List<S5DataObjednavkaPrijataPolozkyPolozkaObjednavkyPrijate> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum());
                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataObjednavkaPrijataPolozkyPolozkaObjednavkyPrijate>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataObjednavkaPrijataPolozkyPolozkaObjednavkyPrijate();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.SazbaDPH_ID = S0_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.Vyrizeno = "True";
                pol.ObsahPolozky = new S5DataObjednavkaPrijataPolozkyPolozkaObjednavkyPrijateObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                };
                polozky.Add(pol);
            }
            addRows(polozky, id);
        }

        private void addRows(List<S5DataObjednavkaPrijataPolozkyPolozkaObjednavkyPrijate> items, string id) {
            var found = _objednavky.Find(delegate (S5DataObjednavkaPrijata doc) {
                return doc.Jmeno == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataObjednavkaPrijataPolozky() {
                PolozkaObjednavkyPrijate = items.ToArray()
            };
        }
    }
}
