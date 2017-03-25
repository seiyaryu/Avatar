using UnityEngine;
using System.Collections;

public class Sign : MonoBehaviour {

    public string text;
    public Vector2 size;

    public Popup popup;
    public Transform popupOrigin;
    private Popup popupInstance;

    private GameObject gameCanvas;

    void Awake ()
    {
        gameCanvas = GameController.GameManager.MainCanvas;
        text = text.Replace(",", "\n");
    }

    void OnTriggerEnter2D (Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !popupInstance)
        {
            popupInstance = Instantiate(popup);
            popupInstance.transform.SetParent(gameCanvas.transform, false);
            popupInstance.Origin = popupOrigin;
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
