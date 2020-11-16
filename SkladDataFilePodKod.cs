using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkladData {
    class SkladDataFilePodKod : SkladDataFile {

        public SkladDataFilePodKod(string[] lines) : base(lines) {
        }

        public override bool IsValidHeaderLine(string line) {
            Regex r = new Regex(@"^");
            return r.IsMatch(line);
        }
        public override bool IsValidDataLine(string line) {
            Regex r = new Regex(@"^\s{2}\p{N}{4}\s{3}");
            return r.IsMatch(line);
        }
    }
}