using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteIrgendwas : MonoBehaviour
{
    [SerializeField] GameObject player;
    public void Update()
    {
        Vector3 pos = player.transform.position;
        pos.x -= 25;
        transform.position = pos;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(collision.gameObject);
    }
}
