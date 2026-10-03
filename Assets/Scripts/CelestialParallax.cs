using UnityEngine;

public class CelestialParallax : MonoBehaviour
{
    [Header("Referenzen")]
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject moon;
    [SerializeField] private Camera targetCamera;

    [Header("Bewegung")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float resetThresholdX = -15f;
    [SerializeField] private float respawnPositionX = 15f;

    [Header("Farben & Übergangs-Geschwindigkeit")]
    [SerializeField] private Color dayColor = new Color(0.3f, 0.6f, 0.95f);
    [SerializeField] private Color nightColor = new Color(0.02f, 0.02f, 0.05f);
    [Tooltip("Wie schnell die Farbe weich wechselt (z. B. 1 = 1 Sekunde für den Übergang)")]
    [SerializeField] private float transitionSpeed = 1.5f;

    [Header("Trigger-Schwellenwert")]
    [Tooltip("Sobald die Sonne dieses X nach links unterschreitet, wird es Nacht")]
    [SerializeField] private float triggerSunsetX = -10f;
    [Tooltip("Sobald der Mond dieses X nach links unterschreitet, wird es Tag")]
    [SerializeField] private float triggerSunriseX = -10f;

    private Color targetColor;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Startfarbe initial auf Tag setzen
        targetColor = dayColor;
        targetCamera.backgroundColor = dayColor;
    }

    private void Update()
    {
        MoveAndWrap(sun);
        MoveAndWrap(moon);

        CheckCycleTriggers();
        UpdateColorTransition();
    }

    private void MoveAndWrap(GameObject celestialBody)
    {
        if (celestialBody == null) return;

        Transform t = celestialBody.transform;
        t.localPosition += Vector3.left * (speed * Time.deltaTime);

        // Teleport nach rechts, sobald weit genug links
        if (t.localPosition.x <= resetThresholdX)
        {
            t.localPosition = new Vector3(respawnPositionX, t.localPosition.y, t.localPosition.z);
        }
    }

    private void CheckCycleTriggers()
    {
        // Sonne verlässt das Bild -> Nacht einleiten
        if (sun != null && sun.transform.localPosition.x <= triggerSunsetX && targetColor != nightColor)
        {
            targetColor = nightColor;
        }

        // Mond verlässt das Bild -> Tag einleiten
        if (moon != null && moon.transform.localPosition.x <= triggerSunriseX && targetColor != dayColor)
        {
            targetColor = dayColor;
        }
    }

    private void UpdateColorTransition()
    {
        if (targetCamera == null) return;

        // Gleitet weich und bildratenunabhängig zur Zielfarbe
        targetCamera.backgroundColor = Color.Lerp(
            targetCamera.backgroundColor,
            targetColor,
            transitionSpeed * Time.deltaTime
        );
    }
}