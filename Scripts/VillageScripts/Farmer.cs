using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmer : Work
{
    public FarmManager farmPlace;
    [NonSerialized] public FarmPlot currentPlot;

    public override void FindWork(AIController source)
    {
        base.FindWork(source);
        currentPlot = farmPlace.FindWork();
    }
}
