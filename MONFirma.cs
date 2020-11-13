using System;
using System.Collections.Generic;
using System.Text;
using Schemas;


namespace sklad_data_parser {

    static class MONFirma {
        public static S5DataFirma[] ConvertToS5Data(List<SKLOdberatel> odberatele, List<SKLDodavatel> dodavatele) {
            List<S5DataFirma> s5DataFirmy = new List<S5DataFirma>();

            foreach (SKLOdberatel o in odberatele) {
                s5DataFirmy.Add(GetS5DataFirma(o));
            }

            foreach (SKLDodavatel d in dodavatele) {
                s5DataFirmy.Add(GetS5DataFirma(d));
            }

            return s5DataFirmy.ToArray();
        }

        private static S5DataFirma GetS5DataFirma(SKLOdberatel f) {
            var firma = new S5DataFirma() {
                Kod = f.kod,
                Nazev = f.nazev,
                Pohledavky = new S5DataFirmaPohledavky() {
                    SplatnostPohledavek = f.splatnost
                },
                Sleva = new S5DataFirmaSleva() {
                    Sleva = f.sleva
                },
                ICO = f.ico,
                DIC = f.dic,
                Poznamka = f.poznamka,
                Group = new group() {
                    ID = f.np == "0" ? S5DataIDs.FirmaGroup["all"] : S5DataIDs.FirmaGroup["np"]
                },
                ReportSDPH_UserData = f.sdph
            };

            firma.Osoby = new S5DataFirmaOsoby() {
                HlavniOsoba = f.objednavajici_prijmeni != "" ? new S5DataFirmaOsobyHlavniOsoba() {
                    Jmeno = f.objednavajici_prijmeni
                } : null,
                SeznamOsob = f.prebirajici_prijmeni != "" ? new S5DataFirmaOsobySeznamOsob() {
                    Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                        new S5DataFirmaOsobySeznamOsobOsoba() {
                            Jmeno = f.prebirajici_prijmeni,
                            Funkce = "Přebírající"
                        }
                    }
                } : null
            };

            firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        f.telefon != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["tel"]
                            },
                            SpojeniCislo = f.telefon,
                        } : null,
                        f.email != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["email"]
                            },
                            SpojeniCislo = f.email,
                        } : null,
                        f.email2 != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["email"]
                            },
                            SpojeniCislo = f.email2,
                        } : null,
                        f.emailFA != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["emailfa"]
                            },
                            SpojeniCislo = f.emailFA,
                        } : null,
                        f.emailFA2 != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["emailfa"]
                            },
                            SpojeniCislo = f.emailFA2,
                        } : null
                    }
            };

            firma.Adresy = new S5DataFirmaAdresy() {
                ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                    Ulice = f.ulice,
                    KodPsc = f.psc,
                    Misto = f.mesto,
                    NazevStatu = ""
                },
                Provozovna = (f.ulicep != "" || f.mestop != "") ? new S5DataFirmaAdresyProvozovna() {
                    Nazev = f.nazevp,
                    Ulice = f.ulicep,
                    KodPsc = f.psc,
                    Misto = f.mestop,
                    NazevStatu = "",
                } : null,
                OdlisnaAdresaProvozovny = (f.ulicep != "" || f.mestop != "" ? "True" : "False")
            };
            return firma;
        }

        private static S5DataFirma GetS5DataFirma(SKLDodavatel f) {
            var firma = new S5DataFirma() {
                Kod = f.kod,
                Nazev = f.nazev,
                ICO = f.ico,
                DIC = f.dic,
                Poznamka = f.poznamka
            };

            firma.Osoby = new S5DataFirmaOsoby() {
                HlavniOsoba = f.zastupujici != "" ? new S5DataFirmaOsobyHlavniOsoba() {
                    Jmeno = f.zastupujici
                } : null,
                SeznamOsob = f.zastupujici_oz != "" ? new S5DataFirmaOsobySeznamOsob() {
                    Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                        new S5DataFirmaOsobySeznamOsobOsoba() {
                            Prijmeni = f.zastupujici_oz,
                            Funkce = "OZ"
                        }
                    }
                } : null
            };

            firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        f.telefon != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["tel"]
                            },
                            SpojeniCislo = f.telefon,
                        } : null,
                        f.email != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["email"]
                            },
                            SpojeniCislo = f.email,
                        } : null,
                        f.email2 != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["email"]
                            },
                            SpojeniCislo = f.email2,
                        } : null,
                        f.email_oz != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["emailoz"]
                            },
                            SpojeniCislo = f.email_oz,
                        } : null,
                        f.email2_oz != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeni["emailoz"]
                            },
                            SpojeniCislo = f.email2_oz,
                        } : null
                    }
            };

            firma.Adresy = new S5DataFirmaAdresy() {
                ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                    Ulice = f.ulice,
                    KodPsc = f.psc,
                    Misto = f.mesto,
                    NazevStatu = ""
                }
            };

            return firma;
        }
    }
}