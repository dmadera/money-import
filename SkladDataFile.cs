using System.Collections.Generic;

namespace SkladData {
    abstract class SkladDataFile {
        private List<int> _columnSizes = new List<int>();
        private List<string> _headerItems = new List<string>();

        private List<SkladDataObj> _data = new List<SkladDataObj>();

        private string[] _lines;

        public SkladDataFile(string[] lines) {
            _lines = lines;
            foreach (string line in _lines) {
                if (IsValidHeaderLine(line)) {
                    parseHeaderLine(line);
                    break;
                }
            }

            if (_columnSizes.Count == _headerItems.Count && _columnSizes.Count == 0) {
                throw new System.Exception("Nebyl nalezen řádek HEADER!");
            }

            foreach (string line in _lines) {
                if (IsValidDataLine(line)) {
                    _data.Add(parseDataLine(line));
                }
            }
        }

        public string[] Lines {
            get => _lines;
        }

        public List<int> ColumnSizes {
            get => _columnSizes;
        }

        public List<string> HeaderItems {
            get => _headerItems;
        }

        public List<SkladDataObj> Data {
            get => _data;
        }

        private void parseHeaderLine(string line) {
            bool prevSpace = false;
            char c;
            string item = "";
            int columnSize = 1;

            _columnSizes = new List<int>();
            _headerItems = new List<string>();

            for (int i = 1; i < line.Length; i++) {
                c = line[i];
                ++columnSize;
                if (char.IsWhiteSpace(c)) {
                    if (!prevSpace) {
                        _headerItems.Add(item);
                    }
                    prevSpace = true;
                } else {
                    if (prevSpace) {
                        _columnSizes.Add(columnSize - 2);
                        columnSize = 2;
                        item = "" + c;
                    } else {
                        item += c;
                    }
                    prevSpace = false;
                }
            }
        }

        private SkladDataObj parseDataLine(string line) {
            int startIndex = 0;
            string item = "";

            var obj = new SkladDataObj();
            for (int i = 0; i < _columnSizes.Count; i++) {
                int columnSize = _columnSizes[i];
                string header = _headerItems[i];
                item = line.Substring(startIndex, columnSize);
                obj.AddItem(header, item);
                startIndex += columnSize;
            }
            return obj;
        }

        public abstract bool IsValidHeaderLine(string line);

        public abstract bool IsValidDataLine(string line);

        public abstract string GetID(string ID);

    }
}
