using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class TorchFlicker : MonoBehaviour
{
    Camera main;
    AudioSource source;
    public float maxLight;
    public float minLight;
    bool outwards = true;

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
        volume = GameObject.Find("GameManager").GetComponent<GameManager>().sfxVolume / 13;
        distance = Vector3.Distance(transform.position, new Vector2(main.transform.position.x, main.transform.position.y));
        if(distance < 24f)
        {
            source.volume = volume * (distance / 24) ;
            if(GameObject.Find("GameManager").GetComponent<GameManager>().gameState == GameState.Menu) { source.volume = source.volume / 2; }
            if (!source.isPlaying) source.Play();
        }
        else
        {
            source.Stop();
        }
        Light2D light1 = transform.GetChild(0).GetComponent<Light2D>();
        Light2D light2 = transform.GetChild(1).GetComponent<Light2D>();
        if (outwards)
        {
            light1.pointLightOuterRadius = Mathf.MoveTowards(light1.pointLightOuterRadius, maxLight, 0.01f);
            light2.pointLightOuterRadius = Mathf.MoveTowards(light2.pointLightOuterRadius, maxLight, 0.01f);
        }
        else
        {
            light1.pointLightOuterRadius = Mathf.MoveTowards(light1.pointLightOuterRadius, minLight, 0.01f);
            light2.pointLightOuterRadius = Mathf.MoveTowards(light2.pointLightOuterRadius, minLight, 0.01f);
        }

        if (light1.pointLightOuterRadius > (maxLight - 0.000001f) || light1.pointLightOuterRadius < (minLight + 0.000001f))
        {
            outwards = !outwards;
        }
    }
}
