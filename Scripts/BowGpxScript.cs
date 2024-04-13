using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BowGpxScript : MonoBehaviour
{
    [Tooltip("ParentConstraint in the string object within the bow")]public ParentConstraint stringConstraint;
    [Tooltip("Spot on the bow where arrow looks so it can move with the bow")]public Transform arrowLookHere;
    [HideInInspector] public Quaternion originalRotation;
    private Vector3 originalPos;

    public void SetPos(ParentConstraint constraint)
    {
        originalPos = constraint.transform.localPosition;
        originalRotation = transform.root.GetComponent<RangedScript>().stringSpot.localRotation;
    }
    public void SetConstraint(Transform parent) //Sets a new constraint to the transform given
    {
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = parent;
        source.weight = 1;

        stringConstraint.AddSource(source);
    }

    public void ActivateConstraint(bool enable) //Activates or disables the constraint
    {
        if (enable)
        {
            stringConstraint.weight = 1;
        }
        else
        {
            stringConstraint.weight = 0;
            stringConstraint.transform.localPosition = originalPos;
        }
        stringConstraint.constraintActive = enable;
    }
    public void ActivateConstraint()
    {
        if(stringConstraint.weight == 1)
        {
            stringConstraint.weight = 0;
            stringConstraint.transform.localPosition = originalPos;
            stringConstraint.constraintActive = false;
        }
        else
        {
            stringConstraint.weight = 1;
            stringConstraint.constraintActive = true;
        }
    }
}
