using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAudioDistance : MonoBehaviour
{
    Camera main;
    AudioSource source;
    float volume;

    float distance;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        main = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        volume = GameObject.Find("GameManager").GetComponent<GameManager>().sfxVolume / 10;
        distance = Vector3.Distance(transform.position, new Vector2(main.transform.position.x, main.transform.position.y));
        if (distance < 30f)
        {
            source.volume = volume * (distance / 30);
            if (GameObject.Find("GameManager").GetComponent<GameManager>().gameState == GameState.Menu) { source.volume = source.volume / 2; }
            if (!source.isPlaying) source.Play();
        }
        else
        {
            source.Stop();
        }
    }
}
