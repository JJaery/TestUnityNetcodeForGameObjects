using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class InGameItem : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == GameManager.LAYER_PLAYER)
        {
            Destroy(this.gameObject);
        }
    }
}
