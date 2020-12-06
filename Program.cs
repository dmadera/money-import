using System;
using System.Text;

using S4DataObjs;

namespace MainProgram {
    class Program {

        static void Main(string[] args) {
            // for n in input/complete/*; do head -n 10 ${n} > input/$(basename ${n}) ; done && cp input/complete/KOD.TXT input/complete/PODKOD.TXT input/complete/SKUP.TXT input/
            string outputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/output/";
            // string inputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/input/complete/";
            string inputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/input/";
            string souborOdb = inputDir + "ODB" + ".TXT";
            string souborDod = inputDir + "DOD" + ".TXT";
            string souborKarty = inputDir + "KARTY" + ".TXT";
            string souborKod = inputDir + "KOD" + ".TXT";
            string souborPodKod = inputDir + "PODKOD" + ".TXT";
            string cpohybob = inputDir + "CPOHYBOB" + ".TXT";
            string pohybob = inputDir + "POHYBOB" + ".TXT";
            string cpohybp = inputDir + "CPOHYBP" + ".TXT";
            string pohybp = inputDir + "POHYBP" + ".TXT";
            string cpohybv = inputDir + "CPOHYBV" + ".TXT";
            string pohybv = inputDir + "POHYBV" + ".TXT";
            string cpohybov = inputDir + "CPOHYBOV" + ".TXT";
            string pohybov = inputDir + "POHYBOV" + ".TXT";
            string cpohybvp = inputDir + "CPOHYBVP" + ".TXT";
            string pohybvp = inputDir + "POHYBVP" + ".TXT";
            string cpohybn = inputDir + "CPOHYBN" + ".TXT";
            string pohybn = inputDir + "POHYBN" + ".TXT";
            string kartyinv = inputDir + "KARTYINV" + ".TXT";

            var enc = CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250");
            S4A_Adresar adresar = new S4A_Adresar(souborOdb, souborDod, enc);
            adresar.serialize(outputDir + typeof(S4A_Adresar).Name + ".xml");

            S4A_Katalog katalog = new S4A_Katalog(souborKarty, souborKod, souborPodKod, enc);
            katalog.serialize(outputDir + typeof(S4A_Katalog).Name + ".xml");

            S4A_Zasoby zasoby = new S4A_Zasoby(souborKarty, enc);
            zasoby.serialize(outputDir + typeof(S4A_Zasoby).Name + ".xml");

            S4K_ObjVyd objednavky = new S4K_ObjVyd(cpohybob, pohybob, enc);
            objednavky.serialize(outputDir + typeof(S4K_ObjVyd).Name + ".xml");

            // S4K_Prijem prijemky = new S4K_Prijem(cpohybp, pohybp, kartyinv, enc);
            // prijemky.serialize(outputDir + typeof(S4K_Prijem).Name + ".xml");

            // S4L_Faktury faktury = new S4L_Faktury(cpohybv, pohybv, enc);
            // faktury.serialize(outputDir + typeof(S4L_Faktury).Name + ".xml");

            // S4L_OstDLV ostDLV = new S4L_OstDLV(cpohybov, pohybov, enc);
            // ostDLV.serialize(outputDir + typeof(S4L_OstDLV).Name + ".xml");

            // S4L_ProdejVyd prodejky = new S4L_ProdejVyd(cpohybvp, pohybvp, enc);
            // prodejky.serialize(outputDir + typeof(S4L_ProdejVyd).Name + ".xml");

            S4O_Nabidky nabidky = new S4O_Nabidky(cpohybn, pohybn, enc);
            nabidky.serialize(outputDir + typeof(S4O_Nabidky).Name + ".xml");

            S4B_SklDokl docs = new S4B_SklDokl(kartyinv, cpohybp, pohybp, cpohybv, pohybv, cpohybov, pohybov, cpohybvp, pohybvp, enc);
            docs.serialize(outputDir + typeof(S4B_SklDokl).Name + ".xml");

        }
    }
}