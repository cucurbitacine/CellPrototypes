using UnityEngine.Events;

namespace Prototype02.Scripts.Values
{
    public class HealthFloat : FloatValue
    {
        public UnityEvent OnEmpty { get; private set; } = new UnityEvent();

        private void Awake()
        {
            OnUpdate.AddListener(_ =>
            {
                if (Value <= 0f) OnEmpty.Invoke();
            });
        }
    }
}