using System;
using System.Collections.Generic;
using System.Text;

using S3_Katalog;
using SkladData;

namespace SDataObjs {
    class S3_Katalog : S0_Generic<S5Data> {
        private List<S5DataArtikl> _artikly = new List<S5DataArtikl>();

        private List<S5DataKategorieArtiklu> _kategorie = new List<S5DataKategorieArtiklu>();

        public static string GetID(string id) {
            return "ART0" + id;
        }

        public S3_Katalog(string dir, Encoding enc) {
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
            string kategorie, kategorieID;

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                kategorie = d["KodZbozi"].GetNum();
                kategorieID = S0_IDs.GetKategorieArtikluID(kategorie);
                if (kategorieID == null) kategorieID = Guid.NewGuid().ToString();

                var cat = new S5DataKategorieArtiklu() {
                    ID = kategorieID,
                    Kod = kategorie,
                    Nazev = d["NazevKodu"].GetText(),
                };
                _kategorie.Add(cat);
            }
        }

        private void convertPodKod(SkladDataFile file) {
            string podkategorie, podkategorieID;

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                podkategorie = d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum();
                podkategorieID = S0_IDs.GetKategorieArtikluID(podkategorie);
                if (podkategorieID == null) podkategorieID = Guid.NewGuid().ToString();

                var cat = new S5DataKategorieArtiklu() {
                    ID = podkategorieID,
                    Kod = podkategorie,
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

                var mernaJednotka = SkladDataItem.RemoveDiacritics(d["MernaJednotka"].GetNoSpaces().ToLower());
                var jednotkaID = S0_IDs.GetJednotkaID(mernaJednotka);

                var artikl = new S5DataArtikl() {
                    Kod = GetID(d["CisloKarty"].GetNum()),
                    Nazev = d["NazevZbozi"].GetText() + " " + d["NazevZbozi2"].GetText(),
                    // Poznamka = obj.ToString(),
                    Poznamka = "",
                    Group = new group() { Kod = "ART" },
                    PosledniCena = d["NakupCena"].GetDecimal(),
                    DruhArtiklu_ID = S0_IDs.GetDruhZboziID("ZBO"),
                    PLU = d["Pozice"].GetAlfaNum().ToUpper(), 
                    SazbyDPH = new S5DataArtiklSazbyDPH() {
                        ArtiklDPH = new S5DataArtiklSazbyDPHArtiklDPH[] {
                            new S5DataArtiklSazbyDPHArtiklDPH() {
                                SazbaVstup = sazbaDPH,
                                SazbaVystup = sazbaDPH
                            }
                        }
                    }
                };

                if (jednotkaID != "") {
                    artikl.Jednotky = new S5DataArtiklJednotky() {
                        HlavniJednotka = new S5DataArtiklJednotkyHlavniJednotka() {
                            Jednotka_ID = jednotkaID,
                        },
                        SeznamJednotek = new S5DataArtiklJednotkySeznamJednotek() {
                            ArtiklJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotka[] {
                                    new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                        Mnozstvi = "1",
                                        VychoziMnozstvi = "1",
                                        Jednotka_ID = jednotkaID,
                                        Kod = mernaJednotka,
                                        NedelitelneMnozstvi = d["MinFol"].GetBoolean() == "True" ? d["VFol"].GetNum() : null
                                    },
                                    d["VFol"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                        Mnozstvi = "1",
                                        VychoziMnozstvi = d["VFol"].GetNum(),
                                        Jednotka_ID = S0_IDs.GetJednotkaID("fol"),
                                        Kod = "fol",
                                        ParentJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotkaParentJednotka() {
                                            Jednotka_ID = jednotkaID
                                        }
                                    } : null,
                                    d["VKart"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                        Mnozstvi = "1",
                                        VychoziMnozstvi = d["VKart"].GetNum(),
                                        Jednotka_ID = S0_IDs.GetJednotkaID("kar"),
                                        Kod = "kar",
                                        ParentJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotkaParentJednotka() {
                                            Jednotka_ID = jednotkaID
                                        },
                                    } : null,
                                    d["VPal"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                        Mnozstvi = "1",
                                        VychoziMnozstvi = d["VPal"].GetNum(),
                                        Kod = "pal",
                                        Jednotka_ID = S0_IDs.GetJednotkaID("pal"),
                                        ParentJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotkaParentJednotka() {
                                            Jednotka_ID = jednotkaID
                                        }
                                    } : null,
                                }
                        }
                    };
                }

                var priznak = d["Priznak"].GetNoSpaces().ToUpper();
                var priznakID = S0_IDs.GetProduktovyKlicID(priznak);

                if(priznak != "P") {
                    artikl.ProduktoveKlice = priznakID != null ? new S5DataArtiklProduktoveKlice() {
                        ArtiklProduktovyKlic = new S5DataArtiklProduktoveKliceArtiklProduktovyKlic[] {
                            new S5DataArtiklProduktoveKliceArtiklProduktovyKlic() {                        
                                ProduktovyKlic_ID = priznakID,
                                ProduktovyKlic = new S5DataArtiklProduktoveKliceArtiklProduktovyKlicProduktovyKlic() {
                                    ID = priznakID
                                }
                            }
                        }
                    } : null;
                } else {
                    // zbozi uvedene v priloze c. 5 - odpad a srot od roku 2012 (HADR 10kg)
                    artikl.PreneseniDane = new S5DataArtiklPreneseniDane() {
                        Kod = "5"
                    };
                }

                artikl.Dodavatele = d["CisloDodavatele"].GetNum() != "00000" ? new S5DataArtiklDodavatele() {
                    HlavniDodavatel = new S5DataArtiklDodavateleHlavniDodavatel() {
                        
                    },
                    SeznamDodavatelu = new S5DataArtiklDodavateleSeznamDodavatelu() {
                        ArtiklDodavatel = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatel[] {
                                new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatel() {
                                    DodavatelskeOznaceni = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatelDodavatelskeOznaceni() {
                                        Kod = d["DodCislo"].GetAlfaNum(),
                                        Nazev = d["DodCislo"].GetText()
                                    },
                                    PosledniCena = d["NakupCena"].GetDecimal(),
                                    Firma = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatelFirma() {
                                        Kod = S3_Adresar.GetDodID(d["CisloDodavatele"].GetNum())
                                    },
                                    Firma_ID = S3_Adresar.GetDodID(d["CisloDodavatele"].GetNum())
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
                artikl.SmazatOstatniProduktoveKlice = "True";

                _artikly.Add(artikl);
            }
        }


    }
}