using UnityEngine;

namespace Prototype02.Scripts.InputSource
{
    public abstract class PlayerControllerBase : MonoBehaviour
    {
        
        [SerializeField] private CharacterController character = default;
        [Space]
        [SerializeField] private InputData input = default;

        public CharacterController Character => character;

        protected abstract void UpdateInput(ref InputData input);

        protected virtual void LateUpdate()
        {
            if (Character == null) return;

            input = default;
            UpdateInput(ref input);
            Character.Input = input;
        }
    }
}