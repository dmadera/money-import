using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SkladData {
    enum SFile {
        CENY, CPOHYBOV, CPOHYBZ, KOD, POHYBOB, POHYBV, CPOHYBP, DOD, ODB, POHYBOV, POHYBZ,
        CPOHYBN, CPOHYBVP, KARTYINV, PODKOD, POHYBP, SEKY,
        CPOHYBOB, CPOHYBV, KARTY, POHYBN, POHYBVP, SKUP
    }

    class SkladDataFile {
        private List<SkladDataObj> _data = new List<SkladDataObj>();

        private SFile _soubor;

        public SFile Soubor {
            get {
                return _soubor;
            }
        }

        public SkladDataFile(string dir, SFile file, Encoding enc) {
            _soubor = file;
            string header = "";
            int dataStartIndex = 0;
            var fileName = Path.Combine(dir, _soubor.ToString() + ".TXT");
            var lines = File.ReadAllLines(fileName, enc);

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
