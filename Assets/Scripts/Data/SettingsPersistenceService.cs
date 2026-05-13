using UnityEngine;

namespace Data
{
    public class SettingsPersistenceService
    {
        public void SavePlayerSettings(PlayerSettings settings)
        {
            PlayerPrefs.SetInt(nameof(PlayerSettings.GridRadius), settings.GridRadius);
            PlayerPrefs.SetInt(nameof(PlayerSettings.VisionRadius), settings.VisionRadius);
            PlayerPrefs.SetInt(nameof(PlayerSettings.PopulationSize), settings.PopulationSize);
            PlayerPrefs.SetInt(nameof(PlayerSettings.ShowFPS), settings.ShowFPS ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void LoadPlayerSettings(PlayerSettings settings)
        {
            settings.GridRadius = PlayerPrefs.GetInt(nameof(PlayerSettings.GridRadius), 100);
            settings.VisionRadius = PlayerPrefs.GetInt(nameof(PlayerSettings.VisionRadius), 10);
            settings.PopulationSize = PlayerPrefs.GetInt(nameof(PlayerSettings.PopulationSize), 1000);
            settings.ShowFPS = PlayerPrefs.GetInt(nameof(PlayerSettings.ShowFPS), 1) == 1;
        }

        public void SaveGameSettings(GameSettings settings)
        {
            PlayerPrefs.SetInt("MasterVolume", settings.MasterVolume);
            PlayerPrefs.Save();
        }

        public void LoadGameSettings(GameSettings settings)
        {
            settings.MasterVolume = PlayerPrefs.GetInt("MasterVolume", 100);
        }

        public void SaveAll(PlayerSettings playerSettings, GameSettings gameSettings)
        {
            SavePlayerSettings(playerSettings);
            SaveGameSettings(gameSettings);
        }

        public void LoadAll(PlayerSettings playerSettings, GameSettings gameSettings)
        {
            LoadPlayerSettings(playerSettings);
            LoadGameSettings(gameSettings);
        }
    }
}
