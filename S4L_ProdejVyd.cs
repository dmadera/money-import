using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

using SL_Faktury;
using SkladData;

namespace S4DataObjs {
    class S4L_ProdejVyd : S4_Generic<S5DataFakturaVydana, S5Data> {

        public static string GetID(string id) {
            return "PRODEJ" + id;
        }

        public S4L_ProdejVyd(string cpohybpFile, string pohybpFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(cpohybpFile, encoding);
            var lines1 = System.IO.File.ReadAllLines(pohybpFile, encoding);
            convert(new SkladDataFile(lines), new SkladDataFile(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                FakturaVydanaList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFile headers, SkladDataFile rows) {
            string id = "";
            // int result = 0;

            foreach (var header in headers.Data) {
                var data = header.Items;
                var doc = new S5DataFakturaVydana();
                id = GetID(data["CisloVydejky"].GetNum());
                doc.Nazev = "Prodejka č." + data["CisloVydejky"].GetNum();
                doc.Jmeno = id;
                doc.Group = new group() { Kod = "IMPORT" };
                doc.DatumSkladovehoPohybu = doc.DatumZauctovani = doc.DatumSchvaleni = doc.DatumVystaveni = data["DatumVydeje"].GetDate();
                // doc.DatumSplatnosti = data["DatumVydeje"].GetDate().AddDays(int.TryParse(data["Splatnost"].GetNum(), out result) ? int.Parse(data["Splatnost"].GetNum()) : 0);
                doc.Poznamka = data["Upozorneni"].GetText() + Environment.NewLine + Environment.NewLine + header.ToString();
                doc.ZapornyPohyb = "False";
                doc.Polozky = new S5DataFakturaVydanaPolozky();
                doc.ZiskZaDoklad = data["Zisk"].GetDecimal();
                doc.PrijemceFaktury = new S5DataFakturaVydanaPrijemceFaktury() { Kod = S4A_Adresar.GetOdbID(data["CisloOdberatele"].GetNum()) };
                doc.Adresa = new S5DataFakturaVydanaAdresa() { Firma = new S5DataFakturaVydanaAdresaFirma() { Kod = S4A_Adresar.GetOdbID(data["CisloOdberatele"].GetNum()) } };
                // doc.PuvodniDoklad = data["CisloZakazky"].GetNum() != "00000" ? data["CisloZakazky"].GetNum() : "";
                // if (data["DatUhrady"].GetNum() != "0") doc.DatumUhrady = data["DatUhrady"].GetDate();
                _data.Add(doc);
            }

            int cisloPolozky = 0;
            string prevId = "";
            var polozky = new List<S5DataFakturaVydanaPolozkyPolozkaFakturyVydane>();
            foreach (var row in rows.Data) {
                var data = row.Items;
                id = GetID(data["CisloVydejky"].GetNum());

                if (prevId != id) {
                    if (prevId != "") {
                        addRows(polozky, prevId);
                    }
                    polozky = new List<S5DataFakturaVydanaPolozkyPolozkaFakturyVydane>();
                    cisloPolozky = 0;
                    prevId = id;
                }

                var pol = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydane();
                pol.CisloPolozky = (++cisloPolozky).ToString();
                pol.Mnozstvi = data["Vydano"].GetNum();
                pol.JednCena = data["ProdCena"].GetDecimal();
                pol.TypObsahu = new enum_TypObsahuPolozky() { Value = enum_TypObsahuPolozky_value.Item1 };
                pol.DPH = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneDPH() { Sazba = data["SazbaD"].GetNum() };
                pol.ObsahPolozky = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneObsahPolozky() {
                    Artikl = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneObsahPolozkyArtikl() {
                        Katalog = S4A_Katalog.GetID(data["CisloKarty"].GetNum()),
                        Group = new group() { Kod = "ART" }
                    },
                    Sklad = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneObsahPolozkySklad() { Kod = "HL" }
                };
                polozky.Add(pol);
            }

            addRows(polozky, id);
        }

        private void addRows(List<S5DataFakturaVydanaPolozkyPolozkaFakturyVydane> items, string id) {
            var found = _data.Find(delegate (S5DataFakturaVydana doc) {
                return doc.Jmeno == id;
            });
            if (found == null) {
                throw new ArgumentException(string.Format("Nebyla nalezena hlavička k dokladu {0} v {1}.", id, this.GetType().Name));
            }
            found.Polozky = new S5DataFakturaVydanaPolozky() {
                PolozkaFakturyVydane = items.ToArray()
            };
        }

        public override void serialize(string output) {
            var serializer = new XmlSerializer(typeof(S5Data));
            using (var stream = new StreamWriter(output)) {
                serializer.Serialize(stream, GetS5Data());
            }

            string text = File.ReadAllText(output);
            text = text.Replace("FakturaVydana", "ProdejkaVydana");
            text = text.Replace("FakturyVydane", "ProdejkyVydane");
            File.WriteAllText(output, text);
        }
    }
}
