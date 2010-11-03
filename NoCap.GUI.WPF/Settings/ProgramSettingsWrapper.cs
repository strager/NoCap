using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    [SettingsManageability(SettingsManageability.Roaming)]
    public class ProgramSettingsWrapper : ApplicationSettingsBase {
        private readonly BinaryFormatter settingsSerializer = new BinaryFormatter();

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

                byte[] configData = ProgramSettingsData;

                if (configData == null) {
                    return this.cachedProgramSettings = GetDefaultSettings();
                } else {
                    return this.cachedProgramSettings = ReadSettingsFromConfigString(configData);
                }
            }

            set {
                using (var writer = new MemoryStream()) {
                    try {
                        this.settingsSerializer.Serialize(writer, value);
                    } catch (SerializationException e) {
                        // TODO Error handling
                        throw;
                    }

                    byte[] configData = writer.GetBuffer().Take((int) writer.Length).ToArray();

                    ProgramSettingsData = configData;
                    this.cachedProgramSettings = value;
                }
            }
        }

        private ProgramSettings ReadSettingsFromConfigString(byte[] configData) {
            using (var stream = new MemoryStream(configData)) {
                return (ProgramSettings) this.settingsSerializer.Deserialize(stream);
            }
        }

        private static ProgramSettings GetDefaultSettings() {
            var settings = new ProgramSettings();

            settings.Commands = new ObservableCollection<ICommand>(
                settings.InfoStuff.CommandFactories
                    .Where((factory) => factory.CommandFeatures.HasFlag(CommandFeatures.StandAlone))
                    .Select((factory) => factory.CreateCommand(settings.InfoStuff))
            );

            return settings;
        }
    }
}
