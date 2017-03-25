using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Popup : MonoBehaviour {

    [SerializeField]
    Transform origin;
    Camera viewpoint;

    [SerializeField]
    Text text;

    [SerializeField]
    bool fitToScreen = false;
    [SerializeField]
    float screenRatio = 0.05f;

    RectTransform rectTransform;

    void Awake ()
    {
        viewpoint = GameController.GameManager.Viewpoint;
        text.resizeTextForBestFit = fitToScreen;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update ()
    {
        if (fitToScreen)
        {
            rectTransform.sizeDelta = screenRatio * viewpoint.pixelRect.size;
        }
        transform.position = viewpoint.WorldToScreenPoint(origin.position);
	}

    public string Text
    {
        set { text.text = value; }
    }

    public Transform Origin
    {
        set
        {
            origin = value;
            transform.position = viewpoint.WorldToScreenPoint(origin.position);
        }
    }
}
