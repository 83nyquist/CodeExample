using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Audio Settings")]
        [Range(0, 100)]
        [SerializeField] private int masterVolume = 100;

        public int MasterVolume
        {
            get => masterVolume;
            set => masterVolume = Mathf.Clamp(value, 0, 100);
        }
    }
}
