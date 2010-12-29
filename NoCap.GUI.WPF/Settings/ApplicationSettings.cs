using System;
using System.IO;
using System.Text;
using System.Xml;
using NoCap.GUI.WPF.Util;

namespace NoCap.GUI.WPF.Settings {
    class ApplicationSettings {
        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static readonly string SettingsPath = Path.Combine(AppDataPath, "NoCap");

        private static readonly string MainSettingsFileName = "settings.xml";
        private static readonly string TimedSettingsFileName = "settings-{0:yyMMdd_HHmmss_ffff}.xml";

        private readonly ProgramSettingsDataSerializer serializer;

        private static void EnsureSettingsPathExists() {
            Directory.CreateDirectory(SettingsPath);
        }

        private static string GetMainSettingsFilePath() {
            return Path.Combine(SettingsPath, MainSettingsFileName);
        }

        private static string GetTimedSettingsFilePath(DateTime date) {
            return Path.Combine(SettingsPath, string.Format(TimedSettingsFileName, date.ToUniversalTime()));
        }

        public ApplicationSettings(ProgramSettingsDataSerializer serializer) {
            this.serializer = serializer;
        }

        public void SaveSettingsData(ProgramSettingsData value) {
            EnsureSettingsPathExists();

            var fileName = GetTimedSettingsFilePath(DateTime.Now);
            
            var xmlSettings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                CloseOutput = true,
                ConformanceLevel = ConformanceLevel.Document,
            };

            using (var xmlWriter = XmlWriter.Create(fileName, xmlSettings)) {
                this.serializer.SerializeSettings(value, xmlWriter);
            }

            File.Copy(fileName, GetMainSettingsFilePath(), true);
        }

        public ProgramSettingsData LoadSettingsData() {
            EnsureSettingsPathExists();

            var fileName = GetMainSettingsFilePath();

            if (!File.Exists(fileName)) {
                return null;
            }

            using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                var settingsDocument = new XmlDocument();
                settingsDocument.Load(file);

                return this.serializer.DeserializeSettings(settingsDocument.DocumentElement);
            }
        }
    }
}
