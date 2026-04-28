using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        private AudioSource _musicSource;
        public AudioSource MusicSource => _musicSource;

        private void Awake()
        {
            _musicSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            _musicSource.Play();
        }
    }
}
