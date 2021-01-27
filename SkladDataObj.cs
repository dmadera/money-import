using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SDataObjs;

namespace SkladData {
    class SkladDataObj {
        private Dictionary<string, SkladDataItem> _items = new Dictionary<string, SkladDataItem>();

        public SkladDataObj() { }

        public Dictionary<string, SkladDataItem> Items {
            get => _items;
        }

        public void AddItem(string header, string item) {
            _items.Add(header, new SkladDataItem(item));
        }

        public string GetF3Note() {
            if (!_items.ContainsKey("Poznamka1") || !_items.ContainsKey("Poznamka2") || !_items.ContainsKey("Poznamka3"))
                return null;
            string output = String.Format("{0}\n{1}\n{2}",
                _items["Poznamka1"].GetText(), _items["Poznamka2"].GetText(), _items["Poznamka3"].GetText());
            var r = new Regex(@"^\s*(\n|\r|\r\n)");
            return r.Replace(output, "");
        }

        public string GetL2Note() {
            if (!_items.ContainsKey("Poznamka4") || !_items.ContainsKey("Poznamka5"))
                return null;
            string output = String.Format("{0}\n{1}",
                _items["Poznamka4"].GetText(), _items["Poznamka5"].GetText());
            var r = new Regex(@"^\s*(\n|\r|\r\n)");
            return r.Replace(output, "");
        }

        public string GetDic() {
            return _items["Dic2"].GetAlfaNum() != "" ?
                _items["Dic1"].GetAlfaNum() + _items["Dic2"].GetAlfaNum() : null;
        }

        public string GetNazevZbozi() {
            return (_items["NazevZbozi"].GetText() + " " + _items["NazevZbozi2"].GetText()).TrimEnd();
        }

        public string GetIco() {
            if(int.TryParse(_items["Ico"].GetNum(), out int result)) {
                var ico = int.Parse(_items["Ico"].GetNum());
                return string.Format("{0:00000000}", ico);
            }
            
            return null;
        }

        public static string GetNazev(SkladDataItem nazevItem, SkladDataItem nazev1Item) {
            string nazev = nazevItem.GetText();
            string nazev1 = nazev1Item.GetText();
            
            var r = new Regex(@"[-–]$");
            if(r.IsMatch(nazev)) return nazev.Substring(0, nazev.Length-1) + nazev1;
            return string.Format("{0} {1}", nazev, nazev1).Trim();
        }

        public static Tuple<string, string> GetTelefony(SkladDataItem telefon) {
            var tel = telefon.GetTextNoDiacritics().ToLower();
            if(tel == "") return new Tuple<string, string>(null, null);

            string tel1, tel2 = null;
            int t1, t2;
     
            try {
                tel = Regex.Replace(tel, @"[^0-9]+", "");
                
                if(tel.Length < 9) tel1 = tel;
                else {
                    tel1 = tel.Substring(0, 9);
                    tel1 = tel1.Length > 9 ? tel1.Substring(tel1.Length - 9) : tel1;
                    t1 = int.Parse(tel1);
                    tel1 = string.Format("{0:000 000 000}", t1);

                    if(tel.Length > 9) {
                        tel2 = tel.Substring(9);
                        tel2 = tel2.Length > 9 ? tel2.Substring(0, 9) : tel2;
                        t2 = int.Parse(tel2);
                        int d = (int) Math.Pow(10, ((int) (Math.Log10(t2) + 1)));
                        if(d == 0) tel2 = null;
                        else {
                            t2 = ((int) t1 / d) * d + t2;
                            tel2 = string.Format("{0:000 000 000}", t2);
                        }
                    }
                    
                    if(tel1 == tel2) tel2 = null;
                }
                return new Tuple<string, string>(tel1, tel2);

            } catch(Exception) {
                return new Tuple<string, string>(tel, null);
            }
        }

        public string GetCelkem() {
            float celkem = _items["Celkem0"].GetFloat() + _items["Celkem5"].GetFloat() + _items["Celkem23"].GetFloat();
            return celkem.ToString().Replace(".", ",");
        }

        public string GetProcentniZisk() {
            float celkem = _items["Celkem0"].GetFloat() + _items["Celkem5"].GetFloat() + _items["Celkem23"].GetFloat();
            float ziskZaDoklad = _items["Zisk"].GetFloat();
            float procentniZisk = 100/celkem*ziskZaDoklad;
            return procentniZisk.ToString().Replace(".", ",");
        }

        public static Tuple<string, string> GetEmaily(SkladDataItem item) {
            var input = item.GetNoSpaces();
            var split = input.Replace(",", ";").Split(";");
            string e1, e2;
            e1 = SkladDataItem.IsValidEmail(split[0]) ? split[0] : null;
            e2 = split.Length == 2 && SkladDataItem.IsValidEmail(split[1]) ? split[1] : null;
            return new Tuple<string, string>(e1, e2);
        }

        public override string ToString() {
            string output = "Zdroj:" + XmlEnv.NewLine;
            int counter = 0;
            foreach (KeyValuePair<string, SkladDataItem> entry in _items) {
                output += string.Format("{0}:{1},", entry.Key, entry.Value.GetRaw());
                if (++counter % 3 == 0) {
                    output += XmlEnv.NewLine;
                }
            }
            return output.Substring(0, output.Length - 2);
        }
    }
}