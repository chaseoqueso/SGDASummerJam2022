using StarterAssets;
using UnityEngine;

public class BasicRigidBodyPush : MonoBehaviour
{
	public CharacterController controller;
	public LayerMask pushLayers;
	public bool canPush;

	public void PushRigidBodies(Vector3 moveVector)
	{
		if(!canPush)
			return;

		Vector3 pushDir = new Vector3(moveVector.x, 0.0f, moveVector.z);

		RaycastHit hit;
		if(Physics.CapsuleCast(controller.transform.position + controller.transform.rotation * (controller.center + Vector3.up*(controller.height/2 - controller.radius)),
							   controller.transform.position + controller.transform.rotation * (controller.center - Vector3.up*(controller.height/2 - controller.radius)),
							   controller.radius,
							   pushDir,
							   out hit,
							   pushDir.magnitude,
							   pushLayers,
							   QueryTriggerInteraction.Collide))
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
				playerScript.SetVelocity(pushDir / Time.fixedDeltaTime);
			}
			else
			{
				body.MovePosition(body.position + pushDir);
				body.velocity = pushDir / Time.fixedDeltaTime + new Vector3(0, body.velocity.y, 0);
			}
		}
	}
}