using System;
using System.Collections.Generic;
using System.Text;

using S7_Dopl;
using SkladData;
using MainProgram;

namespace SDataObjs {
    class S7_Dopl : S0_Generic<S5Data> {

        private List<S5DataFirma> _firmy = new List<S5DataFirma>();
        private List<S5DataArtikl> _artikly = new List<S5DataArtikl>();

        private SkladDataFile kodSumFile;
        private SkladDataFile odbFile;

        public S7_Dopl(string dir, Encoding enc) {
            kodSumFile = new SkladDataFile(dir, SFile.KODSUMFA, enc);
            odbFile = new SkladDataFile(dir, SFile.ODB, enc);
            convertOdb(odbFile);
            convertDod(new SkladDataFile(dir, SFile.DOD, enc));
            convertKarty(new SkladDataFile(dir, SFile.KARTY, enc));
        }

        public static string GetKodPrebirajici(string kodFirmy) { return kodFirmy + "PRE"; }
        public static string GetKodZastoupeny(string kodFirmy) { return kodFirmy + "ZAS"; }
        public static string GetKodZastoupenyOZ(string kodFirmy) { return kodFirmy + "ZOZ"; }
        public static string GetKodTelefon(string kodFirmy) { return kodFirmy + "TEL"; }
        public static string GetKodTelefonFA(string kodFirmy) { return kodFirmy + "TFA"; }
        public static string GetKodEmail1(string kodFirmy) { return kodFirmy + "EM1"; }
        public static string GetKodEmail2(string kodFirmy) { return kodFirmy + "EM2"; }
        public static string GetKodEmailFA1(string kodFirmy) { return kodFirmy + "EF1"; }
        public static string GetKodEmailFA2(string kodFirmy) { return kodFirmy + "EF2"; }

        public override S5Data GetS5Data() {
            return new S5Data() {
                FirmaList = _firmy.ToArray(),
                ArtiklList = _artikly.ToArray()
            };
        }

        private void convertOdb(SkladDataFile file) {
            var statID = S0_IDs.GetStatID("CZ");

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                string kod = S3_Adresar.GetOdbID(d["CisloOdberatele"].GetNum());
                if(kod == S3_Adresar.GetOdbID("00001")) continue;
                
                string kodSumFa = d["KodSumFa"].GetText();

                var firma = new S5DataFirma() { };
                firma.ID = S0_IDs.GetFirmaID(kod);

                if(firma.ID == null) continue;

                string zastoupenyID = S0_IDs.GetOsobaID(GetKodZastoupeny(kod));
                string tel1ID = S0_IDs.GetSpojeniID(GetKodTelefon(kod));
                string email1ID = S0_IDs.GetSpojeniID(GetKodEmail1(kod));

                firma.Osoby = new S5DataFirmaOsoby() {
                    HlavniOsoba = zastoupenyID == null ? null : new S5DataFirmaOsobyHlavniOsoba() {
                        ID = zastoupenyID
                    },
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            zastoupenyID == null ? null : new S5DataFirmaOsobySeznamOsobOsoba() {
                                ID = zastoupenyID,
                                TelefonSpojeni1_ID = tel1ID,
                                EmailSpojeni_ID = email1ID
                            }
                        }
                    }
                };

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        tel1ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = tel1ID,
                            Osoba_ID = zastoupenyID
                        },
                        email1ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = email1ID,
                            Osoba_ID = zastoupenyID
                        }
                    }
                };

                if (kodSumFa == "dr") {
                    firma.Cinnosti = new S5DataFirmaCinnosti() {
                        FirmaCinnost = new S5DataFirmaCinnostiFirmaCinnost[] {
                            new S5DataFirmaCinnostiFirmaCinnost() {
                                Cinnost_ID = S0_IDs.GetCinnostID("DRAK")
                            }
                        }
                    };
                } else if (d["SumFa"].GetBoolean() == "True") {
                    firma.Cinnosti = new S5DataFirmaCinnosti() {
                        FirmaCinnost = new S5DataFirmaCinnostiFirmaCinnost[] {
                            new S5DataFirmaCinnostiFirmaCinnost() {
                                Cinnost_ID = S0_IDs.GetCinnostID("S_FA")
                            }
                        }
                    };
                } else {
                    var kodOdb = S3_Adresar.GetOdbID(findKodOdbKodSum(kodSumFa));
                    var nadrazenaFirmaObj = findOdbByKod(kodOdb);
                    var nadrazenaFirmaID = S0_IDs.GetFirmaID(kodOdb);

                    if (firma.ID != nadrazenaFirmaID && nadrazenaFirmaID != null) {
                        firma.Adresy = new S5DataFirmaAdresy() {
                            OdlisnaAdresaProvozovny = "True",
                            ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                                Nazev = SkladDataObj.GetNazev(nadrazenaFirmaObj.Items["NazevOdberatele"], nadrazenaFirmaObj.Items["NazevOdberatele2"]),
                                Ulice = nadrazenaFirmaObj.Items["Ulice"].GetText(),
                                KodPsc = nadrazenaFirmaObj.Items["Psc"].GetNum(),
                                Misto = nadrazenaFirmaObj.Items["Mesto"].GetText(),
                                Stat = new S5DataFirmaAdresyObchodniAdresaStat() { ID = statID }
                            }
                        };
                        
                        firma.NadrazenaFirma = new S5DataFirmaNadrazenaFirma() {
                            PrevzitObchodniPodminky = "True",
                            PrevzitObchodniUdaje = "True",
                            PrevzitBankovniSpojeni = "True",
                            Firma = new S5DataFirmaNadrazenaFirmaFirma() {
                                ID = nadrazenaFirmaID
                            }
                        };
                    }
                }

                _firmy.Add(firma);
            }
        }

        private SkladDataObj findOdbByKod(string kod) {
            var found = odbFile.Data.Find(delegate (SkladDataObj obj) {
                return S3_Adresar.GetOdbID(obj.Items["CisloOdberatele"].GetText()) == kod;
            });
            return found;
        }

        private string findKodOdbKodSum(string kodSumFa) {
            var found = kodSumFile.Data.Find(delegate (SkladDataObj obj) {
                return obj.Items["KodSumFa"].GetText() == kodSumFa;
            });
            if (found == null) return null;
            return found.Items["CisloOdberatele"].GetText();
        }

        private void convertDod(SkladDataFile file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                string kod = S3_Adresar.GetDodID(d["CisloDodavatele"].GetNum());
                var firma = new S5DataFirma() { };
                firma.ID = S0_IDs.GetFirmaID(kod);

                if(firma.ID == null) continue;

                string zastoupenyID = S0_IDs.GetOsobaID(GetKodZastoupeny(kod));
                string zastoupenyOZID = S0_IDs.GetOsobaID(GetKodZastoupenyOZ(kod));
                string tel1ID = S0_IDs.GetSpojeniID(GetKodTelefon(kod));
                string tel2ID = S0_IDs.GetSpojeniID(GetKodTelefonFA(kod));
                string email1ID = S0_IDs.GetSpojeniID(GetKodEmail1(kod));
                string email2ID = S0_IDs.GetSpojeniID(GetKodEmailFA1(kod));

                firma.Osoby = new S5DataFirmaOsoby() {
                    HlavniOsoba = zastoupenyID == null ? null : new S5DataFirmaOsobyHlavniOsoba() {
                        ID = zastoupenyID
                    },
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            zastoupenyID == null ? null : new S5DataFirmaOsobySeznamOsobOsoba() {
                                ID = zastoupenyID,
                                TelefonSpojeni1_ID = tel1ID,
                                EmailSpojeni_ID = email1ID
                            },
                            zastoupenyOZID == null ? null : new S5DataFirmaOsobySeznamOsobOsoba() {
                                ID = zastoupenyOZID,
                                TelefonSpojeni1_ID = tel2ID,
                                EmailSpojeni_ID = email2ID
                            }
                        }
                    }
                };

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        tel1ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = tel1ID,
                            Osoba_ID = zastoupenyID
                        },
                        email1ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = email1ID,
                            Osoba_ID = zastoupenyID
                        },
                        tel2ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = tel2ID,
                            Osoba_ID = zastoupenyOZID
                        },
                        email2ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = email2ID,
                            Osoba_ID = zastoupenyOZID
                        }
                    }
                };

                _firmy.Add(firma);
            }
        }
        private void convertKarty(SkladDataFile file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                string kod = S3_Katalog.GetID(d["CisloKarty"].GetNum());
                var artikl = new S5DataArtikl() {};
                artikl.ID = S0_IDs.GetArtiklID(kod);
                
                if(artikl.ID == null) continue;

                var kodDodavatele = S3_Adresar.GetDodID(d["CisloDodavatele"].GetNum());
                artikl.HlavniDodavatel_ID = S0_IDs.GetArtiklDodavatelID(artikl.ID, S0_IDs.GetFirmaID(kodDodavatele));
                var mernaJednotka = d["MernaJednotka"].GetNoSpaces().RemoveDiacritics().ToLower();
                var jednotkaID = S0_IDs.GetJednotkaID(mernaJednotka);
                var hlavniJednotkaID = S0_IDs.GetArtiklJednotkaID(artikl.ID, jednotkaID);
                artikl.Jednotky = hlavniJednotkaID != null ? new S5DataArtiklJednotky() {
                    SeznamJednotek = new S5DataArtiklJednotkySeznamJednotek() {
                        ArtiklJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotka[] {
                            new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                ID = hlavniJednotkaID,
                                NedelitelneMnozstvi = d["MinFol"].GetBoolean() == "True" ? d["VFol"].GetNum() : null
                            }
                        }
                    }
                } : null;
                _artikly.Add(artikl);
            }
        }
    }
}