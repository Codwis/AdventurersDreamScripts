using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ObjectPlacer : MonoBehaviour
{
    [Tooltip("Which object to spawn can be changed in code")] public GameObject toSpawn;
    [Tooltip("Players camera")]public Camera cam;

    private GameObject parent;
    private GameObject ghostObject;
    private PlayerController controller;
    private Vector3 offset = new Vector3();

    //Anonymous function where gamobject layer pushes the 1 0001 to right layer and selects all but that
    private LayerMask mask => ~(1 << gameObject.layer);

    private void Start()
    {
        //Creates new gameobject for all the things created
        parent = new GameObject();
        parent.name = "ObjectHolder";

        //Gets player controller
        controller = GetComponent<PlayerController>();
    }

    //Sets Object as preview and prepares it
    private void SetObject(GameObject prefab)
    {
        //Creates ghost preview
        ghostObject = Instantiate(prefab, transform);
        Collider[] cols = ghostObject.GetComponentsInChildren<Collider>();
        System.Array.ForEach(cols, collider => collider.enabled = false);
        //Sets what to spawn
        toSpawn = ghostObject;
    }

    private void ResetObject(GameObject prefab = null)
    {
        Destroy(ghostObject.gameObject);
        if (prefab != null)
            SetObject(prefab);
    }

    //Sets object down
    public void SetObjectDown()
    {
        //Instantiates clone at ghost objects location and rotation and parent it to parent
        Instantiate(toSpawn, ghostObject.transform.position, ghostObject.transform.rotation, parent.transform);
    }

    private void LateUpdate()
    {
        if (ghostObject != null && cam != null)
        {
            //This will put the ghost preview where player is looking
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 50, mask))
            {
                ghostObject.transform.position = hit.point + offset;
            }
        }
    }
    void Update()
    {
        if(ghostObject == null && toSpawn != null)
        {
            SetObject(toSpawn);
        }
        if (ghostObject == null)
            return;

        if (toSpawn == null)
            ResetObject();
        if (!toSpawn.Equals(ghostObject))
        {
            ResetObject(toSpawn);
        }
            

        //Sets down object
        if(Input.GetKeyDown(InputHandler.instance.GetKey("BuildingSetDown")))
        {
            SetObjectDown();
        }
        //Rotate object
        if(Input.GetKey(InputHandler.instance.GetKey("BuildingRotate")))
        {
            //Change the euler angles with mouse x and y rotate it ya know
            Vector3 euler = ghostObject.transform.eulerAngles;
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");
            ghostObject.transform.eulerAngles = new Vector3(euler.x + mouseY, euler.y + mouseX);
        }
        //Move along Y Axis
        else if (Input.GetKey(InputHandler.instance.GetKey("BuildingChangeHeight")))
        {
            //Just move the object along y axis using the mouse Y
            float mouseY = Input.GetAxisRaw("Mouse Y");
            offset += Vector3.up * mouseY;
        }

        // disables camera when rotating or raising
        if (Input.GetKeyDown(InputHandler.instance.GetKey("BuildingRotate")) || Input.GetKeyDown(InputHandler.instance.GetKey("BuildingChangeHeight")))
        {
            controller.EnableCamMovement(false);
        }
        else if(Input.GetKeyUp(InputHandler.instance.GetKey("BuildingRotate")) || Input.GetKeyUp(InputHandler.instance.GetKey("BuildingChangeHeight")))
        {
            controller.EnableCamMovement(true);
        }
    }
}