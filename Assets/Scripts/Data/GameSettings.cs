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

        public void Load()
        {
            masterVolume = PlayerPrefs.GetInt(MasterVolumeKey, 100);
        }
    }
}