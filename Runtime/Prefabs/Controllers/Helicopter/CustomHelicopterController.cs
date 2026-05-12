using UnityEngine;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Rotterdam.DigitalTwins.Runtime
{
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
    public KeyCode turnLeftKey = KeyCode.Q;
    public KeyCode turnRightKey = KeyCode.E;
    public KeyCode strafeLeftKey = KeyCode.Z;
    public KeyCode strafeRightKey = KeyCode.C;

    [Header("Strafe Settings")]
    public float strafeForce = 20f;

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
            liftUp = Keyboard.current.spaceKey.isPressed;
            liftDown = Keyboard.current.leftShiftKey.isPressed;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        liftUp = Input.GetKey(liftUpKey);
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
                forward = Keyboard.current.wKey.isPressed;
                backward = Keyboard.current.sKey.isPressed;
                left = Keyboard.current.aKey.isPressed;
                right = Keyboard.current.dKey.isPressed;
                turnLeft = Keyboard.current.qKey.isPressed;
                turnRight = Keyboard.current.eKey.isPressed;
                strafeLeft = Keyboard.current.zKey.isPressed;
                strafeRight = Keyboard.current.cKey.isPressed;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            forward = Input.GetKey(forwardKey);
            backward = Input.GetKey(backwardKey);
            left = Input.GetKey(leftKey);
            right = Input.GetKey(rightKey);
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
        hTilt.x = Mathf.Lerp(hTilt.x, hMove.x * turnTiltForce, Time.deltaTime);
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
}
}
