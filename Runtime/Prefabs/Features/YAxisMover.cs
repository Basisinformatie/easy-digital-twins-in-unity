using UnityEngine;
#if ROTTERDAM_ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Rotterdam.DigitalTwins.Runtime
{
    public class YAxisMover : MonoBehaviour
    {
        [Header("Settings")]
        public float stepSize = 0.5f;
        public float holdDelay = 0.2f;
        public float continuousSpeed = 5f;

        private float nextStepTime = 0f;
        private bool isHolding = false;
        private float holdStartTime = 0f;

        void Update()
        {
            float input = GetVerticalInput();

            if (Mathf.Abs(input) > 0.1f)
            {
                if (!isHolding)
                {
                    Move(Mathf.Sign(input) * stepSize);
                    isHolding = true;
                    holdStartTime = Time.time;
                    nextStepTime = Time.time + holdDelay * 2f; 
                }
                else if (Time.time >= nextStepTime)
                {
                    Move(Mathf.Sign(input) * stepSize);
                    nextStepTime = Time.time + holdDelay;
                }
            }
            else
            {
                isHolding = false;
            }
        }

        private float GetVerticalInput()
        {
            float vertical = 0f;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.upArrowKey.isPressed) vertical += 1f;
                if (Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            }
            if (Gamepad.current != null)
            {
                vertical += Gamepad.current.dpad.y.ReadValue();
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
#else
            if (Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
#endif
            return Mathf.Clamp(vertical, -1f, 1f);
        }

        private void Move(float amount)
        {
            transform.position += new Vector3(0, amount, 0);
        }
    }
}
