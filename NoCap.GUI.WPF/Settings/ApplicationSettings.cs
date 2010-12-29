using System;
using System.Configuration;
using System.IO;
using System.Xml;
using NoCap.GUI.WPF.Util;

namespace NoCap.GUI.WPF.Settings {
    class ApplicationSettings : ApplicationSettingsBase {
        private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static readonly string SettingsPath = Path.Combine(AppDataPath, "NoCap");

        private static readonly string MainSettingsFileName = "settings.xml";
        private static readonly string TimedSettingsFileName = "settings-{0:yyMMdd_HHmmss_ffff}.xml";

        private readonly ProgramSettingsDataSerializer serializer;

        private static XmlDocument WriteToDocument(string data) {
            var document = new XmlDocument();
            document.LoadXml(data);

            return document;
        }

        private static string ReadFromDocument(XmlDocument document) {
            return document.InnerXml;
        }

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

            var settingsString = this.serializer.SerializeSettings(value);
            var settingsDocument = WriteToDocument(settingsString);

            var fileName = GetTimedSettingsFilePath(DateTime.Now);

            using (var file = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write)) {
                settingsDocument.Save(file);
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

                return this.serializer.DeserializeSettings(ReadFromDocument(settingsDocument));
            }
        }
    }
}
