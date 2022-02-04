using System.Collections.Concurrent;
using Prototype01.Scripts.Control;
using Prototype01.Scripts.Properties;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Prototype01.Scripts
{
    [RequireComponent(typeof(HealthBehaviour))]
    [RequireComponent(typeof(StaminaBehaviour))]
    [RequireComponent(typeof(CellAutopilot))]
    public class CellController : CellBehaviour
    {
        public Color ColorOutLine
        {
            get => spriteOutline.color;
            set => spriteOutline.color = value;
        }
        
        public Color ColorBody
        {
            get => spriteBody.color;
            set => spriteBody.color = value;
        }
        
        private static readonly Collider2D[] Others = new Collider2D[32];
        
        private readonly ConcurrentQueue<Vector2> _targets = new ConcurrentQueue<Vector2>();
        private HealthBehaviour _health = default;
        private StaminaBehaviour _stamina = default;
        private CellAutopilot _autopilot = default;

        public LayerMask FoodLayerMask = 1; 
        
        [Space]
        public bool Moving = false;
        public bool Feeding = false;
        public bool Selected = false;

        [Space]
        
        [SerializeField] private CellGen gen = CellGen.Default;
        
        public float EnergyModificator = 1f;
        public float HealthRestoreModificator = 1f;
        public float StaminaRestoreModificator = 1f;
        
        [Space]
        public bool UseDamp = true;
        [Min(0f)]
        public float DampPower = 8f;

        [Space]
        [Min(0f)]
        public float Tolerance = 0.1f;

        [Space]
        public Transform spriteRoot;
        public SpriteRenderer spriteBody;
        public SpriteRenderer spriteOutline;
        public LineRenderer line;
        
        public FoodBehaviour TargetFood { get; private set; }
        public Vector2 Velocity { get; private set; }
        public Vector2 TargetPosition { get; private set; }
        
        public float RadiusFeeding => Body.Radius * Gen.radiusFeedingScale;
        public float Energy => Mathf.Min(Stamina.Value, Health.Value);
        
        public HealthBehaviour Health => _health != null ? _health : (_health = GetComponent<HealthBehaviour>());
        public StaminaBehaviour Stamina => _stamina != null ? _stamina : (_stamina = GetComponent<StaminaBehaviour>());
        public CellAutopilot Autopilot => _autopilot != null ? _autopilot : (_autopilot = GetComponent<CellAutopilot>());
        
        public CellGen Gen
        {
            get => gen;
            private set => gen = value;
        }
        
        #region Public API

        public void Idle()
        {
            StopMove();

            StopEat();
        }

        public void Move(Vector2 target)
        {
            ClearTargets();
            
            TargetPosition = target;
            Moving = Energy > 0f;
        }
        
        public void Move(Vector2[] targets)
        {
            if (targets == null) return;

            ClearTargets();

            AddTargets(targets);
        }

        public void StopMove()
        {
            Moving = false;
            Velocity = Vector2.zero;
            
            ClearTargets();
        }
        
        public void StartEat(FoodBehaviour food)
        {
            TargetFood = food;
        }
        
        public void StopEat()
        {
            StartEat(null);
        }
        
        public void Divide()
        {
            var prefab = PrefabList.GetPrefab<CellController>(PrefabList.Cell);

            Health.Value *= 0.5f;
            Stamina.Value *= 0.5f;

            var shift = Random.insideUnitCircle * Body.Radius;
            var cell = Instantiate(prefab, Position + shift, Quaternion.identity, transform.parent);
            cell.Entity.GroupId = Entity.GroupId;
            cell.Entity.Generation = Entity.Generation + 1;

            cell.Gen = Gen.Copy();

            if (Random.value < Gen.mutationProbability)
            {
                cell.Gen.Mutation();
            }

            cell.Body.Radius = Gen.size * 0.5f;
            
            cell.Health.Max = cell.Gen.healthMax;
            cell.Health.Value = Health.Value;
            
            cell.Stamina.Max = cell.Gen.staminaMax;
            cell.Stamina.Value = Stamina.Value;
            
            cell.ColorBody = ColorBody;
            
            cell.Autopilot.isOn = Autopilot.isOn;
        }

        public void Die()
        {
            var prefab = PrefabList.GetPrefab<FoodBehaviour>(PrefabList.Food);
            
            var food = Instantiate(prefab, Position, Quaternion.identity, transform.parent);
            food.Health.Max = Health.Max * 0.5f;
            food.Health.Value = food.Health.Max;
            food.Entity.Generation = Entity.Generation + 1;
            
            Destroy(gameObject);
        }
        
        public void SelectedBy(PlayerController player)
        {
            Selected = true;
            
            ColorOutLine = Color.gray;
        }

        public void UnselectedBy(PlayerController player)
        {
            Selected = false;
            
            ColorOutLine = Color.black;
        }

        #endregion

        #region Private API

        private void ClearTargets()
        {
            while (_targets.TryDequeue(out _))
            {
            }
        }

        private void AddTargets(params Vector2[] targets)
        {
            foreach (var target in targets)
            {
                _targets.Enqueue(target);
            }
        }

        private void RemoveEnergy(float energy)
        {
            Health.Value -= energy;
            Stamina.Value -= energy;
        }

        private bool UpdateFeeding(float deltaTime)
        {
            if (Health.Max <= Health.Value) return false;

            if (TargetFood == null) return false;

            if (TargetFood.Health.Value <= TargetFood.Health.Min) return false;
            
            var count = Physics2D.OverlapCircleNonAlloc(Position, RadiusFeeding, Others, FoodLayerMask);
            if (count == 0) return false;

            for (var i = 0; i < count; i++)
            {
                var other = Others[i];
                var food = other.gameObject.GetComponent<FoodBehaviour>();
                if (food == null) continue;
                if (food != TargetFood) continue;
                
                var amount = HealthRestoreModificator * Gen.healthRestoreSpeed * deltaTime;

                if (Health.Max < Health.Value + amount)
                {
                    amount = Health.Max - Health.Value;
                }

                if (amount < TargetFood.Health.Value)
                {
                    Health.Value += amount;
                    TargetFood.Health.Value -= amount;
                }
                else
                {
                    Health.Value += TargetFood.Health.Value;
                    TargetFood.Health.Value = 0f;
                }

                return true;
            }

            return false;
        }

        private void UpdateProperties()
        {
            Stamina.Value += StaminaRestoreModificator * Gen.staminaRestoreSpeed * Time.deltaTime;
        }

        private void UpdateBody()
        {
            if (spriteRoot != null) spriteRoot.localScale = Vector3.one * (Body.Radius * 2f);
        }

        private void DrawMovingLine()
        {
            if (line == null) return;

            var direction = TargetPosition - Position;

            line.enabled = Selected && Moving && direction.magnitude > Body.Radius;
            
            if (!line.enabled) return;
            
            line.positionCount = 2;

            if (line.useWorldSpace)
            {
                line.SetPosition(0, Position + direction.normalized * Body.Radius);
                line.SetPosition(1, TargetPosition);
            }
            else
            {
                line.SetPosition(0, line.transform.InverseTransformPoint(Position + direction.normalized * Body.Radius));
                line.SetPosition(1, line.transform.InverseTransformPoint(TargetPosition));
            }
        }

        private void HandleDeath()
        {
            if (Health.Value <= 0f)
            {
                Die();
            }
        }
        
        private Vector2 EvaluateVelocity(float deltaTime)
        {
            var velocity = Vector2.zero;
            
            if (_targets.TryPeek(out var target))
            {
                TargetPosition = target;
                Moving = Energy > 0f;
            }
            
            if (Moving)
            {
                if (Energy > 0)
                {
                    var energy = EnergyModificator * Gen.speed * deltaTime;
                    if (Energy < energy) energy = Energy;
                    
                    var direction = TargetPosition - Position;
                    var distance = Vector2.Distance(TargetPosition, Position);

                    velocity = direction.normalized * Gen.speed;

                    var nextPosition = Position + velocity * deltaTime;
                    var nextDistance = Vector2.Distance(TargetPosition, nextPosition);

                    if (Tolerance < nextDistance && nextDistance < distance)
                    {
                        if (energy > 0) RemoveEnergy(energy);
                        Velocity = velocity;
                    }
                    else
                    {
                        if (_targets.TryDequeue(out _))
                        {
                            if (_targets.Count == 0)
                            {
                                StopMove();
                                return Vector2.zero;
                            }
                        }
                        else
                        {
                            StopMove();
                            return Vector2.zero;
                        }
                    }
                }
                else
                {
                    StopMove();
                    return Vector2.zero;
                }
            }

            return velocity;
        }
        
        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            Health.Max = Gen.healthMax;
            Stamina.Max = Gen.staminaMax;

            Health.Value = Health.Value;
            Stamina.Value = Stamina.Value;

            Body.Radius = Gen.size * 0.5f;
            Body.Rigidbody.mass = Gen.mass;
        }

        private void Start()
        {
            MetricController.Add(this);
            
            Idle();
        }

        private void Update()
        {
            Feeding = UpdateFeeding(Time.deltaTime);

            UpdateProperties();
            
            UpdateBody();

            DrawMovingLine();

            HandleDeath();
        }

        private void FixedUpdate()
        {
            Velocity = EvaluateVelocity(Time.fixedDeltaTime);
            
            Body.Rigidbody.velocity = UseDamp
                ? Vector2.Lerp(Body.Rigidbody.velocity, Velocity, DampPower * Time.fixedDeltaTime)
                : Velocity;
        }

        private void OnEnable()
        {
            //MetricController.Add(this);
        }
        
        private void OnDestroy()
        {
            MetricController.Remove(this);
        }

        private void OnValidate()
        {
            UpdateBody();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(Position, Body.Radius);

            if (Moving && _targets.Count > 1)
            {
                var points = _targets.ToArray();
                for (var i = 0; i < points.Length - 1; i++)
                {
                    Gizmos.DrawLine(points[i], points[i + 1]);
                }

                Gizmos.DrawWireSphere(points[points.Length - 1], Body.Radius);
            }

            if (Moving)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(Position, TargetPosition);
            }

            Gizmos.color = Feeding ? Color.magenta : Color.gray;
            Gizmos.DrawWireSphere(Position, RadiusFeeding);
        }

        #endregion
    }
}