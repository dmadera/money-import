using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S8_ObjPrij;
using SkladData;

namespace SDataObjs {
    class S8_ObjPrij : S0_Generic<S5Data> {

        private List<S5DataObjednavkaPrijata> _objednavky = new List<S5DataObjednavkaPrijata>();

        public static string GetID(string id) {
            return "OBJ" + id;
        }

        public S8_ObjPrij(string dir, Encoding enc) {
            convert(
                new SkladDataFile(dir, SFile.CPOHYBZ, enc),
                new SkladDataFile(dir, SFile.POHYBZ, enc)
            );
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                ObjednavkaPrijataList = _objednavky.ToArray()
            };
        }

        private void convert(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataObjednavkaPrijata();
                id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = "Objednávka přij. č." + data["CisloVydejky"].GetNum();
                doc.Jmeno = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + Environment.NewLine + Environment.NewLine + header.ToString();
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetDodID(header.Items["CisloOdberatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.ZapornyPohyb = "False";
                doc.Polozky = new S5DataObjednavkaPrijataPolozky();
                _objednavky.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            List<S5DataObjednavkaPrijataPolozkyPolozkaObjednavkyPrijate> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                katalog = id = GetID(data["CisloVydejky"].GetNum());

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
                pol.JednCena = data["NakupCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
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
