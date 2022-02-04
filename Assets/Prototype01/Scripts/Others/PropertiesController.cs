using UnityEngine;
using UnityEngine.UI;

namespace Prototype01.Scripts.Others
{
    public class PropertiesController : MonoBehaviour
    {
        public CellController Cell;

        public bool UseDamp = true;
        public float DampPower = 8f;
        
        public Canvas Canvas;
        public Image HealthBar;
        public Image StaminaBar;

        private void Awake()
        {
            Canvas.worldCamera = Camera.main;
        }

        private void Update()
        {
            if (Cell != null)
            {
                if (HealthBar != null)
                {
                    HealthBar.rectTransform.localPosition = Vector3.up * (Cell.Body.Radius + HealthBar.rectTransform.sizeDelta.y);
                    HealthBar.rectTransform.sizeDelta = new Vector2(Cell.Body.Radius * 2f, HealthBar.rectTransform.sizeDelta.y);
                    
                    var fillAmount = Cell.Health.Progress;
                    HealthBar.fillAmount = UseDamp
                        ? Mathf.Lerp(HealthBar.fillAmount, fillAmount, DampPower * Time.deltaTime)
                        : fillAmount;
                }

                if (StaminaBar != null)
                {
                    StaminaBar.rectTransform.localPosition = -Vector3.up * (Cell.Body.Radius + StaminaBar.rectTransform.rect.height);
                    StaminaBar.rectTransform.sizeDelta = new Vector2(Cell.Body.Radius * 2f, StaminaBar.rectTransform.sizeDelta.y);
                    
                    var fillAmount = Cell.Stamina.Progress;
                    StaminaBar.fillAmount = UseDamp
                        ? Mathf.Lerp(StaminaBar.fillAmount, fillAmount, DampPower * Time.deltaTime)
                        : fillAmount;
                }
            }
        }
    }
}
