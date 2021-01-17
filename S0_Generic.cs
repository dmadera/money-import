using System.IO;
using System.Xml.Serialization;

namespace SDataObjs {
    static class Env {
        public static string XMLNewLine = "&#13;";
    }

    abstract class S0_Generic<T> {
        public abstract T GetS5Data();

        public virtual void serialize(string output) {
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new StreamWriter(output)) {
                serializer.Serialize(stream, GetS5Data());
            }
        }
    }
}