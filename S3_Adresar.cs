using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S3_Adresar;
using SkladData;

namespace SDataObjs {
    class S3_Adresar : S0_Generic<S5Data> {

        private List<S5DataFirma> _firmy = new List<S5DataFirma>();

        public static string GetOdbID(string id) {
            return "ADR" + id;
        }

        public static string GetDodID(string id) {
            return "ADR2" + id.Substring(1);
        }

        public S3_Adresar(string dir, Encoding enc) {
            convertOdb(new SkladDataFile(dir, SFile.ODB, enc));
            convertDod(new SkladDataFile(dir, SFile.DOD, enc));
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                FirmaList = _firmy.ToArray()
            };
        }

        private void convertOdb(SkladDataFile file) {
            var statID = S0_IDs.GetStatID("CZ");

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                group grp = null;
                string kodOdb = d["CisloOdberatele"].GetNum();
                string kod = GetOdbID(kodOdb);

                if(kod == GetOdbID("00001")) continue;

                if (d["NazevOdberatele"].GetText().StartsWith("\\")) {
                    grp = new group() { Kod = "OST" };
                } else if (d["NazevOdberatele"].GetText().StartsWith("||")) {
                    grp = new group() { Kod = "ZRUS" };
                    continue;
                }

                var zpusobPlatbyKod = "H";
                if (d["KupniSmlouva"].GetBoolean() == "True") zpusobPlatbyKod = "F";
                if (d["SumFa"].GetBoolean() == "True") zpusobPlatbyKod = "SF";

                var firma = new S5DataFirma() {
                    Group = grp,
                    Kod = kod,
                    Nazev = SkladDataObj.GetNazev(d["NazevOdberatele"], d["NazevOdberatele2"]),
                    ICO = obj.GetIco(),
                    DIC = obj.GetDic(),
                    Poznamka = obj.GetF3Note(),
                    PoznamkaInterni_UserData = obj.GetL2Note(),                    
                    ZpusobPlatby_ID = S0_IDs.GetZpusobPlatbyID(zpusobPlatbyKod),
                    DatumPorizeni_UserData = d["DatumPorizeni"].GetDate(),
                    DatumPorizeni_UserDataSpecified = true
                };

                firma.Pohledavky = new S5DataFirmaPohledavky() {
                    SplatnostPohledavek = d["Splatnost"].GetNum(),
                    VlastniSplatnostPohledavek = d["Splatnost"].GetFloat() == 0 ? "False" : "True"
                };
                firma.PlatceDPH = firma.DIC == null ? "False" : "True";
                firma.EvidovatNahradniPlneni = d["NahradniPlneni"].GetBoolean();

                // v programu Odber FA: A/N, Money: Pouzivat kredit: true + Kredit: 0 = nepovolí prodej na FA
                firma.Kredit = new S5DataFirmaKredit() {
                    Kredit = "0",
                    PouzivatKredit = d["KupniSmlouva"].GetBooleanNegative()
                };
                firma.Sleva = new S5DataFirmaSleva() {
                    Sleva = d["RabatO"].GetDecimal(),
                    VlastniSleva = "True"
                };
                firma.SlevaUvadena_UserData = d["PRabatO"].GetDecimal();
                // sleva < 0 nastavit prebirani ceny - nejvyssi, jinak z nastaveni skupiny tzn. vyberem
                firma.ObchodniPodminky = new S5DataFirmaObchodniPodminky() {
                    ZpusobVyberuCeny = new enum_ZpusobVyberuCeny() { 
                        // Value = d["RabatO"].GetFloat() < 0 ? enum_ZpusobVyberuCeny_value.Item1 : enum_ZpusobVyberuCeny_value.Item3
                        Value = false ? enum_ZpusobVyberuCeny_value.Item1 : enum_ZpusobVyberuCeny_value.Item3
                    }
                };
    

                firma.AdresniKlice = new S5DataFirmaAdresniKlice() {
                    DeleteItems = "1",
                    FirmaAdresniKlic = new S5DataFirmaAdresniKliceFirmaAdresniKlic[] {
                        d["SDani"].GetAlfaNum().ToUpper() == "A" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("SDPH")
                        } : null,
                        d["SDani"].GetAlfaNum().ToUpper() == "X" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("BEZCEN")
                        } : null,
                        d["DavatSek"].GetAlfaNum().ToUpper() == "N" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("ZAKSEK")
                        } : null,
                        d["Odesilat"].GetAlfaNum().ToUpper() == "A" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("ODESFA")
                        } : null,
                        d["KodOdb"].GetAlfaNum().ToUpper() == "OZ" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("OZ")
                        } : null,
                        d["KodOdb"].GetAlfaNum().ToUpper() == "OZN" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("OZN")
                        } : null,
                    }
                };
                firma.KodOdb_UserData = d["KodOdb"].GetAlfaNum();

                var tels = SkladDataObj.GetTelefony(d["Telefon"]);
                var emails = SkladDataObj.GetEmaily(d["Mail"]);
                var emailsFA = SkladDataObj.GetEmaily(d["MailFA"]);

                firma.Osoby = new S5DataFirmaOsoby() {
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        DeleteItems = "1",
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Nazev = d["Zastoupeny"].GetText(),
                                Prijmeni = d["Zastoupeny"].GetText(),
                                FunkceOsoby_ID = S0_IDs.GetFunkceOsobyID("ZAS"),
                                Kod = S7_Dopl.GetKodZastoupeny(kod)
                            } : null,
                            !d["Prebirajici"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Nazev = d["Prebirajici"].GetText(),
                                Prijmeni = d["Prebirajici"].GetText(),
                                FunkceOsoby_ID = S0_IDs.GetFunkceOsobyID("PRE"),
                                Kod = S7_Dopl.GetKodPrebirajici(kod)
                            } : null
                        }
                    }
                };

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    DeleteItems = "1",
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        tels.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("Tel"),
                            SpojeniCislo = tels.Item1,
                            Kod_UserData = S7_Dopl.GetKodTelefon(kod),
                        } : null,
                        tels.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("Tel"),
                            SpojeniCislo = tels.Item2,
                            Kod_UserData = S7_Dopl.GetKodTelefonFA(kod)
                        } : null,
                        emails.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emails.Item1,
                            Kod_UserData = S7_Dopl.GetKodEmail1(kod),
                        } : null,
                        emails.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emails.Item2,
                            Kod_UserData = S7_Dopl.GetKodEmail2(kod)
                        } : null,
                        emailsFA.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("E-mailFA"),
                            SpojeniCislo = emailsFA.Item1,
                            Kod_UserData = S7_Dopl.GetKodEmailFA1(kod),
                            Popis = "fakturace"
                        } : null,
                        emailsFA.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("E-mailFA"),
                            SpojeniCislo = emailsFA.Item2,
                            Kod_UserData = S7_Dopl.GetKodEmailFA2(kod),
                            Popis = "fakturace"
                        } : null
                    }
                };

                var isPrijemce = !(d["UlicePrijemce"].IsEmpty() && d["MestoPrijemce"].IsEmpty() && d["NazevPrijemce"].IsEmpty());

                firma.Adresy = new S5DataFirmaAdresy() {
                    ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                        Nazev = SkladDataObj.GetNazev(d["NazevOdberatele"], d["NazevOdberatele2"]),
                        Ulice = d["Ulice"].GetText(),
                        KodPsc = d["Psc"].GetNum(),
                        Misto = d["Mesto"].GetText(),
                        Stat = new S5DataFirmaAdresyObchodniAdresaStat() { ID = statID }
                    },
                    Provozovna = isPrijemce ? new S5DataFirmaAdresyProvozovna() {
                        Nazev = SkladDataObj.GetNazev(d["NazevPrijemce"], d["NazevPrijemce2"]),
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
            var statID = S0_IDs.GetStatID("CZ");

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                var kod = GetDodID(d["CisloDodavatele"].GetNum());

                var firma = new S5DataFirma() {
                    Kod = kod,
                    Nazev = SkladDataObj.GetNazev(d["NazevDodavatele"], d["NazevDodavatele2"]),
                    ICO = obj.GetIco(),
                    DIC = obj.GetDic(),
                    DatumPorizeni_UserData = d["DatumPorizeni"].GetDate(),
                    DatumPorizeni_UserDataSpecified = true
                };

                firma.Poznamka = (
                    d["ZavozDen"].GetText() == "" ? "" : 
                    "Závozový den: " + d["ZavozDen"].GetText() + XmlEnv.NewLine + XmlEnv.NewLine
                ) + obj.GetF3Note() + XmlEnv.NewLine + obj.GetL2Note();

                var tels = SkladDataObj.GetTelefony(d["Telefon"]);
                var emails = SkladDataObj.GetEmaily(d["Mail"]);
                var emailsOZ = SkladDataObj.GetEmaily(d["MailOZ"]);

                firma.Osoby = new S5DataFirmaOsoby() {
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                    DeleteItems = "1",
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            !d["Zastoupeny"].IsEmpty() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Nazev = d["Zastoupeny"].GetText(),
                                Prijmeni = d["Zastoupeny"].GetText(),
                                FunkceOsoby_ID = S0_IDs.GetFunkceOsobyID("ZAS"),
                                Kod = S7_Dopl.GetKodZastoupeny(kod)
                            } : null,
                            !d["ZastoupenyOZ"].IsEmpty() && d["Zastoupeny"].GetText() != d["ZastoupenyOZ"].GetText() ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Nazev = d["ZastoupenyOZ"].GetText(),
                                Prijmeni = d["ZastoupenyOZ"].GetText(),
                                FunkceOsoby_ID = S0_IDs.GetFunkceOsobyID("OZ"),
                                Kod = S7_Dopl.GetKodZastoupenyOZ(kod)
                            } : null
                        }
                    }
                };

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    DeleteItems = "1",
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        tels.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("Tel"),
                            SpojeniCislo = tels.Item1,
                            Kod_UserData = S7_Dopl.GetKodTelefon(kod)                        
                        } : null,
                        tels.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("Tel"),
                            SpojeniCislo = tels.Item2,
                            Kod_UserData = S7_Dopl.GetKodTelefonFA(kod)
                        } : null,
                        emails.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emails.Item1,
                            Kod_UserData = S7_Dopl.GetKodEmail1(kod)
                        } : null,
                        emails.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emails.Item2,
                            Kod_UserData = S7_Dopl.GetKodEmail2(kod)
                        } : null,
                        emailsOZ.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emailsOZ.Item1,
                            Kod_UserData = S7_Dopl.GetKodEmailFA1(kod)
                        } : null,
                        emailsOZ.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("E-mail"),
                            SpojeniCislo = emailsOZ.Item2,
                            Kod_UserData = S7_Dopl.GetKodEmailFA2(kod)
                        } : null
                    }
                };

                firma.Adresy = new S5DataFirmaAdresy() {
                    ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                        Nazev = SkladDataObj.GetNazev(d["NazevDodavatele"], d["NazevDodavatele2"]),
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