using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawerSinglePlayer : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject Rawr;
    [SerializeField] Vector3 LarstRawrSpawrnPors;
    [SerializeField] float DistanceTilRawr;
    [SerializeField] float Max, Min;
    [SerializeField] float DistanceToPlayer;


    // Update is called once per frame
    void Update()
    {
        if (Player.transform.position.x + DistanceToPlayer >= LarstRawrSpawrnPors.x + DistanceTilRawr)
        {
            DistanceTilRawr = Random.Range(Min, Max);
            LarstRawrSpawrnPors.x += DistanceTilRawr;
            Instantiate(Rawr, LarstRawrSpawrnPors, Quaternion.identity);
        }

    }
}
