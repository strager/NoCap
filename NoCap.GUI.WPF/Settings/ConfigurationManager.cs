using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using NoCap.Library;

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

        public ProgramSettings LoadSettings(ExtensionManager extensionManager) {
            var data = ProgramSettingsData;

            if (data == null) {
                return GetDefaultSettings(extensionManager);
            }

            var settings = DeserializeSettings(ReadFromDocument(data));
            settings.ExtensionManager = extensionManager;

            return settings;
        }

        private static XmlObjectSerializer GetSettingsSerializer() {
            return new NetDataContractSerializer(
                StreamingContext,
                int.MaxValue,
                false,
                FormatterAssemblyStyle.Simple,
                null
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

        private static ProgramSettings GetDefaultSettings(ExtensionManager extensionManager) {
            // TODO Clean this up

            var settings = new ProgramSettings {
                ExtensionManager = extensionManager
            };

            var commandFactories = settings.InfoStuff.CommandFactories
                    .Where((factory) => factory.CommandFeatures.HasFlag(CommandFeatures.StandAlone));

            var commandFactoriesToCommands = new Dictionary<ICommandFactory, ICommand>();

            // We use two passes because command population often requires the
            // presence of other commands (which may not have been constructed yet).
            // We thus construct all commands, then populate them.
            foreach (var commandFactory in commandFactories) {
                commandFactoriesToCommands[commandFactory] = commandFactory.CreateCommand();
            }

            settings.Commands = new ObservableCollection<ICommand>(commandFactoriesToCommands.Values);

            foreach (var pair in commandFactoriesToCommands) {
                pair.Key.PopulateCommand(pair.Value, settings.InfoStuff);
            }

            return settings;
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
