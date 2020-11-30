using System;
using System.Collections.Generic;
using System.Text;

using SK_Nabidky;
using SkladData;

namespace S4DataObjs {
    class S4K_Nabidky : S4_Generic<S5DataNabidkaVydana, S5Data> {

        public static string GetID(string id) {
            return "NA" + id;
        }

        public S4K_Nabidky(string cpohybpFile, string pohybpFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(cpohybpFile, encoding);
            var lines1 = System.IO.File.ReadAllLines(pohybpFile, encoding);
            convert(new SkladDataFilePohybP(lines), new SkladDataFilePohybP(lines1));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                NabidkaVydanaList = _data.FindAll(_filter).ToArray()
            };
        }

        private void convert(SkladDataFilePohybP headers, SkladDataFilePohybP rows) {
            string columnId = "CisloVydejky";
            string id = "";
            int cisloPolozky = 0;
            SkladDataObj header;
            S5DataNabidkaVydana doklad = null;
            float price;
            List<S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane> polozky = null;

            foreach (SkladDataObj obj in rows.Data) {
                var d = obj.Items;
                if (id != d[columnId].GetNum()) {
                    id = d[columnId].GetNum();
                    cisloPolozky = 0;

                    if (doklad != null && polozky != null) {
                        doklad.Polozky.PolozkaNabidkyVydane = polozky.ToArray();
                        _data.Add(doklad);
                    }

                    header = find(headers.Data.ToArray(), columnId, d[columnId].GetNum());

                    doklad = new S5DataNabidkaVydana();
                    doklad.Nazev = d[columnId].GetNum();
                    doklad.DatumVystaveni = header.Items["DatumVydeje"].GetDate();
                    doklad.PlatnostOd = header.Items["DatumVydeje"].GetDate();
                    doklad.PlatnostDo = header.Items["DatumVydeje"].GetDate().AddDays(30);
                    doklad.PrijemceFaktury = new S5DataNabidkaVydanaPrijemceFaktury() {
                        Kod = S4A_Adresar.GetDodID(header.Items["CisloOdberatele"].GetNum())
                    };
                    doklad.Adresa = new S5DataNabidkaVydanaAdresa() {
                        Firma = new S5DataNabidkaVydanaAdresaFirma() {
                            Kod = S4A_Adresar.GetDodID(header.Items["CisloOdberatele"].GetNum())
                        }
                    };
                    doklad.TypDokladu = new enum_TypDokladu() {
                        EnumValueName = enum_TypDokladuEnumValueName.Prijaty
                    };
                    price = header.Items["Celkem0"].GetFloat()
                            + header.Items["Celkem5"].GetFloat()
                            + header.Items["Celkem23"].GetFloat();
                    doklad.CelkovaCastka = price.ToString("0.00").Replace(".", ",");
                    doklad.Polozky = new S5DataNabidkaVydanaPolozky();
                    doklad.ZiskZaDoklad = header.Items["Zisk"].GetDecimal();
                    doklad.Poznamka = header.Items["Upozorneni"].GetText() + "\n" + header.Items["Prebirajici"].GetText();

                    polozky = new List<S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane>();
                }

                polozky.Add(
                    new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydane() {
                        ID = Guid.NewGuid().ToString(),
                        Mnozstvi = d["Prijato"].GetNum(),
                        Nazev = d["NazevZbozi"].GetText(),
                        Jednotka = d["MernaJednotka"].GetAlfaNum().ToLower(),
                        DPH = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydaneDPH() {
                            Sazba = d["SazbaD"].GetNum()
                        },
                        JednCena = d["NakupCena"].GetDecimal(),
                        // DruhPolozky = new S5DataNabidkaVydanaPolozkyPolozkaNabidkyVydaneDruhPolozky() {
                        //     Kod = "ZBO"
                        // },
                        TypPolozky = new enum_TypPolozkyDokladu() {
                            EnumValueName = enum_TypPolozkyDokladuEnumValueName.Prijata
                        },
                        // ParentObject_ID = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),
                        FormatPolozky = new enum_FormatPolozek() {
                            EnumValueName = enum_FormatPolozekEnumValueName.Normalni
                        },
                        // Katalog = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),     
                        TypObsahu = new enum_TypObsahuPolozky() {
                            EnumValueName = enum_TypObsahuPolozkyEnumValueName.SObsahem
                        },
                        // ParentObject_ID = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),
                        // ObsahPolozky_ID = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),
                        CisloPolozky = cisloPolozky.ToString(),
                        Katalog = S4A_Katalog.GetID(d["CisloKarty"].GetNum()),

                    }

                );
                cisloPolozky++;
            }

            doklad.Polozky.PolozkaNabidkyVydane = polozky.ToArray();
            _data.Add(doklad);
        }
    }
}
