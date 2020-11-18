using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using SK_Prijem;
using SkladData;

namespace S4DataObjs {
    class S4K_Prijem : S4K_Obj<S5DataDodaciListPrijaty, S5Data> {

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
            SkladDataObj header;
            S5DataDodaciListPrijaty dl = null;
            float price;
            List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho> polozky = null;

            foreach (SkladDataObj obj in rows.Data) {
                var d = obj.Items;
                if (id != d[columnId].GetNum()) {
                    id = d[columnId].GetNum();

                    if (dl != null && polozky != null) {
                        dl.Polozky.PolozkaDodacihoListuPrijateho = polozky.ToArray();
                        _data.Add(dl);
                    }

                    header = find(headers.Data.ToArray(), columnId, d[columnId].GetNum());

                    dl = new S5DataDodaciListPrijaty();
                    dl.StareCisloPrijemky_UserData = d[columnId].GetNum();
                    dl.CasSkladovehoPohybu = header.Items["DatumVydeje"].GetDate();
                    dl.DatumVystaveni = header.Items["DatumVydeje"].GetDate();
                    dl.PrijemceFaktury = new S5DataDodaciListPrijatyPrijemceFaktury() {
                        Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum())
                    };
                    dl.Adresa = new S5DataDodaciListPrijatyAdresa() {
                        Firma = new S5DataDodaciListPrijatyAdresaFirma() {
                            Kod = S4A_Adresar.GetDodID(header.Items["CisloDodavatele"].GetNum())
                        }
                    };
                    price = header.Items["Celkem0"].GetFloat()
                            + header.Items["Celkem5"].GetFloat()
                            + header.Items["Celkem23"].GetFloat();
                    dl.CelkovaCastka = price.ToString("0.00").Replace(".", ",");
                    dl.UcetMD_ID = "FP006";
                    dl.Polozky = new S5DataDodaciListPrijatyPolozky();
                    polozky = new List<S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho>();
                }

                polozky.Add(
                    new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijateho() {
                        ID = Guid.NewGuid().ToString(),
                        Mnozstvi = d["Prijato"].GetNum(),
                        Nazev = d["NazevZbozi"].GetText(),
                        Jednotka = d["MernaJednotka"].GetAlfaNum().ToLower(),
                        DPH = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoDPH() {
                            Sazba = d["SazbaD"].GetNum()
                        },
                        JednCena = d["NakupCena"].GetDecimal(),
                        DruhPolozky = new S5DataDodaciListPrijatyPolozkyPolozkaDodacihoListuPrijatehoDruhPolozky() {
                            Kod = "ZBO"
                        },
                        TypPolozky = new enum_TypPolozkyDokladu() {
                            EnumValueName = enum_TypPolozkyDokladuEnumValueName.Prijata
                        },
                        // ParentObject_ID = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),
                        FormatPolozky = new enum_FormatPolozek() {
                            EnumValueName = enum_FormatPolozekEnumValueName.Normalni
                        },
                        // Katalog = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),            

                    }

                );
            }

            dl.Polozky.PolozkaDodacihoListuPrijateho = polozky.ToArray();
            _data.Add(dl);
        }
    }
}
