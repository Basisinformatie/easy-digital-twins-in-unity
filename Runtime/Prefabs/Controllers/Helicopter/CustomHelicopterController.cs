using UnityEngine;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Rotterdam.DigitalTwins.Runtime
{
    /// <summary>
    /// Controller for helicopter flight physics and input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CustomHelicopterController : MonoBehaviour
    {
    [Header("Rotors")]
    public CustomRotor mainRotor;
    public CustomRotor tailRotor;
    public float maxRotorSpeed = 700f;
    public float mainRotorMultiplier = 80f;
    public float tailRotorMultiplier = -40f;

    [Header("Engine Settings")]
    public float engineIncreaseSpeed = 5f;
    public float engineDecreaseSpeed = 20f; 
    public float maxEngineForce = 33f;
    public float minAirEngineForce = 4f;
    public float engineForce = 0f;

    [Header("Physics Settings")]
    public float forwardForce = 40f;
    public float forwardTiltForce = 10f;
    public float turnForce = 10f;
    public float turnTiltForce = 30f;
    public float effectiveHeight = 500f;

    public float turnTiltForcePercent = 1.5f;
    public float turnForcePercent = 10f;

    [Header("Input Keys")]
    public KeyCode liftUpKey = KeyCode.Space;
    public KeyCode liftDownKey = KeyCode.LeftShift;
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode turnLeftKey = KeyCode.Z;
    public KeyCode turnRightKey = KeyCode.C;
    public KeyCode strafeLeftKey = KeyCode.Q;
    public KeyCode strafeRightKey = KeyCode.E;

    [Header("Gamepad Buttons")]
    public string liftUpButton = "buttonSouth";
    public string liftDownButton = "buttonEast";
    public string forwardAxis = "leftStick/y";
    public string backwardAxis = "leftStick/y";
    public string leftAxis = "leftStick/x";
    public string rightAxis = "leftStick/x";
    public string turnLeftButton = "leftShoulder";
    public string turnRightButton = "rightShoulder";
    public string strafeLeftAxis = "rightStick/x";
    public string strafeRightAxis = "rightStick/x";

    [Header("Strafe Settings")]
    public float strafeForce = 20f;
    public float strafeTiltForce = 20f;

    private Rigidbody rb;
    private Vector2 hMove = Vector2.zero;
    private Vector2 hTilt = Vector2.zero;
    private float hTurn = 0f;
    private float hStrafe = 0f;
    public bool isOnGround = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        HandleEngine();
        HandleInput();
        LiftProcess();
        MoveProcess();
        TiltProcess();
        UpdateRotors();
    }

    private void HandleEngine()
    {
        bool liftUp = false;
        bool liftDown = false;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var liftUpControl = Keyboard.current[KeyFromKeyCode(liftUpKey)];
            var liftDownControl = Keyboard.current[KeyFromKeyCode(liftDownKey)];
            if (liftUpControl != null) liftUp = liftUpControl.isPressed;
            if (liftDownControl != null) liftDown = liftDownControl.isPressed;
        }

        if (Gamepad.current != null)
        {
            var liftUpControl = Gamepad.current[liftUpButton] as ButtonControl;
            var liftDownControl = Gamepad.current[liftDownButton] as ButtonControl;
            if (liftUpControl != null && liftUpControl.isPressed) liftUp = true;
            if (liftDownControl != null && liftDownControl.isPressed) liftDown = true;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        liftUp = Input.GetKey(liftUpKey) || Input.GetButton("Jump");
        liftDown = Input.GetKey(liftDownKey);
#endif

        if (liftUp)
            engineForce += engineIncreaseSpeed * Time.fixedDeltaTime;
        else if (liftDown)
            engineForce -= engineDecreaseSpeed * Time.fixedDeltaTime;

        float minForce = isOnGround ? 0f : minAirEngineForce;
        engineForce = Mathf.Clamp(engineForce, minForce, maxEngineForce);
    }

    private void HandleInput()
    {
        float tempY = 0;
        float tempX = 0;
        float tempStrafe = 0;

        if (hMove.y > 0)
            tempY = -Time.fixedDeltaTime;
        else if (hMove.y < 0)
            tempY = Time.fixedDeltaTime;

        if (hMove.x > 0)
            tempX = -Time.fixedDeltaTime;
        else if (hMove.x < 0)
            tempX = Time.fixedDeltaTime;

        if (hStrafe > 0)
            tempStrafe = -Time.fixedDeltaTime;
        else if (hStrafe < 0)
            tempStrafe = Time.fixedDeltaTime;

        if (!isOnGround)
        {
            bool forward = false;
            bool backward = false;
            bool left = false;
            bool right = false;
            bool turnLeft = false;
            bool turnRight = false;
            bool strafeLeft = false;
            bool strafeRight = false;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                forward = Keyboard.current[KeyFromKeyCode(forwardKey)].isPressed;
                backward = Keyboard.current[KeyFromKeyCode(backwardKey)].isPressed;
                left = Keyboard.current[KeyFromKeyCode(leftKey)].isPressed;
                right = Keyboard.current[KeyFromKeyCode(rightKey)].isPressed;
                turnLeft = Keyboard.current[KeyFromKeyCode(turnLeftKey)].isPressed;
                turnRight = Keyboard.current[KeyFromKeyCode(turnRightKey)].isPressed;
                strafeLeft = Keyboard.current[KeyFromKeyCode(strafeLeftKey)].isPressed;
                strafeRight = Keyboard.current[KeyFromKeyCode(strafeRightKey)].isPressed;
            }

            if (Gamepad.current != null)
            {
                float fwdValue = GetGamepadAxis(forwardAxis);
                float sideValue = GetGamepadAxis(leftAxis);
                float strafeValue = GetGamepadAxis(strafeLeftAxis);

                if (fwdValue > 0.1f) forward = true;
                if (fwdValue < -0.1f) backward = true;
                if (sideValue < -0.1f) left = true;
                if (sideValue > 0.1f) right = true;
                if (strafeValue < -0.1f) strafeLeft = true;
                if (strafeValue > 0.1f) strafeRight = true;

                var tLeftControl = Gamepad.current[turnLeftButton] as ButtonControl;
                var tRightControl = Gamepad.current[turnRightButton] as ButtonControl;
                if (tLeftControl != null && tLeftControl.isPressed) turnLeft = true;
                if (tRightControl != null && tRightControl.isPressed) turnRight = true;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            forward = Input.GetKey(forwardKey) || Input.GetAxis("Vertical") > 0.1f;
            backward = Input.GetKey(backwardKey) || Input.GetAxis("Vertical") < -0.1f;
            left = Input.GetKey(leftKey) || Input.GetAxis("Horizontal") < -0.1f;
            right = Input.GetKey(rightKey) || Input.GetAxis("Horizontal") > 0.1f;
            turnLeft = Input.GetKey(turnLeftKey);
            turnRight = Input.GetKey(turnRightKey);
            strafeLeft = Input.GetKey(strafeLeftKey);
            strafeRight = Input.GetKey(strafeRightKey);
#endif

            if (forward) tempY = Time.fixedDeltaTime;
            else if (backward) tempY = -Time.fixedDeltaTime;

            if (left) tempX = -Time.fixedDeltaTime;
            else if (right) tempX = Time.fixedDeltaTime;

            if (strafeLeft) tempStrafe = -Time.fixedDeltaTime;
            else if (strafeRight) tempStrafe = Time.fixedDeltaTime;

            if (turnRight)
            {
                float force = (turnForcePercent - Mathf.Abs(hMove.y)) * rb.mass;
                rb.AddRelativeTorque(0f, force, 0);
            }
            else if (turnLeft)
            {
                float force = -(turnForcePercent - Mathf.Abs(hMove.y)) * rb.mass;
                rb.AddRelativeTorque(0f, force, 0);
            }
        }

        hMove.x += tempX;
        hMove.x = Mathf.Clamp(hMove.x, -1, 1);

        hMove.y += tempY;
        hMove.y = Mathf.Clamp(hMove.y, -1, 1);

        hStrafe += tempStrafe;
        hStrafe = Mathf.Clamp(hStrafe, -1, 1);
    }

    private void LiftProcess()
    {
        float upForceFactor = 1 - Mathf.Clamp(transform.position.y / effectiveHeight, 0, 1);
        float totalLift = Mathf.Lerp(0f, engineForce, upForceFactor) * rb.mass;
        rb.AddRelativeForce(Vector3.up * totalLift);
    }

    private void MoveProcess()
    {
        var turn = turnForce * Mathf.Lerp(hMove.x, hMove.x * (turnTiltForcePercent - Mathf.Abs(hMove.y)), Mathf.Max(0f, hMove.y));
        hTurn = Mathf.Lerp(hTurn, turn, Time.fixedDeltaTime * turnForce);
        rb.AddRelativeTorque(0f, hTurn * rb.mass, 0f);
        rb.AddRelativeForce(Vector3.forward * Mathf.Max(0f, hMove.y * forwardForce * rb.mass));
        rb.AddRelativeForce(Vector3.right * hStrafe * strafeForce * rb.mass);
    }

    private void TiltProcess()
    {
        hTilt.x = Mathf.Lerp(hTilt.x, hMove.x * turnTiltForce + hStrafe * strafeTiltForce, Time.deltaTime);
        hTilt.y = Mathf.Lerp(hTilt.y, hMove.y * forwardTiltForce, Time.deltaTime);
        transform.localRotation = Quaternion.Euler(hTilt.y, transform.localEulerAngles.y, -hTilt.x);
    }

    private void UpdateRotors()
    {
        if (mainRotor != null)
            mainRotor.rotationSpeed = Mathf.Clamp(engineForce * mainRotorMultiplier, -maxRotorSpeed, maxRotorSpeed);

        if (tailRotor != null)
            tailRotor.rotationSpeed = Mathf.Clamp(engineForce * tailRotorMultiplier, -maxRotorSpeed, maxRotorSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isOnGround = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isOnGround = false;
    }

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
    private Key KeyFromKeyCode(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.A: return Key.A;
            case KeyCode.B: return Key.B;
            case KeyCode.C: return Key.C;
            case KeyCode.D: return Key.D;
            case KeyCode.E: return Key.E;
            case KeyCode.F: return Key.F;
            case KeyCode.G: return Key.G;
            case KeyCode.H: return Key.H;
            case KeyCode.I: return Key.I;
            case KeyCode.J: return Key.J;
            case KeyCode.K: return Key.K;
            case KeyCode.L: return Key.L;
            case KeyCode.M: return Key.M;
            case KeyCode.N: return Key.N;
            case KeyCode.O: return Key.O;
            case KeyCode.P: return Key.P;
            case KeyCode.Q: return Key.Q;
            case KeyCode.R: return Key.R;
            case KeyCode.S: return Key.S;
            case KeyCode.T: return Key.T;
            case KeyCode.U: return Key.U;
            case KeyCode.V: return Key.V;
            case KeyCode.W: return Key.W;
            case KeyCode.X: return Key.X;
            case KeyCode.Y: return Key.Y;
            case KeyCode.Z: return Key.Z;
            case KeyCode.Alpha0: return Key.Digit0;
            case KeyCode.Alpha1: return Key.Digit1;
            case KeyCode.Alpha2: return Key.Digit2;
            case KeyCode.Alpha3: return Key.Digit3;
            case KeyCode.Alpha4: return Key.Digit4;
            case KeyCode.Alpha5: return Key.Digit5;
            case KeyCode.Alpha6: return Key.Digit6;
            case KeyCode.Alpha7: return Key.Digit7;
            case KeyCode.Alpha8: return Key.Digit8;
            case KeyCode.Alpha9: return Key.Digit9;
            case KeyCode.Space: return Key.Space;
            case KeyCode.LeftShift: return Key.LeftShift;
            case KeyCode.RightShift: return Key.RightShift;
            case KeyCode.LeftControl: return Key.LeftCtrl;
            case KeyCode.RightControl: return Key.RightCtrl;
            case KeyCode.LeftAlt: return Key.LeftAlt;
            case KeyCode.RightAlt: return Key.RightAlt;
            case KeyCode.Tab: return Key.Tab;
            case KeyCode.Return: return Key.Enter;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.Backspace: return Key.Backspace;
            case KeyCode.Delete: return Key.Delete;
            case KeyCode.UpArrow: return Key.UpArrow;
            case KeyCode.DownArrow: return Key.DownArrow;
            case KeyCode.LeftArrow: return Key.LeftArrow;
            case KeyCode.RightArrow: return Key.RightArrow;
            default: return Key.None;
        }
    }

    private float GetGamepadAxis(string path)
    {
        if (Gamepad.current == null) return 0f;
        var control = Gamepad.current[path];
        if (control is AxisControl axis) return axis.ReadValue();
        return 0f;
    }
#endif
}
}
