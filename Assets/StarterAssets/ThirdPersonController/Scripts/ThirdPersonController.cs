using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(SphereCollider))]
	[RequireComponent(typeof(Rigidbody))]
	public class ThirdPersonController : MonoBehaviour
	{
		[Header("Input")]
		[Tooltip("The script that provides the movement inputs to this controller.")]
		public StarterAssetsInputs InputScript;
		[Tooltip("Whether this controller can accept input right now.")]
		public bool AcceptInputScript = true;

		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;
		// [Tooltip("How fast the character turns to face movement direction")]
		// [Range(0.0f, 0.3f)]
		// public float RotationSmoothTime = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;
		
		[Space(10)]
		[Tooltip("Rolling speed of the character in m/s")]
		public float RollSpeed = 2.0f;
		// [Tooltip("How fast the character turns to face movement direction while rolling")]
		// [Range(0.0f, 0.3f)]
		// public float RollingRotationSmoothTime = 0.06f;
		[Tooltip("Acceleration and deceleration while rolling")]
		public float RollingSpeedChangeRate = 6.0f;
		[Tooltip("The player's head object")]
		public GameObject PlayerHead;
		[Tooltip("The radius of the player's head object")]
		public float PlayerHeadRadius;
		[Tooltip("Whether this controller is rolling.")]
		public bool IsRolling = false;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;
		// [Tooltip("How fast the character turns to face movement direction in midair")]
		// [Range(0.0f, 0.3f)]
		// public float AerialRotationSmoothTime = 0.03f;
		[Tooltip("Acceleration and deceleration in midair")]
		public float AerialSpeedChangeRate = 3.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;
		[Tooltip("Time required to pass between toggling roll state. Gives time for any animation to occur")]
		public float RollTimeout = 0.50f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.28f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 70.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -30.0f;
		[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
		public float CameraAngleOverride = 0.0f;
		[Tooltip("For locking the camera position on all axis")]
		public bool LockCameraPosition = false;

		// cinemachine
		private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;

		// player
		private float _animationBlend;
		private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;
		private float _rollTimeoutDelta;

		// animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;

		private Animator _animator;
		private CharacterController _controller;
		private GameObject _mainCamera;
		private SphereCollider _collider;
		private Rigidbody _rb;
		private Vector3 _horizontalSpeed;

		private const float _threshold = 0.01f;

		private bool _hasAnimator;

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			_collider = GetComponent<SphereCollider>();
			_rb = GetComponent<Rigidbody>();

			_collider.radius = PlayerHeadRadius;
			_collider.enabled = false;
			_rb.detectCollisions = false;

			AssignAnimationIDs();

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			RollCheck();
			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		private void RollCheck()
		{
			// Toggle roll state
			if (InputScript.roll && _rollTimeoutDelta <= 0.0f)
			{
				// Reset roll timer
				_rollTimeoutDelta = RollTimeout;
				
				// Flip the roll state
				IsRolling = !IsRolling;

				// If we are entering the rolling state
				if(IsRolling)
				{
					// Transfer all velocity into the rigidbody
					_rb.velocity = _controller.velocity;
				}
				else
				{
					_rb.velocity = Vector3.zero;
					_rb.angularVelocity = Vector3.zero;
				}

				_rb.detectCollisions = IsRolling;
				_controller.enabled = !IsRolling;
				_collider.enabled = true;
			}

			// roll timeout
			if (_rollTimeoutDelta >= 0.0f)
			{
				InputScript.roll = false;
				_rollTimeoutDelta -= Time.deltaTime;
			}
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition;
			if(IsRolling)
			{
				spherePosition = new Vector3(PlayerHead.transform.position.x, PlayerHead.transform.position.y - PlayerHeadRadius - GroundedOffset, PlayerHead.transform.position.z);
			}
			else
			{
				spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			}
			
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

			// update animator if using character
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDGrounded, Grounded);
			}
		}

		private void CameraRotation()
		{
			// if there is an input and camera position is not fixed
			if (InputScript.look.sqrMagnitude >= _threshold && !LockCameraPosition)
			{
				_cinemachineTargetYaw += InputScript.look.x * Time.deltaTime;
				_cinemachineTargetPitch += InputScript.look.y * Time.deltaTime;
			}

			// clamp our rotations so our values are limited 360 degrees
			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Cinemachine will follow this target
			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
		}

		private void Move()
		{
			// set target speed based on move speed
			float targetSpeed = IsRolling ? RollSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (InputScript.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			Vector3 horizonalVector;

			if(IsRolling)
			{
				horizonalVector = new Vector3(_rb.velocity.x, 0.0f, _rb.velocity.z);
			}
			else
			{
				horizonalVector = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z);
			}

			float currentHorizontalSpeed = horizonalVector.magnitude;

			float inputMagnitude = InputScript.analogMovement ? InputScript.move.magnitude : 1f;

			float speedChangeRate = Grounded ? (IsRolling ? RollingSpeedChangeRate : SpeedChangeRate) : AerialSpeedChangeRate;
			// float rotationSmoothSpeed = Grounded ? (IsRolling ? RollingRotationSmoothTime : RotationSmoothTime) : AerialRotationSmoothTime;

			// move towards target input direction and magnitude
			Vector3 input = new Vector3(InputScript.move.x, 0.0f, InputScript.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (InputScript.move != Vector2.zero)
			{
				_targetRotation = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
				//float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothSpeed);

				// rotate to face input direction relative to camera position
				// transform.rotation = Quaternion.Euler(0.0f, _targetRotation, 0.0f);
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			// accelerate or decelerate to target speed
			_horizontalSpeed = Vector3.MoveTowards(_horizontalSpeed, targetSpeed * targetDirection, Time.deltaTime * speedChangeRate);

			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);

			// move the player
			if(IsRolling)
			{
				float xAccel = Mathf.Min(speedChangeRate, _horizontalSpeed.x -_rb.velocity.x);
				float yAccel = _verticalVelocity -_rb.velocity.y;
				float zAccel = Mathf.Min(speedChangeRate, _horizontalSpeed.z -_rb.velocity.z);
				Debug.Log(new Vector3(xAccel, yAccel, zAccel));
				_rb.AddForce(new Vector3(xAccel, yAccel, zAccel), ForceMode.Acceleration);
			}
			else
			{
				_controller.Move(_horizontalSpeed * Time.deltaTime + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
			}

			// if the player is moving, face the player in the direction they're moving
			if(_horizontalSpeed != Vector3.zero && !IsRolling)
			{
				transform.rotation = Quaternion.LookRotation(_horizontalSpeed, Vector3.up);
			}

			// update animator if using character
			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
			}
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// update animator if using character
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, false);
					_animator.SetBool(_animIDFreeFall, false);
				}

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (InputScript.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

					// update animator if using character
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDJump, true);
					}
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
				else
				{
					// update animator if using character
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDFreeFall, true);
					}
				}

				// if we are not grounded, do not jump
				InputScript.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		void OnCollisionEnter(Collision collision)
		{
			// This should resonably only be called while rolling

			_horizontalSpeed = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
			_verticalVelocity = _rb.velocity.y;
		}
	}
}