using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Core.Components
{
    public class ScreenFade : MonoBehaviour
    {
        private Phases Phase = Phases.Out;
        public enum Phases
        {
            Out,
            Inn
        }

        private bool _isActive = false;

        private float _timeMid;
        private float _timeEnd;

        private UnityAction _innAction;
        private UnityAction _outAction;

        private float _targetAlpha;


        private CanvasGroup _cg;
        private float _currentTime = 0f;
        private float _threshold = 0.01f;
        private float _duration;

        public Image FadeImage;
        public Text FadeText;
        public bool StartFadedOut = true;

        /// <summary>
        /// Initializes the CanvasGroup and sets initial alpha based on settings.
        /// </summary>
        void Awake()
        {
            _cg = FadeImage.GetComponent<CanvasGroup>();

            if (FadeImage == null)
            {
                FadeImage = GetComponent<Image>();
            }

            if (FadeText == null)
            {
                FadeText = GetComponentInChildren<Text>();
            }

            if (StartFadedOut)
            {
                _cg.alpha = 0;
            }
            else
            {
                _cg.alpha = 1;
            }
        }

        /// <summary>
        /// Updates the fade progress if active.
        /// </summary>
        void Update()
        {
            if (_isActive)
            {
                HandleFade();
            }
        }

        /// <summary>
        /// Interpolates the alpha value of the CanvasGroup towards the target.
        /// </summary>
        void HandleFade()
        {
            _currentTime += Time.deltaTime / _duration;
            _cg.alpha = Mathf.MoveTowards(_cg.alpha, _targetAlpha, _currentTime);

            HandleEnd();
        }

        /// <summary>
        /// Evaluates if the fade animation has reached its completion threshold.
        /// </summary>
        public void HandleEnd()
        {
            if (Phase == Phases.Out)
            {
                if (_cg.alpha >= _targetAlpha)
                {
                    OnFadedOut();
                }
            }
            else
            {
                if (_cg.alpha <= _targetAlpha)
                {
                    OnFadedInn();
                }
            }
        }

        /// <summary>
        /// Configures and starts the fade-out (to opaque) sequence.
        /// </summary>
        public void FadeOut(float dur, string text, UnityAction action = null)
        {
            Phase = Phases.Out;

            if (FadeText != null)
            {
                FadeText.text = text;
            }

            _currentTime = 0;
            _targetAlpha = 1 - _threshold;
            _duration = dur;
            _outAction = action;
            _isActive = true;
        }

        /// <summary>
        /// Finalizes the fade-out state and invokes the callback.
        /// </summary>
        public void OnFadedOut()
        {
            print("OnFadedOut");
            _isActive = false;
            _cg.alpha = 1;

            if (_outAction != null)
            {
                _outAction.Invoke();
            }
        }

        /// <summary>
        /// Configures and starts the fade-in (to transparent) sequence.
        /// </summary>
        public void FadeInn(float dur, string text, UnityAction action = null)
        {
            Phase = Phases.Inn;

            if (FadeText != null)
            {
                FadeText.text = text;
            }

            _currentTime = 0;
            _targetAlpha = 0 + _threshold;
            _duration = dur;
            _innAction = action;
            _isActive = true;
        }

        /// <summary>
        /// Finalizes the fade-in state and invokes the callback.
        /// </summary>
        public void OnFadedInn()
        {
            print("OnFadedInn");
            _isActive = false;
            _cg.alpha = 0;

            if (_innAction != null)
            {
                _innAction.Invoke();
            }
        }

        /// <summary>
        /// Performs a full screen transition: fading out, executing a mid-action, and fading in.
        /// </summary>
        public void Transition(float dur, string text, UnityAction actionMid = null, UnityAction actionEnd = null)
        {
            if (FadeText != null)
            {
                FadeText.text = text;
            }

            FadeOut(dur/2, text, () =>
            {
                if (actionMid != null)
                {
                    actionMid.Invoke();
                }

                FadeInn(dur/2, text, () =>
                {
                    if (actionEnd != null)
                    {
                        actionEnd.Invoke();
                    }
                });
            });
        }
    }
}
