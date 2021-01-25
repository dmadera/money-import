using System;
using System.IO;
using System.Xml.Serialization;

using S0_IDs;

namespace SDataObjs {
    static class S0_IDs {
        private static S5Data _data;

        public static void Deserialize(string input) {
            var serializer = new XmlSerializer(typeof(S5Data));

            using (var stringReader = new StringReader(File.ReadAllText(input))) {
                _data = (S5Data)serializer.Deserialize(stringReader);
            }
        }

        public static bool IsReadyForDocs() {
            if (_data.ArtiklList.Length <= 1) { return false; }
            if (_data.FirmaList.Length <= 1) { return false; }

            return true;
        }

        public static string GetArtiklID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataArtikl artikl in _data.ArtiklList) {
                if (artikl.Kod == kod) {
                    return artikl.ID;
                }
            }

            return null;
        }

        public static string GetFirmaID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataFirma firma in _data.FirmaList) {
                if (firma.Kod == kod) {
                    return firma.ID;
                }
            }

            return null;
        }

        public static string GetSkladID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataSklad sklad in _data.SkladList) {
                if (sklad.Kod == kod) {
                    return sklad.ID;
                }
            }

            return null;
        }

        public static string GetSazbaDPHID(string sazba) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataSazbaDPH dph in _data.SazbaDPHList) {
                if (dph.Sazba.StartsWith(sazba)) {
                    return dph.ID;
                }
            }

            return null;
        }

        public static string GetTypSpojeniID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataTypSpojeni spojeni in _data.TypSpojeniList) {
                if (spojeni.Kod == kod) {
                    return spojeni.ID;
                }
            }

            return null;
        }

        public static string GetFunkceOsobyID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataFunkceOsoby funkceOsoby in _data.FunkceOsobyList) {
                if (funkceOsoby.Code == kod) {
                    return funkceOsoby.ID;
                }
            }

            return null;
        }

        public static string GetStatID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataStat stat in _data.StatList) {
                if (stat.Kod == kod) {
                    return stat.ID;
                }
            }

            return null;
        }

        public static string GetZpusobPlatbyID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataZpusobPlatby zpusobPlatby in _data.ZpusobPlatbyList) {
                if (zpusobPlatby.Kod == kod) {
                    return zpusobPlatby.ID;
                }
            }

            return null;
        }

        public static string GetJednotkaID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataJednotka jednotka in _data.JednotkaList) {
                if (jednotka.Kod == kod) {
                    return jednotka.ID;
                }
            }

            return null;
        }

        public static string GetDruhZboziID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataDruhArtiklu druhZbozi in _data.DruhArtikluList) {
                if (druhZbozi.Kod == kod) {
                    return druhZbozi.ID;
                }
            }

            return null;
        }

        public static string GetProduktovyKlicID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataProduktovyKlic produktovyKlic in _data.ProduktovyKlicList) {
                if (produktovyKlic.Kod == kod) {
                    return produktovyKlic.ID;
                }
            }

            return null;
        }

        public static string GetKategorieArtikluID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataKategorieArtiklu kategorieArtiklu in _data.KategorieArtikluList) {
                if (kategorieArtiklu.Kod == kod) {
                    return kategorieArtiklu.ID;
                }
            }

            return null;
        }

        public static string GetOsobaID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataOsoba osoba in _data.OsobaList) {
                if (osoba.Kod == kod) {
                    return osoba.ID;
                }
            }

            return null;
        }

        public static string GetSpojeniID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataSpojeni spojeni in _data.SpojeniList) {
                if (spojeni.Kod_UserData == kod) {
                    return spojeni.ID;
                }
            }

            return null;
        }
        public static string GetAdresniKlicID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataAdresniKlic adresniKlic in _data.AdresniKlicList) {
                if (adresniKlic.Kod == kod) {
                    return adresniKlic.ID;
                }
            }

            return null;
        }
        public static string GetCinnostID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataCinnost cinnost in _data.CinnostList) {
                if (cinnost.Kod == kod) {
                    return cinnost.ID;
                }
            }

            return null;
        }
        public static string GetCeniktID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataCenik cenik in _data.CenikList) {
                if (cenik.Kod == kod) {
                    return cenik.ID;
                }
            }

            return null;
        }

        public static string GetElektronickyObchodID(string kod) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataElektronickyObchod eobchod in _data.ElektronickyObchodList) {
                if (eobchod.Kod == kod) {
                    return eobchod.ID;
                }
            }

            return null;
        }

        public static string GetArtiklJednotkaID(string artiklID, string jednotkaID) {
            if (_data == null) throw new Exception("First call Deserialize method.");

            foreach (S5DataArtikl artikl in _data.ArtiklList) {
                if (artikl.ID == artiklID) {
                    foreach (S5DataArtiklJednotkySeznamJednotekArtiklJednotka jednotka in artikl.Jednotky.SeznamJednotek.ArtiklJednotka) {
                        if (jednotka.Jednotka_ID == jednotkaID) {
                            return jednotka.ID;
                        }
                    }
                }
            }

            return null;
        }
    }
}