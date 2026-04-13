using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Rotterdam.DigitalTwins.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public float walkingSpeed = 7.5f;
        public float runningSpeed = 11.5f;
        public float jumpSpeed = 8.0f;
        public float gravity = 20.0f;
        public Camera playerCamera;
        public float lookSpeed = 2.0f;
        public float lookXLimit = 45.0f;

        CharacterController characterController;
        Vector3 moveDirection = Vector3.zero;
        float rotationX = 0;

        [HideInInspector]
        public bool canMove = true;

        void Start()
        {
            characterController = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            bool isRunning = false;
            float verticalAxis = 0f;
            float horizontalAxis = 0f;
            bool jumpPressed = false;
            float mouseX = 0f;
            float mouseY = 0f;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                isRunning = Keyboard.current.leftShiftKey.isPressed;
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
        isRunning = Input.GetKey(KeyCode.LeftShift);
        verticalAxis = Input.GetAxis("Vertical");
        horizontalAxis = Input.GetAxis("Horizontal");
        jumpPressed = Input.GetButton("Jump");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
#endif

            float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * verticalAxis : 0;
            float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * horizontalAxis : 0;
            float movementDirectionY = moveDirection.y;
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            if (jumpPressed && canMove && characterController.isGrounded)
            {
                moveDirection.y = jumpSpeed;
            }
            else
            {
                moveDirection.y = movementDirectionY;
            }

            if (!characterController.isGrounded)
            {
                // Gravity is multiplied over time
                moveDirection.y -= gravity * Time.deltaTime;
            }

            characterController.Move(moveDirection * Time.deltaTime);

            if (canMove)
            {
                rotationX += -mouseY * lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, mouseX * lookSpeed, 0);
            }
        }
    }
}