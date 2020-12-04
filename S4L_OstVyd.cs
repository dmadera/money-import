using System;
using System.Collections.Generic;
using System.Text;

using SL_OVydej;
using SkladData;

namespace S4DataObjs {
    class S4L_OstDLV : S4_Generic<S5DataDodaciListVydany, S5Data> {

        public static string GetID(string id) {
            return "OSTV" + id;
        }

        public S4L_OstDLV(string cpohybpFile, string pohybpFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(cpohybpFile, encoding);
            var lines1 = System.IO.File.ReadAllLines(pohybpFile, encoding);
            convert(new SkladDataFile(lines), new SkladDataFile(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                DodaciListVydanyList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFile headers, SkladDataFile rows) {
            string id = "";

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataDodaciListVydany();
                id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = "Ostatní výdejka. č." + data["CisloVydejky"].GetNum();
                doc.Jmeno = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.Poznamka = data["Upozorneni"].GetText() + Environment.NewLine + Environment.NewLine + header.ToString();
                doc.ZapornyPohyb = "False";
                doc.Polozky = new S5DataDodaciListVydanyPolozky();
                doc.ZiskZaDoklad = data["Zisk"].GetDecimal();
                doc.PrijemceFaktury = new S5DataDodaciListVydanyPrijemceFaktury() { Kod = S4A_Adresar.GetOdbID(data["CisloOdberatele"].GetNum()) };
                doc.Adresa = new S5DataDodaciListVydanyAdresa() { Firma = new S5DataDodaciListVydanyAdresaFirma() { Kod = S4A_Adresar.GetOdbID(data["CisloOdberatele"].GetNum()) } };
                _data.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "";
            var polozky = new List<S5DataDodaciListVydanyPolozkyPolozkaDodacihoListuVydaneho>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataDodaciListVydanyPolozkyPolozkaDodacihoListuVydaneho>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                var pol = new S5DataDodaciListVydanyPolozkyPolozkaDodacihoListuVydaneho();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.DPH = new S5DataDodaciListVydanyPolozkyPolozkaDodacihoListuVydanehoDPH() { Sazba = data["SazbaD"].GetNum() };
                pol.ObsahPolozky = new S5DataDodaciListVydanyPolozkyPolozkaDodacihoListuVydanehoObsahPolozky() {
                    Artikl = new S5DataDodaciListVydanyPolozkyPolozkaDodacihoListuVydanehoObsahPolozkyArtikl() {
                        Katalog = S4A_Katalog.GetID(data["CisloKarty"].GetNum()),
                        Group = new group() { Kod = "ART" }
                    },
                    Sklad = new S5DataDodaciListVydanyPolozkyPolozkaDodacihoListuVydanehoObsahPolozkySklad() { Kod = "HL" }
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void addRows(List<S5DataDodaciListVydanyPolozkyPolozkaDodacihoListuVydaneho> items, string id) {
            var found = _data.Find(delegate (S5DataDodaciListVydany doc) {
                return doc.Jmeno == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataDodaciListVydanyPolozky() {
                PolozkaDodacihoListuVydaneho = items.ToArray()
            };
        }
    }
}
