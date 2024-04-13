using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHeadController : MonoBehaviour
{
    public bool rotate = true;
    public Vector3 rotateAmount;
    [Tooltip("Empty gameobject transform where head will look")] public Transform transformToLookAt;
    private void LateUpdate()
    {
        //Makes the head look at the transform
        transform.LookAt(transformToLookAt);
        if(rotate)
        {
            transform.Rotate(rotateAmount);
        }
    }
}
