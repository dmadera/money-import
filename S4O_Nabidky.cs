using System;
using System.Collections.Generic;
using System.Text;

using SO_Nabidky;
using SkladData;

namespace S4DataObjs {
    class S4O_Nabidky : S4_Generic<S5DataNabidkaVydana, S5Data> {

        public static string GetID(string id) {
            return "NAB" + id;
        }

        public S4O_Nabidky(string cpohybpFile, string pohybpFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(cpohybpFile, encoding);
            var lines1 = System.IO.File.ReadAllLines(pohybpFile, encoding);
            convert(new SkladDataFile(lines), new SkladDataFile(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                NabidkaVydanaList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFile headers, SkladDataFile rows) {
            string id = "";

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataNabidkaVydana();
                id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = "Nabídka č." + data["CisloVydejky"].GetNum();
                doc.Jmeno = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.PlatnostOd = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                doc.PlatnostDo = data["DatumVydeje"].GetDate().AddDays(30);
                doc.Poznamka = data["Upozorneni"].GetText() + Environment.NewLine + Environment.NewLine + header.ToString();
                doc.ZapornyPohyb = "False";
                doc.Polozky = new S5DataNabidkaVydanaPolozky();
                doc.ZiskZaDoklad = data["Zisk"].GetDecimal();
                doc.PrijemceFaktury = new S5DataNabidkaVydanaPrijemceFaktury() { Kod = S4A_Adresar.GetOdbID(data["CisloOdberatele"].GetNum()) };
                doc.Adresa = new S5DataNabidkaVydanaAdresa() { Firma = new S5DataNabidkaVydanaAdresaFirma() { Kod = S4A_Adresar.GetOdbID(data["CisloOdberatele"].GetNum()) } };
                doc.Vyrizeno = "True";
                _data.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "";
            var polozky = new List<S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                var pol = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.JednCena = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.DPH = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydaneDPH() { Sazba = data["SazbaD"].GetNum() };
                pol.ObsahPolozky = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydaneObsahPolozky() {
                    Artikl = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydaneObsahPolozkyArtikl() {
                        Katalog = S4A_Katalog.GetID(data["CisloKarty"].GetNum()),
                        Group = new group() { Kod = "ART" }
                    },
                    Sklad = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydaneObsahPolozkySklad() { Kod = "HL" }
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void addRows(List<S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane> items, string id) {
            var found = _data.Find(delegate (S5DataNabidkaVydana doc) {
                return doc.Jmeno == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataNabidkaVydanaPolozky() {
                PolozkaNabidkyVydane = items.ToArray()
            };
        }
    }
}
