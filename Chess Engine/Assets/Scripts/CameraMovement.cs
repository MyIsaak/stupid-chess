using UnityEngine;

public class CameraMovement : MonoBehaviour {

	private const float sensititity = 1000f;
	private Transform _tr;
	private Rigidbody _rb;

	private void Awake() {
		_rb = GetComponent<Rigidbody>();
		_tr = GetComponent<Transform>();
	}

	private void FixedUpdate () {
		if (!BoardBuilder.Instance().playing)
			return;
		
		float horizontal = Input.GetAxis ("Horizontal");
		float vertical = Input.GetAxis ("Vertical");
		float height = Input.GetAxis ("Jump");

		_rb.AddRelativeForce (new Vector3 (horizontal, height, vertical)  * 1000f * Time.fixedDeltaTime);
		_rb.AddRelativeTorque(new Vector3 (-Input.GetAxis ("Mouse Y") * 0.333f, Input.GetAxis ("Mouse X"), 0) * sensititity * Time.deltaTime);

		_tr.rotation = Quaternion.Euler(_tr.rotation.eulerAngles.x, _tr.eulerAngles.y, 0);

		//Quaternion lookDir = Quaternion.Euler(new Vector3 (_tr.rotation.eulerAngles.x + -Input.GetAxis ("Mouse Y") * sensititity * 0.333f, _tr.rotation.eulerAngles.y + Input.GetAxis ("Mouse X") * sensititity, 0));
		//_tr.rotation = Quaternion.Slerp (_oldRot, lookDir, 0.8f);
	}
}
