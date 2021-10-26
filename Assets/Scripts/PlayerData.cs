using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float time;
    public float[] position;
    public float[] velocity;
    public int deaths;

    public PlayerData(PlayerController player, Timer timer)
    {
        time = timer.currentTime;
        position = new float[2];
        position[0] = player.lastSafePosition.x;
        position[1] = player.lastSafePosition.y;

        deaths = player.deaths;

        velocity = new float[2];
        velocity[0] = player.gameObject.GetComponent<Rigidbody2D>().velocity.x;
        velocity[1] = player.gameObject.GetComponent<Rigidbody2D>().velocity.y;
       
    }
}
