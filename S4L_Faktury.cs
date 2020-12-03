using System;
using System.Collections.Generic;
using System.Text;

using SL_Faktury;
using SkladData;

namespace S4DataObjs {
    class S4L_Faktury : S4_Generic<S5DataFakturaVydana, S5Data> {

        public static string GetID(string id) {
            return "FA" + id;
        }

        public S4L_Faktury(string cpohybpFile, string pohybpFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(cpohybpFile, encoding);
            var lines1 = System.IO.File.ReadAllLines(pohybpFile, encoding);
            convert(new SkladDataFilePohybV(lines), new SkladDataFilePohybV(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                FakturaVydanaList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFilePohybV headers, SkladDataFilePohybV rows) {
            string columnId = "CisloVydejky";
            string id = "";
            int cisloPolozky = 1, result;
            SkladDataObj header;
            S5DataFakturaVydana doklad = null;
            float price;
            List<S5DataFakturaVydanaPolozkyPolozkaFakturyVydane> polozky = null;

            foreach (SkladDataObj obj in rows.Data) {
                var d = obj.Items;
                if (id != d[columnId].GetNum()) {
                    id = d[columnId].GetNum();
                    cisloPolozky = 1;

                    if (doklad != null && polozky != null) {
                        doklad.Polozky.PolozkaFakturyVydane = polozky.ToArray();
                        _data.Add(doklad);
                    }

                    header = find(headers.Data.ToArray(), columnId, d[columnId].GetNum());

                    doklad = new S5DataFakturaVydana();
                    doklad.Nazev = "Importovaná FV z faktury č." + id;
                    doklad.ParovaciSymbol = GetID(d[columnId].GetNum());
                    doklad.Group = new group() { Kod = "IMPORT" };
                    doklad.DatumSkladovehoPohybu = header.Items["DatumVydeje"].GetDate();
                    doklad.DatumVystaveni = header.Items["DatumVydeje"].GetDate();
                    doklad.DatumSchvaleni = header.Items["DatumVydeje"].GetDate();
                    doklad.DatumZauctovani = header.Items["DatumVydeje"].GetDate();
                    doklad.PrijemceFaktury = new S5DataFakturaVydanaPrijemceFaktury() {
                        Kod = S4A_Adresar.GetOdbID(header.Items["CisloOdberatele"].GetNum())
                    };
                    doklad.Adresa = new S5DataFakturaVydanaAdresa() {
                        Firma = new S5DataFakturaVydanaAdresaFirma() {
                            Kod = S4A_Adresar.GetOdbID(header.Items["CisloOdberatele"].GetNum())
                        }
                    };

                    doklad.Poznamka = header.Items["Upozorneni"].GetText();
                    price = header.Items["Celkem0"].GetFloat()
                            + header.Items["Celkem5"].GetFloat()
                            + header.Items["Celkem23"].GetFloat();
                    doklad.CelkovaCastka = price.ToString("0.00").Replace(".", ",");
                    doklad.ZiskZaDoklad = header.Items["Zisk"].GetDecimal();
                    doklad.ZapornyPohyb = "False";
                    doklad.Polozky = new S5DataFakturaVydanaPolozky();
                    doklad.TypDokladu = new enum_TypDokladu() {
                        EnumValueName = enum_TypDokladuEnumValueName.Prijaty
                    };
                    doklad.GenerovatSkladovyDoklad = "True";
                    // doklad.PuvodniDoklad = S4K_ObjVyd.GetID(header.Items["CisloObjednavkyD"].GetText());
                    doklad.Poznamka = string.Join(Environment.NewLine, header.Items);
                    doklad.DatumSplatnosti = header.Items["DatumVydeje"].GetDate().AddDays(int.TryParse(header.Items["Splatnost"].GetNum(), out result) ? int.Parse(header.Items["Splatnost"].GetNum()) : 0);
                    polozky = new List<S5DataFakturaVydanaPolozkyPolozkaFakturyVydane>();
                }

                polozky.Add(
                    new S5DataFakturaVydanaPolozkyPolozkaFakturyVydane() {
                        Mnozstvi = d["Vydano"].GetNum(),
                        Nazev = d["NazevZbozi"].GetText(),
                        DPH = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneDPH() {
                            Sazba = d["SazbaD"].GetNum()
                        },
                        JednCena = d["ProdCena"].GetDecimal(),
                        TypPolozky = new enum_TypPolozkyDokladu() {
                            EnumValueName = enum_TypPolozkyDokladuEnumValueName.Prijata
                        },
                        TypCeny = new enum_TypCeny() {
                            EnumValueName = enum_TypCenyEnumValueName.BezDane
                        },
                        TypObsahu = new enum_TypObsahuPolozky() {
                            Value = enum_TypObsahuPolozky_value.Item1
                        },
                        ObsahPolozky = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneObsahPolozky() {
                            GENERATEZASOBA = "1",
                            Artikl = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneObsahPolozkyArtikl() {
                                Katalog = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),
                                Group = new group() {
                                    Kod = "ART"
                                }
                            },
                            Sklad = new S5DataFakturaVydanaPolozkyPolozkaFakturyVydaneObsahPolozkySklad() {
                                Kod = "HL"
                            }
                        },
                        CisloPolozky = cisloPolozky.ToString()
                    }

                );
                cisloPolozky++;
            }

            doklad.Polozky.PolozkaFakturyVydane = polozky.ToArray();
            _data.Add(doklad);
        }
    }
}
