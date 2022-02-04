using System.Collections.Generic;
using Prototype02.Scripts.DamageSystem;
using UnityEngine;

namespace Prototype02.Scripts.Values
{
    public class DamagableHealth : MonoBehaviour
    {
        public HealthFloat Health;
        public DamageReceiver receiver;

        public void RecieveDamage(DamageInfo damageInfo)
        {
            if (Health.Value > 0f)
            {
                Health.Value -= damageInfo.damageCount;
                
                BloodEffect.Play(transform.position, (transform.position - damageInfo.damageSource.transform.position).normalized);

                if (Health.Value <= 0)
                {
                    receiver.IsEnabled = false;
                    
                    Destroy(gameObject);
                }
            }
        }

        private void Awake()
        {
            receiver.OnDamage.AddListener(RecieveDamage);
        }
    }
    
    public class BloodEffect
    {
        private static BloodEffect Instance { get; }
        
        static BloodEffect()
        {
            Instance = new BloodEffect();
        }

        private BloodEffect()
        {
            _prefab = Resources.Load<ParticleSystem>("Blood");
        }
        
        private readonly List<ParticleSystem> _list = new List<ParticleSystem>();
        private readonly ParticleSystem _prefab = null;

        public static void Play(Vector2 position, Vector2 direction)
        {
            Instance.PlayInternal(position, direction);    
        }
        
        private void PlayInternal(Vector2 position, Vector2 direction)
        {
            for (var i = 0; i < _list.Count; i++)
            {
                if (_list[i].isPlaying) continue;
                _list[i].transform.position = position;
                _list[i].transform.up = direction;
                _list[i].Play();
                return;
            }

            var effect = Object.Instantiate(_prefab);
            effect.transform.position = position;
            effect.transform.up = direction;
            effect.Play();
            _list.Add(effect);
        }
    }
}