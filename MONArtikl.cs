using System;
using System.Collections.Generic;
using System.Text;
using Schemas;


namespace sklad_data_parser {

    static class MONArtikl {
        public static S5DataArtikl[] ConvertToS5Data(SKLKarta[] karty, SKLDodavatel[] dodavatele) {
            var s5DataArtikly = new List<S5DataArtikl>();

            foreach (SKLKarta k in karty) {
                s5DataArtikly.Add(GetS5DataArtikl(k));
            }

            return s5DataArtikly.ToArray();
        }

        private static S5DataArtikl GetS5DataArtikl(SKLKarta f) {
            var artikl = new S5DataArtikl() {
                Kod = f.kod,
                Nazev = f.nazev,
                Poznamka = f.poznamka,
                HlavniJednotka_ID = S5DataIDs.ArtiklJednotka[f.mernajednotka],
                Jednotky = new S5DataArtiklJednotky() {
                    SeznamJednotek = new S5DataArtiklJednotkySeznamJednotek() {
                        ArtiklJednotka = new S5DataArtiklJednotkySeznamJednotekArtiklJednotka[] {
                            f.vkart != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                Jednotka_ID = S5DataIDs.ArtiklJednotka["kart"],
                                VychoziMnozstvi = f.vkart
                            } : null,
                            f.vfol != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                Jednotka_ID = S5DataIDs.ArtiklJednotka["fol"],
                                VychoziMnozstvi = f.vkart,
                                NedelitelneMnozstvi = f.minfol == "True" ? f.vkart : null
                            } : null,
                            f.vpal != "0" ? new S5DataArtiklJednotkySeznamJednotekArtiklJednotka() {
                                Jednotka_ID = S5DataIDs.ArtiklJednotka["kart"],
                                VychoziMnozstvi = f.vpal
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
                            ID = S5DataIDs.ArtiklSazbyDPH[f.sazbad]
                        }
                    }
                },
                Dodavatele = new S5DataArtiklDodavatele() {
                    HlavniDodavatel = new S5DataArtiklDodavateleHlavniDodavatel() {
                        // nakup cena
                        // cislododavatele
                        // nazevdodavatele
                    }
                },

            };

            return artikl;
        }
    }
}