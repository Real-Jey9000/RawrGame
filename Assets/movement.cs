using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    [SerializeField]bool Grounded = true;
    [SerializeField]float JumpForce = 35;
    [SerializeField] float MovementSpeed = 12;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.W) && Grounded)
        {
           gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * JumpForce,ForceMode2D.Impulse);
        }
        transform.Translate(Vector2.right * Time.deltaTime * MovementSpeed);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
       if (collision.gameObject.layer == 3)
        {
            Grounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            Grounded = false;
        }
    }
}
