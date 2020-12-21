using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using SA_Adresar;
using SkladData;

namespace S4DataObjs {
    class S4A_Adresar : S4_Generic<S5Data> {

        private List<S5DataFirma> _firmy = new List<S5DataFirma>();

        public static string GetOdbID(string id) {
            return "ADR" + id;
        }

        public static string GetOdbOstID(string id) {
            return "ADO" + id;
        }

        public static string GetDodID(string id) {
            return "ADR2" + id.Substring(1);
        }

        public S4A_Adresar(string dir, Encoding enc) {
            convertOdb(new SkladDataFile(dir, SFile.ODB, enc));
            convertDod(new SkladDataFile(dir, SFile.DOD, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                FirmaList = _firmy.ToArray()
            };
        }

        private void convertOdb(SkladDataFile file) {
            var statID = S4_IDs.GetStatID("CZ");

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                group grp = null;
                string kodOdb = d["CisloOdberatele"].GetNum();
                string kod = GetOdbID(kodOdb);
                if (d["NahradniPlneni"].GetBoolean() == "True") {
                    grp = new group() { Kod = "NP" };
                } else if (d["NazevOdberatele"].GetText().StartsWith("\\")) {
                    grp = new group() { Kod = "OST" };
                    kod = GetOdbOstID(kodOdb);
                }

                var nazev = (d["NazevOdberatele"].GetText() + " " + d["NazevOdberatele2"].GetText()).Trim();
                var zpusobPlatbyKod = "H";
                if (d["KupniSmlouva"].GetBoolean() == "True") zpusobPlatbyKod = "F";
                if (d["SumFa"].GetBoolean() == "True") zpusobPlatbyKod = "SF";

                var firma = new S5DataFirma() {
                    Kod = kod,
                    Nazev = nazev,
                    Pohledavky = new S5DataFirmaPohledavky() {
                        SplatnostPohledavek = d["Splatnost"].GetNum(),
                        VlastniSplatnostPohledavek = "True"
                    },
                    Sleva = new S5DataFirmaSleva() {
                        Sleva = d["RabatO"].GetDecimal(),
                        VlastniSleva = "True"
                    },
                    ICO = d["Ico"].GetNum(),
                    DIC = obj.GetDic(),
                    Poznamka = obj.Get5Note() + obj.ToString(),
                    Group = grp,
                    ReportSDPH_UserData = d["SDani"].GetBoolean(),
                    ZpusobPlatby_ID = S4_IDs.GetZpusobPlatbyID(zpusobPlatbyKod),
                };


                var emails = obj.ParseEmails("Mail");
                var emailsFA = obj.ParseEmails("MailFA");
                
                firma.Osoby = new S5DataFirmaOsoby() {
                    HlavniOsoba = !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobyHlavniOsoba() {
                        Prijmeni = d["Zastoupeny"].GetText()
                    } : null,
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Nazev = d["Zastoupeny"].GetText(),
                                Prijmeni = d["Zastoupeny"].GetText(),
                                EmailSpojeni_ID = emails[0],
                                FunkceOsoby_ID = S4_IDs.GetFunkceOsobyID("JED")
                            } : null,
                            !d["Prebirajici"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Nazev = d["Prebirajici"].GetText(),
                                Prijmeni = d["Prebirajici"].GetText(),
                                FunkceOsoby_ID = S4_IDs.GetFunkceOsobyID("PŘE")
                            } : null
                        }
                    }
                };

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        !d["Telefon"].IsEmpty() ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("Tel"),
                            SpojeniCislo = d["Telefon"].GetText(),
                        } : null,
                        emails[0] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emails[0],
                        } : null,
                        emails[1] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emails[1],
                        } : null,
                        emailsFA[0] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("E-mailFA"),
                            SpojeniCislo = emailsFA[0],
                        } : null,
                        emailsFA[1] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("E-mailFA"),
                            SpojeniCislo = emailsFA[1],
                        } : null
                    }
                };

                var isPrijemce = !(d["UlicePrijemce"].IsEmpty() && d["MestoPrijemce"].IsEmpty() && d["NazevPrijemce"].IsEmpty());

                firma.Adresy = new S5DataFirmaAdresy() {
                    ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                        Nazev = nazev,
                        Ulice = d["Ulice"].GetText(),
                        KodPsc = d["Psc"].GetNum(),
                        Misto = d["Mesto"].GetText(),
                        Stat = new S5DataFirmaAdresyObchodniAdresaStat() { ID = statID }
                    },
                    Provozovna = isPrijemce ? new S5DataFirmaAdresyProvozovna() {
                        Nazev = (d["NazevPrijemce"].GetText() + " " + d["NazevPrijemce2"].GetText()).Trim(),
                        Ulice = d["UlicePrijemce"].GetText(),
                        KodPsc = d["PscPrijemce"].GetNum(),
                        Misto = d["MestoPrijemce"].GetText(),
                        Stat = new S5DataFirmaAdresyProvozovnaStat() { ID = statID }
                    } : null,
                    OdlisnaAdresaProvozovny = isPrijemce ? "True" : "False"
                };

                _firmy.Add(firma);
            }
        }

        private void convertDod(SkladDataFile file) {
            var statID = S4_IDs.GetStatID("CZ");

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                var nazev = (d["NazevDodavatele"].GetText() + " " + d["NazevDodavatele2"].GetText()).Trim();
                var firma = new S5DataFirma() {
                    Kod = GetDodID(d["CisloDodavatele"].GetNum()),
                    Nazev = nazev,
                    ICO = d["Ico"].GetNum(),
                    DIC = obj.GetDic(),
                    Poznamka = obj.Get5Note() + obj.ToString(),
                };

                var emails = obj.ParseEmails("Mail");
                var emailsOZ = obj.ParseEmails("MailOZ");

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        !d["Telefon"].IsEmpty() ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("Tel"),
                            SpojeniCislo = d["Telefon"].GetText(),
                        } : null,
                        emails[0] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emails[0],
                        } : null,
                        emails[1] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emails[1],
                        } : null,
                        emailsOZ[0] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emailsOZ[0],
                        } : null,
                        emailsOZ[1] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S4_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emailsOZ[1],
                        } : null
                    }
                };

                firma.Osoby = new S5DataFirmaOsoby() {
                    HlavniOsoba = !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobyHlavniOsoba() {
                        Prijmeni = d["Zastoupeny"].GetText(),

                    } : null,
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Nazev = d["Zastoupeny"].GetText(),
                                Prijmeni = d["Zastoupeny"].GetText(),
                                FunkceOsoby_ID = S4_IDs.GetFunkceOsobyID("JED")
                            } : null,
                            !d["ZastoupenyOZ"].IsEmpty() && d["Zastoupeny"].GetText() != d["ZastoupenyOZ"].GetText() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Nazev = d["ZastoupenyOZ"].GetText(),
                                Prijmeni = d["ZastoupenyOZ"].GetText(),
                                FunkceOsoby_ID = S4_IDs.GetFunkceOsobyID("OZ")
                            } : null
                        }
                    }
                };

                firma.Adresy = new S5DataFirmaAdresy() {
                    ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                        Nazev = nazev,
                        Ulice = d["Ulice"].GetText(),
                        KodPsc = d["Psc"].GetNum(),
                        Misto = d["Mesto"].GetText(),
                        Stat = new S5DataFirmaAdresyObchodniAdresaStat() { ID = statID }
                    }
                };
                _firmy.Add(firma);
            }

        }
    }
}