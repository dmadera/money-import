using System.Collections.Generic;
using SkladData;
using Schemas;

namespace MoneyData {

    class MoneyDataArtikl {
        private List<S5DataArtikl> _s5DataArtikls = new List<S5DataArtikl>();
        public void Add(SkladDataFileKarty kar) {
            foreach (SkladDataObj obj in kar.Data) {
                var d = obj.Items;

                var artikl = new S5DataArtikl() {
                    Kod = kar.GetID(d["CisloKarty"].GetNum()),
                    Nazev = d["NazevZbozi"].GetText(),
                    Poznamka = d["NazevZbozi2"].GetText(),
                    HlavniJednotka_ID = S5DataIDs.ArtiklJednotka[d["MernaJednotka"].GetAlfaNum()],
                    Jednotky = new S5DataArtiklJednotky() {
                        SeznamJednotek = new S5DataArtiklJednotkySeznamJednotek() {
                            ArtiklJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotka[] {
                            d["VKart"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                Jednotka_ID = S5DataIDs.ArtiklJednotka["kart"],
                                VychoziMnozstvi = d["VKart"].GetNum()
                            } : null,
                            d["VFol"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                Jednotka_ID = S5DataIDs.ArtiklJednotka["fol"],
                                VychoziMnozstvi = d["VFol"].GetNum(),
                                NedelitelneMnozstvi = d["MinFol"].GetBoolean() == "True" ? d["VFol"].GetNum() : null
                            } : null,
                            d["VPal"].GetNum() != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                Jednotka_ID = S5DataIDs.ArtiklJednotka["pal"],
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
                    SazbyDPH = new S5DataArtiklSazbyDPH() {
                        ArtiklDPH = new S5DataArtiklSazbyDPHArtiklDPH[] {
                        new S5DataArtiklSazbyDPHArtiklDPH() {
                            ID = S5DataIDs.ArtiklSazbyDPH[d["SazbaD"].GetDecimal()]
                        }
                    }
                    },
                    Dodavatele = new S5DataArtiklDodavatele() {
                        HlavniDodavatel = new S5DataArtiklDodavateleHlavniDodavatel() {
                            // nakup cena
                            // cislododavatele
                            // nazevdodavatele
                        }
                    }
                };
                _s5DataArtikls.Add(artikl);
            }
        }
    }
}