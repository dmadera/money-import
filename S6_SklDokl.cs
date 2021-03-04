using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S6_SklDokl;
using SkladData;

namespace SDataObjs {
    class S6_SklDokl : S0_Generic<S5Data> {

        private List<S5DataSkladovyDoklad> _doklady = new List<S5DataSkladovyDoklad>();
        private enum_TypDokladu prijemka = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item1 };
        private enum_TypDokladu vydejka = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item2 };

        public S6_SklDokl(string dir, Encoding enc) {
            convertKartyInv(new SkladDataFile(dir, SFile.KARTYINV, enc));
            convertPohybP(new SkladDataFile(dir, SFile.CPOHYBP, enc), new SkladDataFile(dir, SFile.POHYBP, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBV, enc), new SkladDataFile(dir, SFile.POHYBV, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBVP, enc), new SkladDataFile(dir, SFile.POHYBVP, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBZ, enc), new SkladDataFile(dir, SFile.POHYBZ, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBOV, enc), new SkladDataFile(dir, SFile.POHYBOV, enc));            
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
                case SFile.CPOHYBVP: return "VP" + id;
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
                
                doc.TypDokladu = vydejka;
                if(header.GetCelkemFloat() < 0) doc.TypDokladu = prijemka;
                
                _doklady.Add(doc);
            }

            Console.WriteLine("Zpracovávám " + rows.Soubor.ToString());
            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            var polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
            enum_TypDokladu typDokladu = vydejka;
            S5DataSkladovyDoklad skladovyDoklad = null;

            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum(), headers.Soubor);
                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());
                skladovyDoklad = find(id);
                typDokladu = skladovyDoklad != null ? skladovyDoklad.TypDokladu : vydejka;

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
                
                pol.Vratka = "False";
                pol.Mnozstvi = data["Vydano"].GetNum();
                if (typDokladu.Value == vydejka.Value && pol.Mnozstvi.StartsWith("-")) pol.Vratka = "True";
                if (typDokladu.Value == prijemka.Value && !pol.Mnozstvi.StartsWith("-")) pol.Vratka = "True";
                pol.Mnozstvi = pol.Mnozstvi.Replace("-", "");

                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["ProdCena"].GetDecimal().Replace("-","");
                pol.TypCeny = new enum_TypCeny() { Value = enum_TypCeny_value.Item0 };
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
            doc.Adresa = new S5DataSkladovyDokladAdresa() {
                Nazev = "Invetura zásob"
            };
            doc.Nazev = GetNazev("00000", karty.Soubor);
            doc.ParovaciSymbol = GetID("00000", karty.Soubor);
            doc.Poznamka = "Inventura zásob, množství, kde bylo na 0, nastaveno na 1. K narovnání došlo dalším výdejem.";
            doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = firstDay;
            doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
            doc.Schvaleno = "True";
            doc.TypDokladu = prijemka;
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
                if (pol.Mnozstvi.StartsWith("-")) {
                    pol.Mnozstvi = pol.Mnozstvi.Replace("-","");
                    pol.Vratka = "True";
                }

                if(pol.Mnozstvi == "0") pol.Mnozstvi = "1";
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID
                };
                pol.Vyrizeno = "True";
                polozky.Add(pol);
            }

            doc.Polozky = new S5DataSkladovyDokladPolozky() {
                PolozkaSkladovehoDokladu = polozky.ToArray()
            };

            _doklady.Add(doc);

            // narovnani inventury, kde bylo k mnozstvi 0 prictena 1
            // aby byla evidence o porizovacich cenach v money
            doc = new S5DataSkladovyDoklad();
            doc.Adresa = new S5DataSkladovyDokladAdresa() {
                Nazev = "Invetura zásob - narovnání"
            };
            doc.Nazev = GetNazev("0000N", karty.Soubor);
            doc.ParovaciSymbol = GetID("0000N", karty.Soubor);
            doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = firstDay;
            doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
            doc.Schvaleno = "True";
            doc.TypDokladu = vydejka;
            cisloPolozky = 0;
            polozky = new List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu>();
            
            foreach (var row in karty.Data) {
                var data = row.Items;

                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());
                artiklID = S0_IDs.GetArtiklID(katalog);
                skladID = S0_IDs.GetSkladID("HL");

                if (artiklID == null) continue;

                var pol = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["PocetInv"].GetNum().Replace("-","");
                
                if(pol.Mnozstvi == "0") pol.Mnozstvi = "1";
                else continue;

                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID
                };
                pol.Vyrizeno = "True";
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
                
                doc.TypDokladu = prijemka;
                if(header.GetCelkemFloat() < 0) doc.TypDokladu = vydejka;
                
                doc.Polozky = new S5DataSkladovyDokladPolozky();
                _doklady.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;
            Console.WriteLine("Zpracovávám " + rows.Soubor.ToString());
            List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu> polozky = null;
            enum_TypDokladu typDokladu = prijemka;
            S5DataSkladovyDoklad skladovyDoklad = null;

            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloPrijemky"].GetNum(), headers.Soubor);
                katalog = S3_Katalog.GetID(data["CisloKarty"].GetNum());
                skladovyDoklad = find(id);
                typDokladu = skladovyDoklad != null ? skladovyDoklad.TypDokladu : prijemka;

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

                pol.Vratka = "False";
                pol.Mnozstvi = data["Prijato"].GetNum();
                if (typDokladu.Value == prijemka.Value && pol.Mnozstvi.StartsWith("-")) pol.Vratka = "True";
                if (typDokladu.Value == vydejka.Value && !pol.Mnozstvi.StartsWith("-")) pol.Vratka = "True";
                pol.Mnozstvi = pol.Mnozstvi.Replace("-", "");

                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["NakupCena"].GetDecimal().Replace("-","");
                pol.JednotkovaPorizovaciCena = pol.JednCena;
                pol.TypCeny = new enum_TypCeny() { Value = enum_TypCeny_value.Item0 };
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
            var found = find(id);
            if (found == null) {
                Console.WriteLine(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
                return;
            }
            found.Polozky = new S5DataSkladovyDokladPolozky() {
                PolozkaSkladovehoDokladu = items.ToArray()
            };
        }

        private S5DataSkladovyDoklad find(string id) {
            return _doklady.Find(delegate (S5DataSkladovyDoklad doc) {
                return doc.ParovaciSymbol == id;
            });
        }

        
    }
}