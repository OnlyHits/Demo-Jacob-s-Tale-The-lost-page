using System;

namespace CustomArchitecture
{
    public enum Language
    {
        French = 0,
        English = 1,
    }

    [System.Serializable]
    public class SettingDatas
    {
        public float m_musicVolume = 1f;
        public float m_effectVolume = 1f;
        public Language m_language = Language.French;
    }

    public class Settings
    {
        public SettingDatas m_settingDatas = null;
        private readonly SaveUtilitary<SettingDatas> m_saveUtilitary;

        public Settings()
        {
            m_saveUtilitary = new SaveUtilitary<SettingDatas>("SettingDatas", FileType.SaveFile);

            m_settingDatas = new();
            m_saveUtilitary.Save(m_settingDatas);

            m_settingDatas = m_saveUtilitary.Load();
        }
    }
}