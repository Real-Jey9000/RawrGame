using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class movementGhost : MonoBehaviourPun
{
    public static bool isGameRunning = false;
    public static readonly List<movementGhost> AllActiveRunners = new List<movementGhost>();
    public static movementGhost LocalScenePlayerInstance;
    public static PhotonView LocalNetworkView;

    [Header("Player Role")]
    [Tooltip("Haken setzen, wenn dieses Objekt der feste Spieler in der Szene ist!")]
    [SerializeField] private bool isMyScenePlayer = false;

    [Header("Movement Settings")]
    [SerializeField] private bool Grounded = true;
    [SerializeField] private float JumpForce = 35f;
    [SerializeField] private AudioSource SprungSound;

    [Header("Ghost Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color ghostColor = new Color(1f, 1f, 1f, 0.45f);

    // --- SKIN SYNC: Eigene Renderer für Hat und Jacket ---
    [Header("Skin Visuals")]
    [SerializeField] private SpriteRenderer hatRenderer;
    [SerializeField] private SpriteRenderer jacketRenderer;

    [Header("Revive Settings")]
    [SerializeField] private float respawnDelay = 10f;
    [SerializeField] private float fadeInDuration = 2f;

    public bool IsAlive { get; private set; } = true;
    public bool IsMyScenePlayer => isMyScenePlayer;

    private Rigidbody2D rb;
    private Collider2D col;

    // Timer für sanfte Positions-Korrekturen
    private float syncTimer = 0f;
    private const float SYNC_INTERVAL = 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (!AllActiveRunners.Contains(this))
        {
            AllActiveRunners.Add(this);
        }

        if (isMyScenePlayer)
        {
            LocalScenePlayerInstance = this;
        }
    }

    private void OnDestroy()
    {
        AllActiveRunners.Remove(this);
    }

    private void Start()
    {
        isGameRunning = false;
        IsAlive = true;

        // 1. Lokaler Szenen-Spieler setzt seine Skins direkt
        if (isMyScenePlayer)
        {
            ApplyLocalSkins();
            return;
        }

        // 2. Netzwerk-Instanz aus Resources
        if (photonView.IsMine)
        {
            LocalNetworkView = photonView;

            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (hatRenderer != null) hatRenderer.enabled = false;
            if (jacketRenderer != null) jacketRenderer.enabled = false;

            if (col != null) col.enabled = false;
            rb.isKinematic = true;
            rb.simulated = false;

            // Sende die eigenen Skins an den Mitspieler (gepuffert für Nachzügler)
            SendMySkinsToOthers();
        }
        else
        {
            // Das ist der Geist des Mitspielers
            if (spriteRenderer != null) spriteRenderer.color = ghostColor;
            ApplyGhostAlpha(hatRenderer);
            ApplyGhostAlpha(jacketRenderer);

            if (col != null)
            {
                col.isTrigger = false;

                // Nicht mit dem eigenen Szenen-Spieler kollidieren
                if (LocalScenePlayerInstance != null && LocalScenePlayerInstance.col != null)
                {
                    Physics2D.IgnoreCollision(col, LocalScenePlayerInstance.col, true);
                }
            }
        }
    }

    // ==========================================
    // SKIN LOGIK & SYNC
    // ==========================================

    private void ApplyLocalSkins()
    {
        if (SkinManager.Instance == null) return;

        if (hatRenderer != null)
            hatRenderer.sprite = SkinManager.Instance.GetCurrentHatSprite();

        if (jacketRenderer != null)
            jacketRenderer.sprite = SkinManager.Instance.GetCurrentJacketSprite();
    }

    private void SendMySkinsToOthers()
    {
        if (SkinManager.Instance == null) return;

        var curHat = SkinManager.Instance.GetCurrentHat();
        var curJacket = SkinManager.Instance.GetCurrentJacket();

        string hatId = curHat != null ? curHat.uniqueId : "";
        string jacketId = curJacket != null ? curJacket.uniqueId : "";

        photonView.RPC(nameof(RPC_SetRemoteSkins), RpcTarget.OthersBuffered, hatId, jacketId);
    }

    [PunRPC]
    public void RPC_SetRemoteSkins(string hatId, string jacketId)
    {
        if (SkinManager.Instance == null) return;

        if (hatRenderer != null && !string.IsNullOrEmpty(hatId))
        {
            hatRenderer.sprite = SkinManager.Instance.GetHatSpriteById(hatId);
            ApplyGhostAlpha(hatRenderer);
        }

        if (jacketRenderer != null && !string.IsNullOrEmpty(jacketId))
        {
            jacketRenderer.sprite = SkinManager.Instance.GetJacketSpriteById(jacketId);
            ApplyGhostAlpha(jacketRenderer);
        }
    }

    private void ApplyGhostAlpha(SpriteRenderer rend)
    {
        if (rend != null)
        {
            Color c = rend.color;
            c.a = ghostColor.a;
            rend.color = c;
        }
    }

    // ==========================================
    // UPDATE & RESTLICHE LOGIK
    // ==========================================

    private void Update()
    {
        if (!isGameRunning) return;

        float currentSpeed = GameRunnerAnchor.Instance != null ? GameRunnerAnchor.Instance.CurrentSpeed : 12f;

        // 1. Eigener Szenen-Spieler
        if (isMyScenePlayer)
        {
            if (!IsAlive) return;

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                if (Grounded && Time.timeScale == 1f)
                {
                    ExecuteJump();

                    if (LocalNetworkView != null && PhotonNetwork.InRoom)
                    {
                        LocalNetworkView.RPC(nameof(RPC_Jump), RpcTarget.Others);
                    }
                }
            }

            transform.Translate(Vector2.right * (Time.deltaTime * currentSpeed));

            // Heartbeat: Regelmäßig Position und Status an Mitspieler senden
            syncTimer += Time.deltaTime;
            if (syncTimer >= SYNC_INTERVAL)
            {
                syncTimer = 0f;
                if (LocalNetworkView != null && PhotonNetwork.InRoom)
                {
                    LocalNetworkView.RPC(nameof(RPC_HeartbeatState), RpcTarget.Others, transform.position, IsAlive);
                }
            }
        }
        else
        {
            // Geist des Mitspielers: läuft nur weiter, wenn er laut Autorität am Leben ist
            if (!photonView.IsMine && IsAlive)
            {
                transform.Translate(Vector2.right * (Time.deltaTime * currentSpeed));
            }
        }
    }

    public void ExecuteJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);

        if (SprungSound != null)
        {
            SprungSound.Play();
        }
    }

    [PunRPC]
    public void RPC_Jump()
    {
        ExecuteJump();
    }

    // --- AUTORITÄRE STATUS-KORREKTUR ---
    [PunRPC]
    public void RPC_HeartbeatState(Vector3 authoritativePos, bool aliveState)
    {
        if (!IsAlive && aliveState)
        {
            IsAlive = true;
            rb.isKinematic = false;
            ResetRenderersColor();
        }

        if (Vector2.Distance(transform.position, authoritativePos) > 0.35f)
        {
            transform.position = new Vector3(authoritativePos.x, transform.position.y, authoritativePos.z);
            if (rb != null) rb.position = transform.position;
        }
    }

    // --- TOD & RESPAWN LOGIK ---

    public void KillPlayer()
    {
        if (!isMyScenePlayer || !IsAlive) return;

        IsAlive = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        if (LocalNetworkView != null && PhotonNetwork.InRoom)
        {
            LocalNetworkView.RPC(nameof(RPC_SyncDeath), RpcTarget.Others);
        }

        if (AtLeastOnePlayerAlive())
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            TriggerFullGameOver();
        }
    }

    [PunRPC]
    public void RPC_SyncDeath()
    {
        IsAlive = false;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        if (LocalScenePlayerInstance != null && LocalScenePlayerInstance.IsAlive)
        {
            return;
        }

        if (!AtLeastOnePlayerAlive())
        {
            TriggerFullGameOver();
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        movementGhost survivingMate = GetLivingPartner();
        if (survivingMate == null)
        {
            TriggerFullGameOver();
            yield break;
        }

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        transform.position = survivingMate.transform.position;
        if (rb != null) rb.position = survivingMate.transform.position;

        SetRenderersAlpha(0f);

        if (LocalNetworkView != null && PhotonNetwork.InRoom)
        {
            LocalNetworkView.RPC(nameof(RPC_PrepareGhostRespawn), RpcTarget.Others, transform.position);
        }

        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInDuration;
            SetRenderersAlpha(Mathf.Clamp01(progress));

            if (survivingMate != null && survivingMate.IsAlive)
            {
                transform.position = survivingMate.transform.position;
                if (rb != null) rb.position = transform.position;
            }

            yield return null;
        }

        SetRenderersAlpha(1f);
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        IsAlive = true;

        if (LocalNetworkView != null && PhotonNetwork.InRoom)
        {
            LocalNetworkView.RPC(nameof(RPC_SyncRevive), RpcTarget.Others, transform.position);
        }
    }

    [PunRPC]
    public void RPC_PrepareGhostRespawn(Vector3 pos)
    {
        transform.position = pos;
        if (rb != null)
        {
            rb.position = pos;
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }

    [PunRPC]
    public void RPC_SyncRevive(Vector3 pos)
    {
        transform.position = pos;
        if (rb != null)
        {
            rb.position = pos;
            rb.velocity = Vector2.zero;
            rb.isKinematic = false;
        }

        IsAlive = true;
        ResetRenderersColor();
    }

    private void SetRenderersAlpha(float a)
    {
        if (spriteRenderer != null) { Color c = spriteRenderer.color; c.a = a; spriteRenderer.color = c; }
        if (hatRenderer != null) { Color c = hatRenderer.color; c.a = a; hatRenderer.color = c; }
        if (jacketRenderer != null) { Color c = jacketRenderer.color; c.a = a; jacketRenderer.color = c; }
    }

    private void ResetRenderersColor()
    {
        if (spriteRenderer != null) { Color c = ghostColor; c.a = ghostColor.a; spriteRenderer.color = c; }
        if (hatRenderer != null) ApplyGhostAlpha(hatRenderer);
        if (jacketRenderer != null) ApplyGhostAlpha(jacketRenderer);
    }

    private void TriggerFullGameOver()
    {
        CollisionIrgendwas deathHandler = FindObjectOfType<CollisionIrgendwas>();
        if (deathHandler != null)
        {
            deathHandler.TriggerGlobalDeath();
        }
    }

    public static bool AtLeastOnePlayerAlive()
    {
        if (LocalScenePlayerInstance != null && LocalScenePlayerInstance.IsAlive)
        {
            return true;
        }

        foreach (var p in AllActiveRunners)
        {
            if (p == null) continue;
            if (p.photonView != null && p.photonView.IsMine && !p.isMyScenePlayer) continue;

            if (p.IsAlive) return true;
        }

        return false;
    }

    public static movementGhost GetLivingPartner()
    {
        foreach (var p in AllActiveRunners)
        {
            if (p == null) continue;
            if (p.photonView != null && p.photonView.IsMine && !p.isMyScenePlayer) continue;
            if (p.IsAlive) return p;
        }
        return null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3) Grounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3) Grounded = false;
    }
}