using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform player;
    public Transform leftEnd;
    public Transform rightEnd;

    private Camera viewpoint;

	void Awake ()
    {
        viewpoint = GetComponent<Camera>();
	}
	
    public void Reset ()
    {
        Vector3 position = transform.position;
        position.x = 0f;
        transform.position = position;
    }

	void Update ()
    {
        if(GameController.GameManager.GameOn)
        {
            Vector3 position = transform.position;
            float viewpointWidth = viewpoint.orthographicSize * viewpoint.aspect;
            position.x = Mathf.Min(Mathf.Max(player.position.x, leftEnd.position.x + viewpointWidth), rightEnd.position.x - viewpointWidth);
            transform.position = position;
        }
	}
}
