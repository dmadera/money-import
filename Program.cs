using System.Text;
using System.Xml.Serialization;
using System.IO;

using Schemas;
using SkladData;
using MoneyDataObjects;

namespace MainProgram {
    class Program {

        static void Main(string[] args) {
            string datasourceDir = @"/home/dmadera/projects/sklad-moneys4-convertor/datasource/";
            string souborOdb = datasourceDir + "headODB";
            string souborDod = datasourceDir + "headDOD";
            string souborKarty = datasourceDir + "headKARTY";
            string souborKod = datasourceDir + "KOD";
            string souborPodKod = datasourceDir + "PODKOD";

            MoneyData data = new MoneyData();

            string[] lines = System.IO.File.ReadAllLines(souborOdb,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFileOdb(lines));

            lines = System.IO.File.ReadAllLines(souborDod,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFileDod(lines));

            lines = System.IO.File.ReadAllLines(souborKod,
              CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFileKod(lines));

            lines = System.IO.File.ReadAllLines(souborPodKod,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFilePodKod(lines));

            lines = System.IO.File.ReadAllLines(souborKarty,
                CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250"));

            data.Add(new SkladDataFileKarty(lines));

            var serializer = new XmlSerializer(typeof(S5Data));
            using (var stream = new StreamWriter(datasourceDir + "ZaklDat.xml")) {
                serializer.Serialize(stream, data.GetS5Data());
            }
        }
    }
}
