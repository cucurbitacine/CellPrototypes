using UnityEngine;
using UnityEngine.Events;

namespace Prototype02.Scripts.Values
{
    public abstract class BaseValue<T> : MonoBehaviour
    {
        public abstract T Value { get; set; }
        public abstract T Min { get; set; }
        public abstract T Max { get; set; }
        
        public abstract T Range { get; }
        public abstract T Length { get; }
        public abstract float Progress { get; }

        public abstract UnityEvent<T> OnUpdate { get; }
    }
    
    public abstract class FloatValue : BaseValue<float>
    {
        [SerializeField] private float value = 100;
        [SerializeField] private float max = 100;
        [Space]
        [SerializeField] private UnityEvent<float> onUpdate = default;
        
        public override float Value
        {
            get => value;
            set
            {
                var newValue = Mathf.Clamp(value, Min, Max);
                if (Mathf.Abs(newValue - this.value) > float.Epsilon)
                {
                    this.value = newValue;
                    OnUpdate.Invoke(newValue);
                }
            }
        }

        public override float Min
        {
            get => 0f;
            set { }
        }

        public override float Max
        {
            get => max;
            set => max = Mathf.Max(Min, value);
        }
        
        public override float Range => Max - Min;
        public override float Length => Value - Min;
        public override float Progress => Length / Range;

        public override UnityEvent<float> OnUpdate => onUpdate != null ? onUpdate : (onUpdate = new UnityEvent<float>());

        private void OnValidate()
        {
            max = Mathf.Max(Min, Max);
            value = Mathf.Clamp(Value, Min, Max);
        }
    }
    
    public abstract class IntValue : BaseValue<int>
    {
        [SerializeField] private int value = 100;
        [SerializeField] private int max = 100;
        [Space]
        [SerializeField] private UnityEvent<int> onUpdate = default;
        
        public override int Value
        {
            get => value;
            set
            {
                var newValue = Mathf.Clamp(value, Min, Max);
                if (newValue == this.value) return;
                this.value = newValue;
                OnUpdate.Invoke(newValue);
            }
        }
        
        public override int Min
        {
            get => 0;
            set { }
        }

        public override int Max
        {
            get => max;
            set => max = Mathf.Max(Min, value);
        }

        public override int Range => Max - Min;
        public override int Length => Value - Min;
        public override float Progress => (float) Length / Range;

        public override UnityEvent<int> OnUpdate => onUpdate != null ? onUpdate : (onUpdate = new UnityEvent<int>());
        
        private void OnValidate()
        {
            max = Mathf.Max(Min, Max);
            value = Mathf.Clamp(Value, Min, Max);
        }
    }
}