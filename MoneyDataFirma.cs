using System.Collections.Generic;

using SkladData;
using Schemas;

namespace MoneyDataObjects {

    class MoneyDataFirma {
        public static List<S5DataFirma> GetData(SkladDataFileOdb odb) {
            var data = new List<S5DataFirma>();

            foreach (SkladDataObj obj in odb.Data) {
                var d = obj.Items;

                group grp = null;
                string kodOdb = d["CisloOdberatele"].GetNum();
                string kod = "AD" + kodOdb;
                if (d["NahradniPlneni"].GetBoolean() == "True") {
                    grp = new group() { Kod = "NP" };
                } else if (d["NazevOdberatele"].GetText().StartsWith("\\")) {
                    grp = new group() { Kod = "OST" };
                    kod = "OAD" + kodOdb;
                }

                var firma = new S5DataFirma() {
                    Kod = kod,
                    Nazev = d["NazevOdberatele"].GetText() + " " + d["NazevOdberatele2"].GetText(),
                    Pohledavky = new S5DataFirmaPohledavky() {
                        SplatnostPohledavek = d["Splatnost"].GetNum()
                    },
                    Sleva = new S5DataFirmaSleva() {
                        Sleva = d["RabatO"].GetDecimalNegative(),
                    },
                    ICO = d["Ico"].GetNum(),
                    DIC = obj.GetDic(),
                    Poznamka = obj.Get5Note(),
                    Group = grp,
                    ReportSDPH_UserData = d["SDani"].GetBoolean()
                };

                firma.Osoby = new S5DataFirmaOsoby() {
                    HlavniOsoba = !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobyHlavniOsoba() {
                        Prijmeni = d["Zastoupeny"].GetText()
                    } : null,
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Prijmeni = d["Zastoupeny"].GetText(),
                            } : null,
                            !d["Prebirajici"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Prijmeni = d["Prebirajici"].GetText(),
                                Funkce = "Přebírající"
                            } : null
                        }
                    }
                };

                var emails = obj.ParseEmails("Mail");
                var emailsFA = obj.ParseEmails("MailFA");

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        !d["Telefon"].IsEmpty() ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "Tel",
                            SpojeniCislo = d["Telefon"].GetText(),
                        } : null,
                        emails[0] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "E-mail",
                            SpojeniCislo = emails[0],
                        } : null,
                        emails[1] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "E-mail",
                            SpojeniCislo = emails[1],
                        } : null,
                        emailsFA[0] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "E-mailFA",
                            SpojeniCislo = emailsFA[0],
                        } : null,
                        emailsFA[1] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "E-mailFA",
                            SpojeniCislo = emailsFA[1],
                        } : null
                    }
                };

                var isPrijemce = !d["UlicePrijemce"].IsEmpty() ||
                    !d["MestoPrijemce"].IsEmpty() || !d["NazevPrijemce"].IsEmpty();

                firma.Adresy = new S5DataFirmaAdresy() {
                    ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                        Ulice = d["Ulice"].GetText(),
                        KodPsc = d["Psc"].GetNum(),
                        Misto = d["Mesto"].GetText(),
                        NazevStatu = "Česká republika"
                    },
                    Provozovna = isPrijemce ? new S5DataFirmaAdresyProvozovna() {
                        Nazev = d["NazevPrijemce"].GetText(),
                        Ulice = d["UlicePrijemce"].GetText(),
                        KodPsc = d["Psc"].GetNum(),
                        Misto = d["MestoPrijemce"].GetText(),
                        NazevStatu = "Česká republika"
                    } : null,
                    OdlisnaAdresaProvozovny = !isPrijemce ? "True" : "False"
                };

                data.Add(firma);
            }

            return data;
        }

        public static List<S5DataFirma> GetData(SkladDataFileDod dod) {
            var data = new List<S5DataFirma>();

            foreach (SkladDataObj obj in dod.Data) {
                var d = obj.Items;

                var firma = new S5DataFirma() {
                    Kod = dod.GetID(d["CisloDodavatele"].GetNum()),
                    Nazev = d["NazevDodavatele"].GetText() + " " + d["NazevDodavatele2"].GetText(),
                    ICO = d["Ico"].GetNum(),
                    DIC = obj.GetDic(),
                    Poznamka = obj.Get5Note()
                };

                firma.Osoby = new S5DataFirmaOsoby() {
                    HlavniOsoba = !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobyHlavniOsoba() {
                        Prijmeni = d["Zastoupeny"].GetText()
                    } : null,
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Prijmeni = d["Zastoupeny"].GetText()
                            } : null,
                            !d["ZastoupenyOZ"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Prijmeni = d["ZastoupenyOZ"].GetText(),
                                Funkce = "OZ"
                            } : null
                        }
                    }
                };

                var emails = obj.ParseEmails("Mail");
                var emailsOZ = obj.ParseEmails("MailOZ");

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        !d["Telefon"].IsEmpty() ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "Tel",
                            SpojeniCislo = d["Telefon"].GetText(),
                        } : null,
                        emails[0] != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "E-mail",
                            SpojeniCislo = emails[0],
                        } : null,
                        emails[1] != null  ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "E-mail",
                            SpojeniCislo = emails[1],
                        } : null,
                        emailsOZ[0] != null  ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "E-mailOZ",
                            SpojeniCislo = emailsOZ[0],
                        } : null,
                        emailsOZ[1] != null  ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = "E-mailOZ",
                            SpojeniCislo = emailsOZ[1],
                        } : null
                    }
                };

                firma.Adresy = new S5DataFirmaAdresy() {
                    ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                        Ulice = d["Ulice"].GetText(),
                        KodPsc = d["Psc"].GetNum(),
                        Misto = d["Mesto"].GetText(),
                        NazevStatu = "Česká republika"
                    }
                };
                data.Add(firma);
            }

            return data;
        }
    }
}