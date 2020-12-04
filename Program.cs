using System;
using System.Text;

using S4DataObjs;

namespace MainProgram {
    class Program {

        static void Main(string[] args) {
            // for n in input/complete/*; do head -n 10 ${n} > input/$(basename ${n}) ; done && cp input/complete/KOD.TXT input/complete/PODKOD.TXT input/complete/SKUP.TXT input/
            string outputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/output/";
            string inputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/input/complete/";
            string souborOdb = inputDir + "ODB" + ".TXT";
            string souborDod = inputDir + "DOD" + ".TXT";
            string souborKarty = inputDir + "KARTY" + ".TXT";
            string souborKod = inputDir + "KOD" + ".TXT";
            string souborPodKod = inputDir + "PODKOD" + ".TXT";
            string cpohypob = inputDir + "CPOHYBOB" + ".TXT";
            string pohypob = inputDir + "POHYBOB" + ".TXT";
            string cpohypp = inputDir + "CPOHYBP" + ".TXT";
            string pohypp = inputDir + "POHYBP" + ".TXT";
            string cpohypv = inputDir + "CPOHYBV" + ".TXT";
            string pohypv = inputDir + "POHYBV" + ".TXT";
            string cpohypov = inputDir + "CPOHYBOV" + ".TXT";
            string pohypov = inputDir + "POHYBOV" + ".TXT";
            string cpohypvp = inputDir + "CPOHYBVP" + ".TXT";
            string pohypvp = inputDir + "POHYBVP" + ".TXT";
            string cpohypn = inputDir + "CPOHYBN" + ".TXT";
            string pohypn = inputDir + "POHYBN" + ".TXT";
            string kartyinv = inputDir + "KARTYINV" + ".TXT";

            var enc = CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250");
            S4A_Adresar adresar = new S4A_Adresar(souborOdb, souborDod, enc);
            adresar.serialize(outputDir + typeof(S4A_Adresar).Name + ".xml");

            S4A_Katalog katalog = new S4A_Katalog(souborKarty, souborKod, souborPodKod, enc);
            katalog.serialize(outputDir + typeof(S4A_Katalog).Name + ".xml");

            S4A_Zasoby zasoby = new S4A_Zasoby(souborKarty, enc);
            zasoby.serialize(outputDir + typeof(S4A_Zasoby).Name + ".xml");

            S4K_ObjVyd objednavky = new S4K_ObjVyd(cpohypob, pohypob, enc);
            objednavky.serialize(outputDir + typeof(S4K_ObjVyd).Name + ".xml");

            S4K_Prijem prijemky = new S4K_Prijem(cpohypp, pohypp, kartyinv, enc);
            prijemky.serialize(outputDir + typeof(S4K_Prijem).Name + ".xml");

            S4L_Faktury faktury = new S4L_Faktury(cpohypv, pohypv, enc);
            faktury.serialize(outputDir + typeof(S4L_Faktury).Name + ".xml");

            S4L_OstDLV ostDLV = new S4L_OstDLV(cpohypov, pohypov, enc);
            ostDLV.serialize(outputDir + typeof(S4L_OstDLV).Name + ".xml");

            S4L_ProdejVyd prodejky = new S4L_ProdejVyd(cpohypvp, pohypvp, enc);
            prodejky.serialize(outputDir + typeof(S4L_ProdejVyd).Name + ".xml");

            S4O_Nabidky nabidky = new S4O_Nabidky(cpohypn, pohypn, enc);
            nabidky.serialize(outputDir + typeof(S4O_Nabidky).Name + ".xml");

        }
    }
}