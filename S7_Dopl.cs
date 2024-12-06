using System;
using System.Collections.Generic;
using System.Text;

using S7_Dopl;
using SkladData;
using MainProgram;

namespace SDataObjs
{
    class S7_Dopl : S0_Generic<S5Data>
    {

        private List<S5DataFirma> _firmy = new List<S5DataFirma>();
        private List<S5DataArtikl> _artikly = new List<S5DataArtikl>();

        private SkladDataFile kodSumFile;
        private SkladDataFile kodOdbFile;
        private SkladDataFile odbFile;

        public S7_Dopl(string dir, Encoding enc)
        {
            kodSumFile = new SkladDataFile(dir, SFile.KODSUMFA, enc);
            kodOdbFile = new SkladDataFile(dir, SFile.KODODB, enc);
            odbFile = new SkladDataFile(dir, SFile.ODB, enc);
            convertOdb(odbFile);
            convertDod(new SkladDataFile(dir, SFile.DOD, enc));
            convertKarty(new SkladDataFile(dir, SFile.KARTY, enc));
        }

        public static string GetKodPrebirajici(string kodFirmy) { return kodFirmy + "PRE"; }
        public static string GetKodZastoupeny(string kodFirmy) { return kodFirmy + "ZAS"; }
        public static string GetKodZastoupenyOZ(string kodFirmy) { return kodFirmy + "ZOZ"; }
        public static string GetKodTelefon0(string kodFirmy) { return kodFirmy + "TL0"; }
        public static string GetKodTelefon1(string kodFirmy) { return kodFirmy + "TL1"; }
        public static string GetKodTelefon2(string kodFirmy) { return kodFirmy + "TL2"; }
        public static string GetKodTelefon3(string kodFirmy) { return kodFirmy + "TL3"; }
        public static string GetKodTelefonFA(string kodFirmy) { return kodFirmy + "TFA"; }
        public static string GetKodEmail1(string kodFirmy) { return kodFirmy + "EM1"; }
        public static string GetKodEmail2(string kodFirmy) { return kodFirmy + "EM2"; }
        public static string GetKodEmailFA1(string kodFirmy) { return kodFirmy + "EF1"; }
        public static string GetKodEmailFA2(string kodFirmy) { return kodFirmy + "EF2"; }

        public override S5Data GetS5Data()
        {
            return new S5Data()
            {
                FirmaList = _firmy.ToArray(),
                ArtiklList = _artikly.ToArray()
            };
        }

        private void convertOdb(SkladDataFile file)
        {
            var statID = S0_IDs.GetStatID("CZ");

            foreach (SkladDataObj obj in file.Data)
            {
                var d = obj.Items;
                string kod = S3_Adresar.GetOdbID(d["CisloOdberatele"].GetNum());
                if (kod == S3_Adresar.GetOdbID("00001")) continue;

                string kodSumFa = d["KodSumFa"].GetAlfaNum();
                string kodOdb = d["KodOdb"].GetAlfaNum();

                var firma = new S5DataFirma() { };
                firma.ID = S0_IDs.GetFirmaID(kod);

                if (firma.ID == null) continue;

                string tel1ID = S0_IDs.GetSpojeniID(GetKodTelefon0(kod));
                string tel1copyID = S0_IDs.GetSpojeniID(GetKodTelefon1(kod));
                string email1ID = S0_IDs.GetSpojeniID(GetKodEmail1(kod));

                if (kodSumFa == "dr")
                {
                    firma.Cinnosti = new S5DataFirmaCinnosti()
                    {
                        FirmaCinnost = new S5DataFirmaCinnostiFirmaCinnost[] {
                            new S5DataFirmaCinnostiFirmaCinnost() {
                                Cinnost_ID = S0_IDs.GetCinnostID("DRAK")
                            }
                        }
                    };
                }
                else
                {
                    if (d["SumFa"].GetBoolean() == "True")
                    {
                        firma.Cinnosti = new S5DataFirmaCinnosti()
                        {
                            FirmaCinnost = new S5DataFirmaCinnostiFirmaCinnost[] {
                                new S5DataFirmaCinnostiFirmaCinnost() {
                                    Cinnost_ID = S0_IDs.GetCinnostID("S_FA")
                                }
                            }
                        };
                    }

                    var cisloOdbKodSumFa = S3_Adresar.GetOdbID(findKodOdbKodSum(kodSumFa));
                    var nadrazenaFirmaObj = findOdbByKod(cisloOdbKodSumFa);
                    var nadrazenaFirmaID = S0_IDs.GetFirmaID(cisloOdbKodSumFa);
                    bool isKodSumFa = true;

                    if (nadrazenaFirmaID == null)
                    {
                        var cisloOdbKodOdb = S3_Adresar.GetOdbID(findKodOdbKodOdb(kodOdb));
                        nadrazenaFirmaObj = findOdbByKod(cisloOdbKodOdb);
                        nadrazenaFirmaID = S0_IDs.GetFirmaID(cisloOdbKodOdb);
                        isKodSumFa = false;
                    }

                    if (firma.ID != nadrazenaFirmaID && nadrazenaFirmaID != null)
                    {
                        firma.Adresy = new S5DataFirmaAdresy()
                        {
                            OdlisnaAdresaProvozovny = "True",
                            ObchodniAdresa = new S5DataFirmaAdresyObchodniAdresa()
                            {
                                Nazev = SkladDataObj.GetNazev(nadrazenaFirmaObj.Items["NazevOdberatele"], nadrazenaFirmaObj.Items["NazevOdberatele2"]),
                                Ulice = nadrazenaFirmaObj.Items["Ulice"].GetText(),
                                KodPsc = nadrazenaFirmaObj.Items["Psc"].GetNum(),
                                Misto = nadrazenaFirmaObj.Items["Mesto"].GetText(),
                                Stat = new S5DataFirmaAdresyObchodniAdresaStat() { ID = statID }
                            }
                        };

                        firma.Nazev = SkladDataObj.GetNazev(nadrazenaFirmaObj.Items["NazevOdberatele"], nadrazenaFirmaObj.Items["NazevOdberatele2"]);

                        firma.NadrazenaFirma = new S5DataFirmaNadrazenaFirma()
                        {
                            PrevzitObchodniPodminky = isKodSumFa ? "True" : "False",
                            PrevzitObchodniUdaje = "True",
                            PrevzitBankovniSpojeni = "True",
                            Firma = new S5DataFirmaNadrazenaFirmaFirma()
                            {
                                ID = nadrazenaFirmaID
                            }
                        };

                        if (email1ID == null)
                        {
                            var emails = SkladDataObj.GetEmaily(nadrazenaFirmaObj.Items["Mail"]);
                            if (firma.SeznamSpojeni == null) firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni()
                            {
                                Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                                    new S5DataFirmaSeznamSpojeniSpojeni() {
                                        TypSpojeni_ID = S0_IDs.GetTypSpojeniID("Email"),
                                        SpojeniCislo = emails.Item1,
                                        Kod_UserData = S7_Dopl.GetKodEmail1(kod),
                                        Popis = "z nadřazené firmy"
                                    }
                                }
                            };
                        }
                    }
                }

                _firmy.Add(firma);
            }
        }

        private SkladDataObj findOdbByKod(string kod)
        {
            var found = odbFile.Data.Find(delegate (SkladDataObj obj)
            {
                return S3_Adresar.GetOdbID(obj.Items["CisloOdberatele"].GetText()) == kod;
            });
            return found;
        }

        private string findKodOdbKodSum(string kodSumFa)
        {
            var found = kodSumFile.Data.Find(delegate (SkladDataObj obj)
            {
                return obj.Items["KodSumFa"].GetText() == kodSumFa;
            });
            if (found == null) return null;
            return found.Items["CisloOdberatele"].GetText();
        }

        private string findKodOdbKodOdb(string kodOdb)
        {
            var found = kodOdbFile.Data.Find(delegate (SkladDataObj obj)
            {
                return obj.Items["KodOdb"].GetText() == kodOdb;
            });
            if (found == null) return null;
            return found.Items["CisloOdberatele"].GetText();
        }

        private void convertDod(SkladDataFile file) { }

        private void convertKarty(SkladDataFile file)
        {
            foreach (SkladDataObj obj in file.Data)
            {
                var d = obj.Items;
                string kod = S3_Katalog.GetID(d["CisloKarty"].GetNum());
                var artikl = new S5DataArtikl() { };
                artikl.ID = S0_IDs.GetArtiklID(kod);

                if (artikl.ID == null) continue;

                var kodDodavatele = S3_Adresar.GetDodID(d["CisloDodavatele"].GetNum());
                artikl.HlavniDodavatel_ID = S0_IDs.GetArtiklDodavatelID(artikl.ID, S0_IDs.GetFirmaID(kodDodavatele));
                var mernaJednotka = d["MernaJednotka"].GetNoSpaces().RemoveDiacritics().ToLower();
                var jednotkaID = S0_IDs.GetJednotkaID(mernaJednotka);
                var hlavniJednotkaID = S0_IDs.GetArtiklJednotkaID(artikl.ID, jednotkaID);
                artikl.Jednotky = hlavniJednotkaID != null ? new S5DataArtiklJednotky()
                {
                    SeznamJednotek = new S5DataArtiklJednotkySeznamJednotek()
                    {
                        ArtiklJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotka[] {
                            new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                ID = hlavniJednotkaID,
                                NedelitelneMnozstvi = d["MinFol"].GetBoolean() == "True" ? d["VFol"].GetNum() : null
                            }
                        }
                    }
                } : null;

                var kodID = S0_IDs.GetKategorieArtikluID(d["KodZbozi"].GetNum());
                var podKodID = S0_IDs.GetKategorieArtikluID(d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum());

                if (kodID != null && podKodID != null)
                {
                    artikl.Kategorie = string.Format(
                        "{0}|{1}",
                        kodID,
                        podKodID
                    );
                }

                _artikly.Add(artikl);
            }
        }
    }
}
