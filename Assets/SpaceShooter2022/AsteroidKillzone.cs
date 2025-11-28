using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidKillzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Asteroid"))
        {
            Destroy(other.gameObject);
        }
    }
}
