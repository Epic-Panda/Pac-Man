using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighscoreManager : MonoBehaviour
{

	public Text nameText;
	public Text scoreText;
	public Text levelText;

	void Start ()
	{

		string name = "Name\n";
		string score = "Score\n";
		string level = "Lv\n";

		for (int i = 0; i < 10; i++) {
			level += PlayerPrefs.GetInt (i + "level", 1) + "\n";
			name += PlayerPrefs.GetString (i + "name", "") + "\n";
			score += PlayerPrefs.GetInt (i + "score", 0) + "\n";
		}

		levelText.text = level;
		nameText.text = name;
		scoreText.text = score;
	}

	public void Menu ()
	{
		SceneManager.LoadScene ("Menu");
	}
}
