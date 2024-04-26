using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    public LayerMask playerMask;
    public float distanceToGround;
    private Animator anim;
    private PlayerController playerController;

    private void Start()
    {
        TryGetComponent<PlayerController>(out playerController);
        anim = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (anim)
        {
            bool alreadyChanged = false;

            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            RaycastHit hit;
            Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot), Vector3.down);
            if(Physics.Raycast(ray, out hit, distanceToGround * 2, playerMask))
            {
                if(playerController != null)
                {
                    alreadyChanged = true;
                    playerController.onGround = true;
                }
                Vector3 footPos = hit.point;
                footPos.y += distanceToGround;
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPos);
                anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
            else
            {
                if (playerController != null)
                {
                    playerController.onGround = false;
                }
            }

            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot), Vector3.down);
            if (Physics.Raycast(ray, out hit, distanceToGround * 2, playerMask))
            {
                if (playerController != null)
                {
                    playerController.onGround = true;
                }

                Vector3 footPos = hit.point;
                footPos.y += distanceToGround;
                anim.SetIKPosition(AvatarIKGoal.RightFoot, footPos);
                anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
            else
            {
                if (playerController != null && !alreadyChanged)
                {
                    playerController.onGround = false;
                }
            }

        }
    }
}
