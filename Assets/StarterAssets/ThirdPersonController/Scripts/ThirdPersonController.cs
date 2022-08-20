using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	public enum PlayerState
	{
		Walking,
		Rolling,
		ExitingRoll
	}

	[RequireComponent(typeof(CameraManager))]
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Rigidbody))]
	public class ThirdPersonController : MonoBehaviour
	{
		[Header("Input")]
		[Tooltip("The script that provides the movement inputs to this controller.")]
		public StarterAssetsInputs InputScript;
		[Tooltip("Whether this controller can accept input right now.")]
		public bool AcceptInput = true;

		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;
		[Tooltip("The maximum allowed difference between the rigidbody's velocity and the intended velocity before we allow the rigidbody to take over.")]
		public float MaxSpeedDiscrepancy = 1f;
		[Tooltip("The player's currentState.")]
		public PlayerState CurrentState;
		
		[Space(10)]
		[Tooltip("Rolling speed of the character in m/s")]
		public float RollSpeed = 2.0f;
		[Tooltip("Acceleration and deceleration while rolling")]
		public float RollingSpeedChangeRate = 6.0f;
		[Tooltip("The rate to decay the bonus velocity acquired from rolling down hills.")]
		public float BonusSpeedDecayRate = 2.0f;
		[Tooltip("The maximum bonus velocity acquired from rolling down hills.")]
		public float MaximumBonusSpeed = 10.0f;
		[Tooltip("The player's head object")]
		public GameObject PlayerHead;
		[Tooltip("The radius of the player's head object")]
		public float PlayerHeadRadius;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;
		[Tooltip("Acceleration and deceleration in midair")]
		public float AerialSpeedChangeRate = 3.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;
		[Tooltip("Time required to pass between toggling roll state. Gives time for any animation to occur")]
		public float RollTimeout = 0.50f;
		[Tooltip("How long it takes to exit the roll state")]
		public float RollExitTime = 0.25f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.25f;
		[Tooltip("The radius of the grounded check. Should generally match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("The distance to sphereCast while rolling")]
		public float RollingGroundedDistance = 0.1f;
		[Tooltip("The radius of the grounded check. Should generally match PlayerHeadRadius")]
		public float RollingGroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;
		[Tooltip("What layers the character collides with")]
		public LayerMask ObstacleLayers;
		
		[Header("Model")]
		[Tooltip("The parts of the model to disable while rolling")]
		public List<GameObject> BodyParts;

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
		private BasicRigidBodyPush _rbPusher;
		private CameraManager _cameraScript;
		private GameObject _mainCamera;
		private CapsuleCollider _collider;
		private Rigidbody _rb;
		private Vector3 _horizontalSpeed;
		private Vector3 _bonusVelocity;
		private Vector3 _groundedNormal;
		private Vector3 _previousPosition;

		private bool _hasAnimator;
		private bool _velocityAdded;

		private bool IsRolling { get { return CurrentState == PlayerState.Rolling; } }
		private bool CanMove { get { return CurrentState != PlayerState.ExitingRoll; }}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}

			StarterAssetsInputs.currentPlayerObject = gameObject;
		}

		private void Start()
		{
			_hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			_rbPusher = GetComponent<BasicRigidBodyPush>();
			_collider = GetComponent<CapsuleCollider>();
			_rb = GetComponent<Rigidbody>();
			_cameraScript = GetComponent<CameraManager>();

			_collider.radius = PlayerHeadRadius;
			_collider.height = 0;
			_collider.isTrigger = true;
			_rb.detectCollisions = false;

			CurrentState = PlayerState.Walking;

			AssignAnimationIDs();

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
			_rollTimeoutDelta = RollTimeout;
		}

		public void TogglePlayerControl(bool toggle)
		{
			if(!toggle)
			{
				EnterRoll();
			}

			AcceptInput = toggle;
			_cameraScript.TogglePlayerCamera(toggle);
		}

		public void AddVelocity(Vector3 velocity)
		{
			_bonusVelocity += new Vector3(velocity.x, 0, velocity.z);
			_verticalVelocity += velocity.y;
			_velocityAdded = true;
		}

		public void SetVelocity(Vector3 velocity)
		{
			_bonusVelocity = new Vector3(velocity.x, 0, velocity.z);
			_verticalVelocity = velocity.y;
			_velocityAdded = true;
		}

		public void ForceRoll()
		{
			if(!IsRolling)
			{
				EnterRoll();
			}

			_rollTimeoutDelta = RollTimeout;
		}

		private void Update()
		{
			if(CurrentState == PlayerState.Walking)
			{
				RollCheck(Time.deltaTime);
				JumpAndGravity(Time.deltaTime);
				GroundedCheck(Time.deltaTime);
				Move(Time.deltaTime);
			}
		}

		private void FixedUpdate()
		{
			if(CanMove && IsRolling)
			{
				RollCheck(Time.fixedDeltaTime);
				JumpAndGravity(Time.fixedDeltaTime);
				GroundedCheck(Time.fixedDeltaTime);
				CollisionCheck(Time.fixedDeltaTime);
				Move(Time.fixedDeltaTime);
			}
		}

		private void LateUpdate()
		{
			_cameraScript.CameraRotation(InputScript.look, IsRolling);
		}

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		private void RollCheck(float timeStep)
		{
			// If roll is pressed and the timeout timer is done
			if (AcceptInput && InputScript.roll && _rollTimeoutDelta <= 0.0f)
			{
                if (IsRolling)
                {
					// Make sure we have enough space to get bigger
					RaycastHit hit;
					if(Physics.Raycast(PlayerHead.transform.position, Vector3.down, out hit, _controller.height, ObstacleLayers))
					{
						Vector3 bottomPoint = hit.point + Vector3.up * 0.05f;
						Collider[] hits = Physics.OverlapCapsule(bottomPoint + (Vector3.up * _controller.radius), 
																 bottomPoint + (Vector3.up * _controller.height) - (Vector3.up * _controller.radius), 
																 _controller.radius, 
																 ObstacleLayers);
						
						if(hits.Length != 0)
						{
							// We don't fit so don't proceed to the rest of the method
							InputScript.roll = false;
							return;
						}
					}

                	// If we are currently rolling, exit roll mode
					StartCoroutine(ExitRollRoutine());
					_bonusVelocity = Vector3.zero;
                }
                else
                {
                	// If we are not currently rolling, enter roll mode
					EnterRoll();
                }

				// Reset roll timer
				_rollTimeoutDelta = RollTimeout;
			}

			// roll timeout
			if (_rollTimeoutDelta >= 0.0f)
			{
				if (AcceptInput)
					InputScript.roll = false;

				_rollTimeoutDelta -= timeStep;
			}
		}

		private void JumpAndGravity(float timeStep)
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
				if (_verticalVelocity < 0)
				{
					if(IsRolling)
					{
						_verticalVelocity = 0;
					}
					else
					{
						_verticalVelocity = -500f;
					}
				}

				// Jump
				if (AcceptInput && InputScript.jump && _jumpTimeoutDelta <= 0.0f)
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
					_jumpTimeoutDelta -= timeStep;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= timeStep;

					if (_verticalVelocity < 0)
					{
						_verticalVelocity = 0;
					}
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
				if (AcceptInput)
					InputScript.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * timeStep;
			}
		}

		private void GroundedCheck(float timeStep)
		{
			// If rolling, use a sphereCast
			if(IsRolling)
			{
				RaycastHit hit;
				if(Physics.SphereCast(PlayerHead.transform.position, RollingGroundedRadius, Vector3.down, out hit, RollingGroundedDistance, GroundLayers))
				{
					// Recast to get the actual surface normal
                	hit.collider.Raycast(new Ray(hit.point + hit.normal * 0.01f, -hit.normal), out hit, 0.011f);

					if(_bonusVelocity.magnitude < MaximumBonusSpeed)
					{
						// Convert any downward force into forward velocity based on the slope
						Vector3 downwardVelocity;

						// If we're falling, use vertical velocity as the down vector
						if(!Grounded)
						{
							downwardVelocity = Vector3.up * _verticalVelocity;
						}
						else
						{
							// Otherwise use gravity
							downwardVelocity = Vector3.up * Gravity * timeStep;
						}

						// Only add velocity if we're going down
						if(downwardVelocity.y < 0)
						{
							// Get the component of the downward vector coplanar with the collision surface
							Vector3 coplanarForce = Vector3.ProjectOnPlane(downwardVelocity, hit.normal);
							coplanarForce.y = 0;

							// If the coplanar slope force is going in a direction other than the direction of our character
							// (aka, if we're going uphill or sideways on a hill) reduce the velocity by an amount relative to how different it is.
							Vector3 totalVelocity = _horizontalSpeed + _bonusVelocity;
							coplanarForce *= Mathf.InverseLerp(-1, 1, (Vector3.Dot(coplanarForce, totalVelocity)));

							_bonusVelocity += coplanarForce;

							if(_bonusVelocity.magnitude > MaximumBonusSpeed)
							{
								_bonusVelocity = _bonusVelocity.normalized * MaximumBonusSpeed;
							}
						}
					}

					// Only consider this ground if it's within the slope limit
					bool wasGrounded = Grounded;
					Grounded = Vector3.Angle(Vector3.up, hit.normal) < _controller.slopeLimit;

					// Update the ground normal
					if(Grounded)
					{
						_groundedNormal = hit.normal;
					}
					else
					{
						_groundedNormal = Vector3.zero;

						if(wasGrounded)
						{
							_verticalVelocity = _rb.velocity.y;
						}
					}
				}
				else if(Grounded)
				{
					_groundedNormal = Vector3.zero;
					_verticalVelocity = _rb.velocity.y;
					Grounded = false;
				}

				if(Grounded)
				{
					_rb.angularDrag = 5;
				}
				else
				{
					_rb.angularDrag = 0;
				}
			}
			else
			{
				// Otherwise just CheckSphere

				Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
				Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
			}
			

			// update animator if using character
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDGrounded, Grounded);
			}
		}

		private void CollisionCheck(float timestep)
		{
			// If we're rolling, check against the rigidbody's velocity to see if we hit something
			if(IsRolling && !_velocityAdded)
			{
				Vector3 idealVelocity = _horizontalSpeed + _bonusVelocity;
				if(!Grounded)
					idealVelocity += Vector3.up * _verticalVelocity;

				// If we hit something steeper than our slope limit
				RaycastHit hit;
				if(Physics.SphereCast(_previousPosition, _collider.radius, idealVelocity, out hit, idealVelocity.magnitude * timestep, ObstacleLayers) && 
					Vector3.Angle(hit.normal, Vector3.up) > _controller.slopeLimit)
				{
					// Accept _rb.velocity as being correct
					Vector3 horizontalRbVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
					if(horizontalRbVelocity.magnitude > RollSpeed)
					{
						_horizontalSpeed = horizontalRbVelocity.normalized * RollSpeed;
						_bonusVelocity = horizontalRbVelocity - _horizontalSpeed;
					}
					else
					{
						_horizontalSpeed = horizontalRbVelocity;
						_bonusVelocity = Vector3.zero;
					}
					
					// Decreases the vertical velocity based on the impact
					_verticalVelocity -= Vector3.Project(Vector3.Project(_rb.velocity, hit.normal), Vector3.up).y;
				}
			}

			_previousPosition = PlayerHead.transform.position;
			_velocityAdded = false;
		}

		private void Move(float timeStep)
		{
			// set target speed based on move speed
			float targetSpeed = IsRolling ? RollSpeed : MoveSpeed;

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (!AcceptInput || InputScript.move == Vector2.zero) targetSpeed = 0.0f;

			float inputMagnitude = InputScript.analogMovement ? InputScript.move.magnitude : 1f;

			float speedChangeRate = Grounded ? (IsRolling ? RollingSpeedChangeRate : SpeedChangeRate) : AerialSpeedChangeRate;

			// move towards target input direction and magnitude
			Vector3 input = new Vector3(InputScript.move.x, 0.0f, InputScript.move.y).normalized;

			// if there is a move input rotate player when the player is moving
			if (InputScript.move != Vector2.zero)
			{
				_targetRotation = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			// The total amount of movement that the player can apply this frame
			float moveAmount = timeStep * speedChangeRate;

			// If the player is trying to move in the opposite direction of bonusVelocity
			if(Vector3.Dot(targetDirection, _bonusVelocity) < 0 && targetSpeed != 0)
			{
				Vector3 bonusVelocityAgainstTarget = Vector3.Project(_bonusVelocity, targetDirection);

				// If there is more bonusVelocity than the moveAmount is able to counteract, all of our moveAmount goes toward bonusVelocity
				if(bonusVelocityAgainstTarget.magnitude >= moveAmount)
				{
					// Decrease the bonusVelocity first
					_bonusVelocity += targetDirection * moveAmount;
					moveAmount = 0;
				}
				else
				{
					// Otherwise, only part of our movement will fight against bonusVelocity and the rest will move us normally
					_bonusVelocity -= bonusVelocityAgainstTarget;
					moveAmount -= bonusVelocityAgainstTarget.magnitude;
				}
			}

			// accelerate or decelerate to target speed based on how much movement we have left
			_horizontalSpeed = Vector3.MoveTowards(_horizontalSpeed, targetSpeed * targetDirection, moveAmount);

			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, timeStep * speedChangeRate);

			// move the player
			if(IsRolling)
			{
				Vector3 totalHorizontalVelocity = _horizontalSpeed + _bonusVelocity;
				// Choose vertical speed based on if we're grounded or jumping/falling
				Vector3 verticalSpeed = Vector3.zero;
				if(Grounded && _verticalVelocity <= 0)
				{
					// If we're grounded && not jumping, apply a downward force toward the ground

					// Find the normalized projection from our horizontal speed onto the ground plane
					Vector3 projectionVector = Vector3.ProjectOnPlane(totalHorizontalVelocity, _groundedNormal).normalized;

					// Ensure we're not on flat ground or going uphill
					if(projectionVector != Vector3.zero && projectionVector.y < 0)
					{
						// Remove the vertical component from the projection normal
						Vector3 horizontalComponents = new Vector3(projectionVector.x, 0, projectionVector.z);

						// Find the scalar that would allow the horizontal components to reach our horizontal speed
						float scalar = totalHorizontalVelocity.magnitude / horizontalComponents.magnitude;

						// Apply that scalar to the vertical component. This is how far we need to move down this frame to follow the ground plane
						verticalSpeed = new Vector3(0, projectionVector.y * scalar, 0);

						// Extra downhill force
						verticalSpeed += _groundedNormal * 20 * Gravity * timeStep;
					}
					else if(Vector3.Dot(_groundedNormal, Vector3.up) > 0.9f)
					{
						// Extra flat ground force
						verticalSpeed += _groundedNormal * 10 * Gravity * timeStep;
					}
					else
					{
						// Extra uphill force
						verticalSpeed += _groundedNormal * 1 * Gravity * timeStep;
					}
				}
				else
				{
					// If we're jumping or falling, just use verticalVelocity
					verticalSpeed = new Vector3(0.0f, _verticalVelocity, 0.0f);
				}

				// Apply the velocity to the rigidbody
				_rb.velocity = totalHorizontalVelocity + verticalSpeed;

				// Decay bonus velocity
				if(Grounded && _bonusVelocity != Vector3.zero)
				{
					float newMagnitude = (_bonusVelocity.magnitude - BonusSpeedDecayRate * timeStep);
					if(newMagnitude < 0)
					{
						_bonusVelocity = Vector3.zero;
					}
					else
					{
						_bonusVelocity = _bonusVelocity.normalized * newMagnitude;
					}
				}
			}
			else
			{
				Vector3 movement = (_horizontalSpeed + new Vector3(0.0f, _verticalVelocity, 0.0f)) * timeStep;
        		_rbPusher.PushRigidBodies(movement);
				_controller.Move(movement);
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

		private void EnterRoll()
		{
			CurrentState = PlayerState.Rolling;

			// Transfer all velocity into the rigidbody
			_rb.velocity = _controller.velocity;

			// Disable the ghost body
			foreach (GameObject model in BodyParts)
			{
				model.SetActive(false);
			}

			// Swap from character controller to rigidbody
			_rb.detectCollisions = true;
			_collider.isTrigger = false;
			_controller.enabled = false;

			// Set the collider to be a sphere
			_collider.height = PlayerHeadRadius * 2;
			_collider.center = new Vector3(0, PlayerHeadRadius * 2, 0);
		}

		private IEnumerator ExitRollRoutine()
		{
			CurrentState = PlayerState.ExitingRoll;
			float progress = 0;

			// Grab the current rotation
			Quaternion originalRotation = transform.rotation;

			// Create an upright, forwards rotation
			Vector3 lookDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
			Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

			// Rotate to the upright position slowly
			while(progress < 1)
			{
				// Stop the rigidbody from doing anything
				_rb.velocity = Vector3.zero;
				_rb.angularVelocity = Vector3.zero;

				// Increase progress based on how far we are through the roll timer
				progress += Time.deltaTime / RollExitTime;

				// Expand the collider from sphere to capsule
				_collider.height = Mathf.Lerp(PlayerHeadRadius * 2, _controller.height, progress);
				_collider.center = new Vector3(0, Mathf.Lerp(PlayerHeadRadius * 2, _controller.center.y, progress), 0);

				// Transition the rotation toward the target
				Quaternion oldRotation = transform.rotation;
				Quaternion newRotation = Quaternion.Lerp(originalRotation, targetRotation, progress);

				// Transition the player's position to the new position
				Vector3 oldOffset = PlayerHead.transform.position - transform.position;
				Vector3 newOffset = newRotation * Quaternion.Inverse(oldRotation) * oldOffset;

				// Update the position and rotation
				Vector3 axis = Vector3.Cross(oldOffset, newOffset);
				float angle = Vector3.Angle(oldOffset, newOffset);
				transform.RotateAround(PlayerHead.transform.position, axis, angle);

				yield return new WaitForEndOfFrame();
			}

			// Enable the model
			foreach (GameObject model in BodyParts)
			{
				model.SetActive(true);
			}


			// Swap from rigidbody to character controller
			_rb.detectCollisions = false;
			_collider.isTrigger = true;
			_controller.enabled = true;

			// Finally exit roll mode
			CurrentState = PlayerState.Walking;
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			if(IsRolling)
			{
				Gizmos.DrawSphere(new Vector3(PlayerHead.transform.position.x, PlayerHead.transform.position.y - RollingGroundedDistance, PlayerHead.transform.position.z), RollingGroundedRadius);
			}
			else
			{
				Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
			}
		}
	}
}