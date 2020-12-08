using System;
using System.Collections.Generic;
using System.Text;

using SA_Katalog;
using SkladData;

namespace S4DataObjs {
    class S4A_Katalog : S4_Generic<S5Data> {
        private List<S5DataArtikl> _artikly = new List<S5DataArtikl>();

        private List<S5DataKategorieArtiklu> _kategorie = new List<S5DataKategorieArtiklu>();

        public static string GetID(string id) {
            return "KARTY" + id;
        }

        public S4A_Katalog(string dir, Encoding enc) {
            convertKod(new SkladDataFile(dir, SFile.KOD, enc));
            convertPodKod(new SkladDataFile(dir, SFile.PODKOD, enc));
            convertKarty(new SkladDataFile(dir, SFile.KARTY, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                ArtiklList = _artikly.ToArray(),
                KategorieArtikluList = _kategorie.ToArray(),
            };
        }

        private void convertKod(SkladDataFile file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                var cat = new S5DataKategorieArtiklu() {
                    ID = Guid.NewGuid().ToString(),
                    Kod = d["KodZbozi"].GetNum(),
                    Nazev = d["NazevKodu"].GetText(),
                };
                _kategorie.Add(cat);
            }

            string guid = Guid.NewGuid().ToString();
            _kategorie.AddRange(new S5DataKategorieArtiklu[]{
                new S5DataKategorieArtiklu() {
                    ID = guid,
                    Kod = "0000",
                    Nazev = "Nezařazené"
                },
                new S5DataKategorieArtiklu() {
                    ID = Guid.NewGuid().ToString(),
                    Kod = "00000000",
                    Nazev = "Nezařazené",
                    ParentObject_ID = guid
                }
            });
        }

        private void convertPodKod(SkladDataFile file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                var cat = new S5DataKategorieArtiklu() {
                    ID = Guid.NewGuid().ToString(),
                    Kod = d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum(),
                    Nazev = d["NazevPOdKodu"].GetText(),
                    ParentObject_ID = _kategorie.Find((a) => { return a.Kod == d["KodZbozi"].GetNum(); }).ID
                };
                _kategorie.Add(cat);
            }
        }

        private void convertKarty(SkladDataFile file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                var sazbaDPH = new enum_DruhSazbyDPH() {
                    Value = d["SazbaD"].GetNum() == "21" ?
                        enum_DruhSazbyDPH_value.Item1 :
                        (d["SazbaD"].GetNum() == "15" ?
                            enum_DruhSazbyDPH_value.Item0 :
                            enum_DruhSazbyDPH_value.Item2)
                };

                var jednotkaID = S4_IDs.GetJednotkaID(d["MernaJednotka"].GetAlfaNum().ToLower());

                var artikl = new S5DataArtikl() {
                    Katalog = GetID(d["CisloKarty"].GetNum()),
                    Nazev = d["NazevZbozi"].GetText(),
                    Poznamka = d["NazevZbozi2"].GetText() + obj.ToString(),
                    Group = new group() { Kod = "ART" },
                    PosledniCena = d["NakupCena"].GetDecimal(),
                    Jednotky = new S5DataArtiklJednotky() {
                        HlavniJednotka = new S5DataArtiklJednotkyHlavniJednotka() {
                            Jednotka_ID = jednotkaID,
                        },
                        NakupniJednotka = new S5DataArtiklJednotkyNakupniJednotka() {
                            Jednotka_ID = jednotkaID,
                        },
                        ProdejniJednotka = new S5DataArtiklJednotkyProdejniJednotka() {
                            Jednotka_ID = jednotkaID,
                        },
                        SeznamJednotek = new S5DataArtiklJednotkySeznamJednotek() {
                            ArtiklJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotka[] {
                                new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                    Mnozstvi = "1",
                                    VychoziMnozstvi = "1",
                                    Jednotka_ID = jednotkaID,
                                    Kod = d["MernaJednotka"].GetAlfaNum().ToLower(),
                                    NedelitelneMnozstvi = d["MinFol"].GetBoolean() == "True" ? d["VFol"].GetNum() : null
                                },
                                d["VFol"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                    Mnozstvi = "1",
                                    VychoziMnozstvi = d["VFol"].GetNum(),
                                    Jednotka_ID = S4_IDs.GetJednotkaID("fol"),
                                    Kod = "fol",
                                    ParentJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotkaParentJednotka() {
                                        Jednotka_ID = jednotkaID
                                    }
                                } : null,
                                d["VKart"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                    Mnozstvi = "1",
                                    VychoziMnozstvi = d["VKart"].GetNum(),
                                    Jednotka_ID = S4_IDs.GetJednotkaID("kar"),
                                    Kod = "kar",
                                    ParentJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotkaParentJednotka() {
                                        Jednotka_ID = jednotkaID
                                    },
                                } : null,
                                d["VPal"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                    Mnozstvi = "1",
                                    VychoziMnozstvi = d["VPal"].GetNum(),
                                    Kod = "pal",
                                    Jednotka_ID = S4_IDs.GetJednotkaID("pal"),
                                    ParentJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotkaParentJednotka() {
                                        Jednotka_ID = jednotkaID
                                    }
                                } : null,
                            }
                        }
                    },
                    DruhArtiklu_ID = S4_IDs.GetDruhZboziID("ZBO"),
                    SazbyDPH = new S5DataArtiklSazbyDPH() {
                        ArtiklDPH = new S5DataArtiklSazbyDPHArtiklDPH[] {
                            new S5DataArtiklSazbyDPHArtiklDPH() {
                                SazbaVstup = sazbaDPH,
                                SazbaVystup = sazbaDPH
                            }
                        }
                    }
                };

                artikl.Dodavatele = d["CisloDodavatele"].GetNum() != "00000" ? new S5DataArtiklDodavatele() {
                    SeznamDodavatelu = new S5DataArtiklDodavateleSeznamDodavatelu() {
                        ArtiklDodavatel = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatel[] {
                                new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatel() {
                                    DodavatelskeOznaceni = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatelDodavatelskeOznaceni() {
                                        Kod = d["DodCislo"].GetAlfaNum(),
                                        Nazev = d["DodCislo"].GetText()
                                    },
                                    PosledniCena = d["NakupCena"].GetDecimal(),
                                    Firma = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatelFirma() {
                                        Kod = S4A_Adresar.GetDodID(d["CisloDodavatele"].GetNum())
                                    },
                                    Firma_ID = S4A_Adresar.GetDodID(d["CisloDodavatele"].GetNum())
                                }
                            }
                    }
                } : null;

                var kod = _kategorie.Find(k => { return k.Kod == d["KodZbozi"].GetNum(); });
                var podkod = _kategorie.Find(k => { return k.Kod == (d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum()); });

                if (kod != null && podkod != null) {
                    artikl.Kategorie = string.Format(
                        "{0}|{1}",
                        kod.ID,
                        podkod.ID
                    );
                }

                artikl.SmazatOstatniDodavatele = "True";
                artikl.SmazatOstatniJednotky = "True";
                artikl.SmazatOstatniSazbyDPH = "True";

                _artikly.Add(artikl);
            }
        }


    }
}