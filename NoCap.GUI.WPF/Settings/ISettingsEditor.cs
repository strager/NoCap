﻿namespace NoCap.GUI.WPF.Settings {
    public interface ISettingsEditor {
        string DisplayName {
            get;
        }

        ProgramSettings ProgramSettings {
            get;
        }
    }
}