using UnityEngine;

/*
* my movement logic:
* dont check directon comming from - reason: player can go left and right and confuse ai
* check if path is valid, if it is then measure distance from center to player
* compare all distance, if there is 2 same shortest go this order of priority -> Up > Left > Down
*/

public class PathAI : MonoBehaviour
{
	int hitWallMask;

	Vector2 boxSize;
	float boxCastDistance;
	float boxAngle;

	void Start ()
	{
		boxSize = Vector2.one;
		boxCastDistance = 1.25f;
		boxAngle = 0;

		hitWallMask = 1 << 9;
	}

	/// <summary>
	/// Finds direction where to move next.
	/// </summary>
	/// <returns>Return closest path from start to end</returns>
	/// <param name="_startPos">Start position.</param>
	/// <param name="_endPos">End position.</param>
	/// <param name="_currDirection">Curr direction.</param>
	public Vector2 FindBestPath (Vector2 _startPos, Vector2 _endPos, Vector2 _currDirection, bool frightened)
	{
		Vector2 startPos = _startPos;
		Vector2 endPos = _endPos;
		Vector2 currDirection = _currDirection;

		float leftDis = Mathf.Infinity;
		float rightDis = Mathf.Infinity;
		float upDis = Mathf.Infinity;
		float downDis = Mathf.Infinity;

		if (currDirection.x > 1)
			currDirection.x = 1;
		if (currDirection.x < -1)
			currDirection.x = -1;

		// cast rays
		RaycastHit2D hit;

		if (-currDirection != Vector2.right) {
			hit = Physics2D.BoxCast (startPos, boxSize, boxAngle, Vector2.right, boxCastDistance, hitWallMask);
			if (hit.collider == null) {
				rightDis = Vector2.Distance (startPos + Vector2.right, endPos);
			}
		}

		if (-currDirection != Vector2.left) {
			hit = Physics2D.BoxCast (startPos, boxSize, boxAngle, Vector2.left, boxCastDistance, hitWallMask);
			if (hit.collider == null) {
				leftDis = Vector2.Distance (startPos + Vector2.left, endPos);
			}
		}

		if (-currDirection != Vector2.up) {
			hit = Physics2D.BoxCast (startPos, boxSize, boxAngle, Vector2.up, boxCastDistance, hitWallMask);
			if (hit.collider == null) {
				upDis = Vector2.Distance (startPos + Vector2.up, endPos);
			}
		}

		if (-currDirection != Vector2.down) {
			hit = Physics2D.BoxCast (startPos, boxSize, boxAngle, Vector2.down, boxCastDistance, hitWallMask);
			if (hit.collider == null) {
				downDis = Vector2.Distance (startPos + Vector2.down, endPos);
			}
		}


		// up resirected areas
		if(!frightened)
		if (startPos.x >= -1.5f && startPos.x <= 1.5f) {
			if (startPos.y == 3.5f || startPos.y == -8.5f)
				upDis = Mathf.Infinity;
		}

		// up
		if (upDis <= leftDis && upDis <= downDis && upDis <= rightDis)
			return Vector2.up;

		// left
		if (leftDis < upDis && leftDis <= downDis && leftDis <= rightDis)
			return Vector2.left;

		// down
		if (downDis < upDis && downDis < leftDis && downDis <= rightDis)
			return Vector2.down;

		// right
		return Vector2.right;
	}
}