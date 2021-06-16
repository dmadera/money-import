using System;
using System.Text;
using System.IO;
using System.Globalization;

using SDataObjs;

namespace MainProgram {
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => input,
                _ => input[0].ToString().ToUpper() + input.Substring(1)
            };
        
        public static string RemoveDiacritics(this string input)  {
            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString) {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
            
    }

    class Program {

        static int Main(string[] args) {
            Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

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

            var fileSeznamy = Path.Combine(outputDir, "S0_Seznamy.xml");
            var fileSeznamyExists = File.Exists(fileSeznamy);
            Console.WriteLine("Generuji soubor {0}", Path.GetFileName(fileSeznamy));
            File.Copy("staticke-seznamy.xml", fileSeznamy, true);
    
            if(!fileSeznamyExists) {
                return 0;
            }

            var fileIDs = Path.Combine(outputDir, "S0_IDs.xml");
            if (!File.Exists(fileIDs)) {
                Console.WriteLine("Proveďte export S_ID z programu Money. Chybí soubor: " + fileIDs);
                return 2;
            }

            SDataObjs.S0_IDs.Deserialize(fileIDs);

            // var files = Directory.GetFiles(outputDir, "*.xml");
            // foreach (var f in files) {
            //     File.Delete(f);
            // }

            var enc = CodePagesEncodingProvider.Instance.GetEncoding("Windows-1250");

            string outputFile;

            outputFile = Path.Combine(outputDir, typeof(SDataObjs.S3_Adresar).Name + ".xml");
            Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
            SDataObjs.S3_Adresar adresar = new SDataObjs.S3_Adresar(inputDir, enc);
            adresar.serialize(outputFile);

            outputFile = Path.Combine(outputDir, typeof(SDataObjs.S3_Katalog).Name + ".xml");
            Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
            SDataObjs.S3_Katalog katalog = new SDataObjs.S3_Katalog(inputDir, enc);
            katalog.serialize(outputFile);

            if (SDataObjs.S0_IDs.IsReadyForDocs()) {
                outputFile = Path.Combine(outputDir, typeof(SDataObjs.S6_Ceny).Name + ".xml");
                Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
                SDataObjs.S6_Ceny ceny = new SDataObjs.S6_Ceny(inputDir, enc);
                ceny.serialize(outputFile);

                outputFile = Path.Combine(outputDir, typeof(SDataObjs.S6_SklDokl).Name + ".xml");
                Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
                SDataObjs.S6_SklDokl docs = new SDataObjs.S6_SklDokl(inputDir, enc);
                docs.serialize(outputFile);

                outputFile = Path.Combine(outputDir, typeof(SDataObjs.S7_Dopl).Name + ".xml");
                Console.WriteLine("Generuji soubor {0}", Path.GetFileName(outputFile));
                SDataObjs.S7_Dopl osSpoj = new SDataObjs.S7_Dopl(inputDir, enc);
                osSpoj.serialize(outputFile);
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