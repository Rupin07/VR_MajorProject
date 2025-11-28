using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupControl : MonoBehaviour
{
    private void Update()
    {
        transform.LookAt(Camera.main.transform);
        
        //remove after 3 seconds
        Destroy(gameObject,3f);
    }
}
