using System.Collections;
using UnityEngine;

public class CelestialParallax : MonoBehaviour
{
    private enum CyclePhase
    {
        SunMoving,
        TransitionToNight,
        MoonMoving,
        TransitionToDay
    }

    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject moon;
    [SerializeField] private Camera targetCamera;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float delayBeforeMoonRise = 1.5f;
    [SerializeField] private float delayBeforeSunRise = 1.5f;

    [SerializeField] private Color dayColor = new Color(0.3f, 0.6f, 0.95f);
    [SerializeField] private Color nightColor = new Color(0.02f, 0.02f, 0.05f);
    [SerializeField] private float transitionSpeed = 1.5f;

    private Color targetColor;
    private CyclePhase currentPhase = CyclePhase.SunMoving;
    private SpriteRenderer sunRenderer;
    private SpriteRenderer moonRenderer;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (sun != null) sunRenderer = sun.GetComponentInChildren<SpriteRenderer>();
        if (moon != null) moonRenderer = moon.GetComponentInChildren<SpriteRenderer>();

        targetColor = dayColor;
        if (targetCamera != null)
        {
            targetCamera.backgroundColor = dayColor;
        }

        if (moon != null)
        {
            ParkOffscreen(moon);
        }

        currentPhase = CyclePhase.SunMoving;
    }

    private void Update()
    {
        UpdateColorTransition();

        switch (currentPhase)
        {
            case CyclePhase.SunMoving:
                if (sun != null)
                {
                    sun.transform.position += Vector3.left * (speed * Time.deltaTime);

                    if (IsCompletelyOffscreenLeft(sun, sunRenderer))
                    {
                        StartCoroutine(HandleSunsetTransition());
                    }
                }
                break;

            case CyclePhase.MoonMoving:
                if (moon != null)
                {
                    moon.transform.position += Vector3.left * (speed * Time.deltaTime);

                    if (IsCompletelyOffscreenLeft(moon, moonRenderer))
                    {
                        StartCoroutine(HandleSunriseTransition());
                    }
                }
                break;

            case CyclePhase.TransitionToNight:
            case CyclePhase.TransitionToDay:
                break;
        }
    }

    private IEnumerator HandleSunsetTransition()
    {
        currentPhase = CyclePhase.TransitionToNight;
        targetColor = nightColor;

        ParkOffscreen(sun);

        yield return new WaitForSeconds(delayBeforeMoonRise);

        if (moon != null)
        {
            SpawnAtRightEdge(moon, moonRenderer);
        }

        currentPhase = CyclePhase.MoonMoving;
    }

    private IEnumerator HandleSunriseTransition()
    {
        currentPhase = CyclePhase.TransitionToDay;
        targetColor = dayColor;

        ParkOffscreen(moon);

        yield return new WaitForSeconds(delayBeforeSunRise);

        if (sun != null)
        {
            SpawnAtRightEdge(sun, sunRenderer);
        }

        currentPhase = CyclePhase.SunMoving;
    }

    private bool IsCompletelyOffscreenLeft(GameObject obj, SpriteRenderer rend)
    {
        if (targetCamera == null || obj == null) return false;

        float radius = rend != null ? rend.bounds.extents.x : 1f;
        Vector3 rightEdgeWorldPos = obj.transform.position + new Vector3(radius, 0f, 0f);

        Vector3 viewportPoint = targetCamera.WorldToViewportPoint(rightEdgeWorldPos);
        return viewportPoint.x < 0f;
    }

    private void SpawnAtRightEdge(GameObject obj, SpriteRenderer rend)
    {
        if (targetCamera == null || obj == null) return;

        float radius = rend != null ? rend.bounds.extents.x : 1f;

        float cameraDistance = Mathf.Abs(targetCamera.transform.position.z - obj.transform.position.z);
        Vector3 rightViewportEdge = new Vector3(1.0f, 0.5f, cameraDistance);
        Vector3 worldEdge = targetCamera.ViewportToWorldPoint(rightViewportEdge);

        float targetX = worldEdge.x + radius + 0.1f;
        obj.transform.position = new Vector3(targetX, obj.transform.position.y, obj.transform.position.z);
    }

    private void ParkOffscreen(GameObject obj)
    {
        if (obj == null) return;
        obj.transform.position = new Vector3(9999f, obj.transform.position.y, obj.transform.position.z);
    }

    private void UpdateColorTransition()
    {
        if (targetCamera == null) return;

        targetCamera.backgroundColor = Color.Lerp(
            targetCamera.backgroundColor,
            targetColor,
            transitionSpeed * Time.deltaTime
        );
    }
}