using System;
using System.Collections.Generic;
using System.Text;

using SK_Prijem;
using SkladData;

namespace S4DataObjs {
    class S4K_Prijem : S4_Generic<S5DataDodaciListPrijaty, S5Data> {

        public static string GetID(string id) {
            return "DP" + id;
        }

        public S4K_Prijem(string cpohybpFile, string pohybpFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(cpohybpFile, encoding);
            var lines1 = System.IO.File.ReadAllLines(pohybpFile, encoding);
            convert(new SkladDataFilePohybP(lines), new SkladDataFilePohybP(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                DodaciListPrijatyList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFilePohybP headers, SkladDataFilePohybP rows) {
            string columnId = "CisloPrijemky";
            string id = "";
            int cisloPolozky = 0;
            SkladDataObj header;
            S5DataDodaciListPrijaty doklad = null;
            float price;
            List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho> polozky = null;

            foreach (SkladDataObj obj in rows.Data) {
                var d = obj.Items;
                if (id != d[columnId].GetNum()) {
                    id = d[columnId].GetNum();
                    cisloPolozky = 0;

                    if (doklad != null && polozky != null) {
                        doklad.Polozky.PolozkaDodacihoListuPrijateho = polozky.ToArray();
                        _data.Add(doklad);
                    }

                    header = find(headers.Data.ToArray(), columnId, d[columnId].GetNum());

                    doklad = new S5DataDodaciListPrijaty();
                    doklad.ParovaciSymbol = d[columnId].GetNum();
                    doklad.Group = new group() { Kod = "IMPORT" };
                    doklad.DatumSkladovehoPohybu = header.Items["DatumVydeje"].GetDate();
                    doklad.DatumVystaveni = header.Items["DatumVydeje"].GetDate();
                    doklad.DatumSchvaleni = header.Items["DatumVydeje"].GetDate();
                    doklad.DatumZauctovani = header.Items["DatumVydeje"].GetDate();
                    doklad.PrijemceFaktury = new S5DataDodaciListPrijatyPrijemceFaktury() {
                        Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum())
                    };
                    doklad.Adresa = new S5DataDodaciListPrijatyAdresa() {
                        Firma = new S5DataDodaciListPrijatyAdresaFirma() {
                            Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum())
                        }
                    };
                    price = header.Items["Celkem0"].GetFloat()
                            + header.Items["Celkem5"].GetFloat()
                            + header.Items["Celkem23"].GetFloat();
                    doklad.CelkovaCastka = price.ToString("0.00").Replace(".", ",");
                    doklad.ZapornyPohyb = "False";
                    doklad.Polozky = new S5DataDodaciListPrijatyPolozky();
                    doklad.TypDokladu = new enum_TypDokladu() {
                        EnumValueName = enum_TypDokladuEnumValueName.Prijaty
                    };
                    polozky = new List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho>();
                }

                polozky.Add(
                    new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho() {
                        Mnozstvi = d["Prijato"].GetNum(),
                        Nazev = d["NazevZbozi"].GetText(),
                        Jednotka = d["MernaJednotka"].GetAlfaNum().ToLower(),
                        DPH = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoDPH() {
                            Sazba = d["SazbaD"].GetNum()
                        },
                        JednCena = d["NakupCena"].GetDecimal(),
                        TypPolozky = new enum_TypPolozkyDokladu() {
                            EnumValueName = enum_TypPolozkyDokladuEnumValueName.Prijata
                        },
                        TypCeny = new enum_TypCeny() {
                            EnumValueName = enum_TypCenyEnumValueName.BezDane
                        },
                        // Katalog = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),
                        TypObsahu = new enum_TypObsahuPolozky() {
                            Value = enum_TypObsahuPolozky_value.Item1
                        },
                        ObsahPolozky = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozky() {
                            GENERATEZASOBA = "1",
                            Artikl = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozkyArtikl() {
                                Kod = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),
                                Group = new group() {
                                    Kod = "ART"
                                }
                            },
                            Sklad = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoObsahPolozkySklad() {
                                Kod = "HL"
                            }
                        },
                        CisloPolozky = cisloPolozky.ToString()
                    }

                );
                cisloPolozky++;
            }

            doklad.Polozky.PolozkaDodacihoListuPrijateho = polozky.ToArray();
            _data.Add(doklad);
        }
    }
}
