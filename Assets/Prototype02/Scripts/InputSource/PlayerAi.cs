using System;
using Prototype02.Scripts.DamageSystem;
using UnityEngine;

namespace Prototype02.Scripts.InputSource
{
    public class PlayerAi : PlayerControllerBase
    {
        public DamageReceiver Target;
        public float Distance;

        public bool wasAttack;

        public DamageReceiver Self;

        private void Awake()
        {
            Self = Character.GetComponent<DamageReceiver>();
        }

        protected override void UpdateInput(ref InputData input)
        {
            if (Character.IsAttacking) return;

            Distance = float.MaxValue;

            if (Target == null || Target.IsEnabled)
            {
                Target = null;
                var colliders = Physics2D.OverlapCircleAll(Character.Position, 10f);
                foreach (var cld in colliders)
                {
                    var target = cld.GetComponent<DamageReceiver>();
                    if (target == null) continue;
                    if (target == Self) continue;
                    if (!target.IsEnabled) continue;

                    var distance = Vector2.Distance(target.Position, Character.Position);

                    if (distance < Distance)
                    {
                        Distance = distance;
                        Target = target;
                    }
                }
            }

            if (Target == null) return;

            var direction = Target.Position - Character.Position;
            Distance = direction.magnitude;

            input.move = direction.normalized;
            input.view = input.move;

            var needAttack = Distance < Character.AttackDistance /** 0.333f*/;

            input.attack = !wasAttack && needAttack;

            wasAttack = input.attack;
        }
    }
}