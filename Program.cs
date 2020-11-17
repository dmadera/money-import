using System.Text;

using S4DataObjs;

namespace MainProgram {
    class Program {

        static void Main(string[] args) {
            string outputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/output/";
            string inputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/input/";
            string souborOdb = inputDir + "headODB";
            string souborDod = inputDir + "headDOD";
            string souborKarty = inputDir + "headKARTY";
            string souborKod = inputDir + "KOD";
            string souborPodKod = inputDir + "PODKOD";

            var enc = CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250");
            S4A_Adresar adresar = new S4A_Adresar(souborOdb, souborDod, enc);
            adresar.serialize(outputDir + typeof(S4A_Adresar).Name + ".xml");

            S4A_Katalog katalog = new S4A_Katalog(souborKarty, souborKod, souborPodKod, enc);
            katalog.serialize(outputDir + typeof(S4A_Katalog).Name + ".xml");

            S4A_Zasoby zasoby = new S4A_Zasoby(souborKarty, enc);
            zasoby.serialize(outputDir + typeof(S4A_Zasoby).Name + ".xml");

        }
    }
}