using UnityEngine;
using System.Collections;

public class DamagePopupController : MonoBehaviour {

    public Transform origin;
    private Camera viewpoint;

    void Start ()
    {
        viewpoint = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Awake()
    {
        Destroy(gameObject, 0.5f);
    }

    void Update () {
        transform.position = viewpoint.WorldToScreenPoint(origin.position);
	}
}
