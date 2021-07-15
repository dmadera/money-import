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
            return "AD" + id;
        }

        public static string GetDodID(string id) {
            return "AD2" + id.Substring(1);
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

                string kodOdb = d["CisloOdberatele"].GetNum();
                string kod = GetOdbID(kodOdb);
                bool jeOstatni = d["NazevOdberatele"].GetText().StartsWith("\\");

                if(kod == GetOdbID("00001")) continue;
                if (d["NazevOdberatele"].GetText().StartsWith("||")) continue;

                var firma = new S5DataFirma() {
                    Kod = kod,
                    Nazev = SkladDataObj.GetNazev(d["NazevOdberatele"], d["NazevOdberatele2"]),
                    ICO = obj.GetIco(),
                    DIC = obj.GetDic(),
                    Zprava = obj.GetF3Note().Replace("\n", "\n\n"),
                    PoznamkaInterni_UserData = obj.GetL2Note(),                    
                    DatumPorizeni_UserData = d["DatumPorizeni"].GetDate(),
                    DatumPorizeni_UserDataSpecified = true,
                    KodOdb_UserData = d["KodOdb"].GetAlfaNum(),
                    KodSumFa_UserData = d["KodSumFa"].GetAlfaNum()
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
                    Sleva = (firma.Group != null && jeOstatni) ? "0" : d["RabatO"].GetDecimal(),
                    VlastniSleva = (firma.Group != null && jeOstatni) ? "False" : "True"
                };
                firma.SlevaUvadena_UserData = d["PRabatO"].GetDecimal();
                                
                var akceCenikID = S0_IDs.GetCenikID("_AKCE");
                var prodejCenikID = S0_IDs.GetCenikID("_PRODEJ");
                var nakupCenikID = S0_IDs.GetCenikID("_NAKUP");
                
                var prirazka = !(d["RabatO"].GetFloat() < 0 && d["Prirazka"].GetBoolean() == "False");

                if(jeOstatni) {
                    firma.Sleva = new S5DataFirmaSleva() {
                        Sleva = "0",
                        VlastniSleva = "False"
                    };
                    firma.ObchodniPodminky = new S5DataFirmaObchodniPodminky() {
                        ZpusobVyberuCeny = new enum_ZpusobVyberuCeny() {
                            Value = enum_ZpusobVyberuCeny_value.Item2 // zpusob prebirani ceny poradi
                        },
                        SeznamCeniku = new S5DataFirmaObchodniPodminkySeznamCeniku() {
                            DeleteItems = "1",
                            FirmaCenik = new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik[] {                                
                                new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik() {
                                    Poradi = "1",
                                    Cenik_ID = nakupCenikID
                                }
                            }
                        }
                    };
                } else {
                    firma.ObchodniPodminky = new S5DataFirmaObchodniPodminky() {
                        ZpusobVyberuCeny = new enum_ZpusobVyberuCeny() {
                            Value = enum_ZpusobVyberuCeny_value.Item3 // zpusob prebirani ceny vyberem
                        },
                        SeznamCeniku = new S5DataFirmaObchodniPodminkySeznamCeniku() {
                            DeleteItems = "1",
                            FirmaCenik = new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik[] {                                
                                new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik() {
                                    Poradi = "1",
                                    Cenik_ID = akceCenikID
                                },
                                new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik() {
                                    Poradi = "2",
                                    Cenik_ID = prodejCenikID
                                },
                            }
                        }
                    };
                }
                
                firma.AdresniKlice = new S5DataFirmaAdresniKlice() {
                    DeleteItems = "1",
                    FirmaAdresniKlic = new S5DataFirmaAdresniKliceFirmaAdresniKlic[] {
                        d["SDani"].GetAlfaNum().ToUpper() == "A" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("sDPH")
                        } : null,
                        d["SDani"].GetAlfaNum().ToUpper() == "X" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("-DLCENY")
                        } : null,
                        d["DavatSek"].GetAlfaNum().ToUpper() == "N" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("-SEK")
                        } : null,
                        d["Odesilat"].GetAlfaNum().ToUpper() == "N" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("-LETAK")
                        } : null,
                        d["KodOdb"].GetAlfaNum().ToUpper() == "OZ" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("OZ")
                        } : null,
                        d["KodOdb"].GetAlfaNum().ToUpper() == "OZN" ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("OZN")
                        } : null,
                        int.Parse(kodOdb) > 7000 ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("INT")
                        } : null,
                        jeOstatni ? new S5DataFirmaAdresniKliceFirmaAdresniKlic() {
                            AdresniKlic_ID = S0_IDs.GetAdresniKlicID("OST")
                        } : null,
                    }
                };

                var tels = SkladDataObj.GetTelefony(d["Telefon"]);
                var emails = SkladDataObj.GetEmaily(d["Mail"]);
                var emailsFA = SkladDataObj.GetEmaily(d["MailFA"]);

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    DeleteItems = "1",
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        tels.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("TelPreb"),
                            SpojeniCislo = tels.Item1,
                            Kod_UserData = S7_Dopl.GetKodTelefon0(kod),
                            Popis = d["Prebirajici"].GetText()
                        } : null,
                        tels.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("TelPreb"),
                            SpojeniCislo = tels.Item2,
                            Kod_UserData = S7_Dopl.GetKodTelefon1(kod),
                            Popis = d["Prebirajici"].GetText()
                        } : null,
                        emails.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("EmailObj"),
                            SpojeniCislo = emails.Item1,
                            Kod_UserData = S7_Dopl.GetKodEmail1(kod),
                            Popis = d["Prebirajici"].GetText()
                        } : null,
                        emails.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("EmailObj"),
                            SpojeniCislo = emails.Item2,
                            Kod_UserData = S7_Dopl.GetKodEmail2(kod)
                        } : null,
                        !SkladDataObj.jePodobne(d["Zastoupeny"].GetText(), d["Prebirajici"].GetText()) ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("TelVed"),
                            SpojeniCislo = "",
                            Kod_UserData = S7_Dopl.GetKodTelefon2(kod),
                            Popis = d["Zastoupeny"].GetText()
                        } : null,
                        emailsFA.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("EmailFa"),
                            SpojeniCislo = emailsFA.Item1,
                            Kod_UserData = S7_Dopl.GetKodEmailFA1(kod)
                        } : null,
                        emailsFA.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("EmailFa"),
                            SpojeniCislo = emailsFA.Item2,
                            Kod_UserData = S7_Dopl.GetKodEmailFA2(kod)
                        } : null,
                        tels.Item1 == null && tels.Item2 == null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("TelPreb"),
                            SpojeniCislo = "#",
                            Kod_UserData = S7_Dopl.GetKodTelefon0(kod),
                            Popis = "<Neznamy>"
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
                    DatumPorizeni_UserDataSpecified = true,
                    Sleva = new S5DataFirmaSleva() {
                        Sleva = "0",
                        VlastniSleva = "False"
                    }
                };

                firma.Poznamka = (
                    d["ZavozDen"].GetText() == "" ? "" : 
                    "Závozový den: " + d["ZavozDen"].GetText() + XmlEnv.NewLine + XmlEnv.NewLine
                ) + obj.GetF3Note() + XmlEnv.NewLine + obj.GetL2Note();

                var tels = SkladDataObj.GetTelefony(d["Telefon"]);
                var telsOZ = SkladDataObj.GetTelefony(d["TelefonOz"]);
                var emails = SkladDataObj.GetEmaily(d["Mail"]);
                var emailsOZ = SkladDataObj.GetEmaily(d["MailOZ"]);

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    DeleteItems = "1",
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        tels.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("TelObj"),
                            SpojeniCislo = tels.Item1,
                            Kod_UserData = S7_Dopl.GetKodTelefon0(kod),
                            Popis = d["Zastoupeny"].GetText()
                        } : null,
                        emails.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("EmailObj"),
                            SpojeniCislo = emails.Item1,
                            Kod_UserData = S7_Dopl.GetKodEmail1(kod),
                            Popis = d["Zastoupeny"].GetText()
                        } : null,
                        tels.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("TelObj"),
                            SpojeniCislo = tels.Item2,
                            Kod_UserData = S7_Dopl.GetKodTelefon1(kod),
                            Popis = d["Zastoupeny"].GetText() 
                        } : null,
                        telsOZ.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("TelOz"),
                            SpojeniCislo = tels.Item1,
                            Kod_UserData = S7_Dopl.GetKodTelefon2(kod),
                            Popis = d["ZastoupenyOZ"].GetText() 
                        } : null,
                        telsOZ.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("TelOz"),
                            SpojeniCislo = telsOZ.Item2,
                            Kod_UserData = S7_Dopl.GetKodTelefon3(kod),
                            Popis = d["ZastoupenyOZ"].GetText() 
                        } : null,
                        emailsOZ.Item1 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("EmailOz"),
                            SpojeniCislo = emailsOZ.Item1,
                            Kod_UserData = S7_Dopl.GetKodEmailFA1(kod),
                            Popis = d["ZastoupenyOZ"].GetText()
                        } : null,
                        emails.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("Email"),
                            SpojeniCislo = emails.Item2,
                            Kod_UserData = S7_Dopl.GetKodEmail2(kod)
                        } : null,
                        emailsOZ.Item2 != null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("EmailOz"),
                            SpojeniCislo = emailsOZ.Item2,
                            Kod_UserData = S7_Dopl.GetKodEmailFA2(kod)
                        } : null,
                        tels.Item1 == null && tels.Item2 == null ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni_ID = S0_IDs.GetTypSpojeniID("Tel"),
                            SpojeniCislo = "#",
                            Kod_UserData = S7_Dopl.GetKodTelefon0(kod),
                            Popis = "<Neznamy>"
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

                var akceCenikID = S0_IDs.GetCenikID("_AKCE");
                var prodejCenikID = S0_IDs.GetCenikID("_PRODEJ");

                firma.ObchodniPodminky = new S5DataFirmaObchodniPodminky() {
                    ZpusobVyberuCeny = new enum_ZpusobVyberuCeny() {
                        Value = enum_ZpusobVyberuCeny_value.Item3 // zpusob prebirani ceny poradim
                    },
                    SeznamCeniku = new S5DataFirmaObchodniPodminkySeznamCeniku() {
                        DeleteItems = "1",
                        FirmaCenik = new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik[] {                                
                            new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik() {
                                Poradi = "1",
                                Cenik_ID = akceCenikID
                            },
                            new S5DataFirmaObchodniPodminkySeznamCenikuFirmaCenik() {
                                Poradi = "2",
                                Cenik_ID = prodejCenikID
                            },
                        }
                    }
                };

                _firmy.Add(firma);
            }

        }
    }
}