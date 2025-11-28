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
            other.GetComponent<Animator>().SetTrigger("FadeOut");
            Destroy(other.gameObject, 3f);
        }
    }
}
