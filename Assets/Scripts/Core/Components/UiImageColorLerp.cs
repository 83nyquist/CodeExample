using UnityEngine;
using UnityEngine.UI;

namespace Core.Components
{
    public class UiImageColorLerp : MonoBehaviour
    {
        private bool isActive = false;
        private bool IsEnding = false;
        private bool IsTimed = false;
        public Color StartColor;
        public Color Endcolor;

        public Image TargetImage;
        public float Speed = 2;
        public float targetTime;
    
        void Update ()
        {
            if (isActive)
            {
                TargetImage.color = Color.Lerp(StartColor, Endcolor, Mathf.PingPong(Time.time * Speed, 1));

                if (IsTimed)
                {
                    if (Time.time > targetTime)
                    {
                        StopLerp();
                    }
                }
            }
            else if (IsEnding)
            {
                TargetImage.color = Color.Lerp(TargetImage.color, StartColor, Time.time * Speed);

                if (TargetImage.color.Equals(StartColor))
                {
                    IsEnding = false;
                }
            }
        }

        public void StartLerp(float speed, float duration = 0)
        {
            Speed = speed;
            isActive = true;
            IsEnding = false;

            if (duration > 0)
            {
                IsTimed = true;
                targetTime = Time.time + duration;
            }
        }

        public void StopLerp()
        {
            isActive = false;
            IsEnding = true;
            IsTimed = false;
            TargetImage.color = StartColor;
        }
    }
}
