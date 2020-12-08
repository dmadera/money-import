using System.IO;
using System.Xml.Serialization;

namespace S4DataObjs {
    abstract class S4_Generic<T> {
        public abstract T GetS5Data();

        public virtual void serialize(string output) {
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new StreamWriter(output)) {
                serializer.Serialize(stream, GetS5Data());
            }
        }
    }
}