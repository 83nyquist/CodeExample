using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        private const string MasterVolumeKey = "MasterVolume";

        [Header("Audio Settings")]
        [Range(0, 100)]
        [SerializeField] private int masterVolume = 100;

        /// <summary> Gets or sets the master audio volume (0-100). </summary>
        public int MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp(value, 0, 100);
                PlayerPrefs.SetInt(MasterVolumeKey, masterVolume);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Loads settings from PlayerPrefs.
        /// </summary>
        public void Load()
        {
            masterVolume = PlayerPrefs.GetInt(MasterVolumeKey, 100);
        }
    }
}