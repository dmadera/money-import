using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SB_Ceny;
using SkladData;

namespace S4DataObjs {

    class S4B_Ceny : S4_Generic<S5Data> {

        private List<S5DataCenik> _ceniky = new List<S5DataCenik>();
        private List<S5DataPolozkaCeniku> _ceny = new List<S5DataPolozkaCeniku>();
        private List<S5DataZasoba> _zasoby = new List<S5DataZasoba>();

        public string GetID(string cislo) {
            return "SK" + cislo;
        }

        public S4B_Ceny(string dir, Encoding enc) {
            convertZaklCeny(new SkladDataFile(dir, SFile.KARTY, enc));
            convertCeniky(new SkladDataFile(dir, SFile.SKUP, enc));
            convertCeny(new SkladDataFile(dir, SFile.CENY, enc));
            convertZasoby(new SkladDataFile(dir, SFile.KARTY, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                CenikList = _ceniky.ToArray(),
                PolozkaCenikuList = _ceny.ToArray(),
                ZasobaList = _zasoby.ToArray()
            };
        }

        private void convertZaklCeny(SkladDataFile karty) {
            var skladID = S4_IDs.GetSkladID("HL");

            foreach (var karta in karty.Data) {
                var d = karta.Items;
                var k = new S5DataPolozkaCeniku();
                k.Cenik = new S5DataPolozkaCenikuCenik() { Kod = "ZAKL" };
                k.Kod = "ZAKL_" + S4A_Katalog.GetID(d["CisloKarty"].GetNum());
                k.Artikl_ID = S4_IDs.GetArtiklID(S4A_Katalog.GetID(d["CisloKarty"].GetNum()));
                k.Sklad_ID = skladID;
                k.ZmenaVProcentech = "True";
                k.CanGetDataFromGroup = "False";
                k.VychoziCena = new S5DataPolozkaCenikuVychoziCena() {
                    TypCeny = new enum_TypVychoziCeny() { Value = enum_TypVychoziCeny_value.Item1 },
                };
                k.VypocetCeny = new S5DataPolozkaCenikuVypocetCeny() {
                    VyseZmeny = d["Rabat"].GetDecimalNegative(),
                    ZpusobVypoctu = new enum_ZpusobVypoctuCeny() { Value = enum_ZpusobVypoctuCeny_value.Item1 }
                };
                _ceny.Add(k);
            }
        }

        private void convertCeniky(SkladDataFile skupiny) {
            var skladID = S4_IDs.GetSkladID("HL");

            foreach (var skupina in skupiny.Data) {
                var data = skupina.Items;
                var cenik = new S5DataCenik();
                cenik.Kod = GetID(data["CisloSkup"].GetNum());
                cenik.Nazev = data["NazevSkup"].GetText();
                if (cenik.Nazev == "") cenik.Nazev = "Neznámý";
                cenik.Poznamka = data["MinRabatC"].GetDecimal();
                cenik.VychoziSklad_ID = skladID;
                _ceniky.Add(cenik);
            }
        }

        private void convertCeny(SkladDataFile ceny) {
            var skladID = S4_IDs.GetSkladID("HL");

            foreach (var cena in ceny.Data) {
                var d = cena.Items;
                var c = new S5DataPolozkaCeniku();
                c.Cenik = new S5DataPolozkaCenikuCenik() { Kod = GetID(d["CisloSkup"].GetNum()) };
                c.Kod = GetID(d["CisloSkup"].GetNum()) + "_" + S4A_Katalog.GetID(d["CisloKarty"].GetNum());
                c.Artikl_ID = S4_IDs.GetArtiklID(S4A_Katalog.GetID(d["CisloKarty"].GetNum()));
                c.Sklad_ID = skladID;
                c.Cena = d["SpecCena"].GetDecimal();
                c.CanGetDataFromGroup = "False";
                c.VychoziCena = new S5DataPolozkaCenikuVychoziCena() {
                    TypCeny = new enum_TypVychoziCeny() { Value = enum_TypVychoziCeny_value.Item0 },
                };
                c.VypocetCeny = new S5DataPolozkaCenikuVypocetCeny() {
                    ZpusobVypoctu = new enum_ZpusobVypoctuCeny() { Value = enum_ZpusobVypoctuCeny_value.Item0 }
                };
                _ceny.Add(c);
            }
        }

        private void convertZasoby(SkladDataFile file) {

            var skladID = S4_IDs.GetSkladID("HL");
            string artiklID, katalog;

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                katalog = S4A_Katalog.GetID(d["CisloKarty"].GetNum());
                artiklID = S4_IDs.GetArtiklID(katalog);

                var zasoba = new S5DataZasoba() {
                    Kod = katalog,
                    HistorickaCena = d["NakupCena"].GetDecimal(),
                    Sklad_ID = skladID,
                    Artikl_ID = artiklID,
                    NastaveniZasoby = new S5DataZasobaNastaveniZasoby() {
                        VydejDoMinusu = new enum_VydejDoMinusu() {
                            Value = enum_VydejDoMinusu_value.Item0
                        }
                    }
                };

                _zasoby.Add(zasoba);
            }
        }
    }
}