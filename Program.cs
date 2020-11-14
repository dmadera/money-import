

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Text;
using Schemas;
using SkladData;
using MoneyDataObjects;

namespace MainProgram {
    class Program {

        static void Main(string[] args) {
            string souborOdb = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/headODB";
            string souborDod = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/headDOD";
            string souborKarty = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/headKARTY";

            string[] lines = System.IO.File.ReadAllLines(souborOdb,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            var odb = new SkladDataFileOdb(lines);

            lines = System.IO.File.ReadAllLines(souborDod,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));
            var dod = new SkladDataFileDod(lines);

            lines = System.IO.File.ReadAllLines(souborKarty,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));
            var karty = new SkladDataFileKarty(lines);

            // var serializer = new XmlSerializer(typeof(S5Data));
            // using (var stream = new StreamWriter("/home/dmadera/downloads/output.xml")) {
            //     serializer.Serialize(stream, data);
            // }
        }
    }
}
