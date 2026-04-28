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

        //public static ScreenFade Instance = null;
        //void Awake()
        //{
        //    //Check if instance already exists
        //    if (Instance == null)
        //    {
        //        //if not, set instance to this
        //        Instance = this;
        //    }

        //    //If instance already exists and it's not this:
        //    else if (Instance != this)
        //    {
        //        //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance present.
        //        Debug.LogError("Destroy GameObject: More than one Instance");
        //        Destroy(gameObject);
        //    }
        //}

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

        void Update()
        {
            if (_isActive)
            {
                HandleFade();
            }
        }

        void HandleFade()
        {
            _currentTime += Time.deltaTime / _duration;
            _cg.alpha = Mathf.MoveTowards(_cg.alpha, _targetAlpha, _currentTime);

            HandleEnd();
        }

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
