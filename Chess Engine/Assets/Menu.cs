using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	private const string singleplayerName = "Main";
	private const string multiplayerName = "ENTER NAME HERE";

	public GameObject loadingIcon;

	public void LoadSinglePlayer() {
		StartCoroutine(LoadAsync(singleplayerName));
	}

	private IEnumerator LoadAsync(string sceneName) {
		var loading = SceneManager.LoadSceneAsync (sceneName);
		loadingIcon.SetActive (true);

		while(!loading.isDone) {
			loadingIcon.transform.Rotate (0, 0, 200 * Time.deltaTime);
			yield return null;
		}
	}
}
