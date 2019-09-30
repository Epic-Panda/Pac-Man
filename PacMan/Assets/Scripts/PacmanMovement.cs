using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacmanMovement : MonoBehaviour
{
	[Header ("Tile attachment")]
	public Tile dotTile;
	public Tile energizedTile;
	public Tilemap tMap;
	public Grid grid;

	[Header ("Attachments")]
	public GameManager gameManager;
	public Rigidbody2D rb;
	public Animator animController;

	[Header ("Sound")]
	public AudioSource audioSource;
	public AudioClip death;
	public AudioClip eatDots;

	[Header ("Atributes")]
	public float maxSpeed = 9f;
	public float speedProcentNormal = 0.8f;
	public float speedProcentDots = 0.71f;
	public float speedProcentFrightened = 0.9f;
	public float speedProcentEnergized = 0.79f;
	public Vector2 startPos;
	public float leftTunelPos = -13.56f;
	public float rightTunelPos = 13.44f;

	[HideInInspector]
	public Vector2 direction;
	Vector2 oldDir;

	bool frighten = false;
	float speed = 8f;
	Vector2 dest;
	BoxCollider2D box;
	bool preturnHalf = false;
	bool preturnLow = false;

	void Start ()
	{
		PacmanStart ();	
	}

	/// <summary>
	/// Defines pacman to start with starting atributes.
	/// </summary>
	public void PacmanStart ()
	{
		transform.position = startPos;
		box = transform.GetComponent<BoxCollider2D> ();
		dest = (Vector2)transform.position;
		oldDir = Vector2.left;
		direction = Vector2.left;
		animController.SetBool ("left", true);
		animController.SetBool ("up", false);
		animController.SetBool ("right", false);
		animController.SetBool ("down", false);

		preturnHalf = false;
		preturnLow = false;

		speed = maxSpeed * speedProcentNormal;
	}

	// Update is called once per frame
	void Update ()
	{
		PacmanMoveDirectionInput ();
	}

	/// <summary>
	/// Handles player input
	/// </summary>
	void PacmanMoveDirectionInput ()
	{
		
		if ((Vector2)transform.position == dest && preturnLow == false && preturnHalf == false) {
			if (Input.GetKey (KeyCode.UpArrow) && ValidPath (Vector2.up)) {
				direction = Vector2.up;
				if (oldDir != direction) {
					ResetAnim ();
					animController.SetBool ("up", true);
				}
			}
			if (Input.GetKey (KeyCode.DownArrow) && ValidPath (Vector2.down)) {
				direction = Vector2.down;
				if (oldDir != direction) {
					ResetAnim ();
					animController.SetBool ("down", true);
				}
			}
			if (Input.GetKey (KeyCode.LeftArrow) && ValidPath (Vector2.left)) {
				direction = Vector2.left;
				if (oldDir != direction) {
					ResetAnim ();
					animController.SetBool ("left", true);
				}
			}
			if (Input.GetKey (KeyCode.RightArrow) && ValidPath (Vector2.right)) {
				direction = Vector2.right;
				if (oldDir != direction) {
					ResetAnim ();
					animController.SetBool ("right", true);
				}
			}
		} // pacman preturn, used to escape ghosts, causing pacman to go to next destination before reaching old one, this can be helpful in higher levels
		else if (preturnLow == false && preturnHalf == false) {
			if (direction == Vector2.left || direction == Vector2.right) {

				// if half preturn
				if ((rb.position - dest).magnitude >= 0.25f && (rb.position - dest).magnitude <= 0.5f) {

					if (Input.GetKey (KeyCode.UpArrow) && ValidPath (Vector2.up)) {

						direction = Vector2.up;
						dest += Vector2.up / 2;
						preturnHalf = true;

						ResetAnim ();
						animController.SetBool ("up", true);

					} else if (Input.GetKey (KeyCode.DownArrow) && ValidPath (Vector2.down)) {

						direction = Vector2.down;
						dest += Vector2.down / 2;
						preturnHalf = true;

						ResetAnim ();
						animController.SetBool ("down", true);
					}
				} // else if low preturn
				else if ((rb.position - dest).magnitude > 0f && (rb.position - dest).magnitude < 0.25f) {

					if (Input.GetKey (KeyCode.UpArrow) && ValidPath (Vector2.up)) {

						direction = Vector2.up;
						dest += Vector2.up / 4;
						preturnLow = true;

						ResetAnim ();
						animController.SetBool ("up", true);

					} else if (Input.GetKey (KeyCode.DownArrow) && ValidPath (Vector2.down)) {

						direction = Vector2.down;
						dest += Vector2.down / 4;
						preturnLow = true;

						ResetAnim ();
						animController.SetBool ("down", true);
					}
				}
			} 
			// if pacman moving up - down preturn can be left or right
		else if (direction == Vector2.up || direction == Vector2.down) {

				// if half preturn
				if ((rb.position - dest).magnitude >= 0.25f && (rb.position - dest).magnitude <= 0.5f) {

					if (Input.GetKey (KeyCode.LeftArrow) && ValidPath (Vector2.left)) {

						direction = Vector2.left;
						dest += Vector2.left / 2;
						preturnHalf = true;

						ResetAnim ();
						animController.SetBool ("left", true);

					} else if (Input.GetKey (KeyCode.RightArrow) && ValidPath (Vector2.right)) {

						direction = Vector2.right;
						dest += Vector2.right / 2;
						preturnHalf = true;

						ResetAnim ();
						animController.SetBool ("right", true);
					}
				} // else if low preturn
				else if ((rb.position - dest).magnitude > 0f && (rb.position - dest).magnitude < 0.25f) {

					if (Input.GetKey (KeyCode.LeftArrow) && ValidPath (Vector2.left)) {

						direction = Vector2.left;
						dest += Vector2.left / 4;
						preturnLow = true;

						ResetAnim ();
						animController.SetBool ("left", true);

					} else if (Input.GetKey (KeyCode.RightArrow) && ValidPath (Vector2.right)) {

						direction = Vector2.right;
						dest += Vector2.right / 4;
						preturnLow = true;

						ResetAnim ();
						animController.SetBool ("right", true);
					}
				}
			}
		}
	}

	/// <summary>
	/// Reset animation direction
	/// </summary>
	void ResetAnim ()
	{
		if (oldDir == Vector2.up)
			animController.SetBool ("up", false);
		else if (oldDir == Vector2.left)
			animController.SetBool ("left", false);
		else if (oldDir == Vector2.down)
			animController.SetBool ("down", false);
		else if (oldDir == Vector2.right)
			animController.SetBool ("right", false);

		oldDir = direction;
	}

	void FixedUpdate ()
	{
		PacmanMove ();

		EatDots ();
	}

	/// <summary>
	/// Moves pacman
	/// </summary>
	void PacmanMove ()
	{
		// using tunnel
		if (rb.position.x < leftTunelPos) {
			transform.position = new Vector3 (rightTunelPos, transform.position.y);
			dest = new Vector2 (rightTunelPos, transform.position.y) + direction;
		} else if (rb.position.x > rightTunelPos) {
			transform.position = new Vector3 (leftTunelPos, transform.position.y);
			dest = new Vector2 (leftTunelPos, transform.position.y) + direction;
		}

		Vector2 move = Vector2.MoveTowards (transform.position, dest, speed * Time.deltaTime);
		rb.MovePosition (move);

		// check if path is valid
		if (rb.position == dest) {
			// set speed
			if (frighten)
				speed = maxSpeed * speedProcentFrightened;
			else
				speed = maxSpeed * speedProcentNormal;

			if (preturnHalf) {
				// return to regular path
				dest += direction / 2;
				preturnHalf = false;
			} // else if preturn cuart
			else if (preturnLow) {
				dest += 3 * direction / 4;
				preturnLow = false;
			}
			// if preturn wasn't used go as usual
			else {
				if (ValidPath (direction))
					dest += direction;
				else
					ResetAnim ();
			}
		}
	}

	/// <summary>
	/// Checks if pacman is above dot, eat it and add points
	/// </summary>
	void EatDots ()
	{
		
		Vector3Int tilePos = grid.WorldToCell (transform.position);
		if (tMap.GetTile (tilePos) == dotTile) {

			if (audioSource.clip != eatDots)
				audioSource.clip = eatDots;

			if (!audioSource.isPlaying)
				audioSource.Play ();

			speed = maxSpeed * speedProcentDots;
			gameManager.AddPoint (10);
			gameManager.AddDots ();
			tMap.SetTile (tilePos, null);
		} else if (tMap.GetTile (tilePos) == energizedTile) {

			if (audioSource.clip != eatDots)
				audioSource.clip = eatDots;

			audioSource.Play ();

			speed = maxSpeed * speedProcentEnergized;
			gameManager.AddPoint (50);
			gameManager.ScareGhosts ();
			gameManager.AddEnergizer ();
			tMap.SetTile (tilePos, null);
		}
	}

	/// <summary>
	/// Pacman is eaten
	/// </summary>
	public void Eaten ()
	{
		audioSource.loop = false;
		audioSource.clip = death;
		audioSource.Play ();

		gameManager.RemoveLife ();
	}

	/// <summary>
	/// Checks if current path is valid
	/// </summary>
	/// <returns><c>true</c>, if path was valided, <c>false</c> otherwise.</returns>
	/// <param name="dir">Direction that pacman moves.</param>
	bool ValidPath (Vector2 dir)
	{
		int layerMask = (1 << 9);
		RaycastHit2D hit = Physics2D.BoxCast (dest, box.size, 0, dir, 1, layerMask);
		return hit.collider == null;
	}
}