using System;
using System.Collections.Generic;
using System.Text;

using SK_Prijem;
using SkladData;

namespace S4DataObjs {
    class S4K_Prijem : S4_Generic<S5DataDodaciListPrijaty, S5Data> {

        public static string GetID(string id) {
            return "PRIJEM" + id;
        }

        public S4K_Prijem(string cpohybpFile, string pohybpFile, string kartyinvFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(cpohybpFile, encoding);
            var lines1 = System.IO.File.ReadAllLines(pohybpFile, encoding);
            var lines2 = System.IO.File.ReadAllLines(kartyinvFile, encoding);
            convert(new SkladDataFile(lines2));
            convert(new SkladDataFile(lines), new SkladDataFile(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                DodaciListPrijatyList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFile karty) {
            int year = DateTime.Now.Year;
            DateTime firstDay = new DateTime(year, 1, 1);

            var doc = new S5DataDodaciListPrijaty();
            doc.Nazev = "Úvodní stavy zásob";
            doc.Jmeno = "00000";
            doc.Group = new group() { Kod = "IMPORT" };
            doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = firstDay;
            doc.ZapornyPohyb = "False";
            doc.Vyrizeno = "True";

            int cisloPolozky = 0;
            var polozky = new List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho>();
            foreach (var row in karty.Data) {
                var data = row.Items;
                var pol = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["PocetInv"].GetNum();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozky() {
                    Artikl = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozkyArtikl() {
                        Katalog = S4A_Katalog.GetID(data["CisloKarty"].GetNum()),
                        Group = new group() { Kod = "ART" }
                    },
                    Sklad = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozkySklad() { Kod = "HL" }
                };
                polozky.Add(pol);
            }

            doc.Polozky = new S5DataDodaciListPrijatyPolozky() {
                PolozkaDodacihoListuPrijateho = polozky.ToArray()
            };

            _data.Add(doc);
        }

        private void convert(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataDodaciListPrijaty();
                id = GetID(data["CisloPrijemky"].GetNum());
                doc.Nazev = "Příjemka č." + data["CisloPrijemky"].GetNum();
                doc.Jmeno = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.Poznamka = data["Upozorneni"].GetText() + Environment.NewLine + Environment.NewLine + header.ToString();
                doc.PrijemceFaktury = new S5DataDodaciListPrijatyPrijemceFaktury() { Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()) };
                doc.Adresa = new S5DataDodaciListPrijatyAdresa() { Firma = new S5DataDodaciListPrijatyAdresaFirma() { Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()) } };
                doc.ZapornyPohyb = "False";
                doc.Vyrizeno = "True";
                doc.Polozky = new S5DataDodaciListPrijatyPolozky();
                _data.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "";
            List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloPrijemky"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                var pol = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Prijato"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednotkovaPorizovaciCena = data["NakupCena"].GetDecimal();
                if (pol.Mnozstvi.StartsWith("-")) pol.JednotkovaPorizovaciCena = "0";
                pol.DPH = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoDPH() { Sazba = data["SazbaD"].GetNum() };
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozky() {
                    Artikl = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozkyArtikl() {
                        Katalog = S4A_Katalog.GetID(data["CisloKarty"].GetNum()),
                        Group = new group() {
                            Kod = "ART"
                        }
                    },
                    Sklad = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozkySklad() {
                        Kod = "HL"
                    }
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void addRows(List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho> items, string id) {
            var found = _data.Find(delegate (S5DataDodaciListPrijaty doc) {
                return doc.Jmeno == id;
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
