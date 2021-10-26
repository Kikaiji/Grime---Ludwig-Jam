using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("enter");
        if (collision.CompareTag("Player"))
        {
            Debug.Log("player");
            PlayerController player = collision.GetComponent<PlayerController>();
            player.KillPlayer();
        }
    }
}
