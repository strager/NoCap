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

        public string SerializeSettings(ProgramSettingsData settings) {
            var serializer = GetSettingsSerializer();

            return Serialize(settings, serializer);
        }

        public ProgramSettingsData DeserializeSettings(string configData) {
            var deserializer = GetSettingsSerializer();

            return (ProgramSettingsData) Deserialize(configData, deserializer);
        }

        private string Serialize(object obj, XmlObjectSerializer serializer) {
            var output = new StringBuilder();

            var xmlSettings = new XmlWriterSettings {
                Indent = true
            };

            using (var writer = XmlWriter.Create(output, xmlSettings)) {
                try {
                    serializer.WriteObject(writer, obj);
                } catch (SerializationException e) {
                    // TODO Error handling
                    throw;
                }
            }

            return output.ToString();
        }

        private object Deserialize(string data, XmlObjectSerializer serializer) {
            using (var stringReader = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            using (var xmlReader = XmlDictionaryReader.CreateTextReader(stringReader, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null)) {
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