using UnityEngine;

#if ROTTERDAM_ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Rotterdam.DigitalTwins.Runtime
{
    public class CustomCarController : MonoBehaviour
    {
    [Header("Motor Settings")]
    [SerializeField, Min(0f)] private float _maxForwardSpeedKPH = 180f;
    [SerializeField, Min(0f)] private float _maxBackwardSpeedKPH = 60f;
    [SerializeField, Min(0f)] private float _maxMotorTorque = 300f;
    [SerializeField, Min(0f)] private float _minMotorFrictionTorque = 15f;
    [SerializeField, Min(0f)] private float _maxMotorFrictionTorque = 75f;
    [SerializeField, Min(0.001f)] private float _motorInertia = 0.1f;
    [SerializeField, Min(0f)] private float _finalGearRatio = 8f;

    [Header("Input Settings")]
    [SerializeField, Min(0.001f)] private float _steerTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _steerReleaseTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _throttleTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _throttleReleaseTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _brakeTime = 0.1f;
    [SerializeField, Min(0.001f)] private float _brakeReleaseTime = 0.1f;

    [Header("Driving Assists")]
    [SerializeField] private bool _steerLimitByFriction = false;
    [SerializeField, Min(0f)] private float _steerMu = 2f;
    [SerializeField] private bool _autoShiftToReverse = true;
    [SerializeField, Min(0f)] private float _switchToReverseSpeedKPH = 1f;

    [Header("Car Settings")]
    [SerializeField] private Wheel[] _steerableWheels;
    [SerializeField, Min(0f)] private float _maxSteerAngle = 30f;
    [SerializeField, Min(0f)] private float _maxTurnSpeed = 60f;
    [SerializeField, Min(0f)] private float _peakFrictionSlipAngle = 5f;
    [SerializeField, Min(0f)] private float _mu = 2f;
    [SerializeField] private bool _useAddTorque = false;
    [SerializeField, Min(0.001f)] private float _wheelRadius = 0.3f;
    [SerializeField] private float _centerOfMassHeight = 0.3f;
    [SerializeField, Min(0f)] private float _maxBrakeTorque = 1000f;
    [SerializeField, Min(0f)] private float _rollingResistanceCoef = 0.015f;
    [SerializeField, Min(0f)] private float _airResistanceCoef = 1.5f;
    [SerializeField, Min(0f)] private float _downforceCoef = 0f;
    [SerializeField, Range(0f, 1f)] private float _airResistanceReduction = 0f;
    [SerializeField] private bool _autoAdjustSuspension = true;
    [SerializeField, Min(0.001f)] private float _suspensionStroke = 0.1f;
    [SerializeField, Min(0f)] private float _suspensionNaturalFrequency = 2f;
    [SerializeField, Range(0f, 1f)] private float _suspensionDampingRatio = 0.35f;
    [SerializeField] private float _addForceOffset = -0.1f;

    private Rigidbody _rigidbody;
    private Wheel[] _wheels;
    private float _wheelbase;
    private float _steerInput;
    private float _throttleInput;
    private float _brakeInput;
    private float _angularVelocity;
    private Vector3 _groundNormal;
    private Vector3 _groundForward;
    private Vector3 _groundSideways;
    private float _forwardSpeed;
    private float _sidewaysSpeed;
    private float _speed;
    private float _normalForce;
    private Vector3 _addForcePosition;
    private float _slipAngle;
    private float _tiltAngle;
    private Vector3 _totalForce;

    private float _maxMotorForwardRPM;
    private float _maxMotorBackwardRPM;
    private float _motorRPM;
    private bool _reverse;

    private const float MPSToKPH = 3.6f;
    private const float KPHToMPS = 1f / 3.6f;
    private const float RPSToRPM = 30f / Mathf.PI;
    private const float RPMToRPS = 1f / (30f / Mathf.PI);

    public bool Reverse
    {
        get => _reverse;
        set => _reverse = value;
    }

    public float MotorRevolutionRate => _motorRPM / Mathf.Max(_maxMotorForwardRPM, _maxMotorBackwardRPM);
    public float MotorRPM => _motorRPM;
    public float MaxSpeedKPH => Mathf.Max(_maxForwardSpeedKPH, _maxBackwardSpeedKPH);

    private bool IsExceedMaxMotorRPM
    {
        get
        {
            var maxRPM = _reverse ? _maxMotorBackwardRPM : _maxMotorForwardRPM;
            var rpm = Mathf.Abs(_motorRPM);
            return rpm > maxRPM;
        }
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _wheels = GetComponentsInChildren<Wheel>();
        
        if (_wheels == null || _wheels.Length == 0)
        {
            Debug.LogError("[CustomCarController] No Wheel components found in children!");
        }

        CalcWheelbase();
        AdjustCenterOfMass();

        if (_autoAdjustSuspension)
        {
            AdjustSuspension();
        }

        _maxMotorForwardRPM = CalcMotorRPMFromSpeedKPH(_maxForwardSpeedKPH);
        _maxMotorBackwardRPM = CalcMotorRPMFromSpeedKPH(_maxBackwardSpeedKPH);
    }

    private void Update()
    {
        UpdateInput();
    }

    private void FixedUpdate()
    {
        _groundNormal = Vector3.zero;
        _groundForward = Vector3.zero;
        _groundSideways = Vector3.zero;
        _forwardSpeed = 0f;
        _sidewaysSpeed = 0f;
        _slipAngle = 0f;
        _normalForce = 0f;
        _addForcePosition = Vector3.zero;
        _totalForce = Vector3.zero;

        _speed = _rigidbody.linearVelocity.magnitude;

        UpdateSteerAngle();
        AddAirResistanceForce();
        AddDownforce();

        if (IsGrounded())
        {
            _groundNormal = GetGroundNormal();
            _groundForward = Vector3.ProjectOnPlane(transform.forward, _groundNormal).normalized;
            _groundSideways = Vector3.ProjectOnPlane(transform.right, _groundNormal).normalized;

            _forwardSpeed = Vector3.Dot(_rigidbody.linearVelocity, _groundForward);
            _sidewaysSpeed = Vector3.Dot(_rigidbody.linearVelocity, _groundSideways);

            var denom = Mathf.Max(Mathf.Abs(_forwardSpeed), 1f);
            _slipAngle = Mathf.Atan2(_sidewaysSpeed, denom) * Mathf.Rad2Deg;
            _tiltAngle = Vector3.Angle(_groundNormal, transform.up);

            _normalForce = _rigidbody.mass * Physics.gravity.magnitude;
            _addForcePosition = _rigidbody.worldCenterOfMass + transform.up * _addForceOffset;

            Turn();
            AddFrictionForce();
            AddRollingResistanceForce();
            AddBrakeForce();
        }

        AddDriveTorqueLogic();
    }

    private void UpdateInput()
    {
        UpdateSteerInput();
        UpdateThrottleAndBrakeInput();
    }

    private void UpdateSteerInput()
    {
        var maxSteerInput = 1f;
        if (_steerLimitByFriction)
        {
            var speed = _speed;
            var minTurnR = (speed * speed) / (_steerMu * Physics.gravity.magnitude);
            if (minTurnR > 0f)
            {
                var optimalSteerAngle = Mathf.Asin(_wheelbase / minTurnR) * Mathf.Rad2Deg;
                maxSteerInput = Mathf.Min(optimalSteerAngle / _maxSteerAngle, 1f);
            }
        }

        var steerInput = GetRawSteerInput();
        steerInput = Mathf.Clamp(steerInput, -maxSteerInput, maxSteerInput);

        var steerTime = steerInput != 0f ? _steerTime : _steerReleaseTime;

        if (steerInput != 0f && Mathf.Sign(steerInput) != Mathf.Sign(_steerInput))
        {
            _steerInput = 0f;
        }

        _steerInput = Mathf.MoveTowards(_steerInput, steerInput, Time.deltaTime / steerTime);
    }

    private void UpdateThrottleAndBrakeInput()
    {
        var rawThrottle = GetRawThrottleInput();
        var rawBrake = GetRawBrakeInput();

        var throttleInput = rawThrottle;
        var brakeInput = rawBrake;

        if (_autoShiftToReverse)
        {
            if (IsGrounded())
            {
                var speedKPH = _forwardSpeed * MPSToKPH;
                if (_reverse)
                {
                    if (throttleInput > 0f && speedKPH > -_switchToReverseSpeedKPH)
                    {
                        _reverse = false;
                    }
                }
                else
                {
                    if (brakeInput > 0f && speedKPH < _switchToReverseSpeedKPH)
                    {
                        _reverse = true;
                    }
                }
            }

            if (_reverse)
            {
                (throttleInput, brakeInput) = (brakeInput, throttleInput);
            }
        }

        var throttleTime = throttleInput != 0f ? _throttleTime : _throttleReleaseTime;
        _throttleInput = Mathf.MoveTowards(_throttleInput, throttleInput, Time.deltaTime / throttleTime);

        var brakeTime = brakeInput != 0f ? _brakeTime : _brakeReleaseTime;
        _brakeInput = Mathf.MoveTowards(_brakeInput, brakeInput, Time.deltaTime / brakeTime);
    }

    private float GetRawSteerInput()
    {
#if ROTTERDAM_ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) return -1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) return 1f;
        }
        return 0f;
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) return -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) return 1f;
        return 0f;
#else
        return 0f;
#endif
    }

    private float GetRawThrottleInput()
    {
#if ROTTERDAM_ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) return 1f;
        }
        return 0f;
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) return 1f;
        return 0f;
#else
        return 0f;
#endif
    }

    private float GetRawBrakeInput()
    {
#if ROTTERDAM_ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) return 1f;
        }
        return 0f;
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) return 1f;
        return 0f;
#else
        return 0f;
#endif
    }

    private void AddDriveTorqueLogic()
    {
        var throttleInput = _throttleInput;
        if (IsExceedMaxMotorRPM)
        {
            throttleInput = 0f;
        }

        if (IsGrounded())
        {
            _motorRPM = CalcMotorRPMFromSpeedKPH(_speed * MPSToKPH);
            var motorTorque = GetMotorTorque() * throttleInput;
            var motorFriTorque = GetMotorFrictionTorque() * (1f - throttleInput);

            var driveTorque = motorTorque * _finalGearRatio;
            var friTorque = motorFriTorque * _finalGearRatio;

            AddDriveTorque(driveTorque);
            AddBrakeTorque(friTorque);
        }
        else
        {
            var motorTorque = GetMotorFrictionTorque() * throttleInput;
            var motorFiTorque = GetMotorFrictionTorque() * (1f - throttleInput);

            var totalBrakeTorque = _maxBrakeTorque * _brakeInput * _wheels.Length;

            var driveTorque = motorTorque * _finalGearRatio;
            var drivetrainI = _finalGearRatio * _finalGearRatio * _motorInertia;

            var friTorque = motorFiTorque * _finalGearRatio;
            var brakeTorque = totalBrakeTorque * _finalGearRatio;

            _motorRPM += (driveTorque / drivetrainI) * Time.fixedDeltaTime * RPSToRPM;
            DecelerateMotor(friTorque, drivetrainI);
            DecelerateMotor(brakeTorque, drivetrainI);
        }
    }

    private float CalcMotorRPMFromSpeedKPH(float speedKPH)
    {
        return (speedKPH * 1f * _finalGearRatio * 1000f) / (2f * Mathf.PI * _wheelRadius * 60f);
    }

    private float GetMotorTorque()
    {
        if (IsExceedMaxMotorRPM) return 0f;

        var revRate = Mathf.Clamp01(MotorRevolutionRate);
        var coef = 1f;
        if (revRate >= 0.5f)
        {
            coef = (1f - revRate) * 2f;
            coef *= coef;
        }

        var sign = _reverse ? -1f : 1f;
        return sign * _maxMotorTorque * coef;
    }

    private float GetMotorFrictionTorque()
    {
        var motorRevRate = MotorRevolutionRate;
        return Mathf.Lerp(_minMotorFrictionTorque, _maxMotorFrictionTorque, motorRevRate * motorRevRate);
    }

    private void DecelerateMotor(float torque, float inertia)
    {
        var acc = -Mathf.Sign(_motorRPM) * (torque / inertia) * Time.fixedDeltaTime * RPSToRPM;
        if (Mathf.Abs(acc) > Mathf.Abs(_motorRPM))
        {
            _motorRPM = 0f;
        }
        else
        {
            _motorRPM += acc;
        }
    }

    private void CalcWheelbase()
    {
        if (_wheels == null || _wheels.Length == 0) return;
        var minLocalZ = float.MaxValue;
        var maxLocalZ = float.MinValue;
        foreach (var wheel in _wheels)
        {
            minLocalZ = Mathf.Min(wheel.transform.localPosition.z, minLocalZ);
            maxLocalZ = Mathf.Max(wheel.transform.localPosition.z, maxLocalZ);
        }
        _wheelbase = Mathf.Abs(minLocalZ - maxLocalZ);
    }

    private void AdjustCenterOfMass()
    {
        if (_wheels == null || _wheels.Length == 0) return;
        var com = Vector3.zero;
        foreach (var wheel in _wheels)
        {
            com += wheel.transform.localPosition;
        }
        com /= (float)_wheels.Length;
        com.y = _centerOfMassHeight;
        _rigidbody.centerOfMass = com;
    }

    private void AdjustSuspension()
    {
        if (_wheels == null || _wheels.Length == 0) return;
        var mass = _rigidbody.mass / _wheels.Length;
        var spring = 4f * Mathf.PI * Mathf.PI * _suspensionNaturalFrequency * _suspensionNaturalFrequency * mass;
        var damper = 2f * Mathf.Sqrt(mass * spring) * _suspensionDampingRatio;

        foreach (var wheel in _wheels)
        {
            wheel.SuspensionStroke = _suspensionStroke;
            wheel.SuspensionSpring = spring;
            wheel.SuspensionDamper = damper;
        }
    }

    private void UpdateSteerAngle()
    {
        var steerAngle = _maxSteerAngle * _steerInput;
        foreach (var wheel in _steerableWheels)
        {
            wheel.SteerAngle = steerAngle;
        }
    }

    public bool IsGrounded()
    {
        if (_wheels == null) return false;
        foreach (var wheel in _wheels)
        {
            if (wheel.Grounded) return true;
        }
        return false;
    }

    private Vector3 GetGroundNormal()
    {
        var normal = Vector3.zero;
        var count = 0;
        foreach (var wheel in _wheels)
        {
            if (!wheel.Grounded) continue;
            normal += wheel.HitInfo.normal;
            count++;
        }
        if (count == 0) return Vector3.zero;
        normal /= (float)count;
        return normal.normalized;
    }

    private void Turn()
    {
        var steerAngle = _maxSteerAngle * _steerInput;
        var targetAngVel = 0f;
        if (steerAngle != 0f)
        {
            var turnR = _wheelbase / Mathf.Sin(steerAngle * Mathf.Deg2Rad);
            targetAngVel = _forwardSpeed / turnR;
            var minTurnR = (_speed * _speed) / (_mu * Physics.gravity.magnitude);
            var maxAngVel1 = _speed / minTurnR;
            var maxAngVel2 = _maxTurnSpeed * Mathf.Deg2Rad;
            var maxAngVel = Mathf.Max(maxAngVel1, maxAngVel2);
            targetAngVel = Mathf.Clamp(targetAngVel, -maxAngVel, maxAngVel);
        }

        var currAngVel = _useAddTorque ? _rigidbody.angularVelocity.y : _angularVelocity;
        var angVelDiff = targetAngVel - currAngVel;
        var velDiff = (_wheelbase / 2f) * angVelDiff;
        var torque = ((_normalForce / 2f) * velDiff) * 2f;
        var maxFriction = _normalForce * _mu;
        torque = Mathf.Clamp(torque, -maxFriction, maxFriction);

        if (_useAddTorque)
        {
            _rigidbody.AddTorque(transform.up * torque);
            _angularVelocity = _rigidbody.angularVelocity.y;
        }
        else
        {
            _angularVelocity += torque / _rigidbody.inertiaTensor.y * Time.fixedDeltaTime;
            var angVel = _rigidbody.angularVelocity;
            angVel.y = _angularVelocity;
            _rigidbody.angularVelocity = angVel;
        }
    }

    private void AddFrictionForce()
    {
        var friForce = (_rigidbody.mass * -_sidewaysSpeed) / Time.fixedDeltaTime;
        var fri = Mathf.InverseLerp(0f, _peakFrictionSlipAngle, Mathf.Abs(_slipAngle)) * _mu;
        var tilt = Mathf.Cos(_tiltAngle * Mathf.Deg2Rad);
        var maxFriForce = _normalForce * fri * tilt;
        friForce = Mathf.Clamp(friForce, -maxFriForce, maxFriForce);

        var friForceVec = _groundSideways * friForce;
        _rigidbody.AddForceAtPosition(friForceVec, _addForcePosition);
        _totalForce += friForceVec;
    }

    private void AddDriveTorque(float driveTorque)
    {
        var driveForce = driveTorque / _wheelRadius;
        var maxDriveForce = _normalForce * _mu;
        driveForce = Mathf.Clamp(driveForce, -maxDriveForce, maxDriveForce);

        var driveForceVec = _groundForward * driveForce;
        _rigidbody.AddForceAtPosition(driveForceVec, _addForcePosition);
        _totalForce += driveForceVec;
    }

    private void AddBrakeTorque(float brakeTorque)
    {
        var brakeForce = -Mathf.Sign(_forwardSpeed) * Mathf.Abs(brakeTorque / _wheelRadius);
        var maxBrakeForce1 = (_rigidbody.mass * Mathf.Abs(_forwardSpeed)) / Time.fixedDeltaTime;
        var maxBrakeForce2 = _normalForce * _mu;
        var maxBrakeForce = Mathf.Min(maxBrakeForce1, maxBrakeForce2);
        brakeForce = Mathf.Clamp(brakeForce, -maxBrakeForce, maxBrakeForce);

        var brakeForceVec = _groundForward * brakeForce;
        _rigidbody.AddForceAtPosition(brakeForceVec, _addForcePosition);
        _totalForce += brakeForceVec;
    }

    private void AddRollingResistanceForce()
    {
        var rollResForce = _normalForce * _rollingResistanceCoef * _wheelRadius;
        AddBrakeTorque(rollResForce);
    }

    private void AddBrakeForce()
    {
        var brakeTorque = _maxBrakeTorque * _brakeInput;
        var totalBrakeForce = brakeTorque * _wheels.Length;
        AddBrakeTorque(totalBrakeForce);
    }

    private void AddAirResistanceForce()
    {
        var vel = _rigidbody.linearVelocity;
        var force = -vel.normalized * vel.sqrMagnitude * _airResistanceCoef * (1f - _airResistanceReduction);
        _rigidbody.AddForce(force);
        _totalForce += force;
    }

    private void AddDownforce()
    {
        var forwardVel = Vector3.Dot(_rigidbody.linearVelocity, transform.forward);
        var force = -transform.up * forwardVel * forwardVel * _downforceCoef * (1f - _airResistanceReduction);
        _rigidbody.AddForce(force);
        _totalForce += force;
    }

    private void OnDrawGizmosSelected()
    {
        if (_rigidbody != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.TransformPoint(_rigidbody.centerOfMass), _totalForce * 0.0001f);
        }
    }
}
}
