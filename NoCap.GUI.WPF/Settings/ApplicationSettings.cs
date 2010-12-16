using System.Configuration;
using System.Xml;
using NoCap.GUI.WPF.Util;

namespace NoCap.GUI.WPF.Settings {
    [SettingsManageability(SettingsManageability.Roaming)]
    class ApplicationSettings : ApplicationSettingsBase {
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

        public void SaveSettingsData(ProgramSettingsData value) {
            ProgramSettingsData = WriteToDocument(ProgramSettingsDataSerializer.SerializeSettings(value));

            Save();
        }

        public ProgramSettingsData LoadSettingsData() {
            var data = ProgramSettingsData;

            if (data == null) {
                return null;
            }

            return ProgramSettingsDataSerializer.DeserializeSettings(ReadFromDocument(data));
        }
    }
}
