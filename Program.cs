using System.Text;
using System.Xml.Serialization;
using System.IO;

using Schemas;
using SkladData;
using MoneyDataObjects;

namespace MainProgram {
    class Program {

        static void Main(string[] args) {
            string souborOdb = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/headODB";
            string souborDod = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/headDOD";
            string souborKarty = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/headKARTY";
            string souborKod = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/KOD";
            string souborPodKod = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/PODKOD";

            MoneyData data = new MoneyData();

            string[] lines = System.IO.File.ReadAllLines(souborOdb,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFileOdb(lines));

            lines = System.IO.File.ReadAllLines(souborDod,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFileDod(lines));

            lines = System.IO.File.ReadAllLines(souborKarty,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFileKarty(lines));

            lines = System.IO.File.ReadAllLines(souborKod,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFileKod(lines));

            lines = System.IO.File.ReadAllLines(souborPodKod,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFilePodKod(lines));


            var serializer = new XmlSerializer(typeof(S5Data));
            using (var stream = new StreamWriter("/home/dmadera/downloads/ZaklDat.xml")) {
                serializer.Serialize(stream, data.GetS5Data());
            }
        }
    }
}
