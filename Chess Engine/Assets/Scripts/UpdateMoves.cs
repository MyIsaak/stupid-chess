using UnityEngine;
using ChessDotNet;

public class UpdateMoves : MonoBehaviour {

	private Position originalPosition;
	private Position newPosition;
	private Transform _tr;
	private AudioSource _as;

	private Vector3 cachePosition;

	private Rigidbody _rb;
	private Quaternion _rot;

	public Player owner;

	private void Awake() 
	{
		_rb = GetComponent<Rigidbody>();
		_tr = GetComponent<Transform>();
		_as = GetComponent<AudioSource>();
		_rot = Quaternion.Euler (Vector3.zero);
	}

	public void Setup(Position startPosition)
	{
		originalPosition = startPosition;
		UpdatePositionName (startPosition);

		if (ReadPiecePosition ().Rank >= 7) {
			GetComponent<MeshRenderer> ().material.color = Color.black;
			owner = Player.Black;
		}
		else {
			owner = Player.White;
		}
	}

	public void UpdatePositionName(Position position) 
	{
		if(name.Contains("_"))
		{
			gameObject.name = gameObject.name.Split ('_') [0] + "_" + position.ToString ();
		}
		else 
		{
			gameObject.name += "_" + position.ToString ();
		}		
	}

	public void OnPieceDropped()
	{
		var move = new Move(originalPosition, newPosition, owner);
		move.TransformPiece = _tr;

		if (BoardBuilder.Instance().MovePiece (move))
		{
			originalPosition = newPosition;
			UpdatePositionName (originalPosition);

			foreach(var piece in FindObjectsOfType<UpdateMoves>()) {
				if(piece.name.Contains("_") && piece.name != gameObject.name) {
					if(piece.name.Split('_')[1] == newPosition.ToString()) {
						Destroy (piece.gameObject);
					}
				}
			}
		}
		else
		{
			MoveBackPiece ();
		}
	}

	public void MoveBackPiece() 
	{
		BoardBuilder.Instance().DropPiece ();
		_rb.velocity = Vector3.zero;
		_rb.rotation = Quaternion.Euler(Vector3.zero);

		cachePosition = GameObject.Find (gameObject.name.Split ('_') [1]).transform.position;
		
		_rb.MovePosition (new Vector3 (cachePosition.x, 3, cachePosition.z));
	}

	private void OnCollisionEnter(Collision collision) {
		if (_as != null) 
		{
			_as.Play ();
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if(Mathf.Abs(_tr.position.x - cachePosition.x) > 0.01f || Mathf.Abs(_tr.position.z - cachePosition.z) > 0.01f) 
		{
			if (Input.GetMouseButton (0) && owner == Player.White)
				return;

			if (GetPiecePosition () != originalPosition) {
				newPosition = GetPiecePosition ();
				if(newPosition == null) return;
				OnPieceDropped ();
			}
		}
	}

	public Position ReadPiecePosition() {
		if(gameObject.name.Contains("_")) {
			return new Position(gameObject.name.Split('_')[1]);
		}
		return null;
	}

	public Position GetPiecePosition() 
	{		
		RaycastHit[] hits = Physics.RaycastAll (_tr.position, Vector3.down);

		foreach(var hit in hits) 
		{
			if(hit.collider.name.Length == 2) {
				return new Position (hit.collider.name);
			}
		}

		return null;
	}

	private Vector3 RoundVector3(Vector3 input) 
	{
		return new Vector3(Mathf.Round(input.x), input.y, Mathf.Round(input.z));
	}

	public void LateUpdate() 
	{
		if (_tr.position.y < -10f || _tr.position.y > 10f)
		{
			MoveBackPiece ();
		}
		_tr.rotation = _rot;
	}
}
