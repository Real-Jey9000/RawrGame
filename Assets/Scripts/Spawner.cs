using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] GameObject Rawr;
    [SerializeField] Vector3 LarstRawrSpawrnPors;
    [SerializeField] float DistanceTilRawr;
    [SerializeField] float Max, Min;
    

    // Update is called once per frame
    void Update()
    {
        if (Player.transform.position.x + 300 >= LarstRawrSpawrnPors.x + DistanceTilRawr)
        {
            DistanceTilRawr = Random.Range(Min, Max);
            LarstRawrSpawrnPors.x += DistanceTilRawr;
            Instantiate(Rawr,LarstRawrSpawrnPors,Quaternion.identity);
        }

    }
}

