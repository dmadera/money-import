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
            convertKartyInv(new SkladDataFile(dir, SFile.KARTYINV, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBVP, enc), new SkladDataFile(dir, SFile.POHYBVP, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBV, enc), new SkladDataFile(dir, SFile.POHYBV, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBZ, enc), new SkladDataFile(dir, SFile.POHYBZ, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBOV, enc), new SkladDataFile(dir, SFile.POHYBOV, enc));
            convertPohybP(new SkladDataFile(dir, SFile.CPOHYBP, enc), new SkladDataFile(dir, SFile.POHYBP, enc));
        }

        public override S5Data GetS5Data() {
            // faktury pouze ty, ktere nemaji cislozakazky
            // s cislemzakazky uz jsou importovane v cislech zakazky
            var filtrovane = _doklady.FindAll(delegate (S5DataSkladovyDoklad doc) {
                return 
                    doc.Polozky.PolozkaSkladovehoDokladu != null &&
                    doc.Polozky.PolozkaSkladovehoDokladu.Length > 0 && // rozepsane vydeje
                    doc.Polozky.PolozkaSkladovehoDokladu[0].Zakazka_ID == null; 
            });

            return new S5Data() {
                SkladovyDokladList = filtrovane.ToArray(),
            };
        }

        public static string GetID(string id, SFile f) {
            switch(f) {
                case SFile.CPOHYBV: return "FA" + id;
                case SFile.CPOHYBVP: return "PR" + id;
                case SFile.CPOHYBOV: return "OV" + id;     
                case SFile.CPOHYBZ: return "ZA" + id;
                case SFile.CPOHYBP: return "PR" + id;
                case SFile.KARTYINV: return "PR" + id;
            }
            return id;
        } 

        public static string GetNazev(string id, SFile f) {
            switch(f) {
                case SFile.CPOHYBV: return "Faktura " + id;
                case SFile.CPOHYBVP: return "Prodejka " + id;
                case SFile.CPOHYBOV: return "Ostatní výdej " + id;
                case SFile.CPOHYBZ: return "Zakázka " + id;
                case SFile.CPOHYBP: return "Příjemka " + id;
                case SFile.KARTYINV: return "Úvodní stav zásob";
            }
            return id;
        }

        private void convertPohybV(SkladDataFile headers, SkladDataFile rows) {
            Console.WriteLine("Zpracovávám " + headers.Soubor.ToString());

            string id = "";

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataSkladovyDoklad();
                doc.Nazev = GetNazev(data["CisloVydejky"].GetNum(), headers.Soubor); 
                doc.ParovaciSymbol = GetID(data["CisloVydejky"].GetNum(), headers.Soubor);
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + XmlEnv.NewLine + XmlEnv.NewLine + header.ToString();
                doc.Polozky = new S5DataSkladovyDokladPolozky();
                doc.ProcentniZisk = header.GetProcentniZisk();
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetOdbID(header.Items["CisloOdberatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.TypDokladu = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item2 };
                _doklady.Add(doc);
            }

            Console.WriteLine("Zpracovávám " + rows.Soubor.ToString());
            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            var polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum(), headers.Soubor);
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
                // na zaklade Zakazka_ID se budou filtrovat faktury (vynechaji ty s cislem zakazky)
                pol.Zakazka_ID = data.ContainsKey("CisloZakazky") && data["CisloZakazky"].GetNum() != "00000" ? "True" : null;
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void convertKartyInv(SkladDataFile karty) {
            Console.WriteLine("Zpracovávám " + karty.Soubor.ToString());
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);

            var doc = new S5DataSkladovyDoklad();
            doc.Nazev = GetNazev("00000", karty.Soubor);
            doc.ParovaciSymbol = GetID("00000", karty.Soubor);
            doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = firstDay;
            doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
            doc.Schvaleno = doc.Vyrizeno = "True";
            doc.TypDokladu = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item1 };
            int cisloPolozky = 0;
            var polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
            string artiklID, skladID, katalog;
            foreach (var row in karty.Data) {
                var data = row.Items;

                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());
                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["PocetInv"].GetNum();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID
                };
                polozky.Add(pol);
            }

            doc.Polozky = new S5DataSkladovyDokladPolozky() {
                PolozkaSkladovehoDokladu = polozky.ToArray()
            };

            _doklady.Add(doc);
        }

        private void convertPohybP(SkladDataFile headers, SkladDataFile rows) {
            Console.WriteLine("Zpracovávám " + headers.Soubor.ToString());
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataSkladovyDoklad();
                doc.Nazev = GetNazev(data["CisloPrijemky"].GetNum(), headers.Soubor);
                doc.ParovaciSymbol = GetID(data["CisloPrijemky"].GetNum(), headers.Soubor);
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText() + XmlEnv.NewLine + XmlEnv.NewLine + header.ToString();
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.Vyrizeno = "True";
                doc.TypDokladu = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item1 };
                doc.Polozky = new S5DataSkladovyDokladPolozky();
                _doklady.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            Console.WriteLine("Zpracovávám " + rows.Soubor.ToString());
            List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloPrijemky"].GetNum(), headers.Soubor);
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
                pol.Mnozstvi = data["Prijato"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednotkovaPorizovaciCena = data["NakupCena"].GetDecimal();
                pol.TypCeny = new enum_TypCeny() { Value = enum_TypCeny_value.Item0 };
                if (pol.Mnozstvi.StartsWith("-")) pol.JednotkovaPorizovaciCena = "0";
                pol.SazbaDPH_ID = S0_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
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
                return doc.ParovaciSymbol == id;
            });
            if (found == null) {
                Console.WriteLine(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
                return;
            }
            found.Polozky = new S5DataSkladovyDokladPolozky() {
                PolozkaSkladovehoDokladu = items.ToArray()
            };
        }

        
    }
}