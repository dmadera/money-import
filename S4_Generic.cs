using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using SkladData;

namespace S4DataObjs {
    abstract class S4_Generic<T, T1> {

        protected List<T> _data = new List<T>();

        protected Predicate<T> _filter = delegate (T a) {
            return true;
        };

        public abstract T1 GetS5Data();

        public void serialize(string output) {
            var serializer = new XmlSerializer(typeof(T1));
            using (var stream = new StreamWriter(output)) {
                serializer.Serialize(stream, GetS5Data());
            }
        }

        protected SkladDataObj find(SkladDataObj[] data, string column, string value) {
            foreach (SkladDataObj obj in data) {
                if (obj.Items[column].GetAlfaNum() == value) {
                    return obj;
                }
            }
            throw new InvalidDataException(string.Format(
                "Nebyla nalezena hodnota {0} ve sloupci {1}",
                value, column
            ));
        }
    }
}