using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SB_SklDokl;
using SkladData;

namespace S4DataObjs {
    class S4B_SklDokl : S4_Generic<S5DataSkladovyDoklad, S5Data> {

        public static string GetID(string id) {
            return "PRIJEM" + id;
        }

        public static string GetID_V(string id) {
            return "FA" + id;
        }

        public static string GetID_OV(string id) {
            return "OSTV" + id;
        }

        public static string GetID_VP(string id) {
            return "PRODEJ" + id;
        }


        public S4B_SklDokl(string kartyinvFile, string cpohybp, string pohybp, string cpohybv, string pohybv, string cpohybov, string pohybov, string cpohybvp, string pohybvp, Encoding encoding) {
            convertKartyInv(new SkladDataFile(System.IO.File.ReadAllLines(kartyinvFile, encoding)));
            convertPohybP(new SkladDataFile(File.ReadAllLines(cpohybp, encoding)), new SkladDataFile(File.ReadAllLines(pohybp, encoding)));
            convertPohybV("V", new SkladDataFile(File.ReadAllLines(cpohybv, encoding)), new SkladDataFile(File.ReadAllLines(pohybv, encoding)));
            convertPohybV("OV", new SkladDataFile(File.ReadAllLines(cpohybov, encoding)), new SkladDataFile(File.ReadAllLines(pohybov, encoding)));
            convertPohybV("VP", new SkladDataFile(File.ReadAllLines(cpohybvp, encoding)), new SkladDataFile(File.ReadAllLines(pohybvp, encoding)));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                SkladovyDokladList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convertKartyInv(SkladDataFile karty) {
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);

            var doc = new S5DataSkladovyDoklad();
            doc.Nazev = "Úvodní stavy zásob";
            doc.ParovaciSymbol = "00000";
            doc.Group = new group() { Kod = "IMPORT" };
            doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = firstDay;
            doc.Vyrizeno = "True";
            doc.TypDokladu = new enum_TypDokladu() {
                EnumValueName = enum_TypDokladuEnumValueName.Prijaty
            };

            int cisloPolozky = 0;
            var polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
            string artiklID, skladID, katalog;
            foreach (var row in karty.Data) {
                var data = row.Items;

                katalog = S4A_Katalog.GetID(data["CisloKarty"].GetNum());
                artiklID = S4_IDs.GetArtiklID(katalog);
                skladID = S4_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["PocetInv"].GetNum();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                };
                polozky.Add(pol);
            }

            doc.Polozky = new S5DataSkladovyDokladPolozky() {
                PolozkaSkladovehoDokladu = polozky.ToArray()
            };

            _data.Add(doc);
        }

        private void convertPohybP(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataSkladovyDoklad();
                id = GetID(data["CisloPrijemky"].GetNum());
                doc.Nazev = "Příjemka č." + data["CisloPrijemky"].GetNum();
                doc.ParovaciSymbol = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.Poznamka = data["Upozorneni"].GetText();
                string firmaID = S4_IDs.GetFirmaID(S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.Vyrizeno = "True";
                doc.Polozky = new S5DataSkladovyDokladPolozky();
                doc.TypDokladu = new enum_TypDokladu() {
                    EnumValueName = enum_TypDokladuEnumValueName.Prijaty,
                };
                _data.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;

            List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                katalog = id = GetID(data["CisloPrijemky"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S4_IDs.GetArtiklID(katalog);
                skladID = S4_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Prijato"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednotkovaPorizovaciCena = data["NakupCena"].GetDecimal();
                if (pol.Mnozstvi.StartsWith("-")) pol.JednotkovaPorizovaciCena = "0";
                pol.SazbaDPH_ID = S4_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void convertPohybV(string type, SkladDataFile headers, SkladDataFile rows) {
            string id = "";

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataSkladovyDoklad();
                id = GetID_V(data["CisloVydejky"].GetNum());
                doc.Nazev = "Faktura vyd. č." + data["CisloVydejky"].GetNum();
                if (type == "OV") {
                    id = GetID_OV(data["CisloVydejky"].GetNum());
                    doc.Nazev = "Ostatní výdej č." + data["CisloVydejky"].GetNum();
                } else if (type == "VP") {
                    id = GetID_VP(data["CisloVydejky"].GetNum());
                    doc.Nazev = "Prodejka č." + data["CisloVydejky"].GetNum();
                }
                doc.ParovaciSymbol = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.Poznamka = data["Upozorneni"].GetText();
                doc.Polozky = new S5DataSkladovyDokladPolozky();
                doc.ZiskZaDoklad = data["Zisk"].GetDecimal();
                string firmaID = S4_IDs.GetFirmaID(S4A_Adresar.GetOdbID(header.Items["CisloOdberatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.TypDokladu = new enum_TypDokladu() {
                    EnumValueName = enum_TypDokladuEnumValueName.Vydany,
                };
                _data.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            var polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                katalog = id = GetID_V(data["CisloVydejky"].GetNum());
                if (type == "OV") {
                    id = GetID_OV(data["CisloVydejky"].GetNum());
                } else if (type == "VP") {
                    id = GetID_VP(data["CisloVydejky"].GetNum());
                }

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                artiklID = S4_IDs.GetArtiklID(katalog);
                skladID = S4_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.SazbaDPH_ID = S4_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }


        private void addRows(List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu> items, string id) {
            var found = _data.Find(delegate (S5DataSkladovyDoklad doc) {
                return doc.ParovaciSymbol == id;
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
