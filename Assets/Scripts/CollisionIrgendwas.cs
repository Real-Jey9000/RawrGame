using UnityEngine;

public class CollisionIrgendwas : MonoBehaviour
{
    [Header("Mode Configuration")]
    [Tooltip("Haken an = Multiplayer mit movementGhost & Respawn. Haken aus = Singleplayer mit direktem Freeze & Canvas.")]
    [SerializeField] private bool isMultiplayer = true;

    [Header("UI & Audio")]
    [SerializeField] private GameObject Canvas;
    [SerializeField] private AudioClip[] TodClips;
    [SerializeField] private AudioClip[] TodClipsRare;
    [SerializeField] private AudioSource TodHalt;

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
        if (Canvas != null)
        {
            Canvas.SetActive(true);
        }

        Time.timeScale = 0f;

        PlayDeathSound();

        Save saveComp = GetComponent<Save>();
        if (saveComp != null)
        {
            saveComp.SaveScore();
        }
    }

    private void HandleMultiplayerDeath()
    {
        movementGhost ghost = GetComponent<movementGhost>();

        // Nur wenn das MEIN echter lokaler Szenen-Spieler ist und er noch lebt
        if (ghost != null && ghost.IsMyScenePlayer && ghost.IsAlive)
        {
            PlayDeathSound();
            ghost.KillPlayer();
        }
    }

    private void PlayDeathSound()
    {
        if (TodHalt == null) return;

        if (Random.value < 0.9f && TodClips != null && TodClips.Length > 0)
        {
            TodHalt.clip = TodClips[Random.Range(0, TodClips.Length)];
        }
        else if (TodClipsRare != null && TodClipsRare.Length > 0)
        {
            TodHalt.clip = TodClipsRare[Random.Range(0, TodClipsRare.Length)];
        }

        TodHalt.Play();
    }

    // Wird im Multiplayer aufgerufen, sobald alle Spieler tot sind
    public void TriggerGlobalDeath()
    {
        if (Canvas != null)
        {
            Canvas.SetActive(true);
        }

        Time.timeScale = 0f;

        Save saveComp = GetComponent<Save>();
        if (saveComp != null)
        {
            saveComp.SaveScore();
        }
    }
}