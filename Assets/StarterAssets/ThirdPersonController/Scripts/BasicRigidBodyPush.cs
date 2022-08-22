using StarterAssets;
using UnityEngine;

public enum RBPushUpdateMode
{
	Update,
	FixedUpdate
}

public class BasicRigidBodyPush : MonoBehaviour
{
	public CharacterController controller;
	public LayerMask pushLayers;
	public LayerMask collisionLayers;
	public bool canPush;

	[HideInInspector] public RBPushUpdateMode _updateMode;

	public void PushRigidBodies(Vector3 moveVector, RBPushUpdateMode updateMode)
	{
		Vector3 pushDir = new Vector3(moveVector.x, 0.0f, moveVector.z);
		_updateMode = updateMode;

		// Cast along moveVector to see if we hit anything
		RaycastHit hit;
		if(Physics.CapsuleCast(controller.transform.position + controller.transform.rotation * (controller.center + Vector3.up*(controller.height/2 - controller.radius)),
							   controller.transform.position + controller.transform.rotation * (controller.center - Vector3.up*(controller.height/2 - controller.radius)),
							   controller.radius,
							   moveVector,
							   out hit,
							   moveVector.magnitude,
							   collisionLayers))
		{
			hit.transform.SendMessage("OnRigidBodyPush", this, SendMessageOptions.DontRequireReceiver);
		}

		// Cast along pushDir to see if we should move anything
		if(Physics.CapsuleCast(controller.transform.position + controller.transform.rotation * (controller.center + Vector3.up*(controller.height/2 - controller.radius)),
							   controller.transform.position + controller.transform.rotation * (controller.center - Vector3.up*(controller.height/2 - controller.radius)),
							   controller.radius,
							   pushDir,
							   out hit,
							   pushDir.magnitude,
							   pushLayers))
		{
			
			if(canPush)
			{
				// make sure we hit a non kinematic rigidbody
				Rigidbody body = hit.collider.attachedRigidbody;
				if (body == null || body.isKinematic) return;

				// make sure we only push desired layer(s)
				var bodyLayerMask = 1 << body.gameObject.layer;
				if ((bodyLayerMask & pushLayers.value) == 0) return;
				
				ThirdPersonController playerScript;
				if(body.TryGetComponent<ThirdPersonController>(out playerScript))
				{
					playerScript.SetVelocity(moveVector / Time.fixedDeltaTime);
				}
				else
				{
					body.velocity = moveVector / Time.fixedDeltaTime + new Vector3(0, body.velocity.y, 0);
				}
			}
		}
	}
}