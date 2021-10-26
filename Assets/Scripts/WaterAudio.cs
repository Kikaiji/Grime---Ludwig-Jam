using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAudio : MonoBehaviour
{
    GameManager manager;
    [SerializeField]
    AudioClip bigWater;
    [SerializeField]
    AudioClip smallWater;
    [SerializeField]
    CompositeCollider2D waterColliders;
    [SerializeField]
    GameObject waterPrefab;
    [SerializeField]
    GameObject audioParent;
    List<GameObject> waterAudio = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CreateTorches();
    }

    // Update is called once per frame
    private void CreateTorches()
    {
        List<Vector2> pointList = new List<Vector2>();
        for (int i = 0; i < waterColliders.pathCount; i++)
        {
            waterColliders.GetPath(i, pointList);

            float waterx = 0, watery = 0;
            Vector2 waterCenter = new Vector2();

            
            foreach (Vector2 o in pointList)
            {
                waterx += o.x;
                watery += o.y;
            }
            waterx /= pointList.Count;
            watery /= pointList.Count;


            var water = Instantiate(waterPrefab, new Vector3(waterx, watery, 20f), new Quaternion(0, 0, 0, 0), audioParent.transform);
            if (pointList[1].x - pointList[2].x < 0.1f)
            {
                Debug.Log(pointList[0] + " " + pointList[1]);
                water.GetComponent<AudioSource>().clip = bigWater;
                water.GetComponent<AudioSource>().volume = manager.sfxVolume * 0.1f;
            }
            else
            {
                water.GetComponent<AudioSource>().clip = smallWater;
                water.GetComponent<AudioSource>().volume = manager.sfxVolume * 0.1f;
            }
            
            waterCenter = new Vector2(waterx, watery);
            
            waterAudio.Add(water);
        }
    }
}
