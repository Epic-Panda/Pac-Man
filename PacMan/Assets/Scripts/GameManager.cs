using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{

	[Header ("Attachments - script")]
	public BlinkysAI blinky;
	public BlinkysAI pinky;
	public BlinkysAI inky;
	public BlinkysAI clyde;
	public PacmanMovement pacman;

	[Header ("Attachments - ui")]
	public Text scoreText;
	public Image live1;
	public Image live2;
	public Image bonus1;
	public Image bonus2;
	public Text levelText;
	public GameObject readyText;
	public GameObject pauseText;
	public GameObject gameOverCanvas;
	public InputField nameField;

	[Header ("Attachments - objects")]
	public Grid grid;
	public GameObject dotMap;
	public GameObject dotMapPref;

	[Header ("Bonuses")]
	public BonusController bonusController;
	public Sprite cherry;
	public Sprite strawberry;
	public Sprite peach;
	public Sprite apple;
	public Sprite grapes;
	public Sprite galaxian;
	public Sprite bell;
	public Sprite key;

	[Header ("Sound")]
	public AudioSource audioSource;
	public AudioClip start;
	public AudioClip pause;


	[Header ("Starting settings")]
	public int life = 3;
	public int maxDots = 240;
	public int maxEnergizer = 4;
	public float lastEatenLimit = 4;

	int globalCounter;
	bool globalCounterEnabled = false;
	int points;
	int dots;
	int energizer;
	int currLife;
	float timePassed = 0;
	bool gameOver;

	int currLevel;

	[HideInInspector]
	public int ghostEaten;

	void Awake ()
	{
		Time.timeScale = 0;
	}

	void Start ()
	{
		gameOver = false;
		currLevel = 1;
		points = 0;

		DefineLevel ();
	}

	void Update ()
	{
		if (gameOver)
			return;

		if (readyText.activeSelf && !audioSource.isPlaying) {
			if (audioSource.clip != start)
				audioSource.clip = start;
			
			audioSource.Play ();
		}

		// checks if player pause game
		PauseGame ();

		LeaveHouseTimeTrigger ();
	}

	/// <summary>
	/// Pauses the game.
	/// </summary>
	void PauseGame ()
	{
		if (Input.GetKeyDown ("p")) {

			// remove ready text and set dotmap for pacman
			if (readyText.activeSelf) {
				readyText.SetActive (false);
				pacman.tMap = dotMap.GetComponent<Tilemap> ();
			}
			if (Time.timeScale == 1) {

				if (audioSource.clip != pause)
					audioSource.clip = pause;

				audioSource.Play ();

				Time.timeScale = 0;
				pauseText.SetActive (true);
			} else {
				Time.timeScale = 1;
				audioSource.Stop ();

				pauseText.SetActive (false);
			}
		}
	}

	/// <summary>
	/// Forces ghosts to leave house if any is in it after pacman avoiding etable dots
	/// </summary>
	void LeaveHouseTimeTrigger ()
	{
		timePassed += Time.deltaTime;

		if (timePassed >= lastEatenLimit) {
			timePassed = 0;
			if (pinky.inHouse && !pinky.leavingHouse)
				StartCoroutine (pinky.LeaveHouse ());
			else if (inky.inHouse && !inky.leavingHouse)
				StartCoroutine (inky.LeaveHouse ());
			else if (clyde.inHouse && !clyde.leavingHouse) {
				StartCoroutine (clyde.LeaveHouse ());
			}
		}
	}

	/// <summary>
	/// Go to next level
	/// </summary>
	void NextLevel ()
	{
		// defines level
		currLevel++;
		DefineLevel ();

		// stop them if they have started leaving house
		if (pinky.leavingHouse)
			StopCoroutine (pinky.LeaveHouse ());
		if (inky.leavingHouse)
			StopCoroutine (inky.LeaveHouse ());
		if (clyde.leavingHouse)
			StopCoroutine (clyde.LeaveHouse ());

		// remove old tilemap and set new one
		Destroy (dotMap);
		dotMap = Instantiate (dotMapPref, grid.transform);

		// stops time
		Time.timeScale = 0;
		
		// set everyone on start positions
		pacman.PacmanStart ();
		blinky.StartNewLevel ();
		pinky.StartNewLevel ();
		inky.StartNewLevel ();
		clyde.StartNewLevel ();
	}

	/// <summary>
	/// Sets level atributes 
	/// </summary>
	void DefineLevel ()
	{
		// set game manager atributes to default
		ghostEaten = 0;
		currLife = life;
		dots = 0;
		energizer = 0;
		globalCounterEnabled = false;
		scoreText.text = points.ToString ();
		levelText.text = currLevel.ToString ();

		// set life images to max
		live1.gameObject.SetActive (true);
		live2.gameObject.SetActive (true);
		readyText.SetActive (true);
		bonus1.gameObject.SetActive (true);
		bonus2.gameObject.SetActive (true);

		// set bonus images
		if (currLevel == 1) {
			bonusController.points = 100;
			bonusController.spriteRenderer.sprite = cherry;

			bonus1.sprite = cherry;
			bonus2.sprite = cherry;
		} else if (currLevel == 2) {
			bonusController.points = 300;
			bonusController.spriteRenderer.sprite = strawberry;

			bonus1.sprite = strawberry;
			bonus2.sprite = strawberry;
		} else if (currLevel == 3 || currLevel == 4) {
			bonusController.points = 500;
			bonusController.spriteRenderer.sprite = peach;

			bonus1.sprite = peach;
			bonus2.sprite = peach;
		} else if (currLevel == 5 || currLevel == 6) {
			bonusController.points = 700;
			bonusController.spriteRenderer.sprite = apple;

			bonus1.sprite = apple;
			bonus2.sprite = apple;
		} else if (currLevel == 7 || currLevel == 8) {
			bonusController.points = 1000;
			bonusController.spriteRenderer.sprite = grapes;

			bonus1.sprite = grapes;
			bonus2.sprite = grapes;
		} else if (currLevel == 9 || currLevel == 10) {
			bonusController.points = 2000;
			bonusController.spriteRenderer.sprite = galaxian;

			bonus1.sprite = galaxian;
			bonus2.sprite = galaxian;
		} else if (currLevel == 1 || currLevel == 12) {
			bonusController.points = 3000;
			bonusController.spriteRenderer.sprite = bell;

			bonus1.sprite = bell;
			bonus2.sprite = bell;
		} else {
			bonusController.points = 5000;
			bonusController.spriteRenderer.sprite = key;

			bonus1.sprite = key;
			bonus2.sprite = key;
		}

		// set ghost and pacman atributes based on level
		if (currLevel == 1) {
			
			pacman.speedProcentNormal = 0.8f;
			pacman.speedProcentDots = 0.71f;
			pacman.speedProcentFrightened = 0.9f;
			pacman.speedProcentEnergized = 0.79f;

			blinky.speedProcentNormal = 0.75f;
			blinky.speedProcentTunnel = 0.4f;
			blinky.speedProcentFrighten = 0.5f;
			blinky.scatterChaseTime = new float[]{ 7, 20, 7, 20, 5, 20, 5 };

			pinky.speedProcentNormal = 0.75f;
			pinky.speedProcentTunnel = 0.4f;
			pinky.speedProcentFrighten = 0.5f;
			pinky.scatterChaseTime = new float[]{ 7, 20, 7, 20, 5, 20, 5 };

			inky.speedProcentNormal = 0.75f;
			inky.speedProcentTunnel = 0.4f;
			inky.speedProcentFrighten = 0.5f;
			inky.scatterChaseTime = new float[]{ 7, 20, 7, 20, 5, 20, 5 };
			inky.leaveHouseDotNum = 30;

			clyde.speedProcentNormal = 0.75f;
			clyde.speedProcentTunnel = 0.4f;
			clyde.speedProcentFrighten = 0.5f;
			clyde.scatterChaseTime = new  float[]{ 7, 20, 7, 20, 5, 20, 5 };
			clyde.leaveHouseDotNum = 60;

		} else if (currLevel >= 2 && currLevel <= 4) {
			
			pacman.speedProcentNormal = 0.9f;
			pacman.speedProcentDots = 0.79f;
			pacman.speedProcentFrightened = 0.95f;
			pacman.speedProcentEnergized = 0.83f;

			blinky.speedProcentNormal = 0.85f;
			blinky.speedProcentTunnel = 0.45f;
			blinky.speedProcentFrighten = 0.55f;
			blinky.scatterChaseTime = new float[]{ 7, 20, 7, 20, 5, 1033, 1f / 60 };

			pinky.speedProcentNormal = 0.85f;
			pinky.speedProcentTunnel = 0.45f;
			pinky.speedProcentFrighten = 0.55f;
			pinky.scatterChaseTime = new  float[]{ 7, 20, 7, 20, 5, 1033, 1f / 60 };

			inky.speedProcentNormal = 0.85f;
			inky.speedProcentTunnel = 0.45f;
			inky.speedProcentFrighten = 0.55f;
			inky.scatterChaseTime = new  float[]{ 7, 20, 7, 20, 5, 1033, 1f / 60 };
			inky.leaveHouseDotNum = 0;

			clyde.speedProcentNormal = 0.85f;
			clyde.speedProcentTunnel = 0.45f;
			clyde.speedProcentFrighten = 0.55f;
			clyde.scatterChaseTime = new  float[]{ 7, 20, 7, 20, 5, 1033, 1f / 60 };

			if (currLevel == 2)
				clyde.leaveHouseDotNum = 50;
			else
				clyde.leaveHouseDotNum = 0;

		} else {
			if (currLevel >= 5 && currLevel <= 20) {
				pacman.speedProcentNormal = 1f;
				pacman.speedProcentDots = 0.87f;
				pacman.speedProcentFrightened = 1f;
				pacman.speedProcentEnergized = 0.87f;

			} else {
				pacman.speedProcentNormal = 0.9f;
				pacman.speedProcentDots = 0.79f;
				pacman.speedProcentFrightened = 0.95f;
				pacman.speedProcentEnergized = 0.79f;
			}

			blinky.speedProcentNormal = 0.95f;
			blinky.speedProcentTunnel = 0.5f;
			blinky.speedProcentFrighten = 0.6f;
			blinky.scatterChaseTime = new  float[]{ 5, 20, 5, 20, 5, 1037, 1f / 60 };

			pinky.speedProcentNormal = 0.95f;
			pinky.speedProcentTunnel = 0.5f;
			pinky.speedProcentFrighten = 0.6f;
			pinky.scatterChaseTime = new  float[]{ 5, 20, 5, 20, 5, 1037, 1f / 60 };

			inky.speedProcentNormal = 0.95f;
			inky.speedProcentTunnel = 0.5f;
			inky.speedProcentFrighten = 0.6f;
			inky.scatterChaseTime = new  float[]{ 5, 20, 5, 20, 5, 1037, 1f / 60 };
			inky.leaveHouseDotNum = 0;

			clyde.speedProcentNormal = 0.95f;
			clyde.speedProcentTunnel = 0.5f;
			clyde.speedProcentFrighten = 0.6f;
			clyde.scatterChaseTime = new  float[]{ 5, 20, 5, 20, 5, 1037, 1f / 60 };
			clyde.leaveHouseDotNum = 0;
		}

		// set frightened time per level
		if (currLevel == 1) {

			blinky.frightenedTime = 6;
			pinky.frightenedTime = 6;
			inky.frightenedTime = 6;
			clyde.frightenedTime = 6;
		} else if (currLevel == 2 || currLevel == 6 || currLevel == 10) {

			blinky.frightenedTime = 5;
			pinky.frightenedTime = 5;
			inky.frightenedTime = 5;
			clyde.frightenedTime = 5;
		} else if (currLevel == 3) {

			blinky.frightenedTime = 4;
			pinky.frightenedTime = 4;
			inky.frightenedTime = 4;
			clyde.frightenedTime = 4;
		} else if (currLevel == 4 || currLevel == 14) {

			blinky.frightenedTime = 3;
			pinky.frightenedTime = 3;
			inky.frightenedTime = 3;
			clyde.frightenedTime = 3;
		} else if (currLevel == 5 || currLevel == 7 || currLevel == 8 || currLevel == 11) {

			blinky.frightenedTime = 2;
			pinky.frightenedTime = 2;
			inky.frightenedTime = 2;
			clyde.frightenedTime = 2;
		} else if (currLevel == 9 || currLevel == 12 || currLevel == 13 || currLevel == 15 || currLevel == 16 || currLevel == 18) {

			blinky.frightenedTime = 1;
			pinky.frightenedTime = 1;
			inky.frightenedTime = 1;
			clyde.frightenedTime = 1;
		}

		if (currLevel >= 5)
			lastEatenLimit = 3;
	}

	/// <summary>
	/// Removes pacman's life
	/// </summary>
	public void RemoveLife ()
	{
		currLife--;
		
		// game is over
		if (currLife == 0) {
			Time.timeScale = 0;
			gameOver = true;
			gameOverCanvas.SetActive (true);
			return;
		}

		// hide life images
		if (currLife == 2)
			live2.gameObject.SetActive (false);
		else if (currLife == 1)
			live1.gameObject.SetActive (false);		

		globalCounter = 0;
		globalCounterEnabled = true;
		timePassed = 0;
		pinky.dotCounterEnabled = false;
		inky.dotCounterEnabled = false;
		clyde.dotCounterEnabled = false;

		if (pinky.leavingHouse)
			StopCoroutine (pinky.LeaveHouse ());
		if (inky.leavingHouse)
			StopCoroutine (inky.LeaveHouse ());
		if (clyde.leavingHouse)
			StopCoroutine (clyde.LeaveHouse ());

		pacman.PacmanStart ();
		blinky.GhostStart ();
		pinky.GhostStart ();
		inky.GhostStart ();
		clyde.GhostStart ();

	}

	public void AddPoint (int amount)
	{
		points += amount;
		scoreText.text = points.ToString ();
	}

	public void AddEnergizer ()
	{
		energizer++;
		ghostEaten = 0;
	}

	public void AddDots ()
	{
		timePassed = 0;
		dots++;

		// activate bonus
		if (dots == 70 || dots == 170) {
			bonusController.gameObject.SetActive (true);
			if (bonus2.gameObject.activeSelf)
				bonus2.gameObject.SetActive (false);
			else
				bonus1.gameObject.SetActive (false);
		}
			
		// sets elroy depending on level
		SetElroy ();

		// use ghost local counter to let them free
		if (!globalCounterEnabled) {
			if (inky.dotCount < inky.leaveHouseDotNum)
				inky.DotsCounter ();
			// if inky left house start clyde's counter
			else if (clyde.dotCount < clyde.leaveHouseDotNum)
				clyde.DotsCounter ();
		} // use global counter to make ghosts leave house
		else if (globalCounterEnabled) {
			globalCounter++;

			if (globalCounter == 7 && pinky.inHouse && !pinky.leavingHouse)
				StartCoroutine (pinky.LeaveHouse ());
			else if (globalCounter == 17 && inky.inHouse && !inky.leavingHouse)
				StartCoroutine (inky.LeaveHouse ());
			else if (globalCounter == 32) {
				if (clyde.inHouse && !clyde.leavingHouse) {
					// enable local counters
					pinky.dotCounterEnabled = true;
					inky.dotCounterEnabled = true;
					clyde.dotCounterEnabled = true;
					globalCounterEnabled = false;
				}
			}
		}

		// level won, start next
		if (dots == maxDots) {
			NextLevel ();
		}
	}

	public void ScareGhosts ()
	{
		blinky.Frighten ();
		pinky.Frighten ();
		inky.Frighten ();
		clyde.Frighten ();
	}

	/// <summary>
	/// Sets blinky to elroy at some amount of remaining dots, with speed per level
	/// </summary>
	void SetElroy ()
	{
		if (currLevel == 1) {
			if (maxDots - dots == 20) {
				blinky.elroy = true;
				blinky.elroySpeedProcent = 0.8f;
			} else if (maxDots - dots == 10)
				blinky.elroySpeedProcent = 0.85f;
		} else if (currLevel == 2) {
			if (maxDots - dots == 30) {
				blinky.elroy = true;
				blinky.elroySpeedProcent = 0.9f;
			} else if (maxDots - dots == 15)
				blinky.elroySpeedProcent = 0.95f;
		} else if (currLevel >= 3 && currLevel <= 5) {
			if (maxDots - dots == 40) {
				blinky.elroy = true;
				if (currLevel < 5)
					blinky.elroySpeedProcent = 0.9f;
				else
					blinky.elroySpeedProcent = 1f;
			} else if (maxDots - dots == 20) {
				if (currLevel < 5)
					blinky.elroySpeedProcent = 0.95f;
				else
					blinky.elroySpeedProcent = 1.05f;
			}
		} else if (currLevel >= 6 && currLevel <= 8) {
			if (maxDots - dots == 50) {
				blinky.elroy = true;
				blinky.elroySpeedProcent = 0.95f;
			} else if (maxDots - dots == 25)
				blinky.elroySpeedProcent = 1.05f;
		} else if (currLevel >= 9 && currLevel <= 11) {
			if (maxDots - dots == 60) {
				blinky.elroy = true;
				blinky.elroySpeedProcent = 0.95f;
			} else if (maxDots - dots == 30)
				blinky.elroySpeedProcent = 1.05f;
		} else if (currLevel >= 12 && currLevel <= 14) {
			if (maxDots - dots == 80) {
				blinky.elroy = true;
				blinky.elroySpeedProcent = 0.95f;
			} else if (maxDots - dots == 40)
				blinky.elroySpeedProcent = 1.05f;
		} else if (currLevel >= 15 && currLevel <= 18) {
			if (maxDots - dots == 100) {
				blinky.elroy = true;
				blinky.elroySpeedProcent = 0.95f;
			} else if (maxDots - dots == 50)
				blinky.elroySpeedProcent = 1.05f;
		} else {
			if (maxDots - dots == 120) {
				blinky.elroy = true;
				blinky.elroySpeedProcent = 0.95f;
			} else if (maxDots - dots == 60)
				blinky.elroySpeedProcent = 1.05f;
		}
	}

	public void MainMenu ()
	{
		string name = nameField.text;
		int score = points;
		int level = currLevel;

		string tmpName;
		int tmpScore;
		int tmpLevel;

		// set score
		for (int i = 0; i < 10; i++) {
			if (PlayerPrefs.GetInt (i + "score", 0) < score) {
				tmpName = PlayerPrefs.GetString (i + "name", "");
				tmpScore = PlayerPrefs.GetInt (i + "score", 0);
				tmpLevel = PlayerPrefs.GetInt (i + "level", 1);

				PlayerPrefs.SetString (i + "name", name);
				PlayerPrefs.SetInt (i + "score", score);
				PlayerPrefs.SetInt (i + "level", level);

				level = tmpLevel;
				name = tmpName;
				score = tmpScore;
			}
		}

		// return time to normal
		Time.timeScale = 1;

		SceneManager.LoadScene ("Menu");
	}
}