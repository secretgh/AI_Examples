using UnityEngine;

public class NPCEventPopup : MonoBehaviour
{
    public GameObject PopupPrefab;

    public void SpawnTextPopup(Transform target, Vector3 offset, float duration, string text, Color color) {
        var popup = Instantiate(PopupPrefab, target);
        popup.transform.localPosition = offset;

        TextPopup popupText = popup.GetComponentInChildren<TextPopup>();
        popupText.duration = duration;
        popupText.label.text = text;
        popupText.label.color = color;
    }
}