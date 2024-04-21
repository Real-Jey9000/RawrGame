using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] GameObject player;
    private void Update()
    {
        Vector3 pos = transform.position;
        pos.x = player.transform.position.x;
        transform.position = pos;
    }
}
