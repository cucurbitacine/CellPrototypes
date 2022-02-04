using UnityEngine;

namespace Prototype01.Scripts.Others
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CircleBody : MonoBehaviour
    {
        private CircleCollider2D _collider;
        private Rigidbody2D _rigidbody2D;
        
        [Min(0f)]
        [SerializeField] private float radius = 0.5f;
        
        public CircleCollider2D Collider => _collider != null ? _collider : (_collider = GetComponent<CircleCollider2D>());
        public Rigidbody2D Rigidbody => _rigidbody2D != null ? _rigidbody2D : (_rigidbody2D = GetComponent<Rigidbody2D>());

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        private void Init()
        {
            Rigidbody.bodyType = RigidbodyType2D.Dynamic;
            Rigidbody.gravityScale = 0f;
            Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            Collider.radius = Radius;
            Collider.isTrigger = false;
        }
        
        private void UpdateBody()
        {
            Collider.radius = Radius;
        }

        private void Awake()
        {
            Init();
        }

        private void Update()
        {
            UpdateBody();
        }
        
        private void OnValidate()
        {
            Init();
            
            UpdateBody();
        }
    }
}