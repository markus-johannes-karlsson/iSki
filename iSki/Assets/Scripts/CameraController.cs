using UnityEngine;

/// <summary>
/// Follow players y-position.
/// </summary>
public class CameraController : MonoBehaviour {

    private Transform player;

	private void Start ()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	private void LateUpdate ()
    {
        transform.position = new Vector3(0.0f, player.position.y - 3.0f, -10.0f);
	}
}
