using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using SKDKatalog;
using SkladData;

namespace S5DataObj {
    class S5Katalog {
        private List<S5DataArtikl> _data = new List<S5DataArtikl>();

        private List<S5DataKategorieArtiklu> _cats = new List<S5DataKategorieArtiklu>();

        private Predicate<S5DataArtikl> _filter = delegate (S5DataArtikl a) {
            return true;
        };

        private Predicate<S5DataKategorieArtiklu> _filterCats = delegate (S5DataKategorieArtiklu a) {
            return true;
        };

        public static string GetID(string id) {
            return "ART0" + id;
        }

        public S5Katalog(string kartyFile, string kodFile, string podKodFile, Encoding encoding) {
            var lines = System.IO.File.ReadAllLines(kodFile, encoding);
            convert(new SkladDataFileKod(lines));
            lines = System.IO.File.ReadAllLines(podKodFile, encoding);
            convert(new SkladDataFilePodKod(lines));
            lines = System.IO.File.ReadAllLines(kartyFile, encoding);
            convert(new SkladDataFileKarty(lines));
        }

        public S5Data GetS5Data() {
            return new S5Data() {
                ArtiklList = _data.FindAll(_filter).ToArray(),
                KategorieArtikluList = _cats.FindAll(_filterCats).ToArray()
            };
        }

        public void serialize(string output) {
            var serializer = new XmlSerializer(typeof(S5Data));
            using (var stream = new StreamWriter(output)) {
                serializer.Serialize(stream, GetS5Data());
            }
        }

        private void convert(SkladDataFileKod file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                var cat = new S5DataKategorieArtiklu() {
                    ID = Guid.NewGuid().ToString(),
                    Kod = d["KodZbozi"].GetNum(),
                    Nazev = d["NazevKodu"].GetText(),
                };

                _cats.Add(cat);
            }

            string guid = Guid.NewGuid().ToString();
            _cats.AddRange(new S5DataKategorieArtiklu[]{
                        new S5DataKategorieArtiklu() {
                            ID = guid,
                            Kod = "0000",
                            Nazev = "Nezařazené"
                        },
                        new S5DataKategorieArtiklu() {
                            ID = Guid.NewGuid().ToString(),
                            Kod = "00000000",
                            Nazev = "Nezařazené",
                            ParentObject_ID = guid
                        }
                    });
        }

        private void convert(SkladDataFilePodKod file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                var cat = new S5DataKategorieArtiklu() {
                    Kod = d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum(),
                    Nazev = d["NazevPOdKodu"].GetText(),
                    ParentObject_ID = _cats.Find((a) => { return a.Kod == d["KodZbozi"].GetNum(); }).ID
                };
                _cats.Add(cat);
            }
        }

        private void convert(SkladDataFileKarty file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;

                var artikl = new S5DataArtikl() {
                    Kod = GetID(d["CisloKarty"].GetNum()),
                    Nazev = d["NazevZbozi"].GetText(),
                    Poznamka = d["NazevZbozi2"].GetText(),
                    HlavniJednotka_ID = d["MernaJednotka"].GetAlfaNum().ToLower(),
                    Group = new group() {
                        Kod = "ART"
                    },
                    Jednotky = new S5DataArtiklJednotky() {
                        SeznamJednotek = new S5DataArtiklJednotkySeznamJednotek() {
                            ArtiklJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotka[] {
                                d["VKart"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                    Kod = "kar",
                                    VychoziMnozstvi = d["VKart"].GetNum()
                                } : null,
                                d["VFol"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                    Kod = "fol",
                                    VychoziMnozstvi = d["VFol"].GetNum(),
                                    NedelitelneMnozstvi = d["MinFol"].GetBoolean() == "True" ? d["VFol"].GetNum() : null
                                } : null,
                                d["VPal"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                    Kod = "pal",
                                    VychoziMnozstvi = d["VPal"].GetNum()
                                } : null,
                            }
                        }
                    },
                    DruhArtiklu_ID = "ZBO",
                    SazbyDPH = new S5DataArtiklSazbyDPH() {
                        ArtiklDPH = new S5DataArtiklSazbyDPHArtiklDPH[] {
                                new S5DataArtiklSazbyDPHArtiklDPH() {
                                    SazbaVstup = new enum_DruhSazbyDPH() {
                                        EnumValueName = d["SazbaD"].GetNum() == "21" ?
                                            enum_DruhSazbyDPHEnumValueName.Zakladni :
                                            (d["SazbaD"].GetNum() == "15" ?
                                                enum_DruhSazbyDPHEnumValueName.Snizena :
                                                enum_DruhSazbyDPHEnumValueName.Nulova)
                                    }
                                }
                            }
                    }
                };

                artikl.Dodavatele = d["CisloDodavatele"].GetNum() != "00000" ? new S5DataArtiklDodavatele() {
                    HlavniDodavatel = new S5DataArtiklDodavateleHlavniDodavatel() {
                        Firma_ID = S5Adresar.GetDodID(d["CisloDodavatele"].GetNum()),
                    },
                    SeznamDodavatelu = new S5DataArtiklDodavateleSeznamDodavatelu() {
                        ArtiklDodavatel = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatel[] {
                                new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatel() {
                                    Firma_ID = S5Adresar.GetDodID(d["CisloDodavatele"].GetNum()),
                                    DodavatelskeOznaceni = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatelDodavatelskeOznaceni() {
                                        Kod = d["DodCislo"].GetAlfaNum(),
                                        Nazev = d["DodCislo"].GetText()
                                    },
                                    PosledniCena = d["NakupCena"].GetDecimal(),
                                    Firma = new S5DataArtiklDodavateleSeznamDodavateluArtiklDodavatelFirma() {
                                        Kod = S5Adresar.GetDodID(d["CisloDodavatele"].GetNum())
                                    }
                                }
                            }
                    }
                } : null;

                artikl.Kategorie = string.Format(
                    "{0}|{1}",
                    _cats.Find(k => { return k.Kod == d["KodZbozi"].GetNum(); }).ID,
                    _cats.Find(k => { return k.Kod == (d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum()); }).ID
                );

                artikl.SmazatOstatniDodavatele = "True";
                artikl.SmazatOstatniJednotky = "True";
                artikl.SmazatOstatniSazbyDPH = "True";

                _data.Add(artikl);
            }
        }
    }
}