using System;
using System.Collections.Generic;
using System.Text;

using SK_ObjVyd;
using SkladData;

namespace S4DataObjs {
    class S4K_ObjVyd : S4_Generic<S5DataObjednavkaVydana, S5Data> {

        public static string GetID(string id) {
            return "OBJ" + id;
        }

        public S4K_ObjVyd(string cpohybpFile, string pohybpFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(cpohybpFile, encoding);
            var lines1 = System.IO.File.ReadAllLines(pohybpFile, encoding);
            convert(new SkladDataFile(lines), new SkladDataFile(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                ObjednavkaVydanaList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataObjednavkaVydana();
                id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = "Objednávka vyd. č." + data["CisloVydejky"].GetNum();
                doc.Jmeno = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.Poznamka = data["Upozorneni"].GetText() + Environment.NewLine + Environment.NewLine + header.ToString();
                doc.PrijemceFaktury = new S5DataObjednavkaVydanaPrijemceFaktury() { Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()) };
                doc.Adresa = new S5DataObjednavkaVydanaAdresa() { Firma = new S5DataObjednavkaVydanaAdresaFirma() { Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum()) } };
                doc.ZapornyPohyb = "False";
                doc.Polozky = new S5DataObjednavkaVydanaPolozky();
                _data.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "";
            List<S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane> polozky = null;
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                var pol = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.Nazev = data["NazevZbozi"].GetText();
                pol.JednCena = data["NakupCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.ObsahPolozky = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydaneObsahPolozky() {
                    Artikl = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydaneObsahPolozkyArtikl() {
                        Katalog = S4A_Katalog.GetID(data["CisloKarty"].GetNum()),
                        Group = new group() {
                            Kod = "ART"
                        }
                    },
                    Sklad = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydaneObsahPolozkySklad() {
                        Kod = "HL"
                    }
                };
                polozky.Add(pol);
            }
            addRows(polozky, id);
        }

        private void addRows(List<S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane> items, string id) {
            var found = _data.Find(delegate (S5DataObjednavkaVydana doc) {
                return doc.Jmeno == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataObjednavkaVydanaPolozky() {
                PolozkaObjednavkyVydane = items.ToArray()
            };
        }
    }
}
