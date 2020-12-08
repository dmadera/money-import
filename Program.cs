using System;
using System.Text;

using S4DataObjs;

namespace MainProgram {
    class Program {

        static void Main(string[] args) {
            string outputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/output/";
            string inputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/input/complete/";

            S4_IDs.Deserialize(outputDir + "S_IDs.xml");

            var enc = CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250");

            S4A_Adresar adresar = new S4A_Adresar(inputDir, enc);
            adresar.serialize(outputDir + typeof(S4A_Adresar).Name + ".xml");

            S4A_Katalog katalog = new S4A_Katalog(inputDir, enc);
            katalog.serialize(outputDir + typeof(S4A_Katalog).Name + ".xml");

            S4B_Ceny ceny = new S4B_Ceny(inputDir, enc);
            ceny.serialize(outputDir + typeof(S4B_Ceny).Name + ".xml");

            S4K_ObjVyd objednavky = new S4K_ObjVyd(inputDir, enc);
            objednavky.serialize(outputDir + typeof(S4K_ObjVyd).Name + ".xml");

            S4O_Nabidky nabidky = new S4O_Nabidky(inputDir, enc);
            nabidky.serialize(outputDir + typeof(S4O_Nabidky).Name + ".xml");

            S4B_SklDokl docs = new S4B_SklDokl(inputDir, enc);
            docs.serialize(outputDir + typeof(S4B_SklDokl).Name + ".xml");

        }
    }
}