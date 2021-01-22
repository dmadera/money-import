using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using S7_OsSpoj;
using SkladData;

namespace SDataObjs {
    class S7_OsSpoj: S0_Generic<S5Data> {

        private List<S5DataFirma> _firmy = new List<S5DataFirma>();

        public S7_OsSpoj(string dir, Encoding enc) {
            convertOdb(new SkladDataFile(dir, SFile.ODB, enc));
            convertDod(new SkladDataFile(dir, SFile.DOD, enc));
        }

        public static string GetKodPrebirajici(string kodFirmy) { return kodFirmy + "PRE"; }
        public static string GetKodZastoupeny(string kodFirmy) { return kodFirmy + "ZAS"; }
        public static string GetKodZastoupenyOZ(string kodFirmy) { return kodFirmy + "ZOZ"; }
        public static string GetKodTelefon(string kodFirmy) { return kodFirmy + "TEL"; }
        public static string GetKodTelefonFA(string kodFirmy) { return kodFirmy + "TFA"; }
        public static string GetKodEmail1(string kodFirmy) { return kodFirmy + "EM1"; }
        public static string GetKodEmail2(string kodFirmy) { return kodFirmy + "EM2"; }
        public static string GetKodEmailFA1(string kodFirmy) { return kodFirmy + "EF1"; }
        public static string GetKodEmailFA2(string kodFirmy) { return kodFirmy + "EF2"; }

        public override S5Data GetS5Data() {
            return new S5Data() {
                FirmaList = _firmy.ToArray()
            };
        }

        private void convertOdb(SkladDataFile file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                string kod = S3_Adresar.GetOdbID(d["CisloOdberatele"].GetNum());

                var firma = new S5DataFirma() {};
                firma.ID = S0_IDs.GetFirmaID(kod);

                string zastoupenyID = S0_IDs.GetOsobaID(GetKodZastoupeny(kod));
                string tel1ID = S0_IDs.GetSpojeniID(GetKodTelefon(kod));
                string email1ID = S0_IDs.GetSpojeniID(GetKodEmail1(kod));

                firma.Osoby = new S5DataFirmaOsoby() {
                    HlavniOsoba = zastoupenyID == null ? null : new S5DataFirmaOsobyHlavniOsoba() {
                        ID = zastoupenyID
                    },
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            zastoupenyID == null ? null : new S5DataFirmaOsobySeznamOsobOsoba() {
                                ID = zastoupenyID,
                                TelefonSpojeni1_ID = tel1ID,
                                EmailSpojeni_ID = email1ID
                            }
                        }
                    }
                };

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        tel1ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = tel1ID,
                            Osoba_ID = zastoupenyID
                        },
                        email1ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = email1ID,
                            Osoba_ID = zastoupenyID
                        }
                    } 
                };

                _firmy.Add(firma);
            }
        }

        private void convertDod(SkladDataFile file) {
            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                string kod = S3_Adresar.GetDodID(d["CisloDodavatele"].GetNum());
                var firma = new S5DataFirma() {};
                firma.ID = S0_IDs.GetFirmaID(kod);
                
                string zastoupenyID = S0_IDs.GetOsobaID(GetKodZastoupeny(kod));
                string zastoupenyOZID = S0_IDs.GetOsobaID(GetKodZastoupenyOZ(kod));
                string tel1ID = S0_IDs.GetSpojeniID(GetKodTelefon(kod));
                string tel2ID = S0_IDs.GetSpojeniID(GetKodTelefonFA(kod));
                string email1ID = S0_IDs.GetSpojeniID(GetKodEmail1(kod));
                string email2ID = S0_IDs.GetSpojeniID(GetKodEmailFA1(kod));

                firma.Osoby = new S5DataFirmaOsoby() {
                    HlavniOsoba = zastoupenyID == null ? null : new S5DataFirmaOsobyHlavniOsoba() {
                        ID = zastoupenyID
                    },
                    SeznamOsob = new S5DataFirmaOsobySeznamOsob() {
                        Osoba = new S5DataFirmaOsobySeznamOsobOsoba[] {
                            zastoupenyID == null ? null : new S5DataFirmaOsobySeznamOsobOsoba() {
                                ID = zastoupenyID,
                                TelefonSpojeni1_ID = tel1ID,
                                EmailSpojeni_ID = email1ID
                            },
                            zastoupenyOZID == null ? null : new S5DataFirmaOsobySeznamOsobOsoba() {
                                ID = zastoupenyOZID,
                                TelefonSpojeni1_ID = tel2ID,
                                EmailSpojeni_ID = email2ID
                            }
                        }
                    }
                };

                firma.SeznamSpojeni = new S5DataFirmaSeznamSpojeni() {
                    Spojeni = new S5DataFirmaSeznamSpojeniSpojeni[] {
                        tel1ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = tel1ID,
                            Osoba_ID = zastoupenyID
                        },
                        email1ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = email1ID,
                            Osoba_ID = zastoupenyID
                        },
                        tel2ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = tel2ID,
                            Osoba_ID = zastoupenyOZID
                        },
                        email2ID == null ? null : new S5DataFirmaSeznamSpojeniSpojeni() {
                            ID = email2ID,
                            Osoba_ID = zastoupenyOZID
                        }
                    } 
                };
                
                _firmy.Add(firma);
                _firmy.Add(firma);
            }

        }
    }
}