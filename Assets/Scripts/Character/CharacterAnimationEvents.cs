using UnityEngine;

namespace Character
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        private AudioSource _audioSource;

        /// <summary>
        /// Gets the AudioSource component used for animation-triggered sounds.
        /// </summary>
        public AudioSource AudioSource => _audioSource;

        /// <summary>
        /// Initializes the audio source component.
        /// </summary>
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Triggered by animation events to play a left-footstep sound.
        /// </summary>
        public void FootL()
        {
            _audioSource.PlayOneShot(_audioSource.clip);
        }

        /// <summary>
        /// Triggered by animation events to play a right-footstep sound.
        /// </summary>
        public void FootR()
        {
            _audioSource.PlayOneShot(_audioSource.clip);
        }
    }
}
