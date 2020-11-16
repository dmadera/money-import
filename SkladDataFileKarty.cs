using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkladData {
    class SkladDataFileKarty : SkladDataFile {

        public SkladDataFileKarty(string[] lines) : base(lines) {
        }

        public static string GetID(string id) {
            return "ART0" + id;
        }

        public override bool IsValidHeaderLine(string line) {
            Regex r = new Regex(@"^");
            return r.IsMatch(line);
        }
        public override bool IsValidDataLine(string line) {
            Regex r = new Regex(@"^\s{3}\p{N}{4}\s{4}");
            return r.IsMatch(line);
        }
    }
}