using System;
using UnityEngine;

namespace Prototype01.Scripts.Properties
{
    public class PropertyBehaviour : MonoBehaviour
    {
        [SerializeField] private float value = 100;
        [SerializeField] private float max = 100;

        public float Value
        {
            get => value;
            set => this.value = Mathf.Clamp(value, Min, Max);
        }

        public float Min => 0f;

        public float Max
        {
            get => max;
            set => max = Math.Max(Min, value);
        }
        
        public float Range => Max - Min;
        public float Length => Value - Min;
        public float Progress => Length / Range;
    }
}