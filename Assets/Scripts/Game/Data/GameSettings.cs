using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        private const string MasterVolumeKey = "MasterVolume";

        [Header("Audio Settings")]
        [Range(0, 100)]
        [SerializeField] private int masterVolume = 100;

        public int MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp(value, 0, 100);
                Save();
            }
        }

        private void OnEnable()
        {
            // Load from PlayerPrefs when the asset is initialized
            masterVolume = PlayerPrefs.GetInt(MasterVolumeKey, 100);
            ApplySettings();
        }

        private void OnValidate()
        {
            // Save and apply changes immediately when edited in Inspector
            Save();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(MasterVolumeKey, masterVolume);
            PlayerPrefs.Save();
            ApplySettings();
        }

        private void ApplySettings()
        {
            AudioListener.volume = masterVolume / 100f;
        }
    }
}