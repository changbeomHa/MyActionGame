using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour
{
	private Animator anim;
	private Camera cam;
	private CharacterController controller;

	private Vector3 desiredMoveDirection;
	private Vector3 moveVector;

	public Vector2 moveAxis;
	private float verticalVel;

	[Header("Settings")]
	[SerializeField] float movementSpeed;
	[SerializeField] float rotationSpeed = 0.1f;
	[SerializeField] float fallSpeed = .2f;
	public float acceleration = 1;

	[Header("Booleans")]
	[SerializeField] bool blockRotationPlayer;
	private bool isGrounded;

	[Header("Ŀ???? ????")]
	public bool isDash;
	public bool isPFNN;
	Vector3 DashVec;

    void Start()
	{
		anim = this.GetComponent<Animator>();
		cam = Camera.main;
		controller = this.GetComponent<CharacterController>();
		isPFNN = this.GetComponent<SIGGRAPH_2018.BioAnimation_Adam>() != null;
	}

	void Update()
	{
		InputMagnitude();

		if (!ControlManager.isBasicControl && isPFNN)
		{
			return;
		}
		isGrounded = controller.isGrounded;

		if (isGrounded)
			verticalVel -= 0;
		else
			verticalVel -= 1;

		moveVector = new Vector3(0, verticalVel * fallSpeed * Time.deltaTime, 0);
		controller.Move(moveVector);
	}

	void PlayerMoveAndRotation()
	{
		var camera = Camera.main;
		var forward = cam.transform.forward;
		var right = cam.transform.right;

		forward.y = 0f;
		right.y = 0f;

		forward.Normalize();
		right.Normalize();

		desiredMoveDirection = forward * moveAxis.y + right * moveAxis.x;

		if (blockRotationPlayer == false)
		{
			//Camera
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), rotationSpeed * acceleration);
			controller.Move(desiredMoveDirection * Time.deltaTime * (movementSpeed * acceleration));
		}
		else
		{
			//Strafe
			controller.Move((transform.forward * moveAxis.y + transform.right * moveAxis.y) * Time.deltaTime * (movementSpeed * acceleration));
		}
	}

	public void LookAt(Vector3 pos)
	{
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pos), rotationSpeed);
	}

	public void RotateToCamera(Transform t)
	{
		var forward = cam.transform.forward;

		desiredMoveDirection = forward;
		Quaternion lookAtRotation = Quaternion.LookRotation(desiredMoveDirection);
		Quaternion lookAtRotationOnly_Y = Quaternion.Euler(transform.rotation.eulerAngles.x, lookAtRotation.eulerAngles.y, transform.rotation.eulerAngles.z);

		t.rotation = Quaternion.Slerp(transform.rotation, lookAtRotationOnly_Y, rotationSpeed);
	}

	void InputMagnitude()
	{
		//Calculate the Input Magnitude
		float inputMagnitude = new Vector2(moveAxis.x, moveAxis.y).sqrMagnitude;

		//Physically move player
		if (inputMagnitude > 0.1f)
		{
			if (Input.GetKeyDown(KeyCode.LeftShift) && !isDash)
			{
				Dash();
			}

			if (!ControlManager.isBasicControl && isPFNN)
			{
				return;
			}
			anim.SetFloat("InputMagnitude", inputMagnitude * acceleration, .1f, Time.deltaTime);
			PlayerMoveAndRotation();
		}
		else
		{
			anim.SetFloat("InputMagnitude", inputMagnitude * acceleration, .1f,Time.deltaTime);
		}


	}
	void Dash()
	{
		DashVec = moveVector;
		movementSpeed *= 2;
		anim.SetTrigger("Dash");
		isDash = true;
		Invoke("DashOut", 0.5f);
	}

	void DashOut()
	{
		movementSpeed *= 0.5f;
		isDash = false;
	}

	#region Input

	public void OnMove(InputValue value)
	{
		moveAxis.x = value.Get<Vector2>().x;
		moveAxis.y = value.Get<Vector2>().y;
	}

	#endregion

	private void OnDisable()
	{
		anim.SetFloat("InputMagnitude", 0);
	}

}
