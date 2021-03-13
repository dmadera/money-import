using System;
using System.Collections.Generic;
using System.Text;

using S9_Seky;
using SkladData;
using MainProgram;

namespace SDataObjs {
    class S9_Seky : S0_Generic<S5Data> {

        private List<S5DataPokladniDoklad> _seky = new List<S5DataPokladniDoklad>();

        public S9_Seky(string dir, Encoding enc) {
            convertSeky(new SkladDataFile(dir, SFile.SEKY, enc));        
        }

        public override S5Data GetS5Data() {
            return new S5Data() {
                PokladniDokladList = _seky.ToArray()
            };
        }

        private void convertSeky(SkladDataFile file) {

            foreach (SkladDataObj obj in file.Data) {
                var d = obj.Items;
                string cisloOdberatele = S3_Adresar.GetOdbID(d["CisloOdberatele"].GetNum());
                var sek = new S5DataPokladniDoklad() {
                    VariabilniSymbol = d["CisloSeku"].GetNum(),
                    DatumVystaveni = d["DatVystaveni"].GetDate(),
                    DatumVystaveniSpecified = true,
                    ParovaciSymbol = d["Druh"].GetAlfaNum().ToUpper() + d["CisloVydejky"].GetNum(),
                    Firma_ID = S0_IDs.GetFirmaID(cisloOdberatele),
                    Adresa = new S5DataPokladniDokladAdresa() {
                        KontaktniOsobaNazev = d["Prebirajici"].GetText()
                    },
                    Vystavil = d["Obsluha"].GetText(),
                    Group = new group() {
                        Kod = "SEKY"
                    },
                    TypDokladu = new enum_TypDokladu() { Value = enum_TypDokladu_value.Item1 }
                };

                sek.Polozky = new S5DataPokladniDokladPolozkaPokladnihoDokladu[] {
                    new S5DataPokladniDokladPolozkaPokladnihoDokladu() {
                        CelkovaCena = d["Castka"].GetDecimal(),
                        CelkovaCenaCM =  d["Castka"].GetDecimal(),
                        CisloPolozky = "1"
                    }
                };

                _seky.Add(sek);
            }
        }
    }
}