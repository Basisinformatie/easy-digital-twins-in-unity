using UnityEngine;
#if ROTTERDAM_ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Rotterdam.DigitalTwins.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        public float speed = 7.5f;
        public float jumpSpeed = 8.0f;
        public float gravity = 20.0f;
        public Transform playerCameraParent;
        public float lookSpeed = 2.0f;
        public float lookXLimit = 60.0f;

        CharacterController characterController;
        Vector3 moveDirection = Vector3.zero;
        Vector2 rotation = Vector2.zero;

        [HideInInspector]
        public bool canMove = true;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            rotation.y = transform.eulerAngles.y;
        }

        void Update()
        {
            float verticalAxis = 0f;
            float horizontalAxis = 0f;
            bool jumpPressed = false;
            float mouseX = 0f;
            float mouseY = 0f;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                verticalAxis = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
                horizontalAxis = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
                jumpPressed = Keyboard.current.spaceKey.isPressed;
            }

            if (Mouse.current != null)
            {
                mouseX = Mouse.current.delta.x.ReadValue() * 0.05f;
                mouseY = Mouse.current.delta.y.ReadValue() * 0.05f;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            verticalAxis = Input.GetAxis("Vertical");
            horizontalAxis = Input.GetAxis("Horizontal");
            jumpPressed = Input.GetButton("Jump");
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
#endif

            if (characterController.isGrounded)
            {
                // We are grounded, so recalculate move direction based on axes
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 right = transform.TransformDirection(Vector3.right);
                float curSpeedX = canMove ? speed * verticalAxis : 0;
                float curSpeedY = canMove ? speed * horizontalAxis : 0;
                moveDirection = (forward * curSpeedX) + (right * curSpeedY);

                if (jumpPressed && canMove)
                {
                    moveDirection.y = jumpSpeed;
                }
            }

            // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
            // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
            // as an acceleration (ms^-2)
            moveDirection.y -= gravity * Time.deltaTime;

            // Move the controller
            characterController.Move(moveDirection * Time.deltaTime);

            // Player and Camera rotation
            if (canMove)
            {
                rotation.y += mouseX * lookSpeed;
                rotation.x += -mouseY * lookSpeed;
                rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
                playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
                transform.eulerAngles = new Vector2(0, rotation.y);
            }
        }
    }
}