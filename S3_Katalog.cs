using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using S3_Katalog;
using SkladData;
using MainProgram;

namespace SDataObjs {
    class S3_Katalog : S0_Generic<S5Data> {
        private List<S5DataArtikl> _artikly = new List<S5DataArtikl>();

        private List<S5DataKategorieArtiklu> _kategorie = new List<S5DataKategorieArtiklu>();

        public static string GetID(string id) {
            return id;
        }

        public static string GetNazev(string id) {
            return "Skladová karta " + id;
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
                    Poradi_UserData = d["Poradi"].GetAlfaNum()
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
                    Poradi_UserData = d["PodPoradi"].GetAlfaNum(),
                    ParentObject_ID = _kategorie.Find((a) => { return a.Kod == d["KodZbozi"].GetNum(); }).ID
                };
                _kategorie.Add(cat);
            }
        }

        private void convertKarty(SkladDataFile file) {
            var eobchodID = S0_IDs.GetElektronickyObchodID("EO_PEMA");

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                var nazevZbozi = obj.GetNazevZbozi();                
                var artikl = new S5DataArtikl() {
                    Kod = GetID(d["CisloKarty"].GetNum()),
                    PosledniCena = d["NakupCena"].GetDecimal(),
                    Zkratka12 = d["Pozice"].GetAlfaNum().ToUpper(), 
                    Zkratka20 = d["NazevZbozi2"].GetText()
                };

                var druhZboziKod = "ZBO";
                var regexLom = new Regex(@"^\\[a-zA-Z0-9]+\\");
                var regexObal = new Regex(@"^\|[a-zA-Z0-9]");
                var regexZrus = new Regex(@"^\|\|[0-9]");
                if(regexLom.IsMatch(nazevZbozi)) { // specialni do vyberovek
                    druhZboziKod = "SPE";
                    artikl.InterniOznaceni_UserData = regexLom.Match(nazevZbozi).Value.Replace(@"\", "").ToUpper();
                    nazevZbozi = regexLom.Replace(nazevZbozi, "").FirstCharToUpper();
                } else if(regexObal.IsMatch(nazevZbozi)) { // obaly, palety
                    druhZboziKod = "OBA";
                    nazevZbozi = nazevZbozi.Replace("|", "").Trim().FirstCharToUpper();
                    artikl.NepodlehatSleveDokladu = "True";
                } else if(regexZrus.IsMatch(nazevZbozi)) { // zrusene zbozi
                    druhZboziKod = "ZRU";
                    nazevZbozi = nazevZbozi.Replace("|", "").Trim().FirstCharToUpper();
                    int aktualniRok = DateTime.Now.Year;
                    var rokOdstranStr = "20" + nazevZbozi.Substring(0, 2);
                    int rokOdstran = int.TryParse(rokOdstranStr, out int result) ? int.Parse(rokOdstranStr) : aktualniRok;
                    if(rokOdstran < aktualniRok) continue;
                }

                artikl.Nazev = nazevZbozi;
                artikl.DruhArtiklu_ID = S0_IDs.GetDruhZboziID(druhZboziKod);

                var sazbaDPH = new enum_DruhSazbyDPH() {};
                switch(d["SazbaD"].GetNum()) {
                    case "21": sazbaDPH.Value = enum_DruhSazbyDPH_value.Item1; break;
                    case "15": sazbaDPH.Value = enum_DruhSazbyDPH_value.Item0; break;
                    case "0": sazbaDPH.Value = enum_DruhSazbyDPH_value.Item2; break;
                    default: sazbaDPH.Value = enum_DruhSazbyDPH_value.Item2; break;
                 }

                artikl.SazbyDPH = new S5DataArtiklSazbyDPH() {
                    ArtiklDPH = new S5DataArtiklSazbyDPHArtiklDPH[] {
                        new S5DataArtiklSazbyDPHArtiklDPH() {
                            SazbaVstup = sazbaDPH,
                            SazbaVystup = sazbaDPH,
                            Zacatek = DateTime.Now.Date
                        }
                    }
                };

                var mernaJednotka = d["MernaJednotka"].GetNoSpaces().RemoveDiacritics().ToLower();
                var jednotkaID = S0_IDs.GetJednotkaID(mernaJednotka);
                artikl.SmazatOstatniJednotky = "True";
                if (jednotkaID != null) {
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
                                    d["VFol"].GetNum() != "0" && d["VFol"].GetNum() != d["VKart"].GetNum() ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
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
                var kodStr = d["KodZbozi"].GetNum();
                var podkodStr = d["PodKodZbozi"].GetNum();
                var webNezobrazovatExtra = d["Zobrazovat"].GetBooleanNegative();
                var webNezobrazovat = kodStr == "" || kodStr == "0000" || podkodStr == "" || podkodStr == "0000" ? "True" :  "False";

                artikl.ProduktoveKlice = new S5DataArtiklProduktoveKlice() {
                    DeleteItems = "1",
                    ArtiklProduktovyKlic = new S5DataArtiklProduktoveKliceArtiklProduktovyKlic[] {
                        priznakID != null ? new S5DataArtiklProduktoveKliceArtiklProduktovyKlic() {                        
                            ProduktovyKlic_ID = priznakID,
                            ProduktovyKlic = new S5DataArtiklProduktoveKliceArtiklProduktovyKlicProduktovyKlic() {
                                ID = priznakID
                            }
                        } : null,
                        webNezobrazovat == "True" ? new S5DataArtiklProduktoveKliceArtiklProduktovyKlic() {                        
                            ProduktovyKlic_ID = S0_IDs.GetProduktovyKlicID("#"),
                            ProduktovyKlic = new S5DataArtiklProduktoveKliceArtiklProduktovyKlicProduktovyKlic() {
                                ID = S0_IDs.GetProduktovyKlicID("#")
                            }
                        } : null,
                        webNezobrazovatExtra == "True" ? new S5DataArtiklProduktoveKliceArtiklProduktovyKlic() {                        
                            ProduktovyKlic_ID = S0_IDs.GetProduktovyKlicID("@"),
                            ProduktovyKlic = new S5DataArtiklProduktoveKliceArtiklProduktovyKlicProduktovyKlic() {
                                ID = S0_IDs.GetProduktovyKlicID("@")
                            }
                        } : null,
                    }
                };
                
                if (priznak == "P") {
                    // zbozi uvedene v priloze c. 5 - odpad a srot od roku 2012 (HADR 10kg)
                    artikl.PreneseniDane = new S5DataArtiklPreneseniDane() {
                        Kod = "5",
                        PlatnostDo = new DateTime(9998, 12, 31)
                    };
                }                

                artikl.Dodavatele = d["CisloDodavatele"].GetNum() != "00000" ? new S5DataArtiklDodavatele() {
                    SeznamDodavatelu = new S5DataArtiklDodavateleSeznamDodavatelu() {
                        DeleteItems = "1",
                        ArtiklDodavatel = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatel[] {
                                new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatel() {
                                    DodavatelskeOznaceni = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatelDodavatelskeOznaceni() {
                                        Kod = d["DodCislo"].GetAlfaNum()
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

                _artikly.Add(artikl);
            }
        }

    }
}