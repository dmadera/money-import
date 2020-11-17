using System;
using System.Collections.Generic;

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

        public string Get5Note() {
            return String.Format("{0}\n{1}\n{2}\n{3}\n{4}",
                _items["Poznamka1"].GetText(), _items["Poznamka2"].GetText(), _items["Poznamka3"].GetText(),
                _items["Poznamka4"].GetText(), _items["Poznamka5"].GetText());
        }

        public string GetDic() {
            return _items["Dic2"].GetAlfaNum() != "" ?
                _items["Dic1"].GetAlfaNum() + _items["Dic2"].GetAlfaNum() : null;
        }

        public string[] ParseEmails(string key) {
            var input = _items[key].GetNoSpaces();
            var split = input.Replace(",", ";").Split(";");
            var emaily = new string[2];
            emaily[0] = SkladDataItem.IsValidEmail(split[0]) ? split[0] : null;
            emaily[1] = split.Length == 2 && SkladDataItem.IsValidEmail(split[1]) ? split[1] : null;
            return emaily;
        }
    }
}