using System;
using System.Text;
using System.IO;

using S4DataObjs;

namespace MainProgram {
    class Program {

        static int Main(string[] args) {
            Console.WriteLine("{0:yyyy-MM-dd_HH-mm-ss-fff}", DateTime.Now);

            if (args.Length != 2) {
                Console.WriteLine("Chyba vstupních argumentů. Povolený počet: 2.");
                return 1;
            }

            string inputDir = args[0];
            string outputDir = args[1];

            if (!Directory.Exists(inputDir)) {
                Console.WriteLine("Vstupní adresář neexistuje.");
                return 1;
            }

            if (!Directory.Exists(outputDir)) {
                Console.WriteLine("Výstupní adresář neexistuje.");
                return 1;
            }

            var fileIDs = Path.Combine(outputDir, "S_IDs.xml");
            if (!File.Exists(fileIDs)) {
                Console.WriteLine("Proveďte export S_ID z programu Money. Chybí soubor: " + fileIDs);
                return 2;
            }

            S4_IDs.Deserialize(fileIDs);

            var files = Directory.GetFiles(outputDir, "S4*.xml");
            foreach (var f in files) {
                File.Delete(f);
            }

            var enc = CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250");

            string outputFile;


            outputFile = Path.Combine(outputDir, typeof(S4A_Adresar).Name + ".xml");
            Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
            S4A_Adresar adresar = new S4A_Adresar(inputDir, enc);
            adresar.serialize(outputFile);

            outputFile = Path.Combine(outputDir, typeof(S4A_Katalog).Name + ".xml");
            Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
            S4A_Katalog katalog = new S4A_Katalog(inputDir, enc);
            katalog.serialize(outputFile);

            if (S4_IDs.IsReadyForDocs()) {
                outputFile = Path.Combine(outputDir, typeof(S4B_Ceny).Name + ".xml");
                Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
                S4B_Ceny ceny = new S4B_Ceny(inputDir, enc);
                ceny.serialize(outputFile);

                outputFile = Path.Combine(outputDir, typeof(S4C_ObjVyd).Name + ".xml");
                Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
                S4C_ObjVyd objednavky = new S4C_ObjVyd(inputDir, enc);
                objednavky.serialize(outputFile);

                outputFile = Path.Combine(outputDir, typeof(S4C_Nabidky).Name + ".xml");
                Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
                S4C_Nabidky nabidky = new S4C_Nabidky(inputDir, enc);
                nabidky.serialize(outputFile);

                outputFile = Path.Combine(outputDir, typeof(S4B_SklDokl).Name + ".xml");
                Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
                S4B_SklDokl docs = new S4B_SklDokl(inputDir, enc);
                docs.serialize(outputFile);
            }

            return 0;
        }

        private static bool PromptConfirmation(string confirmText) {
            Console.Write(confirmText + " [y/n] : ");
            ConsoleKey response = Console.ReadKey(false).Key;
            Console.WriteLine();
            return (response == ConsoleKey.Y);
        }
    }
}