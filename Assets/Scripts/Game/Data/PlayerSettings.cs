using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "Settings/PlayerSettings")]
    public class PlayerSettings : ScriptableObject
    {
        [Header("Grid Settings")]
        public int gridRadius = 100;
        public int visionRadius = 10;

        [Header("Simulation")]
        public int populationSize = 1000;
        public bool showFPS = true;

        private void OnEnable()
        {
            Load();
        }

        private void OnValidate()
        {
            Save();
        }

        public void Save()
        {
            PlayerPrefs.SetInt(nameof(gridRadius), gridRadius);
            PlayerPrefs.SetInt(nameof(visionRadius), visionRadius);
            PlayerPrefs.SetInt(nameof(populationSize), populationSize);
            PlayerPrefs.SetInt(nameof(showFPS), showFPS ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            gridRadius = PlayerPrefs.GetInt(nameof(gridRadius), 100);
            visionRadius = PlayerPrefs.GetInt(nameof(visionRadius), 10);
            populationSize = PlayerPrefs.GetInt(nameof(populationSize), 1000);
            showFPS = PlayerPrefs.GetInt(nameof(showFPS), 1) == 1;
        }
    }
}