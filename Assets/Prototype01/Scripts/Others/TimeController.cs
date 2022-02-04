using UnityEngine;
using UnityEngine.UI;

namespace Prototype01.Scripts.Others
{
    public class TimeController : MonoBehaviour
    {
        public const float MinTimeScale = 0.01f;
        public const float MaxTimeScale = 32f;

        public Slider TimeSlider;
        public Button ResetTime;

        private void Awake()
        {
            if (TimeSlider != null)
            {
                TimeSlider.minValue = MinTimeScale;
                TimeSlider.maxValue = MaxTimeScale;
                TimeSlider.value = Time.timeScale;

                if (ResetTime != null) ResetTime.onClick.AddListener(() => TimeSlider.value = 1f);
            }
        }

        private void Update()
        {
            if (TimeSlider != null)
            {
                Time.timeScale = TimeSlider.value;
            }
        }
    }
}