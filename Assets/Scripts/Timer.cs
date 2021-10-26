using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Timer : MonoBehaviour
{
    GameManager manager;

    bool active = true;
    public float currentTime = 0f;
    Text timerText;

    float seconds = 0f;
    float minutes = 0f;

    void Start()
    {
        timerText = GetComponent<Text>();
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (active && manager.gameState == GameState.Play)
        {
            currentTime += Time.deltaTime;
            minutes = Mathf.FloorToInt(currentTime / 60);
            seconds = (currentTime % 60);
            timerText.text = (minutes + "  " + seconds.ToString("F2"));
        }
    }
}
