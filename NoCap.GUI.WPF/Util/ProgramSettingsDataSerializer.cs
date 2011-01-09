using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Linq;
using NoCap.GUI.WPF.Runtime;
using NoCap.GUI.WPF.Settings;

namespace NoCap.GUI.WPF.Util {
    class ProgramSettingsDataSerializer {
        private readonly ExtensionManager extensionManager;

        public ProgramSettingsDataSerializer(ExtensionManager extensionManager) {
            this.extensionManager = extensionManager;
        }

        public static XmlDocument WriteToDocument(string data) {
            var document = new XmlDocument();
            document.LoadXml(data);

            return document;
        }

        private XmlObjectSerializer GetSettingsSerializer() {
            return new DataContractSerializer(
                typeof(ProgramSettingsData),
                Enumerable.Empty<Type>(),
                int.MaxValue,
                false,
                true,
                new SafeRoundTripSurrogate(),
                new ExtensionDataContractResolver(this.extensionManager)
            );
        }

        public void SerializeSettings(ProgramSettingsData settings, XmlWriter writer) {
            var serializer = GetSettingsSerializer();

            Serialize(settings, writer, serializer);
        }

        public ProgramSettingsData DeserializeSettings(XmlElement configData) {
            var deserializer = GetSettingsSerializer();

            return (ProgramSettingsData) Deserialize(configData, deserializer);
        }

        private static void Serialize(object obj, XmlWriter writer, XmlObjectSerializer serializer) {
            try {
                serializer.WriteObject(writer, obj);
            } catch (SerializationException e) {
                // TODO Error handling
                throw;
            }
        }

        private static object Deserialize(XmlElement node, XmlObjectSerializer serializer) {
            using (var xmlReader = new XmlNodeReader(node)) {
                try {
                    return serializer.ReadObject(xmlReader);
                } catch (SerializationException e) {
                    // TODO Error handling
                    throw;
                }
            }
        }

        public static T Clone<T>(T obj) {
            var cloner = new BinaryFormatter {
                Context = new StreamingContext(StreamingContextStates.Clone)
            };

            using (var stream = new MemoryStream()) {
                cloner.Serialize(stream, obj);

                stream.Position = 0;

                return (T) cloner.Deserialize(stream);
            }
        }
    }
}
