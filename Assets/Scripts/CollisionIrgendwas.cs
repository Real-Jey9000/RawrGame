using UnityEngine;

public class CollisionIrgendwas : MonoBehaviour
{
    public static CollisionIrgendwas Instance { get; private set; }

    [Header("Mode Configuration")]
    [Tooltip("Haken an = Multiplayer mit movementGhost & Respawn. Haken aus = Singleplayer mit direktem Freeze & Canvas.")]
    [SerializeField] private bool isMultiplayer = true;

    [Header("UI & Audio")]
    [SerializeField] private GameObject Canvas;
    [SerializeField] private Soundplayer deathSoundplayer;

    private void Awake()
    {
        movementGhost ghost = GetComponent<movementGhost>();

        // Nur als globale Instanz setzen, wenn es der echte Szenen-Spieler ist (kein Geist!)
        if (ghost == null || ghost.IsMyScenePlayer)
        {
            Instance = this;
        }

        // Automatischer Fallback, falls im Inspector vergessen wurde, das Canvas reinzuziehen
        if (Canvas == null)
        {
            Canvas = FindDeathCanvasInScene();
        }

        // Automatischer Fallback für den Soundplayer, falls nicht im Inspector zugewiesen
        if (deathSoundplayer == null)
        {
            deathSoundplayer = GetComponent<Soundplayer>();
        }
    }

    private void Start()
    {
        if (Canvas != null)
        {
            Canvas.SetActive(false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            if (isMultiplayer)
            {
                HandleMultiplayerDeath();
            }
            else
            {
                HandleSingleplayerDeath();
            }
        }
    }

    private void HandleSingleplayerDeath()
    {
        TriggerDeathVisuals();

        if (deathSoundplayer != null)
        {
            deathSoundplayer.playRandom();
        }

        Save saveComp = GetComponent<Save>();
        if (saveComp != null)
        {
            saveComp.SaveScore();
        }
    }

    private void HandleMultiplayerDeath()
    {
        movementGhost ghost = GetComponent<movementGhost>();

        if (ghost != null && ghost.IsMyScenePlayer && ghost.IsAlive)
        {
            if (deathSoundplayer != null)
            {
                deathSoundplayer.playRandom();
            }

            ghost.KillPlayer();
        }
    }

    public void TriggerGlobalDeath()
    {
        TriggerDeathVisuals();

        // 1. Erst auf diesem GameObject suchen, wenn nicht da -> überall in der Szene suchen
        Save saveComp = GetComponent<Save>();
        if (saveComp == null)
        {
            saveComp = FindObjectOfType<Save>();
        }

        if (saveComp != null)
        {
            Debug.Log("[CollisionIrgendwas] Rufe SaveScore() auf...");
            saveComp.SaveScore();
        }
        else
        {
            Debug.LogError("[CollisionIrgendwas] FEHLER: Kein Save-Skript in der Szene gefunden!");
        }
    }

    private void TriggerDeathVisuals()
    {
        if (Canvas == null)
        {
            Canvas = FindDeathCanvasInScene();
        }

        if (Canvas != null)
        {
            Canvas.SetActive(true);
        }
        else
        {
            Debug.LogError("[CollisionIrgendwas] Konnte in der gesamten Szene kein Canvas finden!", this);
        }

        Time.timeScale = 0f;
    }

    private GameObject FindDeathCanvasInScene()
    {
        // Sucht alle Canvases in der Szene (auch inaktive)
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            // Ignoriere Prefabs im Asset-Ordner, nimm nur Objekte aus der aktiven Szene
            if (c.gameObject.scene.isLoaded)
            {
                // Wenn der Name "Death" oder "GameOver" enthält, haben wir das richtige
                if (c.gameObject.name.ToLower().Contains("death") || c.gameObject.name.ToLower().Contains("over"))
                {
                    return c.gameObject;
                }
            }
        }

        // Not-Fallback: Nimm das erste gefundene Szenen-Canvas
        foreach (Canvas c in allCanvases)
        {
            if (c.gameObject.scene.isLoaded)
            {
                return c.gameObject;
            }
        }

        return null;
    }
}