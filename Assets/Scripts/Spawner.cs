using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject playerOverride;
    [SerializeField] private GameObject Rawr;
    [SerializeField] private Vector3 LarstRawrSpawrnPors;
    [SerializeField] private float DistanceTilRawr;
    [SerializeField] private float Max = 15f;
    [SerializeField] private float Min = 5f;
    [SerializeField] private float DistanceToPlayer = 20f;

    private System.Random prng;

    // Wird nur im Multiplayer vom LobbyManager aufgerufen
    public void StartWithSeed(int seed)
    {
        prng = new System.Random(seed);
    }

    private void Update()
    {
        // 1. Singleplayer: Wenn playerOverride belegt ist
        if (playerOverride != null)
        {
            if (playerOverride.transform.position.x + DistanceToPlayer >= LarstRawrSpawrnPors.x + DistanceTilRawr)
            {
                DistanceTilRawr = Random.Range(Min, Max);
                LarstRawrSpawrnPors.x += DistanceTilRawr;
                Instantiate(Rawr, LarstRawrSpawrnPors, Quaternion.identity);
            }
            return;
        }

        // 2. Multiplayer: Nutzt den Anchor und prng
        if (GameRunnerAnchor.Instance != null && prng != null)
        {
            if (GameRunnerAnchor.Instance.transform.position.x + DistanceToPlayer >= LarstRawrSpawrnPors.x + DistanceTilRawr)
            {
                float factor = (float)prng.NextDouble();
                DistanceTilRawr = Mathf.Lerp(Min, Max, factor);
                LarstRawrSpawrnPors.x += DistanceTilRawr;
                Instantiate(Rawr, LarstRawrSpawrnPors, Quaternion.identity);
            }
        }
    }
}