using UnityEngine;

namespace Game
{
    public static class GameSettings
    {
        public static int MasterVolume
        {
            get => PlayerPrefs.GetInt("MasterVolume", 100);  // 0-100 integer
            set
            {
                PlayerPrefs.SetInt("MasterVolume", value);
                AudioListener.volume = value / 100f;  // Convert only when applying
                PlayerPrefs.Save();
            }
        }
        
        public static int GridRadius
        {
            get => PlayerPrefs.GetInt("GridRadius", 100);
            set
            {
                PlayerPrefs.SetInt("GridRadius", value);
                PlayerPrefs.Save();
            }
        }
    
        public static int VisionRadius
        {
            get => PlayerPrefs.GetInt("VisionRadius", 10);
            set
            {
                PlayerPrefs.SetInt("VisionRadius", value);
                PlayerPrefs.Save();
            }
        }
    
        public static int PopulationSize
        {
            get => PlayerPrefs.GetInt("PopulationSize", 1000);
            set
            {
                PlayerPrefs.SetInt("PopulationSize", value);
                PlayerPrefs.Save();
            }
        }
    
        public static bool ShowFPS
        {
            get => PlayerPrefs.GetInt("ShowFPS", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("ShowFPS", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}