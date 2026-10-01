using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionIrgendwas : MonoBehaviour
{
    [SerializeField] GameObject Canvas;
    [SerializeField] AudioClip[] TodClips;
    [SerializeField] AudioClip[] TodClipsRare;
    [SerializeField] AudioSource TodHalt;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 6)
        {
            Canvas.SetActive (true);
            Time.timeScale = 0;
            if(Random.value < 0.9)
                TodHalt.clip = TodClips[Random.Range(0, TodClips.Length)];
            else
                TodHalt.clip = TodClipsRare[Random.Range(0, TodClipsRare.Length)];

            TodHalt.Play();
            gameObject.GetComponent<Save>().SaveScore();
        }
    }
    private void Start()
    {
        Canvas.SetActive(false);
    }
}
