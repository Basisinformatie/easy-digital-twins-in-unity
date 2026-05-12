using UnityEngine;

namespace Rotterdam.DigitalTwins.Runtime
{
    [RequireComponent(typeof(Camera))]
    public class CustomCarCamera : MonoBehaviour
    {
    [SerializeField] private CustomCarController _targetCar;

    [Header("Positioning")]
    [SerializeField, Min(0f)] private float _distance = 9.5f;
    [SerializeField, Min(0f)] private float _height = 2f;
    [SerializeField, Min(0f)] private float _lookAtHeight = 1f;

    [Header("Following")]
    [SerializeField] private bool _followVelocity = true;
    [SerializeField, Min(1f)] private float _flipSpeedKPH = 5f;

    [Header("Damping")]
    [SerializeField, Min(0f)] private float _rotationDamping = 5f;
    [SerializeField, Min(0f)] private float _heightDamping = 5f;
    [SerializeField, Min(0f)] private float _velocityDamping = 5f;

    private bool _flip;
    private Vector3 _velocityDirection;

    public CustomCarController TargetCar
    {
        get => _targetCar;
        set => _targetCar = value;
    }

    public bool FollowVelocity
    {
        get => _followVelocity;
        set => _followVelocity = value;
    }

    private void LateUpdate()
    {
        if (_targetCar == null)
        {
            return;
        }

        Rigidbody carRigidbody = _targetCar.GetComponent<Rigidbody>();
        if (carRigidbody == null)
        {
            return;
        }

        var carPos = _targetCar.transform.TransformPoint(carRigidbody.centerOfMass);
        var targetAngleY = _targetCar.transform.eulerAngles.y;

        if (_followVelocity)
        {
            var carDir = carPos - transform.position;
            carDir.y = 0f;
            carDir.Normalize();

            var carVelDir = carRigidbody.linearVelocity;
            carVelDir.y = 0f;
            carVelDir.Normalize();

            float currentSpeedKPH = carRigidbody.linearVelocity.magnitude * 3.6f;
            if (currentSpeedKPH >= _flipSpeedKPH)
            {
                _velocityDirection = Vector3.Lerp(_velocityDirection, carVelDir, _velocityDamping * Time.deltaTime);
            }
            else
            {
                _velocityDirection = carDir;
            }

            targetAngleY = Mathf.Atan2(_velocityDirection.x, _velocityDirection.z) * Mathf.Rad2Deg;
        }
        else
        {
            float forwardSpeedKPH = Vector3.Dot(carRigidbody.linearVelocity, _targetCar.transform.forward) * 3.6f;

            if (_flip)
            {
                if (!_targetCar.Reverse && forwardSpeedKPH >= _flipSpeedKPH)
                {
                    _flip = false;
                }
            }
            else
            {
                if (_targetCar.Reverse && forwardSpeedKPH <= -_flipSpeedKPH)
                {
                    _flip = true;
                }
            }

            if (_flip)
            {
                targetAngleY += 180f;
            }
        }

        var newAngleY = Mathf.LerpAngle(transform.eulerAngles.y, targetAngleY, _rotationDamping * Time.deltaTime);

        var currY = transform.position.y;
        var targetY = carPos.y + _height;
        var newY = Mathf.Lerp(currY, targetY, _heightDamping * Time.deltaTime);

        var rot = Quaternion.Euler(0f, newAngleY, 0f);
        var camPos = carPos + rot * Vector3.back * _distance;
        camPos.y = newY;
        transform.position = camPos;

        var lookAtPos = carPos + Vector3.up * _lookAtHeight;
        transform.LookAt(lookAtPos);
    }
}
}