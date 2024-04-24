using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] TMP_Text Score;
    private void Update()
    {
       Score.text = Mathf.Round(transform.position.x).ToString();
    }
}
