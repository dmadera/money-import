using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using Schemas;


namespace sklad_data_parser {
    class Firma {
        public string kod, nazev, splatnost, sleva, ico, dic, poznamka, sdph;
        public string objednavajici_prijmeni, prebirajici_prijmeni, telefon, email, email2, emailFA, emailFA2;
        public string ulice, mesto, psc;
        public string nazevp, ulicep, mestop, pscp;
        public string kupnismlouva, sumfa, kodsumfa, davatsek, np, kododb, cisloskupiny, odesilat;

        public Firma(string radek) {

            Regex r = new Regex(@"^\s{5}\p{N}{5}\s{5}");
            if (!r.IsMatch(radek)) {
                throw new ArgumentException("Datový řádek neodpovídá formátu.");
            }

            int[] splits = { 16, 89, 97, 105, 126, 147, 153, 163, 167, 196, 199, 220, 241, 303, 311, 373,
                386, 392, 400, 461, 531, 583, 644, 706, 713, 728, 734, 743, 757, 766, 828, 849, 871,
                883, 891, 900 };

            string[] h = new string[splits.Length + 1];
            int start = 0;
            int i = 0;
            foreach (int split in splits) {
                h[i] = Regex.Replace(radek.Substring(start, split - start).Trim(), @"\s{2,}", " ");
                start = split;
                i++;
            }
            h[i] = radek.Substring(start);
            kod = Regex.Replace(h[0], @"[^0-9]+", "");
            nazev = h[1];
            sleva = String.Format("{0:0.00#}", float.Parse(h[2]) * -1).Replace(".", ",");
            splatnost = Regex.Replace(h[3], @"[^0-9]+", "");
            ulice = h[4];
            mesto = h[5];
            psc = Regex.Replace(h[6], @"[^0-9]+", "");
            ico = Regex.Replace(h[7], @"[^0-9]+", "");
            dic = h[9] == "" ? "" : "CZ" + Regex.Replace(h[9], @"[^0-9]+", "");
            objednavajici_prijmeni = h[10];
            prebirajici_prijmeni = h[11];
            telefon = Regex.Replace(h[12], @"[^0-9]+", "");
            string[] emaily = h[13].Replace(",", ";").Split(";");
            email = Global.IsValidEmail(emaily[0]) ? emaily[0] : "";
            email2 = emaily.Length == 2 && Global.IsValidEmail(emaily[1]) ? emaily[1] : "";
            odesilat = h[14].Contains("A") ? "1" : "0";
            emaily = h[15].Replace(",", ";").Split(";");
            emailFA = Global.IsValidEmail(emaily[0]) ? emaily[0] : "";
            emailFA2 = emaily.Length == 2 && Global.IsValidEmail(emaily[1]) ? emaily[1] : "";
            kupnismlouva = h[16].Contains("A") ? "1" : "0";
            sumfa = h[17].Contains("A") ? "1" : "0";
            kodsumfa = Regex.Replace(h[18], @"[^0-9a-zA-Z]+", "");
            poznamka = String.Format("{0}\n{1}\n{2}\n{3}\n{4}", h[19..24]);
            kododb = Regex.Replace(h[24], @"[^0-9a-zA-Z]+", "");
            np = h[25].Contains("A") ? "1" : "0";
            sdph = h[26].Contains("A") ? "1" : "0";
            davatsek = h[27].Contains("A") ? "1" : "0";
            // datumporizeni = h[28];
            cisloskupiny = Regex.Replace(h[29], @"[^0-9]+", "");
            nazevp = h[30];
            ulicep = h[31];
            mestop = h[32];
            pscp = Regex.Replace(h[33], @"[^0-9]+", "");

            r = new Regex(@"^([\|\*\\]+|a\p{N})+(.*)$");
            if (r.IsMatch(nazev)) {
                throw new ArgumentException("Hodnota názvu neodpovídá odběrateli.");
            }
        }
    }

    static class FirmaDataBridge {
        public static S5DataFirma[] ConvertToGetS5Data(string fileName) {
            List<S5DataFirma> s5DataFirmy = new List<S5DataFirma>();
            string[] radky = System.IO.File.ReadAllLines(fileName,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            foreach (string radek in radky) {
                try {
                    var firma = new Firma(radek);
                    var s5DataFirma = GetS5DataFirma(firma);
                    s5DataFirmy.Add(s5DataFirma);
                } catch (ArgumentException e) {
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine("Detail řádku: " + radek);
                }
            }

            return s5DataFirmy.ToArray();
        }

        private static S5DataFirma GetS5DataFirma(Firma f) {
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
                    ID = f.np == "0" ? S5DataIDs.FirmaGroupALLID : S5DataIDs.FirmaGroupNPID
                },
                ReportSDPH_UserData = f.sdph,
            };

            firma.Osoby = new S5DataFirmaOsoby() {
                HlavniOsoba = new S5DataFirmaOsobyHlavniOsoba(),
                SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                    Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            f.objednavajici_prijmeni != "" ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Prijmeni = f.objednavajici_prijmeni,
                                Funkce = "Objednávající"
                            } : null,
                            f.prebirajici_prijmeni != "" ? new S5DataFirmaOsobySeznamOsobOsoba() {
                                Prijmeni = f.prebirajici_prijmeni,
                                Funkce = "Přebírající"
                            } : null
                        }
                }
            };

            firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        f.telefon != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeniTelID
                            },
                            SpojeniCislo = f.telefon,
                        } : null,
                        f.email != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeniEmailID
                            },
                            SpojeniCislo = f.email,
                        } : null,
                        f.email2 != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeniEmailID
                            },
                            SpojeniCislo = f.email2,
                        } : null,
                        f.emailFA != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeniEmailFaID
                            },
                            SpojeniCislo = f.emailFA,
                        } : null,
                        f.emailFA2 != "" ? new S5DataFirmaSeznamSpojeniSpojeni() {
                            TypSpojeni = new S5DataFirmaSeznamSpojeniSpojeniTypSpojeni {
                                ID = S5DataIDs.FirmaTypSpojeniEmailFaID
                            },
                            SpojeniCislo = f.emailFA2,
                        } : null
                    }
            };

            firma.Adresy = new S5DataFirmaAdresy() {
                ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa() {
                    Ulice = f.ulice,
                    Psc = new S5DataFirmaAdresyObchodniAdresaPsc() {
                        ObjectName = f.psc
                    },
                    Misto = f.mesto,
                    Stat = new S5DataFirmaAdresyObchodniAdresaStat() {
                        ID = S5DataIDs.FirmaStatCZID
                    }
                },
                Provozovna = (f.ulicep != "" || f.mestop != "") ? new S5DataFirmaAdresyProvozovna() {
                    Nazev = f.nazevp,
                    Ulice = f.ulicep,
                    Psc = new S5DataFirmaAdresyProvozovnaPsc() {
                        ObjectName = f.pscp
                    },
                    Misto = f.mestop,
                    Stat = new S5DataFirmaAdresyProvozovnaStat() {
                        ID = S5DataIDs.FirmaStatCZID
                    }
                } : null,
                OdlisnaAdresaProvozovny = (f.ulicep != "" || f.mestop != "" ? "1" : "0")
            };
            return firma;
        }
    }
}