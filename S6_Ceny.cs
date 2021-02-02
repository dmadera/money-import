using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using S6_Ceny;
using SkladData;
using MainProgram;

namespace SDataObjs {

    class S6_Ceny : S0_Generic<S5Data> {

        private List<S5DataCenik> _ceniky = new List<S5DataCenik>();
        private List<S5DataPolozkaCeniku> _ceny = new List<S5DataPolozkaCeniku>();
        private List<S5DataZasoba> _zasoby = new List<S5DataZasoba>();
        private List<S5DataFirma> _firmy = new List<S5DataFirma>();

        private Dictionary<string, string> kodSkupKodCenik = new Dictionary<string, string>();

        public static string GetKod(string nazev) {
            var alfanum = Regex.Replace(nazev.RemoveDiacritics(), @"[^0-9a-zA-Z]+", "").ToUpper();
            if(alfanum.Length > 8) return alfanum.Substring(0, 8);
            return alfanum;
        }

        public S6_Ceny(string dir, Encoding enc) {
            convertZaklCeny(new SkladDataFile(dir, SFile.KARTY, enc));
            convertCeniky(new SkladDataFile(dir, SFile.SKUP, enc));
            convertCeny(new SkladDataFile(dir, SFile.CENY, enc));
            convertOdbCeniky(new SkladDataFile(dir, SFile.ODB, enc));
            convertZasoby(new SkladDataFile(dir, SFile.KARTY, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                CenikList = _ceniky.ToArray(),
                FirmaList = _firmy.ToArray(),
                PolozkaCenikuList = _ceny.ToArray(),
                ZasobaList = _zasoby.ToArray()
            };
        }

        private void convertZaklCeny(SkladDataFile karty) {
            var skladID = S0_IDs.GetSkladID("HL");

            foreach (var karta in karty.Data) {
                var d = karta.Items;
                var k = new S5DataPolozkaCeniku();
                k.Cenik_ID = S0_IDs.GetCeniktID("ZAKL");
                k.Kod = "0000" + d["CisloKarty"].GetNum();
                k.Artikl_ID = S0_IDs.GetArtiklID(S3_Katalog.GetID(d["CisloKarty"].GetNum()));
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
            var skladID = S0_IDs.GetSkladID("HL");

            foreach (var skupina in skupiny.Data) {
                var data = skupina.Items;
                var cenik = new S5DataCenik();
                string cenikNazev = data["NazevSkup"].GetText() != "" ? data["NazevSkup"].GetText() : "Neznámý";
                cenik.Kod = GetKod(cenikNazev);
                cenik.Nazev = cenikNazev;
                cenik.VychoziSklad_ID = skladID;
                _ceniky.Add(cenik);
                kodSkupKodCenik.Add(data["CisloSkup"].GetNum(), cenik.Kod);
            }
        }
        private void convertOdbCeniky(SkladDataFile firmy) {
            foreach (var f in firmy.Data) {
                var data = f.Items;

                if(data["CisloSkup"].GetNum() != "0000") {
                    var firma = new S5DataFirma();
                    var firmaID = S0_IDs.GetFirmaID(S3_Adresar.GetOdbID(data["CisloOdberatele"].GetNum()));
                    var zaklCenikID = S0_IDs.GetCeniktID("ZAKL");
                    firma.ID = firmaID;
                    firma.ObchodniPodminky = new S5DataFirmaObchodniPodminky() {
                        SeznamCeniku = new S5DataFirmaObchodniPodminkySeznamCeniku() {
                            DeleteItems = "1",
                            FirmaCenik = new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik[] {
                                new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik() {
                                    Poradi = "1",
                                    Firma_ID = firmaID,
                                    Cenik = new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenikCenik() {
                                        Kod = kodSkupKodCenik.GetValueOrDefault(data["CisloSkup"].GetNum())
                                    }
                                },
                                new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik() {
                                    Poradi = "2",
                                    Firma_ID = firmaID,
                                    Cenik_ID = zaklCenikID
                                },
                            }
                        }
                    };
                    _firmy.Add(firma);
                }
            }
        }

        private void convertCeny(SkladDataFile ceny) {
            var skladID = S0_IDs.GetSkladID("HL");

            foreach (var cena in ceny.Data) {
                var d = cena.Items;
                var c = new S5DataPolozkaCeniku();
                c.Cenik = new S5DataPolozkaCenikuCenik() { 
                    Kod = kodSkupKodCenik.GetValueOrDefault(d["CisloSkup"].GetNum())
                };
                c.Kod = d["CisloSkup"].GetNum() + d["CisloKarty"].GetNum();
                c.Artikl_ID = S0_IDs.GetArtiklID(S3_Katalog.GetID(d["CisloKarty"].GetNum()));
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

            var skladID = S0_IDs.GetSkladID("HL");
            string artiklID, katalog;

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                katalog = S3_Katalog.GetID(d["CisloKarty"].GetNum());
                artiklID = S0_IDs.GetArtiklID(katalog);

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