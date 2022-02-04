using System;
using System.Collections;
using Prototype02.Scripts.Values;
using UnityEngine;

namespace Prototype02.Scripts
{
    public class CharacterController : MonoBehaviour
    {
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Attack1 = Animator.StringToHash("Attack1");
        private static readonly int AnimationLayer = 0;
        
        public const float VEpsilon = 0.01f;
        
        private bool _wasAttack = false;
        private bool _wasJump = false;
        private Coroutine _attacking = default;
        private Coroutine _jumping = default;

        public InputData Input = default;
        
        [Space]
        public bool IsMoving = default;
        public bool IsAttacking = default;
        public bool IsJumping = default;
        public Vector2 Velocity = default;
        
        [Space]
        public float SpeedMax = 4f;
        
        public float DampMove = 8f;
        public float DampOther = 8f;
        
        public float AttackStaminaCost = 50f;
        public float AttackDistance = 5f;
        public float AttackDuration = 0.2f;
        public float SleepAfterAttack = 1f;

        public float JumpStaminaCost = 25f;
        public float JumpDistance = 5f;
        public float JumpDuration = 0.2f;
        public float SleepAfterJump = 0.1f;
        
        [Space]
        public Rigidbody2D Rigidbody = default;
        public Animator Animator = default;
        
        public Vector2 Position => transform.position;

        public Vector2 VelocityMove { get; private set; }
        public Vector2 VelocityAttack { get; private set; }
        public Vector2 VelocityJump { get; private set; }
        public Vector2 VelocityOther { get; private set; }
        public Vector2 Direction { get; private set; }

        public HealthFloat Health; 
        public StaminaFloat Stamina; 
        public float StaminaRecovery = 10f; 
        
        private void HandleInput(float deltaTime)
        {
            if (Input.view.magnitude > 0) Direction = Input.view.normalized;
            
            VelocityMove =  Input.move.normalized * SpeedMax;
            if (IsAttacking || IsJumping) VelocityMove = Vector2.zero;
            IsMoving = VelocityMove.magnitude > VEpsilon;

            VelocityOther = Vector2.Lerp(VelocityOther, Vector2.zero, DampOther * deltaTime);
            if (VelocityOther.magnitude < VEpsilon) VelocityOther = Vector2.zero;

            var needAttack = Input.attack;
            if (!_wasAttack && needAttack) Attack();
            _wasAttack = needAttack;
            
            var needJump = Input.jump;
            if (!_wasJump && needJump) Jump();
            _wasJump = needJump;
        }

        private void HandlePhysics(float deltaTime)
        {
            Velocity = VelocityMove + VelocityOther + VelocityAttack + VelocityJump;
            
            Rigidbody.velocity = Vector2.Lerp(Rigidbody.velocity, Velocity, DampMove * deltaTime);
            if (!IsAttacking && !IsJumping) Rigidbody.SetRotation(Quaternion.LookRotation(Vector3.forward, Direction));
        }
        
        private void Attack()
        {
            if (IsAttacking || IsJumping) return;
            
            Rigidbody.SetRotation(Quaternion.LookRotation(Vector3.forward, Direction));
            
            if (Stamina.Value < AttackStaminaCost) return;
            Stamina.Value -= AttackStaminaCost;
            
            if (_attacking != null) StopCoroutine(_attacking);
            _attacking = StartCoroutine(_Attack());
        }

        private IEnumerator _Attack()
        {
            IsAttacking = true;
            
            Animator.Play(Attack1, AnimationLayer);
            
            yield return new WaitForEndOfFrame();
            
            AttackDuration = Animator.GetCurrentAnimatorStateInfo(AnimationLayer).length;
            
            VelocityAttack = Direction * AttackDistance / AttackDuration;

             yield return new WaitForSeconds(AttackDuration);
            
            VelocityAttack = Vector2.zero;
            
            yield return new WaitForSeconds(SleepAfterAttack);
            
            IsAttacking = false;
            
            Animator.Play(Idle, AnimationLayer);
        }

        private void Jump()
        {
            if (IsJumping || IsAttacking) return;
            
            Rigidbody.SetRotation(Quaternion.LookRotation(Vector3.forward, Direction));

            if (Stamina.Value < JumpStaminaCost) return;
            Stamina.Value -= JumpStaminaCost;
            
            if (_jumping != null) StopCoroutine(_jumping);
            _jumping = StartCoroutine(_Jump());
        }

        private IEnumerator _Jump()
        {
            IsJumping = true;
            
            VelocityJump = Direction * JumpDistance / JumpDuration;

            yield return new WaitForSeconds(JumpDuration);
            
            VelocityJump = Vector2.zero;
            
            yield return new WaitForSeconds(SleepAfterJump);
            
            IsJumping = false;
        }
        
        private void Awake()
        {
            Direction = transform.up;
        }
        
        private void Update()
        {
            HandleInput(Time.deltaTime);

            Stamina.Value += StaminaRecovery * Time.deltaTime;
        }

        private void FixedUpdate()
        {
            HandlePhysics(Time.fixedDeltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) Direction = transform.up;

            Gizmos.DrawLine(Position, Position + Direction * AttackDistance);
            for (var i = 0; i < 5; i++)
            {
                var distance = (i + 1f) * AttackDistance / 5f;
                Gizmos.DrawWireSphere(Position + Direction * distance, 0.5f);
            } 
            
        }
    }

    [Serializable]
    public struct InputData
    {
        public Vector2 move;
        public Vector2 view;

        public bool attack;
        public bool jump;
        public bool cancel;
    }
}