using UnityEngine;
using System.Collections;

public class SignController : MonoBehaviour {

    public string text;
    public Vector2 size;

    public PopupController popup;
    public Transform popupOrigin;
    private PopupController popupInstance;

    private GameObject gameCanvas;

    void Start ()
    {
        gameCanvas = GameController.GameManager.MainCanvas;
    }

    void Awake ()
    {
        text = text.Replace(",", "\n");
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !popupInstance)
        {
            popupInstance = Instantiate(popup);
            popupInstance.origin = popupOrigin;
            popupInstance.transform.SetParent(gameCanvas.transform, false);
            popupInstance.Text = text;
            popupInstance.GetComponent<RectTransform>().sizeDelta = size;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && popupInstance)
        {
            Destroy(popupInstance.gameObject);
        }
    }
}
