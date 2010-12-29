using System.Configuration;
using System.Xml;
using NoCap.GUI.WPF.Util;

namespace NoCap.GUI.WPF.Settings {
    [SettingsManageability(SettingsManageability.Roaming)]
    class ApplicationSettings : ApplicationSettingsBase {
        private ProgramSettingsDataSerializer serializer;

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

        public ApplicationSettings(ProgramSettingsDataSerializer serializer) {
            this.serializer = serializer;
        }

        public void SaveSettingsData(ProgramSettingsData value) {
            ProgramSettingsData = WriteToDocument(this.serializer.SerializeSettings(value));

            Save();
        }

        public ProgramSettingsData LoadSettingsData() {
            var data = ProgramSettingsData;

            if (data == null) {
                return null;
            }

            return this.serializer.DeserializeSettings(ReadFromDocument(data));
        }
    }
}
