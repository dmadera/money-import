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
            convert(new SkladDataFilePohybV(lines), new SkladDataFilePohybV(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                ObjednavkaVydanaList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFilePohybV headers, SkladDataFilePohybV rows) {
            string columnId = "CisloVydejky";
            string id = "";
            int cisloPolozky = 1;
            SkladDataObj header;
            S5DataObjednavkaVydana doklad = null;
            float price;
            List<S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane> polozky = null;

            foreach (SkladDataObj obj in rows.Data) {
                var d = obj.Items;
                if (id != d[columnId].GetNum()) {
                    id = d[columnId].GetNum();
                    cisloPolozky = 1;

                    if (doklad != null && polozky != null) {
                        doklad.Polozky.PolozkaObjednavkyVydane = polozky.ToArray();
                        _data.Add(doklad);
                    }

                    header = find(headers.Data.ToArray(), columnId, d[columnId].GetNum());

                    doklad = new S5DataObjednavkaVydana();
                    doklad.Nazev = "Importovaná OBV z objednávky č." + id;
                    doklad.Jmeno = GetID(d[columnId].GetNum());
                    doklad.Group = new group() { Kod = "IMPORT" };
                    doklad.DatumVystaveni = header.Items["DatumVydeje"].GetDate();
                    doklad.DatumSchvaleni = header.Items["DatumVydeje"].GetDate();
                    doklad.DatumZauctovani = header.Items["DatumVydeje"].GetDate();
                    doklad.PrijemceFaktury = new S5DataObjednavkaVydanaPrijemceFaktury() {
                        Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum())
                    };
                    doklad.Adresa = new S5DataObjednavkaVydanaAdresa() {
                        Firma = new S5DataObjednavkaVydanaAdresaFirma() {
                            Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum())
                        }
                    };
                    price = header.Items["Celkem0"].GetFloat()
                            + header.Items["Celkem5"].GetFloat()
                            + header.Items["Celkem23"].GetFloat();
                    doklad.CelkovaCastka = price.ToString("0.00").Replace(".", ",");
                    doklad.ZapornyPohyb = "False";
                    doklad.Polozky = new S5DataObjednavkaVydanaPolozky();
                    doklad.TypDokladu = new enum_TypDokladu() {
                        EnumValueName = enum_TypDokladuEnumValueName.Prijaty
                    };
                    doklad.Poznamka = string.Join(Environment.NewLine, d);
                    polozky = new List<S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane>();
                }

                polozky.Add(
                    new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydane() {
                        Mnozstvi = d["Vydano"].GetNum(),
                        Nazev = d["NazevZbozi"].GetText(),
                        JednCena = d["NakupCena"].GetDecimal(),
                        TypPolozky = new enum_TypPolozkyDokladu() {
                            EnumValueName = enum_TypPolozkyDokladuEnumValueName.Prijata
                        },
                        TypCeny = new enum_TypCeny() {
                            EnumValueName = enum_TypCenyEnumValueName.BezDane
                        },
                        TypObsahu = new enum_TypObsahuPolozky() {
                            Value = enum_TypObsahuPolozky_value.Item1
                        },
                        ObsahPolozky = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydaneObsahPolozky() {
                            GENERATEZASOBA = "1",
                            Artikl = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydaneObsahPolozkyArtikl() {
                                Katalog = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),
                                Group = new group() {
                                    Kod = "ART"
                                }
                            },
                            Sklad = new S5DataObjednavkaVydanaPolozkyPolozkaObjednavkyVydaneObsahPolozkySklad() {
                                Kod = "HL"
                            }
                        },
                        CisloPolozky = cisloPolozky.ToString()
                    }

                );
                cisloPolozky++;
            }

            doklad.Polozky.PolozkaObjednavkyVydane = polozky.ToArray();
            _data.Add(doklad);
        }
    }
}
