using ChessDotNet;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class BoardBuilder : MonoBehaviour {

	public Color lightColor, darkColor;
    public GameObject BoardPiece;
    public GameObject Pawn;
    public GameObject Bishop;
    public GameObject Rook;
    public GameObject Knight;
    public GameObject King;
    public GameObject Qween;
	public bool submitted = false;
	public bool playing = true;

	public int Points {
		get
		{ 
			return points; 
		}
		set 
		{
			points = value;
			pointsGUI.text = "Points: " + points;

			if(points < 0) 
			{
				GameOver(Player.White, "Out of points", true);
			}
		}}

	private static BoardBuilder board;
	private Queue<string> notifications = new Queue<string>(5);
	private int points;
	public ChessGame game;
	public Text testGUI;
	public Text pointsGUI;
	public Text logGUI;
	public GameObject HUDCanvas, ScoreboardCanvas;

	private Transform pieceToMove;
	private Camera _cam;

	public static BoardBuilder Instance() 
	{
		if(board == null) {
			board = FindObjectOfType<BoardBuilder>();
		}
		return board;
	}

    private char[,] boardSetup = { { 'R', 'H', 'B', 'Q', 'K', 'B', 'H', 'R' },
                                    { 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P' },
                                    { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
                                    { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
                                    { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
                                    { ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ' },
                                    { 'P', 'P', 'P', 'P', 'P', 'P', 'P', 'P' },
                                    { 'R', 'H', 'B', 'Q', 'K', 'B', 'H', 'R' } };

	public void ResetBoard() {
		StopCoroutine (HoldPiece ());

		var sceneObjects = FindObjectsOfType<GameObject> ();

		Points = 100;
		logGUI.text = string.Empty;

		for(var i=0; i < sceneObjects.Length; i++) 
		{
			if (sceneObjects [i].tag == "Dont Destroy")
				continue;

			Destroy (sceneObjects [i]);
		}

		BuildBoard ();
	}

    public void BuildBoard()
    {
		game = new ChessGame();

        for (var x = 0; x < 8; x++)
        {
            for (var y = 0; y < 8; y++)
            {
                var boardPiece = (GameObject)Instantiate(BoardPiece, new Vector3(x, 0, y), Quaternion.identity);
                if ((y % 2 == 0 && x % 2 != 0) || (y % 2 != 0 && x % 2 == 0))
                {
                    boardPiece.name = string.Format(ChessFormat(x,y));
					boardPiece.GetComponentInChildren<MeshRenderer>().material.color = lightColor;
                }
                else
                {
                    boardPiece.name = string.Format(ChessFormat(x, y));
					boardPiece.GetComponentInChildren<MeshRenderer>().material.color = darkColor;
                }

                switch (boardSetup[y,x])
                {
                    case ' ': break;
				case 'P':
					var pawn = (GameObject)Instantiate (Pawn, new Vector3 (x, 0.6f, y), Quaternion.identity);
					pawn.name = string.Format ("Pawn");
					pawn.GetComponent<UpdateMoves> ().Setup (new Position (ChessFormat (x, y))); break;
				case 'B': var bishop = (GameObject)Instantiate(Bishop, new Vector3(x, 0.6f, y), Quaternion.identity); bishop.name = string.Format("Bishop");bishop.GetComponent<UpdateMoves>().Setup(new Position(ChessFormat(x,y))) ;break;
				case 'H': var knight = (GameObject)Instantiate(Knight, new Vector3(x, 0.6f, y), Quaternion.identity); knight.name = string.Format("Knight");knight.GetComponent<UpdateMoves>().Setup(new Position(ChessFormat(x,y))) ;break;
				case 'R': var rook = (GameObject)Instantiate(Rook, new Vector3(x, 0.6f, y), Quaternion.identity); rook.name = string.Format("Rook");        rook.GetComponent<UpdateMoves>().Setup(new Position(ChessFormat(x,y)))   ;break;
				case 'K': var king = (GameObject)Instantiate(King, new Vector3(x, 0.6f, y), Quaternion.identity); king.name = string.Format("King");        king.GetComponent<UpdateMoves>().Setup(new Position(ChessFormat(x,y)))   ;break;
				case 'Q': var qween = (GameObject)Instantiate(Qween, new Vector3(x, 0.6f, y), Quaternion.identity); qween.name = string.Format("Qween");    qween.GetComponent<UpdateMoves>().Setup(new Position(ChessFormat(x,y)))  ;break;
                }
            }
        }
    }

	public void PickUpPiece(Rigidbody piece) {
		pieceToMove = piece.transform;
		StartCoroutine (HoldPiece ());
	}

	System.Collections.IEnumerator HoldPiece() {
		while(true) 
		{
			if (pieceToMove == null || Input.GetMouseButtonUp(0))
				yield break;
			
			Plane plane = new Plane (Vector3.up, Vector3.zero);
			Ray ray = _cam.ScreenPointToRay (new Vector3(Screen.width/2, Screen.height/2));
			float distance;
			plane.Raycast (ray, out distance);
			Vector3 intPoint = ray.GetPoint (distance);

			pieceToMove.position = Vector3.MoveTowards(pieceToMove.position, new Vector3 (intPoint.x, 3, intPoint.z), 0.5f);

			yield return null;
		}
	}

	public void DropPiece() {
			
		StopCoroutine (HoldPiece ());

	}

	public void AddPoints(int amount, string reason) {
		Points += amount;
		DisplayNotification (reason);
	}

	public void Punishment(int penalty) {
		Points -= penalty;
		DisplayNotification("You did an illegal move! You lose " + penalty + " points!");
	}

	public void Punishment(int penalty, Player owner) {
		if(owner == Player.Black) return;
		
		Points -= penalty;
		DisplayNotification(owner.ToString("D") + " did an illegal move! You lose " + penalty + " points!");
	}

	public void DisplayNotification (string message) 
	{
		notifications.Enqueue(message);
	}

	public void AttackPieceCheck(Move move) {
		if (game.GetPieceAt (move.NewPosition) == null) return;
		if (game.GetPieceAt (move.NewPosition).Owner != ChessUtilities.GetOpponentOf(move.Player)) return; 
			
		var pieces = FindObjectsOfType<UpdateMoves> ();

		foreach (var piece in pieces) {
			if (piece.name.Split ('_') [1] == move.NewPosition.ToString ()) {
				Destroy (piece.gameObject);

				if(move.Player == Player.White)
					AddPoints (15, "Woah! You just attacked your opponent. You gain 15 points");
				else {
					Punishment(15, Player.White);
				}
			}
		}
			
	}

	public bool BotMovePiece() 
	{
		var botMove = GetBestMoveDetailed (Player.Black);

		if (!game.IsValidMove (botMove)) 
		{
			Punishment (10, botMove.Player);
			return false;
		}

		Vector3 newPos = GetTileObject(botMove.NewPosition).position;
		botMove.TransformPiece.position = new Vector3 (newPos.x, 3f, newPos.z);

		return true;
	}

	public bool MovePiece(Move move)
	{
		Player oppositePlayer = ChessUtilities.GetOpponentOf(move.Player);

		if (!game.IsValidMove (move)) {
			if(move.Player == Player.White)
				Punishment (5);

			return false;
		}

		AttackPieceCheck(move);
		
		game.ApplyMove (move, true);

		if(move.Player == Player.White)
			AddPoints (3, "You sucessfully moved a piece. You gain 3 points");

		logGUI.text = move.Player.ToString() + " : " + move.ToString();
		

		if (!game.HasAnyValidMoves (oppositePlayer))
		{
			GameOver (oppositePlayer, "CheckMate", false);
		}
		else if(game.WhoseTurn == Player.Black)
		{
			BotMovePiece();
		}
		return true;
	}

	public class MyOrderingClass : IComparer<DetailedMove>
	{
		public int Compare(DetailedMove x, DetailedMove y)
		{
			int move = (int)x.Piece.GetImportance().CompareTo(y.Piece.GetImportance());
			return move;
		}
	}

	private Move GetBestMoveDetailed (Player owner) {
		List<DetailedMove> normalMoves = new List<DetailedMove>();
		List<DetailedMove> attackMoves = new List<DetailedMove>();

		var gameMoves = game.Moves;

		foreach(var moveGame in gameMoves) {
			if (moveGame.Player != owner)
				break;

			if (moveGame.IsCapture) {
					attackMoves.Add(moveGame);
					break;
			}
			if(game.IsValidMove(moveGame)) {
				normalMoves.Add(moveGame);
			}
		}

		if(attackMoves.Count > 0) {
			attackMoves.Sort (new MyOrderingClass());
			return attackMoves [0];
		}
		var movesBlack = game.GetValidMoves (Player.Black);
		var selectedMove = movesBlack[UnityEngine.Random.Range (0, movesBlack.Count - 1)];

		var pieces = FindObjectsOfType<UpdateMoves> ();

		foreach (var piece in pieces) {
			if (piece.name.Split ('_') [1] == selectedMove.OriginalPosition.ToString ()) {
				selectedMove.TransformPiece = piece.transform;
			}
		}
		
		return selectedMove;
	}

    private void Update()
    {
		if (playing) {
			if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				if (Physics.Raycast (_cam.ScreenPointToRay (new Vector3 (Screen.width / 2, Screen.height / 2)), out hit)) {
					if (hit.collider.name.Length == 2)
						return;

					PickUpPiece (hit.rigidbody);
				}
			} else {
				DropPiece ();
			}
		}

		if(Input.GetKeyDown(KeyCode.Escape)) 
		{
			ScoreboardCanvas.SetActive (false);
			HUDCanvas.SetActive (true);

			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			playing = true;

			GameOver(Player.White, "Forfeit", true);
		}

		if (Input.GetKeyDown (KeyCode.Tab)) {
			if (ScoreboardCanvas.activeInHierarchy) {
				ScoreboardCanvas.SetActive (false);
				HUDCanvas.SetActive (true);
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				playing = true;

				if(submitted)
					ResetBoard ();
			}
			else {
				ScoreboardCanvas.SetActive (true);
				HUDCanvas.SetActive (false);
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				playing = false;
			}
		}
    }

	private void GameOver(Player player, string reason, bool forceRestart)
    {
		notifications.Clear ();

		DisplayNotification("Player " +  player.ToString("D") + " lost! Reason: "+ reason);

		if (forceRestart) {
			ResetBoard ();
		}
		else {
			ScoreboardCanvas.SetActive (true);
			HUDCanvas.SetActive (false);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			playing = false;
			testGUI.text =  "Press ESC to play again or TAB to view the highscores.";
		}
	}

	private Transform GetTileObject(Position pos)
	{
		return GameObject.Find(ChessFormat((int)pos.File,(int)pos.Rank-1)).transform;
	}

	private Transform GetPieceObject(Position pos, Player owner)
	{
		RaycastHit[] hits = Physics.RaycastAll (GetTileObject (pos).position, Vector3.up); 
		foreach(var hit in hits)
		{
			if(hit.collider.GetComponent<UpdateMoves>().owner == owner)
				return hit.transform;
		}
		return null;
	}

    private string ChessFormat(int x, int y)
    {
        string format = string.Empty;

        switch(x)
        {
            case 0: format = "A"; break;
            case 1: format = "B"; break;
            case 2: format = "C"; break;
            case 3: format = "D"; break;
            case 4: format = "E"; break;
            case 5: format = "F"; break;
            case 6: format = "G"; break;
            case 7: format = "H"; break;
        }

        format += (y+1).ToString();

        return format;
    }

	private System.Collections.IEnumerator IDisplayNotificationCycle() 
	{
		while (true)
		{
			if(notifications.Count > 0)
				testGUI.text = notifications.Dequeue ();

			yield return new WaitForSeconds (1.7f);

			testGUI.text = string.Empty;

			yield return new WaitForSeconds (0.2f);
		}
	}

	private void Awake() {
		_cam = FindObjectOfType<Camera>();
	}

    private void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

        BuildBoard();

		Points = 100;

		StartCoroutine (IDisplayNotificationCycle ());
    }
}
