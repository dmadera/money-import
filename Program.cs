using System.Text;

using S4DataObjs;

namespace MainProgram {
    class Program {

        static void Main(string[] args) {
            // for n in input/complete/*; do head -n 10 ${n} > input/$(basename ${n}) ; done
            // cp input/complete/KOD input/complete/PODKOD input/ ;
            string outputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/output/";
            string inputDir = @"/home/dmadera/projects/sklad-moneys4-convertor/input/";
            string souborOdb = inputDir + "ODB";
            string souborDod = inputDir + "DOD";
            string souborKarty = inputDir + "KARTY";
            string souborKod = inputDir + "KOD";
            string souborPodKod = inputDir + "PODKOD";
            string cpohypob = inputDir + "CPOHYBOB";
            string pohypob = inputDir + "POHYBOB";
            string cpohypp = inputDir + "CPOHYBP";
            string pohypp = inputDir + "POHYBP";
            string cpohypv = inputDir + "CPOHYBV";
            string pohypv = inputDir + "POHYBV";

            var enc = CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250");
            S4A_Adresar adresar = new S4A_Adresar(souborOdb, souborDod, enc);
            adresar.serialize(outputDir + typeof(S4A_Adresar).Name + ".xml");

            S4A_Katalog katalog = new S4A_Katalog(souborKarty, souborKod, souborPodKod, enc);
            katalog.serialize(outputDir + typeof(S4A_Katalog).Name + ".xml");

            S4A_Zasoby zasoby = new S4A_Zasoby(souborKarty, enc);
            zasoby.serialize(outputDir + typeof(S4A_Zasoby).Name + ".xml");

            S4K_ObjVyd objednavky = new S4K_ObjVyd(cpohypob, pohypob, enc);
            objednavky.serialize(outputDir + typeof(S4K_ObjVyd).Name + ".xml");

            S4K_Prijem prijemky = new S4K_Prijem(cpohypp, pohypp, enc);
            prijemky.serialize(outputDir + typeof(S4K_Prijem).Name + ".xml");

            S4L_Faktury faktury = new S4L_Faktury(cpohypv, pohypv, enc);
            faktury.serialize(outputDir + typeof(S4L_Faktury).Name + ".xml");

        }
    }
}