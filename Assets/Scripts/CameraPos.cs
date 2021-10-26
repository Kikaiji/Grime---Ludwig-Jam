using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPos : MonoBehaviour
{
    GameManager manager;
    [SerializeField]
    Transform player;
    // Start is called before the first frame update
    public bool attached = true;
    void Start()
    {
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.gameState == GameState.Play && attached) transform.position = new Vector3(player.transform.position.x, player.position.y, transform.position.z);
        else if (manager.gameState == GameState.Menu) transform.position = new Vector3(-24f, -2f, -10f);
    }
}
