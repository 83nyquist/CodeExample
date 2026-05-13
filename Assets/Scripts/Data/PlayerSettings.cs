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
            set => gridRadius = Mathf.Max(1, value);
        }

        public int VisionRadius
        {
            get => visionRadius;
            set => visionRadius = Mathf.Clamp(value, 2, 20);
        }

        public int PopulationSize
        {
            get => populationSize;
            set => populationSize = Mathf.Clamp(value, 0, 10000);
        }

        public bool ShowFPS
        {
            get => showFPS;
            set => showFPS = value;
        }
    }
}
