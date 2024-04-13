using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class ArmorPutOn : MonoBehaviour
{
    public Animator armorAnim;
    private List<BonesSetup> bones = new List<BonesSetup>();

    private ArmorItem r;
    private Animator plaA;
    private void Awake()
    {
        armorAnim = GetComponent<Animator>();
        for(int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            Transform temp = armorAnim.GetBoneTransform((HumanBodyBones)i);
            ParentConstraint pc;
            if (temp == null) continue;

            if(!temp.TryGetComponent<ParentConstraint>(out pc))
            {
                pc = temp.AddComponent<ParentConstraint>();
            }
            BonesSetup tempBone = new BonesSetup();
            tempBone.boneType = (HumanBodyBones)i;
            tempBone.constraint = pc;
            tempBone.boneTransform = temp;

            temp.localPosition = Vector3.zero;
            temp.localRotation = Quaternion.identity;
            bones.Add(tempBone);
        }
    }

#if UNITY_EDITOR
//    private void Update()
//    {
//        if(r != null)
//        {
//            PutOn(plaA, r);
//        }
//    }
#endif
    public void PutOn(Animator playerAnim, ArmorItem armor)
    {
        plaA = playerAnim;
        r = armor;
        for (int i = 0; i < bones.Count; i++)
        {
            ConstraintSource sc = new ConstraintSource();
            Transform playerBone = playerAnim.GetBoneTransform(bones[i].boneType);
            if (playerBone == null)
            {
                continue;
            }
            bones[i].boneTransform.parent = playerBone;
            bones[i].boneTransform.localPosition = Vector3.zero;
            bones[i].boneTransform.localRotation = Quaternion.identity;

            bones[i].boneTransform.Rotate(Vector3.up, armor.localRotation.y);
            bones[i].boneTransform.Rotate(Vector3.forward, armor.localRotation.z);
            bones[i].boneTransform.localScale = armor.localScale;
            bones[i].boneTransform.localPosition = armor.localPosition;

            if (bones[i].boneType == armor.boneToOffset)
            {
                bones[i].boneTransform.localPosition += armor.offset;
                bones[i].boneTransform.localScale += armor.scaleOffset;
            }
        }
    }

    private void OnDestroy()
    {
        for(int i = 0; i < bones.Count; i++)
        {
            Destroy(bones[i].boneTransform.gameObject);
        }
    }
}

[System.Serializable]
public class BonesSetup
{
    public Transform boneTransform;
    public ParentConstraint constraint;
    public HumanBodyBones boneType;
}
