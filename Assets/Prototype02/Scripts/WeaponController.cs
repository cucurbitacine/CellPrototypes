using Prototype02.Scripts.DamageSystem;
using UnityEngine;

namespace Prototype02.Scripts
{
    public class WeaponController : MonoBehaviour
    {
        public WeaponAsset WeaponAsset;

        [Space]
        public SpriteRenderer SpriteRenderer;
        public BoxCollider2D BoxCollider2D;
        public DamageSource DamageSource;

        public void SetupWeapon()
        {
            SpriteRenderer.sprite = WeaponAsset.Sprite;
            BoxCollider2D.offset = WeaponAsset.center;
            BoxCollider2D.size = WeaponAsset.size;
        }

        private void InitWeapon()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
            BoxCollider2D = GetComponent<BoxCollider2D>();
            DamageSource = GetComponent<DamageSource>();
        }
        
        private void Awake()
        {
            InitWeapon();
        }

        private void Start()
        {
            SetupWeapon();
        }

        private void OnValidate()
        {
            InitWeapon();
            
            if (SpriteRenderer != null && BoxCollider2D != null)
                SetupWeapon();
        }
    }
}