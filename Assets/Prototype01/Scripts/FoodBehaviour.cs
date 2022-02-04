using Prototype01.Scripts.Properties;
using UnityEngine;

namespace Prototype01.Scripts
{
    [RequireComponent(typeof(HealthBehaviour))]
    public class FoodBehaviour : CellBehaviour
    {
        private HealthBehaviour _health;

        public HealthBehaviour Health => _health != null ? _health : (_health = GetComponent<HealthBehaviour>());
    }
}