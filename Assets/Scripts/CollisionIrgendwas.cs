using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionIrgendwas : MonoBehaviour
{
    [SerializeField] GameObject Canvas;
    [SerializeField] AudioSource TodHalt;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            Canvas.SetActive (true);
            Time.timeScale = 0;
            TodHalt.Play();
            gameObject.GetComponent<Save>().SaveScore();
        }
    }
    private void Start()
    {
        Canvas.SetActive(false);
    }
}
