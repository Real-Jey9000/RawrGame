using UnityEngine;

public class GameRunnerAnchor : MonoBehaviour
{
    public static GameRunnerAnchor Instance { get; private set; }

    [Header("Speed Settings")]
    [SerializeField] private float movementSpeed = 12f;
    private float speedGainPerSecond = 0.05f;

    public float CurrentSpeed => movementSpeed;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!movementGhost.isGameRunning) return;

        // Läuft weiter, solange mindestens ein Spieler am Leben ist
        if (movementGhost.AtLeastOnePlayerAlive())
        {
            if (movementSpeed < 69f)
            {
                movementSpeed += speedGainPerSecond * Time.deltaTime;
            }

            transform.Translate(Vector2.right * (movementSpeed * Time.deltaTime));
        }
    }
}