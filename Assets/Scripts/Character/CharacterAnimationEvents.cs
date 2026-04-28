using UnityEngine;

namespace Character
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        private AudioSource _audioSource;
        public AudioSource AudioSource => _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void FootL()
        {
            _audioSource.PlayOneShot(_audioSource.clip);
        }

        public void FootR()
        {
            _audioSource.PlayOneShot(_audioSource.clip);
        }
    }
}
