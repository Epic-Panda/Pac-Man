using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	public void Play(){
		SceneManager.LoadScene ("Main");
	}

	public void Highscore(){
		SceneManager.LoadScene ("Highscore");
	}

	public void Quit(){
		Application.Quit ();
	}
}
