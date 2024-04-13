using System;
using UnityEngine;

public class FarmManager : MonoBehaviour
{
    [NonSerialized] public FarmPlot[] farmPlots;

    private void Start()
    {
        farmPlots = GetComponentsInChildren<FarmPlot>();
    }
    public void ProgressFarm()
    {
        foreach(FarmPlot plot in farmPlots)
        {
            if(plot.growing)
            {
                plot.ProgressGrow();
            }
        }
    }

    public FarmPlot FindWork()
    {
        foreach(FarmPlot plot in farmPlots)
        {
            if(!plot.growing)
            {
                return plot;
            }
            else if(plot.readyToHarvest)
            {
                return plot;
            }
        }

        return null;
    }
}
