using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkladData {
    class SkladDataFileDod : SkladDataFile {

        public SkladDataFileDod(string[] lines) : base(lines) {
        }

        public override string GetID(string id) {
            return "AD" + "DOD" + id;
        }

        public override bool IsValidHeaderLine(string line) {
            Regex r = new Regex(@"^");
            return r.IsMatch(line);
        }
        public override bool IsValidDataLine(string line) {
            Regex r = new Regex(@"^\s{5}\p{N}{5}\s{5}");
            return r.IsMatch(line);
        }
    }
}