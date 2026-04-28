namespace Core.Components
{
    public static class Easer
    {
        public enum Types
        {
            Linear,
            quadraticIn
        }

        public static float Linear(float time, float startValue, float changeInValue, float duration)
        {
            return changeInValue * time / duration + startValue;
        }
    }
}
