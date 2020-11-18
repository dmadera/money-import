using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkladData {
    class SkladDataFilePohybP : SkladDataFile {

        public SkladDataFilePohybP(string[] lines) : base(lines) {
        }

        public override bool IsValidHeaderLine(string line) {
            Regex r = new Regex(@"^CisloPrijemky");
            return r.IsMatch(line);
        }
        public override bool IsValidDataLine(string line) {
            Regex r = new Regex(@"^\s{4}\p{N}{5}\s{5}");
            return r.IsMatch(line);
        }
    }
}