using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SkladData {
    class SkladDataFile {
        private List<SkladDataObj> _data = new List<SkladDataObj>();
        public SkladDataFile(string[] lines) {
            string header = "";
            int dataStartIndex = 0;

            foreach (string line in lines) {
                dataStartIndex++;

                if (dataStartIndex == 1) continue;

                if (line.StartsWith("--DATA--")) break;

                header += line;
            }

            var data = lines[dataStartIndex..lines.Length];
            var splitHeader = header.Split(";");

            foreach (string line in data) {
                var splitData = Regex.Split(line, @"\$;");
                var obj = new SkladDataObj();
                for (int i = 0; i < splitData.Length; i++) {
                    obj.AddItem(splitHeader[i], splitData[i]);
                }
                _data.Add(obj);
            }
        }

        public List<SkladDataObj> Data {
            get => _data;
        }
    }
}
