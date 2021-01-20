using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace SDataObjs {
    abstract class S0_Generic<T> {
        public abstract T GetS5Data();

        public virtual void serialize(string output) {
            var xmlWriterSettings = new XmlWriterSettings() { Indent = true };
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = XmlWriter.Create(output, xmlWriterSettings)) {
                serializer.Serialize(stream, GetS5Data(),null, "");
            }
        }
    }
}