using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S7_Faktury;
using SkladData;

namespace SDataObjs {
     class S7_Faktury : S0_Generic<S5Data> {
        public new static string GetID(string id) {
            return "sFA" + id;
        }
        public new static string GetNazev(string id) {
            return "Faktura ze SKLADU " + id;
        }
        private List<S5DataFakturaVydana> _doklady = new List<S5DataFakturaVydana>();

        public S7_Faktury(string dir, Encoding enc) {
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBV, enc), new SkladDataFile(dir, SFile.POHYBV, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                FakturaVydanaList = _doklady.ToArray(),
            };
        }

        private void convertPohybV(SkladDataFile headers, SkladDataFile rows) {
            string id = "";

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataFakturaVydana();
                doc.CisloDokladu = id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = GetNazev(data["CisloVydejky"].GetNum());
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + Env.XMLNewLine + data["Prebirajici"].GetText() + Env.XMLNewLine + Environment.NewLine + header.ToString();
                doc.Polozky = new S5DataFakturaVydanaPolozky();
                doc.ProcentniZisk = header.GetProcentniZisk();
                doc.GenerovatSkladovyDoklad = "True";
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetOdbID(header.Items["CisloOdberatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.TypDokladu = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item2 };
                doc.EvidovatNahradniPlneni = data["NahradniPlneni"].GetBoolean();
                doc.OdkazNaDoklad = data["CisloObjednavkyD"].GetText();
                doc.DatumSplatnosti = doc.DatumVystaveni.AddDays(data["Splatnost"].GetFloat());
                doc.DatumUhrady = data["DatUhrady"].GetDate();
                doc.DatumUhradySpecified = data["DatUhrady"].GetNum() != "0";
                string formaUhrady = data["FormaUhrady"].GetText().ToUpper();
                doc.ZpusobPlatby_ID = S0_IDs.GetZpusobPlatbyID(formaUhrady == "" ? "B" : formaUhrady);
                _doklady.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            var polozky = new List<S5DataFakturaVydanaPolozkyPolozkaFakturyVydane>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum());
                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataFakturaVydanaPolozkyPolozkaFakturyVydane>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydane();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.SazbaDPH_ID = S0_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.ObsahPolozky = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                    GENERATEZASOBA = "1"
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }


        private void addRows(List<S5DataFakturaVydanaPolozkyPolozkaFakturyVydane> items, string id) {
            var found = _doklady.Find(delegate (S5DataFakturaVydana doc) {
                return doc.CisloDokladu == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataFakturaVydanaPolozky() {
                PolozkaFakturyVydane = items.ToArray()
            };
        }
    }
}