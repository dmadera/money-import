using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S7_DlPrij;
using SkladData;

namespace SDataObjs {
    
    class S7_DlPrij : S0_Generic<S5Data> {

        private List<S5DataDodaciListPrijaty> _doklady = new List<S5DataDodaciListPrijaty>();

        public S7_DlPrij(string dir, Encoding enc) {
            convertKartyInv(new SkladDataFile(dir, SFile.KARTYINV, enc));
            convertPohybP(new SkladDataFile(dir, SFile.CPOHYBP, enc), new SkladDataFile(dir, SFile.POHYBP, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                DodaciListPrijatyList = _doklady.ToArray(),
            };
        }

        public new static string GetID(string id) {
            return "S_DP" + id;
        }
        public new static string GetNazev(string id) {
            return "Příjemka ze SKLADU " + id;
        }

        private void convertKartyInv(SkladDataFile karty) {
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);

            var doc = new S5DataDodaciListPrijaty();
            doc.Nazev = "Úvodní stavy zásob";
            doc.CisloDokladu = "S_DP00000";
            doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = firstDay;
            doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
            doc.Schvaleno = doc.Vyrizeno = "True";

            int cisloPolozky = 0;
            var polozky = new List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho>();
            string artiklID, skladID, katalog;
            foreach (var row in karty.Data) {
                var data = row.Items;

                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());
                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["PocetInv"].GetNum();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID
                };
                polozky.Add(pol);
            }

            doc.Polozky = new S5DataDodaciListPrijatyPolozky() {
                PolozkaDodacihoListuPrijateho = polozky.ToArray()
            };

            _doklady.Add(doc);
        }

        private void convertPohybP(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataDodaciListPrijaty();
                doc.CisloDokladu = id = GetID(data["CisloPrijemky"].GetNum());
                doc.Nazev = GetNazev(id);
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + Env.XMLNewLine + header.ToString();
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.Odkaz = data["CisloFakturyD"].GetText();
                doc.Vyrizeno = "True";
                doc.Polozky = new S5DataDodaciListPrijatyPolozky();
                _doklady.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;

            List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloPrijemky"].GetNum());
                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Prijato"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednotkovaPorizovaciCena = data["NakupCena"].GetDecimal();
                pol.TypCeny = new enum_TypCeny() { Value = enum_TypCeny_value.Item0 };
                if (pol.Mnozstvi.StartsWith("-")) pol.JednotkovaPorizovaciCena = "0";
                pol.SazbaDPH_ID = S0_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.Vyrizeno = "True";
                pol.ObsahPolozky = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void addRows(List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho> items, string id) {
            var found = _doklady.Find(delegate (S5DataDodaciListPrijaty doc) {
                return doc.CisloDokladu == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataDodaciListPrijatyPolozky() {
                PolozkaDodacihoListuPrijateho = items.ToArray()
            };
        }
    }
}