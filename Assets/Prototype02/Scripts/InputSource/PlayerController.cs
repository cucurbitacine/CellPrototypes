using UnityEngine;

namespace Prototype02.Scripts.InputSource
{
    public class PlayerController : PlayerControllerBase
    {
        [Space]
        public bool useMouse;

        [Space]
        public Camera Camera;

        private void Awake()
        {
            if (Camera == null) Camera = Camera.main;
        }

        protected override void UpdateInput(ref InputData input)
        {
            if (useMouse)
            {
                var target = (Vector2) Camera.ScreenToWorldPoint(Input.mousePosition);

                var dir = target - Character.Position;

                if (Input.GetAxis("Fire1") > 0.5f || Input.GetAxis("Fire2") > 0.5f)
                {
                    if (dir.magnitude > 0.5f)
                    {
                        input.move = dir.normalized;
                        input.view = input.move;
                    }
                    else
                    {
                        input.move = Vector2.zero;
                        input.view = dir.normalized;
                    }
                }
                else
                {
                    input.move = Vector2.zero;
                    input.view = Vector2.zero;
                }


                input.attack = Input.GetAxis("Jump") > 0.5f;
                input.jump = Input.GetAxis("Fire2") > 0.5f;
                input.cancel = Input.GetAxis("Cancel") > 0.5f;
            }
            else
            {
                input.move = Vector2.zero;

                if (Input.GetKey(KeyCode.W)) input.move.y += 1f;
                if (Input.GetKey(KeyCode.A)) input.move.x += -1f;
                if (Input.GetKey(KeyCode.S)) input.move.y += -1f;
                if (Input.GetKey(KeyCode.D)) input.move.x += 1f;

                input.view = input.move;

                input.attack = Input.GetAxis("Fire1") > 0.5f;
                input.jump = Input.GetAxis("Jump") > 0.5f;
                input.cancel = Input.GetAxis("Cancel") > 0.5f;
            }
        }
    }
}