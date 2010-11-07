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
        [ThreadStatic]
        private static readonly BinaryFormatter SettingsSerializer = new BinaryFormatter();

        private ProgramSettings cachedProgramSettings;

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

        public ProgramSettings ProgramSettings {
            get {
                // TODO More aware caching (e.g. detects changes outside this class)

                if (cachedProgramSettings != null) {
                    return cachedProgramSettings;
                }

                return this.cachedProgramSettings = DeserializeSettings(ProgramSettingsData);
            }

            set {
                ProgramSettingsData = SerializeSettings(value);
                this.cachedProgramSettings = value;
            }
        }

        public static byte[] SerializeSettings(ProgramSettings settings) {
            using (var writer = new MemoryStream()) {
                try {
                    SettingsSerializer.Serialize(writer, settings);
                } catch (SerializationException e) {
                    // TODO Error handling
                    throw;
                }

                return writer.GetBuffer().Take((int) writer.Length).ToArray();
            }
        }

        public static ProgramSettings DeserializeSettings(byte[] configData) {
            if (configData == null) {
                return GetDefaultSettings();
            }

            using (var stream = new MemoryStream(configData)) {
                return (ProgramSettings) SettingsSerializer.Deserialize(stream);
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

        public static ProgramSettings CloneSettings(ProgramSettings settings) {
            return DeserializeSettings(SerializeSettings(settings));
        }
    }
}
