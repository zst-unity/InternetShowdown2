using UnityEngine;
using InternetShowdown.Inputs;

namespace InternetShowdown.Players
{
    public class OnlinePlayer : Player
    {
        private PlayerInputs _inputs;

        [Header("Objects")]
        [SerializeField] private GameObject _mainCameraPrefab;
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private Material _speedlinesMaterial;

        [Header("Camera FOV")]
        [SerializeField] private float _restFov = 60f;

        [Header("Camera Tilting")]
        [SerializeField] private float _tiltSmoothing = 0.15f;
        [SerializeField] private float _tiltAmount = 2.5f;

        [Header("Camera Bobbing")]
        [SerializeField] private float _bobbingAmount = 2.5f;
        [SerializeField] private float _bobbingSpeed = 15.0f;

        private float _cameraRotX;
        private float _cameraRotY;
        private float _cameraRotZ;
        private float _tiltVelocity;
        private float _speedlinesAlpha;

        [SerializeField, Min(0)] private float _jumpBobAmount = 7f;
        [SerializeField] private float _jumpBobTime = 0.2f;
        [SerializeField] private AnimationCurve _jumpBobCurve;
        private float _jumpBob;
        private float _jumpBobReturn;
        private float _jumpBobTimer;

        protected override void OnStart()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            foreach (var renderer in _playerModel.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = 6;
            }

            var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (var camera in cameras)
            {
                Destroy(camera.gameObject);
            }

            InitializeCameras();
            InitializeInputs();

            _jumpBobTimer = _jumpBobTime;
        }

        private void InitializeCameras()
        {
            Instantiate(_mainCameraPrefab, _cameraHolder);
            Camera.main.transform.localPosition = Vector3.zero;
        }

        private void InitializeInputs()
        {
            _inputs = new();
            _inputs.Enable();

            _inputs.Movement.Move.performed += con => _movementInput = con.ReadValue<Vector2>();
            _inputs.Movement.Move.canceled += con => _movementInput = Vector2.zero;
            _inputs.Movement.Jump.performed += con => ResetJumpBuffer();
            _inputs.Movement.Dash.performed += con => TryStartDash();
            _inputs.Movement.GroundDash.performed += con => TryStartGroundDash();
        }

        protected override void OnUpdate()
        {
            var input = _inputs.Camera.Look.ReadValue<Vector2>();

            if (_inputs.Camera.Look.activeControl != null)
            {
                var deviceName = _inputs.Camera.Look.activeControl.device.name.ToLower();
                if (deviceName.Contains("controller") || deviceName.Contains("gameped") || deviceName.Contains("joystick"))
                {
                    input *= Time.deltaTime * 80;
                }
            }

            _cameraRotY += input.x;
            _cameraRotX = Mathf.Clamp(_cameraRotX - input.y, -90f, 90f);

            TiltCamera();
            CalculateJumpBob();

            _orientation.localRotation = Quaternion.Euler(0f, _cameraRotY, 0f);
            Camera.main.transform.localRotation = Quaternion.Euler(_cameraRotX - _jumpBob, 0f, _cameraRotZ);

            _speedlinesAlpha = Mathf.Lerp(_speedlinesAlpha, _state == PlayerState.Dash ? 1 : 0, Time.deltaTime * 2f);
            _speedlinesMaterial.SetFloat("_Alpha", _speedlinesAlpha);

            var targetFov = _restFov + new Vector2(_rigidbody.velocity.x, _rigidbody.velocity.z).magnitude * 2.5f;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFov, Time.deltaTime * 3);
        }

        private void TiltCamera()
        {
            float moving = _state == PlayerState.Move ? 1.0f * _accelerationValue * _decelerationValue : 0f;
            _cameraRotZ = Mathf.SmoothDamp
            (
                current: _cameraRotZ,
                target: (_movementInput.x * -_tiltAmount) + (_movementInput.y * (Mathf.Cos(Time.time * _bobbingSpeed) * _bobbingAmount) * moving),
                currentVelocity: ref _tiltVelocity,
                smoothTime: _tiltSmoothing
            );
        }

        private void CalculateJumpBob()
        {
            if (_jumpBobTimer < _jumpBobTime)
            {
                _jumpBobTimer += Time.deltaTime;
                var value = _jumpBobCurve.Evaluate(_jumpBobTimer / _jumpBobTime);
                _jumpBob = _jumpBobReturn - _jumpBobAmount * value * (1f - (Mathf.Abs(_cameraRotX) / 90f));
            }
            else
            {
                _jumpBob = Mathf.Lerp(_jumpBob, 0f, Time.deltaTime);
                _jumpBobReturn = _jumpBob;
            }
        }

        protected override void OnJump()
        {
            _jumpBobTimer = 0f;
        }

        private void OnDestroy()
        {
            if (!isLocalPlayer) return;
            _inputs.Disable();
        }
    }
}