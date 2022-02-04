using UnityEngine;
using UnityEngine.Events;

namespace Prototype02.Scripts.DamageSystem
{
    public class DamageReceiver : MonoBehaviour
    {
        public bool IsEnabled = true;
        
        public UnityEvent<DamageInfo> OnDamage { get; private set; } = new UnityEvent<DamageInfo>();

        public Vector2 Position => transform.position;
        
        public void Damage(DamageInfo damageInfo)
        {
            if (IsEnabled) OnDamage.Invoke(damageInfo);
        }
    }
}