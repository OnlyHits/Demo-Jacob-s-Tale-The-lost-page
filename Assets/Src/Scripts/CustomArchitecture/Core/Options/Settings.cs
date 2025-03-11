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
        private SettingDatas m_settingDatas = null;
        private readonly SaveUtilitary<SettingDatas> m_saveUtilitary;

        public Settings()
        {
            m_saveUtilitary = new SaveUtilitary<SettingDatas>("SettingDatas", FileType.SaveFile);

            m_settingDatas = new();
            m_settingDatas = m_saveUtilitary.Load();
        }

        public float MusicVolume
        {
            get { return m_settingDatas.m_musicVolume; }
            protected set { }
        }

        public float SoundEffectVolume
        {
            get { return m_settingDatas.m_effectVolume; }
            protected set { }
        }

        public Language Language
        {
            get { return m_settingDatas.m_language; }
            protected set { }
        }

        public void SetMusicVolume(float volume)
        {
            m_settingDatas.m_musicVolume = volume;
            m_saveUtilitary.Save(m_settingDatas);
        }

        public void SetSoundEffectVolume(float volume)
        {
            m_settingDatas.m_effectVolume = volume;
            m_saveUtilitary.Save(m_settingDatas);
        }

        public void SetLanguage(Language language)
        {
            m_settingDatas.m_language = language;
            m_saveUtilitary.Save(m_settingDatas);
        }
    }
}