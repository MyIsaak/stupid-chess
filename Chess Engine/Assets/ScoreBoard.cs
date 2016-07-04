using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreBoard : MonoBehaviour {

	private const string getScoreUrl = "http://isaakeriksson.t15.org/display.php";
	private const string postScoreUrl = "http://isaakeriksson.t15.org/addscore.php?";
	private const string secretKey = "youarenotsuposedtoseethis";
	private GameObject primaryScorePanel = new GameObject();
	public GameObject scorePanel;
	public GameObject loadingIcon;
	private Player primaryPlayer;

	public void OnEnable() {
		for(var i=0;i<transform.childCount; i++) {
			if (transform.GetChild (i).name == "Primary" && BoardBuilder.Instance().submitted)
				continue;
			
			Destroy (transform.GetChild (i).gameObject);
		}

		loadingIcon.SetActive (true);

		StartCoroutine (GetScores ());

		if (!BoardBuilder.Instance().submitted) 
		{
			primaryScorePanel = (GameObject)Instantiate (scorePanel);
			primaryScorePanel.transform.SetParent (transform);
			primaryScorePanel.name = "Primary";

			primaryScorePanel.transform.Find ("InputField").gameObject.SetActive (true);
			primaryScorePanel.transform.Find ("Name").gameObject.SetActive (false);

			primaryScorePanel.GetComponentInChildren<UnityEngine.UI.InputField>().onEndEdit.AddListener((string arg0) => {SubmitScore();});
		}
		else {
			primaryScorePanel.transform.Find ("InputField").gameObject.SetActive (false);
			primaryScorePanel.transform.Find ("Name").gameObject.SetActive (true);
		}
		primaryScorePanel.transform.Find ("Score").GetComponent<UnityEngine.UI.Text> ().text = FindObjectOfType<BoardBuilder> ().Points.ToString();
	}

	public struct Player {
		public string name;
		public int score;
	}

	public IEnumerator GetScores() {
		var headers = new Dictionary<string, string>();
		headers["Access-Control-Allow-Credentials"] = "true";
		headers["Access-Control-Allow-Headers"] = "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time";
		headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS"; 
		headers["Access-Control-Allow-Origin"] = "*";

		using (var request = new WWW (getScoreUrl, null, headers)) {
			yield return request;

			List<Player> playerList = new List<Player>();

			if (string.IsNullOrEmpty (request.error)) 
			{
				Debug.Log (request.text);

				var result = request.text.Split (new [] { '\t', '\n' });

				for(var i =0; i< result.Length-1;i+=2) 
				{
					if (result [i] == null || result [i + 1] == null)
						continue;
					
					playerList.Add (new Player () { name=result [i], score=int.Parse(result[i+1])});
				}
				DisplayPlayers (playerList);
				loadingIcon.SetActive (false);

				FindObjectOfType<BoardBuilder> ().DisplayNotification ("Sucessfully connected online and retreived highscores!");
			} else {
				Debug.LogError (request.error);
				FindObjectOfType<BoardBuilder> ().DisplayNotification ("Failed to connect online to retreive highscores!");
			}
		}
	}

	public void LateUpdate() {
		loadingIcon.transform.Rotate (0, 0, 200 * Time.deltaTime);
	}

	public void SubmitScore() 
	{
		primaryScorePanel.transform.Find ("InputField").gameObject.SetActive (false);
		primaryScorePanel.transform.Find ("Name").gameObject.SetActive (true);

		string playerName = primaryScorePanel.transform.Find ("InputField").GetComponent<UnityEngine.UI.InputField>().text;
		string playerScore = FindObjectOfType<BoardBuilder> ().Points.ToString();

		primaryScorePanel.transform.Find ("Name").GetComponent<UnityEngine.UI.Text> ().text = playerName;

		if(isActiveAndEnabled)
			StartCoroutine (ISubmitScore(playerName, playerScore));
	}
		
	private IEnumerator ISubmitScore(string playerName, string playerScore) 
	{
		primaryPlayer = new Player { name = playerName, score = int.Parse(playerScore) };

		byte[] hashBytes = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.ASCII.GetBytes(primaryPlayer.name + primaryPlayer.score + secretKey));

		string hash = string.Empty;

		for(var i=0; i<hashBytes.Length; i++) {
			hash += hashBytes[i].ToString("x2");
		}

		string postUrl = postScoreUrl + "name=" + WWW.EscapeURL (primaryPlayer.name) + "&score=" + primaryPlayer.score + "&hash=" + hash;

		var headers = new Dictionary<string, string>();
		headers["Access-Control-Allow-Credentials"] = "true"; 
		headers["Access-Control-Allow-Headers"] = "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time";
		headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS"; 
		headers["Access-Control-Allow-Origin"] = "*";

		var post = new WWW (postUrl, null, headers);
		
		yield return post;

		if(!string.IsNullOrEmpty(post.error)) {
			Debug.LogError (post.error);
			FindObjectOfType<BoardBuilder> ().DisplayNotification ("Failed to connect online to submit your highscore!");
		}

		BoardBuilder.Instance().submitted = true;
		FindObjectOfType<BoardBuilder> ().DisplayNotification ("Your highscore has been sucessfully added!");
	}

	public class MyOrderingClass : IComparer<Player>
	{
		public int Compare(Player x, Player y)
		{
			int score = x.score.CompareTo(y.score);
			return score;
		}
	}


	public void DisplayPlayers(List<Player> players) {
		players.Sort (new MyOrderingClass ());

		int rank = players.Count;

		foreach(var player in players) {
			var panel = (GameObject)Instantiate (scorePanel);
			var texts = panel.GetComponentsInChildren<UnityEngine.UI.Text>();

			foreach(var text in texts) {
				if(text.name == "Name") {
					text.text = string.Format("#{0}: {1}", rank, player.name);
				}
				else if(text.name == "Score") {
					text.text = player.score.ToString();
				}
			}

			panel.transform.SetParent (transform);
			panel.transform.SetAsFirstSibling ();

			rank--;
		}

		primaryScorePanel.transform.SetAsFirstSibling ();
	}
}