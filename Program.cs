using System.Text;

using S5DataObj;

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
            S5Adresar adresar = new S5Adresar(souborOdb, souborDod, enc);
            adresar.serialize(outputDir + typeof(S5Adresar).Name + ".xml");

            S5Katalog katalog = new S5Katalog(souborKarty, souborKod, souborPodKod, enc);
            katalog.serialize(outputDir + typeof(S5Katalog).Name + ".xml");

            S5Zasoby zasoby = new S5Zasoby(souborKarty, enc);
            zasoby.serialize(outputDir + typeof(S5Zasoby).Name + ".xml");
        }
    }
}