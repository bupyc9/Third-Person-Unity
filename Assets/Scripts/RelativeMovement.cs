using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RelativeMovement : MonoBehaviour {
	[SerializeField] private Transform target;

	public float rotSpeed = 15.0f;
	public float moveSpeed = 6.0f;

	public float jumpSpeed = 15.0f;
	public float gravity = -9.8f;
	public float terminalVelocity = -10.0f;
	public float minFall = -1.5f;

	private CharacterController _charController;
	private float _vertSpeed;
	private ControllerColliderHit _contract;
	private Animator _animator;
	private bool run = false;

	void Start () {
		_charController = GetComponent<CharacterController> ();
		_vertSpeed = minFall;
		_animator = GetComponent<Animator> ();
	}

	void Update () {
		var movement = Vector3.zero;

		var horInput = Input.GetAxis ("Horizontal");
		var verInput = Input.GetAxis ("Vertical");

		if (Input.GetButtonDown ("Run")) {
			moveSpeed *= 3;
		}
		if (Input.GetButtonUp("Run")) {
			moveSpeed /= 3;
		}

		if (horInput != 0 || verInput != 0) {
			movement.x = horInput * moveSpeed;
			movement.z = verInput * moveSpeed;

			movement = Vector3.ClampMagnitude (movement, moveSpeed);

			var tmp = target.rotation;
			target.eulerAngles = new Vector3 (0, target.eulerAngles.y, 0);
			movement = target.TransformDirection (movement);
			target.rotation = tmp;

			var direction = Quaternion.LookRotation (movement);
			transform.rotation = Quaternion.Lerp (transform.rotation, direction, rotSpeed * Time.deltaTime);
		}

		_animator.SetFloat ("Speed", movement.sqrMagnitude);

		var hitGround = false;
		RaycastHit hit;
		if (_vertSpeed < 0 && Physics.Raycast(transform.position, Vector3.down, out hit)) {
			var check = (_charController.height + _charController.radius) / 1.9f;
			hitGround = hit.distance <= check;
		}

		if (hitGround) {
			if (Input.GetButtonDown("Jump")) {
				_vertSpeed = jumpSpeed;
			} else {
				_vertSpeed = minFall;
				_animator.SetBool ("Jumping", false);
			}
		} else {
			_vertSpeed += gravity * 5 * Time.deltaTime;
			if (_vertSpeed < terminalVelocity) {
				_vertSpeed = terminalVelocity;
			}

			if (_contract != null) {
				_animator.SetBool ("Jumping", true);
			}

			if (_charController.isGrounded) {
				if (Vector3.Dot(movement, _contract.normal) < 0) {
					movement = _contract.normal * moveSpeed;
				} else {
					movement += _contract.normal * moveSpeed;
				}
			}
		}

		movement.y = _vertSpeed;
		movement *= Time.deltaTime;
		_charController.Move (movement);
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		_contract = hit;
	}
}
