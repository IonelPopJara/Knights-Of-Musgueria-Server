using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    public float swordDamage;

    private void OnTriggerEnter(Collider other)
    {
        //print("Take damage");
        if (other != null)
        {
            if (other.CompareTag("Player Main Collider"))
            {
                //print("Take Damage");
                Player _player = other.transform.parent.GetComponent<Player>();
                if (_player != null)
                {
                    _player.TakeDamage(swordDamage, transform.parent.transform.parent.GetComponent<Player>().id);
                }
            }
        }
    }
}
