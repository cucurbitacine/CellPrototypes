using System;
using Prototype02.Scripts.InputSource;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype02.Scripts
{
    public class UiController : MonoBehaviour
    {
        public PlayerControllerBase Player;

        public Image healthBar;
        public Text healthValue;
        public Image staminaBar;
        public Text staminaValue;

        private void UpdateHealth(float value)
        {
            healthBar.fillAmount = value / Player.Character.Health.Max;
            healthValue.text = value.ToString("F0");
        } 
        
        private void UpdateStamina(float value)
        {
            staminaBar.fillAmount = value / Player.Character.Stamina.Max;
            staminaValue.text = value.ToString("F0");
        } 
        
        private void Awake()
        {
            Player.Character.Health.OnUpdate.AddListener(UpdateHealth);
            Player.Character.Stamina.OnUpdate.AddListener(UpdateStamina);
        }

        private void Start()
        {
            UpdateHealth(Player.Character.Health.Value);
            UpdateStamina(Player.Character.Stamina.Value);
        }
    }
}
