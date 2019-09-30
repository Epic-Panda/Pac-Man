using System.Collections;
using UnityEngine;

public class BlinkysAI : MonoBehaviour
{
	[Header ("Attachments")]
	public PathAI directionFinder;
	public Transform player;
	public Transform blinkyT;
	public Rigidbody2D rb;
	public Animator animController;
	public SpriteRenderer ghostRend;
	public AudioSource audioSource;

	[Header ("Atributes")]
	public float maxSpeed = 9;
	public float speedProcentNormal = 0.75f;
	public float speedProcentFrighten = 0.5f;
	public float speedProcentTunnel = 0.4f;
	public float elroySpeedProcent = 0.8f;
	public Vector2 fixedTarget;
	public Vector2 startingPos;
	public float tunnelLeft = -13.5f;
	public float tunnelRight = 13.5f;
	public float[] scatterChaseTime = new float[]{ 7, 20, 7, 20, 5, 20, 5 };
	public float frightenedTime = 6;
	public int leaveHouseDotNum = 0;

	[Header ("Type")]
	public bool blinky = true;
	public bool pinky = false;
	public bool inky = false;
	public bool clyde = false;

	[Header ("Mode")]
	public bool chase = false;
	public bool scatter = true;
	public bool frightened = false;

	[Header ("State")]
	public bool inHouse;
	public bool etable;
	public bool elroy;

	float speed;
	public bool leavingHouse;

	float timer;
	int timeIndex;
	float frightenedTimer = 0;

	private PacmanMovement pacman;
	private BlinkysAI blinkyAI;

	Vector2 nextDirection;
	Vector2 destination;
	Vector2 oldDirection;
	Vector2 move;
	string oldDir;
	bool changeDir;
	[HideInInspector]
	public int dotCount;
	[HideInInspector]
	public bool dotCounterEnabled;

	bool start;
	// Use this for initialization
	void Start ()
	{
		StartNewLevel ();
	}

	public void StartNewLevel ()
	{
		dotCount = 0;
		dotCounterEnabled = true;
		elroy = false;
		GhostStart ();
	}

	/// <summary>
	/// Sets ghost at start position with starting atributes.
	/// </summary>
	public void GhostStart ()
	{
		start = true;
		leavingHouse = false;

		rb.position = startingPos;

		// set speed
		if (!elroy)
			speed = maxSpeed * speedProcentNormal;
		else
			speed = maxSpeed * elroySpeedProcent;
			
		nextDirection = Vector2.left;
		destination = rb.position + nextDirection * 1.5f;
		oldDir = "left";

		// sets states to default
		etable = false;
		chase = false;
		inHouse = false;
		scatter = true;
		frightened = false;
		changeDir = false;

		// sets frightened timers to default
		timer = 0;
		timeIndex = 0;
		frightenedTimer = 0;

		// pacman's script
		pacman = player.GetComponent<PacmanMovement> ();

		// use blinky's script to follow him
		if (inky) {
			blinkyAI = blinkyT.GetComponent<BlinkysAI> ();
		}

		// set animations to default
		animController.SetBool ("frightened", false);
		animController.SetBool ("blink", false);
		animController.SetBool ("up", false);
		animController.SetBool ("left", false);
		animController.SetBool ("down", false);
		animController.SetBool ("right", false);

		if (blinky)
			animController.SetBool ("left", true);
		else if (pinky) {
			animController.SetBool ("down", true);
			inHouse = true;
		} else if (inky || clyde) {
			animController.SetBool ("up", true);
			inHouse = true;
		}

		// if they dont have to wait for dots leave house now
		if (inHouse && !leavingHouse && dotCounterEnabled && dotCount >= leaveHouseDotNum)
			StartCoroutine (LeaveHouse ());
	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		if (inHouse)
			return;
		
		// clearing start position bug on restart ghost pos
		if (blinky && start) {
			destination = startingPos + Vector2.left * 1.5f;
			start = false;
		}

		GhostMove ();
	}

	/// <summary>
	/// Ghosts movement.
	/// </summary>
	void GhostMove ()
	{
		// if ghost is at destination, change his destination
		if (rb.position == destination) {
			// set speed
			if (frightened)
				speed = maxSpeed * speedProcentFrighten;
			else if (!elroy)
				speed = maxSpeed * speedProcentNormal;
			else if (elroy)
				speed = maxSpeed * elroySpeedProcent;
			

			if (changeDir) {
				nextDirection = -oldDirection;
				changeDir = false;
			}

			// used only for changing direction, cause ghost looks one field in front of him, he cant reverse next direction, it may cause unnotices collision
			oldDirection = nextDirection;

			//	if is in tunnel
			if (rb.position.y == 0.5f) {
				//	ghost is in tunnel, reduce speed
				if (rb.position.x > 8.5f || rb.position.x < -8.5f)
					speed = maxSpeed * speedProcentTunnel;

				// using tunnel edge
				if (rb.position.x == tunnelLeft)
					transform.position = new Vector3 (tunnelRight, transform.position.y);
				else if (rb.position.x == tunnelRight)
					transform.position = new Vector3 (tunnelLeft, transform.position.y);
			}

			// if frightened check if pacman eat him
			if (frightened) {
				if (Vector2.Distance (pacman.rb.position, transform.position) <= 1.5f) {
					Eaten ();

					// play sound
					audioSource.Play ();
					return;
				}
			} // if ghost is not frightened check if he eat pacman
			else if (Vector2.Distance (pacman.rb.position, transform.position) <= 1) {

				Vector2 tmpDir = (pacman.rb.position - rb.position).normalized;

				//check this so ghost cannot catch pacman if they are not in same tile
				if (tmpDir == Vector2.up || tmpDir == Vector2.down || tmpDir == Vector2.left || tmpDir == Vector2.right || tmpDir == Vector2.zero) {
					if (!frightened) {
						pacman.Eaten ();
						return;
					}
				}
			}

			destination = rb.position + nextDirection;
			AnimDirection ();

			// checking state of behaviour and direction
			if (frightened) {
				nextDirection = directionFinder.FindBestPath (rb.position + nextDirection, rb.position + new Vector2 (Random.Range (-5, 5), Random.Range (-5, 5)), nextDirection, frightened);
			} else if (chase) {
				if (blinky)
					BlinkyMovement ();
				else if (pinky)
					PinkyMovement ();
				else if (inky)
					InkyMovement ();
				else if (clyde)
					CladeMovement ();
			} else if (scatter) {
				// if blinky is Elroy he chase pacman in scatter mode too
				if (elroy)
					nextDirection = directionFinder.FindBestPath (rb.position + nextDirection, pacman.rb.position, nextDirection, frightened);
				else
					nextDirection = directionFinder.FindBestPath (rb.position + nextDirection, fixedTarget, nextDirection, frightened);
			}
		}
		
		move = Vector2.MoveTowards (rb.position, destination, speed * Time.deltaTime);
		rb.MovePosition (move);

	}

	void Update ()
	{
		if (frightened) {
			frightenedTimer += Time.deltaTime;

			// blink animation
			if (frightenedTimer >= frightenedTime - .5f) {
				animController.SetBool ("frightened", false);
				animController.SetBool ("blink", true);
			}

			if (frightenedTimer >= frightenedTime) {
				animController.SetBool ("blink", false);
				frightenedTimer = 0;
				etable = false;
				frightened = false;
			}

			return;
		}

		if (inHouse)
			return;

		if (timeIndex == scatterChaseTime.Length)
			return;
		
		timer += Time.deltaTime;

		if (timer >= scatterChaseTime [timeIndex]) {
			timer = 0;
			SwitchState ();
			timeIndex++;
		}
	}

	/// <summary>
	/// Set animation direction
	/// </summary>
	void AnimDirection ()
	{
		if (frightened) {
			animController.SetBool ("up", false);
			animController.SetBool ("down", false);
			animController.SetBool ("right", false);
			animController.SetBool ("left", false);
			return;
		} else if (animController.GetBool ("frightened") || animController.GetBool ("blink")) {
			animController.SetBool ("frightened", false);
			animController.SetBool ("blink", false);
		}

		if (nextDirection == Vector2.left) {
			if (oldDir != "left") {
				animController.SetBool (oldDir, false);
				oldDir = "left";
				animController.SetBool ("left", true);
			}
		} else if (nextDirection == Vector2.right) {
			if (oldDir != "right") {
				animController.SetBool (oldDir, false);
				oldDir = "right";
				animController.SetBool ("right", true);
			}
		} else if (nextDirection == Vector2.up) {
			if (oldDir != "up") {
				animController.SetBool (oldDir, false);
				oldDir = "up";
				animController.SetBool ("up", true);
			}
		} else if (nextDirection == Vector2.down) {
			if (oldDir != "down") {
				animController.SetBool (oldDir, false);
				oldDir = "down";
				animController.SetBool ("down", true);
			}
		}
	}

	/// <summary>
	/// Ghost is eaten
	/// </summary>
	public void Eaten ()
	{
		// add score depending of energizer
		if (pacman.gameManager.ghostEaten == 0) {
			pacman.gameManager.AddPoint (200);
			pacman.gameManager.ghostEaten++;
		} else if (pacman.gameManager.ghostEaten == 1) {
			pacman.gameManager.AddPoint (400);
			pacman.gameManager.ghostEaten++;
		} else if (pacman.gameManager.ghostEaten == 2) {
			pacman.gameManager.AddPoint (800);
			pacman.gameManager.ghostEaten++;
		} else {
			pacman.gameManager.AddPoint (1600);
			pacman.gameManager.ghostEaten++;
		}
		// set ghost to start again
		GhostStart ();
	}

	/// <summary>
	/// Frightened ghost setup
	/// </summary>
	public void Frighten ()
	{
		frightened = true;
		changeDir = true;
		etable = true;
		frightenedTimer = 0;
		animController.SetBool ("blink", true);
		animController.SetBool ("frightened", true);
	}

	/// <summary>
	/// switch between 2 states
	/// </summary>
	void SwitchState ()
	{
		changeDir = true;
		chase = !chase;
		scatter = !scatter;
	}

	/// <summary>
	/// Blinky's next position.
	/// </summary>
	void BlinkyMovement ()
	{
		nextDirection = directionFinder.FindBestPath (rb.position + nextDirection, (Vector2)player.position, nextDirection, frightened);
	}

	/// <summary>
	/// Pinky's next position.
	/// </summary>
	void PinkyMovement ()
	{
		nextDirection = directionFinder.FindBestPath (rb.position + nextDirection, pacman.rb.position + pacman.direction * 4, nextDirection, frightened);
	}

	/// <summary>
	/// Inky's next position.
	/// </summary>
	void InkyMovement ()
	{
		Vector2 targetDot = pacman.rb.position + pacman.direction * 2;
		Vector2 neededVector = (targetDot - blinkyAI.rb.position);

		nextDirection = directionFinder.FindBestPath (rb.position + nextDirection, targetDot + neededVector, nextDirection, frightened);
	}

	/// <summary>
	/// Clade's next position.
	/// </summary>
	void CladeMovement ()
	{
		if (Vector2.Distance (pacman.rb.position, rb.position) > 8)
			nextDirection = directionFinder.FindBestPath (rb.position + nextDirection, pacman.rb.position - pacman.direction * 2, nextDirection, frightened);
		else
			nextDirection = directionFinder.FindBestPath (rb.position + nextDirection, fixedTarget, nextDirection, frightened);
	}

	/// <summary>
	/// local counter that spawns ghosts from house, can be deactivated by player that trick ghosts
	/// </summary>
	public void DotsCounter ()
	{
		dotCount++;

		// check if ghost already has started leaving house, if we skip this check function might be called few times
		if (inHouse && !leavingHouse && dotCount >= leaveHouseDotNum)
			StartCoroutine (LeaveHouse ());
	}

	/// <summary>
	/// Leaving ghost house.
	/// </summary>
	public IEnumerator LeaveHouse ()
	{
		leavingHouse = true;

		// just to be sure to get rid of some bug
		transform.position = startingPos;
		
		if (inky) {
			destination = rb.position + Vector2.right * 2;
			while (rb.position != destination) {
				move = Vector2.MoveTowards (rb.position, destination, speed * Time.deltaTime);
				rb.MovePosition (move);
				yield return new WaitForFixedUpdate ();
			}

		} else if (clyde) {
			destination = rb.position + Vector2.left * 2;
			while (rb.position != destination) {
				move = Vector2.MoveTowards (rb.position, destination, speed * Time.deltaTime);
				rb.MovePosition (move);
				yield return new WaitForFixedUpdate ();
			}
		}

		destination = rb.position + Vector2.up * 3;

		while (rb.position != destination) {
			move = Vector2.MoveTowards (rb.position, destination, speed * Time.deltaTime);
			rb.MovePosition (move);
			yield return new WaitForFixedUpdate ();
		}

		destination = rb.position + Vector2.left * 1.5f;
		nextDirection = Vector2.left;

		animController.SetBool ("up", false);
		inHouse = false;
		leavingHouse = false;
	}
}