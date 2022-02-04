using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Prototype01.Scripts.Control
{
    [DisallowMultipleComponent]
    public class CellAutopilot : CellBehaviour
    {
        private int _count = 0;
        private static readonly Collider2D[] Foods = new Collider2D[32];

        public bool isOn = false;
        public CellAIState state = CellAIState.FreeWalk;
        
        [Space]
        [SerializeField] private CellController cell = default;
        
        [Space]
        public Vector2 target;

        [Space]
        [SerializeField] private FreeWalkSetting freeWalkSetting = default;

        public FoodBehaviour targetFood { get; private set; }

        public CellController Cell
        {
            get => cell != null ? cell : (Cell = GetComponent<CellController>());
            set => cell = value;
        }
        
        private void Restore()
        {
            if (Cell.Gen.staminaLevelFreeWalk * Cell.Stamina.Max <= Cell.Stamina.Value)
            {
                state = CellAIState.FreeWalk;
                FreeWalkSetting.offset = Random.insideUnitCircle;
                return;
            }
        }
        
        private void FreeWalk()
        {
            if (Cell.Gen.healthLevelDivide * Cell.Health.Max <= Cell.Health.Value)
            {
                Cell.Divide();
            }

            targetFood = null;
            if (Cell.Health.Value <= Cell.Gen.healthLevelSearchFood * Cell.Health.Max)
            {
                _count = Physics2D.OverlapCircleNonAlloc(Cell.Position, Cell.Body.Radius * Cell.Gen.radiusScaleSearchFood, Foods, Cell.FoodLayerMask);
                for (var i = 0; i < _count; i++)
                {
                    var food = Foods[i].GetComponent<FoodBehaviour>();
                    if (food == null) continue;
                    if (targetFood == null)
                    {
                        targetFood = food;
                    }
                    else
                    {
                        if (Vector2.Distance(Position, food.Position) < Vector2.Distance(Position, targetFood.Position))
                        {
                            targetFood = food;
                        }
                    }
                }

                if (targetFood != null)
                {
                    state = CellAIState.MoveToFood;
                    return;
                }
            }

            if (Cell.Stamina.Value <= Cell.Gen.staminaLevelRestore * Cell.Stamina.Max)
            {
                state = CellAIState.Restore;
                return;
            }

            target = FreeWalkSetting.GetDirection() * (2 * Cell.Body.Radius) + Cell.Position;
            Cell.Move(target);
            
            FreeWalkSetting.UpdateArg(Time.deltaTime);
        }

        private void MoveToFood()
        {
            if (Cell.Feeding)
            {
                state = CellAIState.Feeding;
                return;
            }

            _count = Physics2D.OverlapCircleNonAlloc(Cell.Position, Cell.Body.Radius * Cell.Gen.radiusScaleSearchFood, Foods, Cell.FoodLayerMask);
            for (var i = 0; i < _count; i++)
            {
                var food = Foods[i].GetComponent<FoodBehaviour>();
                if (food == null) continue;
                if (targetFood == null)
                {
                    targetFood = food;
                }
                else
                {
                    if (Vector2.Distance(Position, food.Position) < Vector2.Distance(Position, targetFood.Position))
                    {
                        targetFood = food;
                    }
                }
            }

            if (targetFood == null)
            {
                state = CellAIState.FreeWalk;
                return;
            }
            
            if (targetFood.Health.Value <= targetFood.Health.Min)
            {
                targetFood = null;
                state = CellAIState.FreeWalk;
                return;
            }
            
            Cell.StartEat(targetFood);
            Cell.Move(targetFood.Position);
        }
        
        private void Feeding()
        {
            if (!Cell.Feeding)
            {
                targetFood = null;
                state = CellAIState.FreeWalk;
                return;
            }
        }
        
        private void Start()
        {
            FreeWalkSetting.NewSeed();
        }

        private void Update()
        {
            if (!isOn) return;
            
            if(state == CellAIState.Restore) Restore();
            if(state == CellAIState.FreeWalk) FreeWalk();
            if(state == CellAIState.MoveToFood) MoveToFood();
            if(state == CellAIState.Feeding) Feeding();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(Cell.Position, target);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, Cell.Body.Radius * 0.5f);

            if (state == CellAIState.FreeWalk)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Cell.Position, Cell.Body.Radius * Cell.Gen.radiusScaleSearchFood);
            }
        }
        
        public FreeWalkSetting FreeWalkSetting => freeWalkSetting != null ? freeWalkSetting : (freeWalkSetting = new FreeWalkSetting());
    }

    [Serializable]
    public class FreeWalkSetting
    {
        public Vector2 offset = Vector2.zero;
        
        public Vector2 seed;
        
        [Space]
        public Vector2 argClear;
        public Vector2 argSpeed = Vector2.one;
        
        [Space]
        public Vector2 scale = Vector2.one;

        public Vector2 GetArg()
        {
            return Vector2.Scale(argClear, scale);
        }

        public Vector2 GetDirection()
        {
            var arg = GetArg();

            return (new Vector2(
                        Mathf.PerlinNoise(arg.x, 0) - Mathf.PerlinNoise(0, arg.x),
                        Mathf.PerlinNoise(0, arg.y) - Mathf.PerlinNoise(arg.y, 0)
                    ).normalized + offset.normalized).normalized;
        }
        
        public void NewSeed()
        {
            seed = Random.insideUnitCircle.normalized * (Random.value * 1024);
            argClear = seed;
        }

        public void UpdateArg(float deltaTime)
        {
            argClear += argSpeed * deltaTime;
        }
    }

    public enum CellAIState
    {
        Restore,
        FreeWalk,
        MoveToFood,
        Feeding,
    }
}