using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace SDataObjs {

    public class XmlEnv {
        public static string NewLine = "&#xD;&#xA;";
    }
    abstract class S0_Generic<T> {
        public abstract T GetS5Data();

        public virtual void serialize(string output) {
            var xmlWriterSettings = new XmlWriterSettings() { 
                Indent = true,
                CheckCharacters = false,
            };
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = XmlWriter.Create(output, xmlWriterSettings)) {
                serializer.Serialize(stream, GetS5Data(),null, "");
            }
            
            string text = File.ReadAllText(output);
            text = text.Replace("&amp;#xD;&amp;#xA;" , "&#xD;&#xA;");
            File.WriteAllText(output, text);
        }
    }
}