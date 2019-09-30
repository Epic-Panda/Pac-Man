using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusController : MonoBehaviour {

	public int points = 100;
	public Transform pacman;
	public GameManager gameManager;
	public SpriteRenderer spriteRenderer;
	public AudioSource audioSource;

	float time;

	void OnEnable(){
		time = Random.Range (9, 10);
	}

	// Update is called once per frame
	void Update () {
		CheckDistance ();
		RemainingTime ();
	}

	void CheckDistance(){
		if (Vector2.Distance (pacman.position, transform.position) <= 1) {
			gameManager.AddPoint (points);
			audioSource.Play ();
			gameObject.SetActive (false);
		}
	}

	void RemainingTime(){

		time -= Time.deltaTime;

		if (time <= 0)
			gameObject.SetActive (false);
	}
}
