using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextPopup : MonoBehaviour
{
    public AnimationCurve opacityCurve;
    public AnimationCurve scaleCurve;
    public AnimationCurve heightCurve;
    public float duration;

    public TextMeshProUGUI label;
    private float time = 0;
    private Vector3 origin;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        origin = transform.position;
        label = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (time > duration)
        {
            //Parent is the Canvas
            Destroy(gameObject.transform.parent.gameObject);
        }
        transform.forward = cam.transform.forward;
        label.color = new Color(label.color.r, label.color.g, label.color.b, opacityCurve.Evaluate(time));
        transform.localScale = Vector3.one * scaleCurve.Evaluate(time);
        transform.position = origin + new Vector3(0, 1 + heightCurve.Evaluate(time), 0);
        time += Time.deltaTime;
    }
}
