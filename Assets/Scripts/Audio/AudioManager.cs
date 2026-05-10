using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        private AudioSource _musicSource;

        /// <summary>
        /// Gets the AudioSource used for music playback.
        /// </summary>
        public AudioSource MusicSource => _musicSource;

        /// <summary>
        /// Initializes the music source component.
        /// </summary>
        private void Awake()
        {
            _musicSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Begins music playback on start.
        /// </summary>
        private void Start()
        {
            _musicSource.Play();
        }
    }
}
