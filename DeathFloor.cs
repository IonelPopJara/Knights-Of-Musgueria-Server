using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFloor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player Main Collider"))
        {
            Player player = other.transform.parent.GetComponent<Player>();
            if(player != null)
            {
                player.TakeDamage(1000, -1);
            }
        }
    }
}
