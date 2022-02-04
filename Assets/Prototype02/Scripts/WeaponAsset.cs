using System;
using UnityEngine;

namespace Prototype02.Scripts
{
    [CreateAssetMenu(fileName = nameof(WeaponAsset), menuName = "Weapon/Create")]
    public class WeaponAsset : ScriptableObject
    {
        public Sprite Sprite;

        public Vector2 center;
        public Vector2 size;
        
        private void OnValidate()
        {
            if (Sprite != null)
            {
                center = Sprite.bounds.center;
                size = Sprite.bounds.size;
            }
        }
    }
}