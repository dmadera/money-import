using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S6_SklDokl;
using SkladData;

namespace SDataObjs {
    class SkladovyDoklad {
        public static string GetID(SFile soubor, string cislo) {
            switch (soubor) {
                case SFile.CPOHYBV: return "FA" + cislo;
                case SFile.CPOHYBOV: return "OV" + cislo;
                case SFile.CPOHYBVP: return "VP" + cislo;
                case SFile.CPOHYBP: return "PR" + cislo;
                default: return cislo;
            };
        }

        public static string GetPopis(SFile soubor, string cislo) {
            switch (soubor) {
                case SFile.CPOHYBV: return "Faktura vyd. č. " + cislo;
                case SFile.CPOHYBOV: return "Ostatní výdej č. " + cislo;
                case SFile.CPOHYBVP: return "Prodejka č. " + cislo;
                case SFile.CPOHYBP: return "Příjemka č. " + cislo;
                default: return cislo;
            };
        }
    }

    class S6_SklDokl : S0_Generic<S5Data> {

        private List<S5DataSkladovyDoklad> _doklady = new List<S5DataSkladovyDoklad>();

        public S6_SklDokl(string dir, Encoding enc) {
            convertKartyInv(new SkladDataFile(dir, SFile.KARTYINV, enc));
            convertPohybP(new SkladDataFile(dir, SFile.CPOHYBP, enc), new SkladDataFile(dir, SFile.POHYBP, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBV, enc), new SkladDataFile(dir, SFile.POHYBV, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBOV, enc), new SkladDataFile(dir, SFile.POHYBOV, enc));
            convertPohybV(new SkladDataFile(dir, SFile.CPOHYBVP, enc), new SkladDataFile(dir, SFile.POHYBVP, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                SkladovyDokladList = _doklady.ToArray(),
            };
        }



        private void convertKartyInv(SkladDataFile karty) {
            var firstDay = new DateTime(DateTime.Now.Year, 1, 1);

            var doc = new S5DataSkladovyDoklad();
            doc.Nazev = "Úvodní stavy zásob";
            doc.ParovaciSymbol = "00000";
            doc.Group = new group() { Kod = "IMPORT" };
            doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = firstDay;
            doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
            doc.Vyrizeno = "True";
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
                    Sklad_ID = skladID,
                    GENERATEZASOBA = "1"
                };
                polozky.Add(pol);
            }

            doc.Polozky = new S5DataSkladovyDokladPolozky() {
                PolozkaSkladovehoDokladu = polozky.ToArray()
            };

            _doklady.Add(doc);
        }

        private void convertPohybP(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataSkladovyDoklad();
                id = SkladovyDoklad.GetID(headers.Soubor, data["CisloPrijemky"].GetNum());
                doc.Nazev = SkladovyDoklad.GetPopis(headers.Soubor, id);
                doc.ParovaciSymbol = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText();
                string firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()));
                doc.Firma_ID = doc.FakturacniAdresaFirma_ID = doc.PrijemceFaktury_ID = firmaID;
                doc.Vyrizeno = "True";
                doc.Polozky = new S5DataSkladovyDokladPolozky();
                doc.TypDokladu = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item1 };
                _doklady.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "", artiklID, skladID, katalog;

            List<S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladu> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = SkladovyDoklad.GetID(headers.Soubor, data["CisloPrijemky"].GetNum());
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
                if (pol.Mnozstvi.StartsWith("-")) pol.JednotkovaPorizovaciCena = "0";
                pol.SazbaDPH_ID = S0_IDs.GetSazbaDPHID(data["SazbaD"].GetNum());
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                    GENERATEZASOBA = "1"
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void convertPohybV(SkladDataFile headers, SkladDataFile rows) {
            string id = "";

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataSkladovyDoklad();
                id = SkladovyDoklad.GetID(headers.Soubor, data["CisloVydejky"].GetNum());
                doc.Nazev = SkladovyDoklad.GetPopis(headers.Soubor, id);
                doc.ParovaciSymbol = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.DatumSkladovehoPohybuSpecified = doc.DatumZauctovaniSpecified = doc.DatumSchvaleniSpecified = doc.DatumVystaveniSpecified = true;
                doc.Poznamka = data["Upozorneni"].GetText();
                doc.Polozky = new S5DataSkladovyDokladPolozky();
                doc.ZiskZaDoklad = data["Zisk"].GetDecimal();
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
                id = SkladovyDoklad.GetID(headers.Soubor, data["CisloVydejky"].GetNum());
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
                pol.ObsahPolozky = new S5DataSkladovyDokladPolozkyPolozkaSkladovehoDokladuObsahPolozky() {
                    Artikl_ID = artiklID,
                    Sklad_ID = skladID,
                    GENERATEZASOBA = "1"
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
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataSkladovyDokladPolozky() {
                PolozkaSkladovehoDokladu = items.ToArray()
            };
        }
    }
}