using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform player;

    private BoxCollider2D viewport;
    private Camera viewpoint;

	void Start ()
    {
        viewport = GetComponent<BoxCollider2D>();
        viewpoint = GetComponent<Camera>();
	}
	
	void Update ()
    {
        //Vector3 position = transform.position;
        //position.x = player.position.x;
        //transform.position = position;

        viewport.size = new Vector2(2f * viewpoint.orthographicSize * viewpoint.aspect, 2f * viewpoint.orthographicSize);
	}
}
