using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "Settings/PlayerSettings")]
    public class PlayerSettings : ScriptableObject
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridRadius = 100;
        [SerializeField] private int visionRadius = 10;

        [Header("Simulation")]
        [SerializeField] private int populationSize = 1000;
        [SerializeField] private bool showFPS = true;

        public int GridRadius
        {
            get => gridRadius;
            set
            {
                gridRadius = Mathf.Max(1, value);
                PlayerPrefs.SetInt(nameof(gridRadius), gridRadius);
                PlayerPrefs.Save();
            }
        }

        public int VisionRadius
        {
            get => visionRadius;
            set
            {
                visionRadius = Mathf.Clamp(value, 2, 20);
                PlayerPrefs.SetInt(nameof(visionRadius), visionRadius);
                PlayerPrefs.Save();
            }
        }

        public int PopulationSize
        {
            get => populationSize;
            set
            {
                populationSize = Mathf.Clamp(value, 0, 10000);
                PlayerPrefs.SetInt(nameof(populationSize), populationSize);
                PlayerPrefs.Save();
            }
        }

        public bool ShowFPS
        {
            get => showFPS;
            set
            {
                showFPS = value;
                PlayerPrefs.SetInt(nameof(showFPS), showFPS ? 1 : 0);
                PlayerPrefs.Save();
            }
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