using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float rotationSpeed = 12f;

	[Header("References")]
	[SerializeField] private Animator animator;
	[SerializeField] private Transform cameraTransform;

	private CharacterController characterController;
	private Vector3 moveDirection;

	private void Awake()
	{
		characterController = GetComponent<CharacterController>();

		if (animator == null)
			animator = GetComponentInChildren<Animator>();

		if (cameraTransform == null && Camera.main != null)
			cameraTransform = Camera.main.transform;
	}

	private void Update()
	{
		HandleMovement();
		HandleAnimation();
	}

	private void HandleMovement()
	{
		float inputX = Input.GetAxisRaw("Horizontal");
		float inputZ = Input.GetAxisRaw("Vertical");

		Vector3 inputDirection = new Vector3(inputX, 0f, inputZ).normalized;

		if (inputDirection.magnitude > 0.1f)
		{
			Vector3 forward;
			Vector3 right;

			if (cameraTransform != null)
			{
				forward = cameraTransform.forward;
				right = cameraTransform.right;

				forward.y = 0f;
				right.y = 0f;

				forward.Normalize();
				right.Normalize();
			}
			else
			{
				forward = Vector3.forward;
				right = Vector3.right;
			}

			moveDirection = (forward * inputDirection.z + right * inputDirection.x).normalized;

			Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRotation,
				rotationSpeed * Time.deltaTime
			);
		}
		else
		{
			moveDirection = Vector3.zero;
		}

		characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
	}

	private void HandleAnimation()
	{
		bool isWalking = moveDirection.magnitude > 0.1f;
		animator.SetBool("IsWalking", isWalking);
	}
}