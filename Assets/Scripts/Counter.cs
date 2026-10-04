using UnityEngine;
using TMPro;

public class Counter : MonoBehaviour
{
    [SerializeField] private TMP_Text Score;
    [SerializeField] private GameObject targetOverride;

    private void Update()
    {
        if (Score == null) return;

        if (targetOverride != null)
        {
            Score.text = Mathf.Round(targetOverride.transform.position.x).ToString();
        }
        else if (GameRunnerAnchor.Instance != null)
        {
            Score.text = Mathf.Round(GameRunnerAnchor.Instance.transform.position.x).ToString();
        }
    }
}