using System.Collections.Generic;

using SkladData;
using Schemas;

namespace MoneyDataObjects {

    class MoneyDataArtikl {

        public static List<S5DataArtikl> GetData(SkladDataFile kar,
                List<S5DataKategorieArtiklu> kategorie) {

            var data = new List<S5DataArtikl>();

            foreach (SkladDataObj obj in kar.Data) {
                var d = obj.Items;

                var artikl = new S5DataArtikl() {
                    Kod = kar.GetID(d["CisloKarty"].GetNum()),
                    Nazev = d["NazevZbozi"].GetText(),
                    Poznamka = d["NazevZbozi2"].GetText(),
                    HlavniJednotka_ID = d["MernaJednotka"].GetAlfaNum(),
                    Jednotky = new S5DataArtiklJednotky() {
                        SeznamJednotek = new S5DataArtiklJednotkySeznamJednotek() {
                            ArtiklJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotka[] {
                            d["VKart"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                Kod = "kart",
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
                    TypArtiklu = new enum_TypArtiklu() {
                        EnumValueName = enum_TypArtikluEnumValueName.Jednoduchy
                    },
                    TypEvidence = new enum_TypEvidenceArtiklu() {
                        EnumValueName = enum_TypEvidenceArtikluEnumValueName.Neni
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
                    },
                    Dodavatele = new S5DataArtiklDodavatele() {
                        HlavniDodavatel = new S5DataArtiklDodavateleHlavniDodavatel() {
                            NazevFirmy = d["NazevDodavatele"].GetText(),
                        }
                    }
                };

                artikl.Kategorie = string.Format(
                    "{0}|{1}",
                    kategorie.Find(k => { return k.Kod == d["KodZbozi"].GetNum(); }).ID,
                    kategorie.Find(k => { return k.Kod == d["KodZbozi"].GetNum() + d["PodKodZbozi"].GetNum(); }).ID
                );

                data.Add(artikl);
            }

            return data;
        }
    }
}