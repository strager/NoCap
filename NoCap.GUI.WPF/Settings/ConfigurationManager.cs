using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using Bindable.Linq;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF.Settings {
    [SettingsManageability(SettingsManageability.Roaming)]
    class ConfigurationManager : ApplicationSettingsBase {
        private static readonly StreamingContext StreamingContext =
            new StreamingContext(StreamingContextStates.File | StreamingContextStates.Persistence);

        [UserScopedSetting]
        [DefaultSettingValue(null)]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public XmlDocument ProgramSettingsData {
            get {
                return (XmlDocument) this["ProgramSettingsData"];
            }

            set {
                this["ProgramSettingsData"] = value;
            }
        }

        private static XmlDocument WriteToDocument(string data) {
            var document = new XmlDocument();
            document.LoadXml(data);

            return document;
        }

        private static string ReadFromDocument(XmlDocument document) {
            return document.InnerXml;
        }

        public void SaveSettings(ProgramSettings value) {
            ProgramSettingsData = WriteToDocument(SerializeSettings(value));

            Save();
        }

        public ProgramSettings LoadSettings() {
            var data = ProgramSettingsData;

            if (data == null) {
                return null;
            }

            return DeserializeSettings(ReadFromDocument(data));
        }

        private static XmlObjectSerializer GetSettingsSerializer() {
            return new NetDataContractSerializer(
                StreamingContext,
                int.MaxValue,
                false,
                FormatterAssemblyStyle.Simple,
                new MySurrogateSelector()
            );
        }

        public static string SerializeSettings(ProgramSettings settings) {
            var serializer = GetSettingsSerializer();

            return Serialize(settings, serializer);
        }

        public static ProgramSettings DeserializeSettings(string configData) {
            var deserializer = GetSettingsSerializer();

            return (ProgramSettings) Deserialize(configData, deserializer);
        }

        private static string Serialize(object obj, XmlObjectSerializer serializer) {
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

        private static object Deserialize(string data, XmlObjectSerializer serializer) {
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

    internal class MySurrogateSelector : ISurrogateSelector {
        private ISurrogateSelector nextSelector;

        public void ChainSelector(ISurrogateSelector selector) {
            this.nextSelector = selector;
        }

        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector) {
            if (type.IsGenericType && type.GetInterfaces().Contains(typeof(IBindableCollection))) {
                selector = this;

                return new BindableCollectionSurrogate();
            }

            if (this.nextSelector == null) {
                selector = null;

                return null;
            } else {
                return this.nextSelector.GetSurrogate(type, context, out selector);
            }
        }

        public ISurrogateSelector GetNextSelector() {
            return this.nextSelector;
        }
    }

    internal class BindableCollectionSurrogate : ISerializationSurrogate {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context) {
            var bindableCollection = (IBindableCollection) obj;

            var items = new ArrayList(bindableCollection.Count);

            foreach (var item in bindableCollection) {
                items.Add(item);
            }

            info.AddValue("Items", items);
            info.AddValue("ItemType", obj.GetType());
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) {
            var items = info.GetValue<IEnumerable>("Items");
            var type = info.GetValue<Type>("ItemType");

            dynamic bindableCollection = Activator.CreateInstance(type, new object[] { });

            foreach (var item in items) {
                bindableCollection.Add(item);
            }

            return bindableCollection;
        }
    }
}
