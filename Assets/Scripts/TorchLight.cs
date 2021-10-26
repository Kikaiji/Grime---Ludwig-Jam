using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
public class TorchLight : MonoBehaviour
{
    GameObject devlight;
    GameObject devlight2;
    [SerializeField]
    CompositeCollider2D torchCollider;
    [SerializeField]
    GameObject torchPrefab;
    [SerializeField]
    GameObject lightParent;
    List<GameObject> torches = new List<GameObject>();
    [SerializeField]
    private float minLight, maxLight;


    // Start is called before the first frame update
    void Start()
    {
        devlight = GameObject.Find("DevLight");
        devlight.SetActive(false);
        devlight2 = GameObject.Find("DevLight2");
        devlight2.SetActive(false);

        CreateTorches();
        
    }

    private void CreateTorches()
    {
        List<Vector2> pointList = new List<Vector2>();
        for (int i = 0; i < torchCollider.pathCount; i++)
        {
            torchCollider.GetPath(i, pointList);

            float torchx = 0, torchy = 0;
            Vector2 torchCenter = new Vector2();
            foreach(Vector2 o in pointList)
            {
                torchx += o.x;
                torchy += o.y;
            }
            torchx /= pointList.Count;
            torchy /= pointList.Count;
            torchCenter = new Vector2(torchx, torchy);
            var torch = Instantiate(torchPrefab, new Vector3(torchx, torchy, 20f), new Quaternion(0, 0, 0, 0), lightParent.transform);
            torch.GetComponent<TorchFlicker>().maxLight = maxLight;
            torch.GetComponent<TorchFlicker>().minLight = minLight;
            torches.Add(torch);
        }
    }
}
