using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform player;
    public Transform leftEnd;
    public Transform rightEnd;

    private Camera viewpoint;

	void Start ()
    {
        viewpoint = GetComponent<Camera>();
	}
	
	void Update ()
    {
        if(GameController.GetGameManager().IsGameOn())
        {
            Vector3 position = transform.position;
            float viewpointWidth = viewpoint.orthographicSize * viewpoint.aspect;
            position.x = Mathf.Min(Mathf.Max(player.position.x, leftEnd.position.x + viewpointWidth), rightEnd.position.x - viewpointWidth);
            transform.position = position;
        }
	}
}
