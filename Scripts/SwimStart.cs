using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimStart : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Animator te = collision.gameObject.GetComponentInParent<Animator>();
        te.SetBool("Swimming", true);
    }
    
}
