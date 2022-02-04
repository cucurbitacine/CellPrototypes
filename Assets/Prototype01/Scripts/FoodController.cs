using Prototype01.Scripts.Properties;
using UnityEngine;

namespace Prototype01.Scripts
{
    [RequireComponent(typeof(HealthBehaviour))]
    public class FoodController : FoodBehaviour
    {
        public float minRadius = 0.25f;
        public float maxRadius = 0.5f;
        
        [Space]
        public Transform body;
        
        private void UpdateFood()
        {
            Body.Radius = Mathf.Lerp(minRadius, maxRadius, Health.Progress);

            if (body != null)
            {
                body.localScale = Vector3.one * (Body.Radius * 2f);
            }
        }
        
        private void Update()
        {
            UpdateFood();
            
            if (Health.Value <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void OnValidate()
        {
            Health.Value = Health.Value;

            UpdateFood();
        }
    }
}