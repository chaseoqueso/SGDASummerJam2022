using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		public static GameObject currentPlayerObject;

		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool roll;
		public bool ability1;
		public bool ability2;

		[Header("Movement Settings")]
		public bool analogMovement;
		public bool canUseAbilities = false;

#if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
#endif

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnRoll(InputValue value)
		{
			RollInput(value.isPressed);
		}

		public void OnAbility1(InputValue value)
		{
			if(canUseAbilities)
			{
				Ability1Input(value.isPressed);
			}
		}

		public void OnAbility2(InputValue value)
		{
			if(canUseAbilities)
			{
				Ability2Input(value.isPressed);
			}
		}

		public void OnPause(InputValue value)
		{
			if(SceneManager.GetActiveScene().name != UIManager.GAME_SCENE_NAME){
				return;
			}

			if(PauseMenu.GameIsPaused){
				UIManager.instance.GetPauseMenu().ResumeGame();
			}
			else{
				UIManager.instance.GetPauseMenu().PauseGame();
			}
		}
#else
	// old input sys if we do decide to have it (most likely wont)...
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void RollInput(bool newRollState)
		{
			roll = newRollState;
		}

		public void Ability1Input(bool newRollState)
		{
			ability1 = newRollState;
		}

		public void Ability2Input(bool newRollState)
		{
			ability2 = newRollState;
		}

#if !UNITY_IOS || !UNITY_ANDROID

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

#endif

	}
	
}