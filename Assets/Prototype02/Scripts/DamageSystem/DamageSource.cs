using UnityEngine;

namespace Prototype02.Scripts.DamageSystem
{
    public class DamageSource : MonoBehaviour
    {
        public bool IsEnabled = true;
        public int DamageCount = 10;

        public void SendDamage(DamageReceiver receiver)
        {
            if (receiver == null) return;

            var damageInfo = new DamageInfo();
            damageInfo.damageCount = DamageCount;
            damageInfo.damageSource = this;

            receiver.Damage(damageInfo);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsEnabled) SendDamage(other.GetComponent<DamageReceiver>());
        }
    }
}