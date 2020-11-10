using System.Xml.Serialization;
using System.IO;
using Schemas;

namespace sklad_data_parser {

    static class Global {
        static public bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }
    }

    static class S5DataIDs {
        public static string FirmaTypSpojeniEmailID = "dc748a02-43ed-4cc5-8264-4994b56919d0";
        public static string FirmaTypSpojeniTelID = "dc748a02-43ed-4cc5-8264-4994b56919d0";
        public static string FirmaTypSpojeniEmailFaID = "a26c0533-f56f-49f3-a3c7-911ea1613648";
        public static string FirmaGroupALLID = "d9667a95-9864-4990-b64c-06140e3b47c9";
        public static string FirmaGroupNPID = "afae82e8-afba-498c-9d70-9a2737b48b95";
        public static string FirmaGroupMALOOBCHODID = "1feb45f2-40a1-46eb-855f-fdaa080c44c3";
        public static string FirmaStatCZID = "3d3f235c-df25-42ad-9cce-1b460e3a3c5f";
    }
    class Program {

        static void Main(string[] args) {
            string cestaOdb = @"/home/dmadera/projects/sklad-data-parser/assets/headODB";

            var data = new S5Data() {
                FirmaList = FirmaDataBridge.ConvertToGetS5Data(cestaOdb)
            };
            var serializer = new XmlSerializer(typeof(S5Data));
            using (var stream = new StreamWriter("/home/dmadera/downloads/output.xml")) {
                serializer.Serialize(stream, data);
            }
        }
    }
}
