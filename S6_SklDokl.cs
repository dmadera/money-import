using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S6_SklDokl;
using SkladData;

namespace SDataObjs {
    class S6_SklDokl : S0_Generic<S5Data> {

        private List<S5DataSkladovyDoklad> _doklady = new List<S5DataSkladovyDoklad>();

        public S6_SklDokl(string dir, Encoding enc) {
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBOV, enc), new SkladDataFile(dir, SFile.POHYBOV, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                SkladovyDokladList = _doklady.ToArray(),
            };
        }

        public new static string GetID(string id) {
            return "S_SV" + id;
        }
        public new static string GetNazev(string id) {
            return "Výdejka ze SKLADU " + id;
        }      

        private void convertPohybV(SkladDataFile headers, SkladDataFile rows) {
            string id = "";

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataSkladovyDoklad();
                id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = GetNazev(id); 
                doc.CisloDokladu = id;
                doc.Group = new group() { Kod = "OST" };
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + Env.XMLNewLine + header.ToString();
                doc.Polozky = new S5DataSkladovyDokladPolozky();
                doc.ProcentniZisk = header.GetProcentniZisk();
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetOdbID(header.Items["CisloOdberatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.TypDokladu = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item2 };
                _doklady.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            var polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum());
                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.SazbaDPH_ID = S0_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.Vyrizeno = "True";
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }


        private void addRows(List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu> items, string id) {
            var found = _doklady.Find(delegate (S5DataSkladovyDoklad doc) {
                return doc.CisloDokladu == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataSkladovyDokladPolozky() {
                PolozkaSkladovehoDokladu = items.ToArray()
            };
        }
    }
}