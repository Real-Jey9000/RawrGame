using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform manualTarget;
    [SerializeField] private Vector3 offset = new Vector3(5f, 1f, -10f);
    [SerializeField] private float smoothSpeed = 8f;

    private void LateUpdate()
    {
        Transform target = manualTarget;

        // Wenn kein manuelles Target gesetzt ist -> Multiplayer-Logik
        if (target == null)
        {
            if (movementGhost.LocalScenePlayerInstance != null && movementGhost.LocalScenePlayerInstance.IsAlive)
                target = movementGhost.LocalScenePlayerInstance.transform;
            else if (movementGhost.GetLivingPartner() != null)
                target = movementGhost.GetLivingPartner().transform;
            else if (GameRunnerAnchor.Instance != null)
                target = GameRunnerAnchor.Instance.transform;
        }

        if (target != null)
        {
            Vector3 desiredPosition = new Vector3(target.position.x + offset.x, transform.position.y, offset.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}