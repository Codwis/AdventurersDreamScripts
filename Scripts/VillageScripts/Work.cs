using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Work : MonoBehaviour
{
    public Tool currentTool;
    [Tooltip("Such like plow or other farm tools")] public Tool permamentTool;
    public virtual void FindWork(AIController source)
    {

    }

    public virtual void Equiptool(Tool tool)
    {
        currentTool = tool;
    }
    public virtual void RemoveTool(Tool tool)
    {
        currentTool = null;
    }

}
