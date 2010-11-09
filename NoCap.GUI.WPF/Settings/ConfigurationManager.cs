using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    [SettingsManageability(SettingsManageability.Roaming)]
    public class ConfigurationManager : ApplicationSettingsBase {
        private static readonly StreamingContext StreamingContext =
            new StreamingContext(StreamingContextStates.File | StreamingContextStates.Persistence);

        [UserScopedSetting]
        [DefaultSettingValue(null)]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        public byte[] ProgramSettingsData {
            get {
                return (byte[]) this["ProgramSettingsData"];
            }

            set {
                this["ProgramSettingsData"] = value;
            }
        }

        public void SaveSettings(ProgramSettings value) {
            ProgramSettingsData = SerializeSettings(value);

            Save();
        }

        public ProgramSettings LoadSettings() {
            byte[] data = ProgramSettingsData;

            if (data == null) {
                return GetDefaultSettings();
            }

            return DeserializeSettings(data);
        }

        public static byte[] SerializeSettings(ProgramSettings settings) {
            var serializer = new BinaryFormatter {
                Context = StreamingContext
            };

            return Serialize(settings, serializer);
        }

        private static byte[] Serialize(object obj, IFormatter formatter) {
            using (var writer = new MemoryStream()) {
                try {
                    formatter.Serialize(writer, obj);
                } catch (SerializationException e) {
                    // TODO Error handling
                    throw;
                }

                return writer.GetBuffer().Take((int) writer.Length).ToArray();
            }
        }

        public static ProgramSettings DeserializeSettings(byte[] configData) {
            var deserializer = new BinaryFormatter {
                Context = StreamingContext
            };

            return (ProgramSettings) Deserialize(configData, deserializer);
        }

        private static object Deserialize(byte[] data, IFormatter formatter) {
            using (var stream = new MemoryStream(data)) {
                return formatter.Deserialize(stream);
            }
        }

        private static ProgramSettings GetDefaultSettings() {
            // TODO Clean this up

            var settings = new ProgramSettings();

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

            return (T) Deserialize(Serialize(obj, cloner), cloner);
        }
    }
}
