using System.Collections;
using InternetShowdown.Systems;
using Mirror;
using UnityEngine;

namespace InternetShowdown.Players
{
    [RequireComponent(typeof(Rigidbody), typeof(NetworkIdentity), typeof(NetworkRigidbodyReliable))]
    public abstract class Player : NetworkBehaviour
    {
        [Header("Objects")]
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] protected Animator _animator;
        [SerializeField] protected GameObject _playerModel;
        [SerializeField] protected Transform _orientation;

        [Header("Move")]
        [SerializeField] protected float _movementSpeed;
        [SerializeField] protected float _groundMovementSmoothing = 0.065f;
        [SerializeField] protected float _airMovementSmoothing = 0.15f;

        [Space(9)]
        [SerializeField] protected AnimationCurve _accelerationCurve;
        [SerializeField, Min(0.01f)] protected float _accelerationDuration = 1f;

        [Space(9)]
        [SerializeField] protected AnimationCurve _decelerationCurve;
        [SerializeField, Min(0.01f)] protected float _decelerationDuration = 1f;

        protected Vector2 _movementInput;
        private Vector2 _smoothedMovementInput;
        private Vector2 _lastSmoothedMovementInput;
        private Vector2 _movementInputSmoothingVelocity;

        private float _accelerationTime;
        protected float _accelerationValue;

        private float _decelerationTime;
        protected float _decelerationValue;

        private bool _accelerating;

        [Header("Jump")]
        [SerializeField] private AudioSystem.AudioProperties _jumpAudio;
        [SerializeField] protected float _jumpForce;
        [SerializeField] protected float _jumpBufferTime = 0.1f;
        [SerializeField] protected float _jumpCoyoteTime = 0.1f;

        private float _jumpBufferTimer;
        private float _jumpCoyoteTimer;

        [Header("Dash")]
        [SerializeField] protected float _dashForce;
        [SerializeField, Min(0.01f)] protected float _dashDuration;
        [SerializeField] protected AnimationCurve _dashStoppingCurve;

        [Header("Ground Dash")]
        [SerializeField] private AudioSystem.AudioProperties _groundDashAudio;
        [SerializeField] protected float _groundDashForce;

        protected float _dashTime;
        protected float _dashValue;
        protected bool _dashing;
        protected Vector3 _dashDirection;

        [Header("Map Check")]
        [SerializeField] protected LayerMask _mapLayers;

        [Space(9)]
        [SerializeField] protected Vector3 _groundCheckSize;
        [SerializeField] protected Vector3 _groundCheckOffset;

        protected Vector3 GroundCheckPosition => transform.position + _groundCheckOffset;

        [Space(9)]
        [SerializeField] protected Vector3 _ceilCheckSize;
        [SerializeField] protected Vector3 _ceilCheckOffset;

        protected Vector3 CeilCheckPosition => transform.position + _ceilCheckOffset;

        protected bool _grounded;
        protected Vector3 _groundNormal;
        protected float _groundAngle;

        [Header("Slope Handling")]
        [SerializeField] protected float _slopeAngleLimit = 60f;

        [Header("Gravity")]
        [SerializeField] protected float _gravity = -18f;
        [SerializeField] protected float _gravityClamp;

        protected float _yVelocity;
        protected PlayerState _state;

        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }

        protected virtual void OnJump() { }

        private int _animatorDirX;
        private int _animatorDirY;
        private int _animatorInAir;

        protected override void OnValidate()
        {
            if (TryGetComponent(out _rigidbody))
            {
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.freezeRotation = true;
                _rigidbody.useGravity = false;
            }
        }

        protected void Start()
        {
            if (!isLocalPlayer) return;
            OnStart();

            _dashTime = _dashDuration;
            _animatorDirX = Animator.StringToHash("DirX");
            _animatorDirY = Animator.StringToHash("DirY");
            _animatorInAir = Animator.StringToHash("InAir");
        }

        protected void Update()
        {
            if (!isLocalPlayer) return;
            CheckGrounded();

            if (_grounded) _jumpCoyoteTimer = _jumpCoyoteTime;
            else if (_jumpCoyoteTimer > 0) _jumpCoyoteTimer -= Time.deltaTime;

            if (_grounded && _yVelocity <= 0 && _groundAngle <= _slopeAngleLimit) _yVelocity = 0;
            else if (_yVelocity > _gravityClamp) _yVelocity += _gravity * Time.deltaTime;

            OnUpdate();

            _smoothedMovementInput = Vector2.SmoothDamp
            (
                _smoothedMovementInput,
                _movementInput,
                ref _movementInputSmoothingVelocity,
                _grounded ? _groundMovementSmoothing : _airMovementSmoothing
            );

            if (_movementInput.magnitude > 0)
            {
                _lastSmoothedMovementInput = _smoothedMovementInput;
            }

            Acceleration();
            Deceleration();

            _accelerationValue = _accelerationCurve.Evaluate(_accelerationTime / _accelerationDuration);
            _decelerationValue = _decelerationCurve.Evaluate(_decelerationTime / _decelerationDuration);

            if (_jumpBufferTimer > 0 && _jumpCoyoteTimer > 0)
            {
                Jump();
                _jumpBufferTimer = 0;
            }

            if (_jumpBufferTimer > 0)
            {
                _jumpBufferTimer -= Time.deltaTime;
            }

            if (_dashTime < _dashDuration)
            {
                _dashing = true;
                _dashTime += Time.deltaTime;
            }
            else
            {
                _dashTime = _dashDuration;
                _dashing = false;
            }
            _dashValue = _dashStoppingCurve.Evaluate(_dashTime / _dashDuration);

            UpdateState();
            _animator.SetFloat(_animatorDirX, _smoothedMovementInput.x);
            _animator.SetFloat(_animatorDirY, _smoothedMovementInput.y);
            _animator.SetBool(_animatorInAir, !_grounded);
        }

        private void CheckGrounded()
        {
            _grounded = Physics.CheckBox(GroundCheckPosition, _groundCheckSize / 2, Quaternion.identity, _mapLayers, QueryTriggerInteraction.Ignore);
            if (_grounded)
            {
                var direction = _rigidbody.velocity.y > 0 ? _orientation.forward * _movementInput.y + _orientation.right * _movementInput.x : Vector3.zero;
                var position = new Vector3(transform.position.x + direction.x * 0.255f, transform.position.y + 0.5f, transform.position.z + direction.z * 0.255f);
                Physics.Raycast(position, Vector3.down, out RaycastHit hitInfo, 4, _mapLayers);
                _groundNormal = hitInfo.normal;
                _groundAngle = Mathf.Round(Vector3.Angle(Vector3.up, _groundNormal) * 10) / 10f;
            }
            else
            {
                _groundNormal = Vector3.up;
                _groundAngle = 0;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            foreach (var contact in other.contacts)
            {
                if (contact.normal.y <= -0.5f) _yVelocity = 0;
            }
        }

        private void Acceleration()
        {
            if (_accelerating && (_movementInput.magnitude == 0 || _accelerationTime >= _accelerationDuration))
            {
                _accelerating = false;
            }

            if (_movementInput.magnitude > 0 && !_accelerating)
            {
                _accelerating = true;
                _accelerationTime = _decelerationValue * _decelerationDuration;
            }

            if (_accelerating)
            {
                _accelerationTime += Time.deltaTime;
            }
        }

        private void Deceleration()
        {
            if (_movementInput.magnitude > 0)
                _decelerationTime = 0;
            else if (_decelerationTime < _decelerationDuration)
                _decelerationTime += Time.deltaTime;
        }

        private void UpdateState()
        {
            if (_dashing)
            {
                _state = PlayerState.Dash;
                return;
            }

            if (_grounded)
            {
                if (_movementInput.magnitude == 0)
                {
                    _state = PlayerState.Idle;
                }
                else
                {
                    _state = PlayerState.Move;
                }
            }
            else
            {
                if (_movementInput.magnitude == 0)
                {
                    _state = PlayerState.IdleAir;
                }
                else
                {
                    _state = PlayerState.MoveAir;
                }
            }
        }

        protected void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            var direction = _orientation.forward * _lastSmoothedMovementInput.y + _orientation.right * _lastSmoothedMovementInput.x;
            var moveDirection = _groundAngle <= _slopeAngleLimit ? Vector3.ProjectOnPlane(direction, _groundNormal) : direction;

            var angleX = Vector2.Angle(Vector2.up, new(_groundNormal.x, _groundNormal.y));
            var angleZ = Vector2.Angle(Vector2.up, new(_groundNormal.z, _groundNormal.y));
            var angleBoost = new Vector3
            (
                1 + angleX * angleX / 3600,
                1 + _groundAngle * _groundAngle / 3600,
                1 + angleZ * angleZ / 3600
            );

            var moveVelocity = _accelerationValue * _decelerationValue * _movementSpeed;
            var dashVelocity = _dashForce * _dashValue;

            _rigidbody.velocity = new Vector3
            (
                (moveVelocity * angleBoost.x * moveDirection.x) + (dashVelocity * angleBoost.x * _dashDirection.x),
                _yVelocity + (moveVelocity * angleBoost.y * moveDirection.y) + (dashVelocity * angleBoost.y * _dashDirection.y),
                (moveVelocity * angleBoost.z * moveDirection.z) + (dashVelocity * angleBoost.z * _dashDirection.z)
            );
        }

        protected void ResetJumpBuffer()
        {
            _jumpBufferTimer = _jumpBufferTime;
        }


        private void Jump()
        {
            _yVelocity = _jumpForce;
            _jumpAudio.PlaySFX(transform.position, true);
            OnJump();
        }

        protected void TryStartDash()
        {
            if (_dashing) return;
            _dashTime = 0;

            if (_yVelocity < 0) _yVelocity = 0;

            var straightDashDirection = _movementInput.magnitude == 0 ?
                new(_orientation.forward.x, 0, _orientation.forward.z) :
                _orientation.forward * _movementInput.y + _orientation.right * _movementInput.x;

            _dashDirection = _groundAngle <= _slopeAngleLimit ? Vector3.ProjectOnPlane(straightDashDirection, _groundNormal) : straightDashDirection;
        }

        protected void TryStartGroundDash()
        {
            if (_grounded) return;

            float targetForce = _rigidbody.velocity.y <= -1f ? -_groundDashForce + (_rigidbody.velocity.y * 3) : -_groundDashForce;
            var wasHit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1000f, _mapLayers);
            if (!wasHit) return;

            _yVelocity = targetForce - hit.distance;
            StopCoroutine(nameof(CO_WaitForGroundDashLand));
            StartCoroutine(nameof(CO_WaitForGroundDashLand));
        }

        private IEnumerator CO_WaitForGroundDashLand()
        {
            yield return new WaitUntil(() => _grounded);
            _groundDashAudio.PlaySFX(transform.position, true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(GroundCheckPosition, _groundCheckSize);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(CeilCheckPosition, _ceilCheckSize);
        }
    }

    public enum PlayerState
    {
        Idle,
        Move,
        Dash,
        IdleAir,
        MoveAir
    }
}