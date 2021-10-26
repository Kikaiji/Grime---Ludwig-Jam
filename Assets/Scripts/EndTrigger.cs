using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTrigger : MonoBehaviour
{
    [SerializeField]
    GameObject endScreen;
    [SerializeField]
    Text timeText;
    [SerializeField]
    Text deathsText;

    [SerializeField]
    PlayerController player;
    [SerializeField]
    Timer time;
    [SerializeField]
    Text deathCounter;

    GameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(manager.EndScene(timeText, deathsText, endScreen));
    }
}
