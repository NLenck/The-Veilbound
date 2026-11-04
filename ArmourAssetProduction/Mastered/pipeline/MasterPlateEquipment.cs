using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veilbound.PlateMasters
{
    [Serializable] public sealed class PlateTierBinding
    {
        public string tier;public int itemId;public int meshFamily;public Material material;
    }
    // Masks are authored per face in UV2 and survive FBX vertex splitting.
    // Geometry/material selection and body visibility change only on equipment changes.
    public sealed class MasterPlateEquipment : MonoBehaviour
    {
        public SkinnedMeshRenderer body;
        public Mesh fullBodySource,fullSetBodyPreview;
        public SkinnedMeshRenderer[] slots=new SkinnedMeshRenderer[9];
        public Mesh[] familySlotMeshes=new Mesh[18];
        public PlateTierBinding[] tiers;
        public int[] equippedTierIds=new int[9];
        public int activeSlots=511;
        Mesh workingBody;
        int[][] sourceTriangles,faceAny,faceAll;
        void Awake(){ApplyBodyMask();}
        void EnsureCache()
        {
            if(workingBody||!body||!fullBodySource)return;
            workingBody=Instantiate(fullBodySource);workingBody.name=fullBodySource.name+" instance equipment mask";
            var uv=fullBodySource.uv2;
            if(uv.Length!=fullBodySource.vertexCount)throw new InvalidOperationException("Body slot-mask UV2 is missing.");
            int count=fullBodySource.subMeshCount;sourceTriangles=new int[count][];faceAny=new int[count][];faceAll=new int[count][];
            for(int sub=0;sub<count;sub++)
            {
                var triangles=fullBodySource.GetTriangles(sub);sourceTriangles[sub]=triangles;faceAny[sub]=new int[triangles.Length/3];faceAll[sub]=new int[triangles.Length/3];
                for(int i=0;i<triangles.Length;i+=3)
                {
                    Vector2 encoded=uv[triangles[i]];faceAny[sub][i/3]=Mathf.RoundToInt(encoded.x*512);faceAll[sub][i/3]=Mathf.RoundToInt(encoded.y*512);
                }
            }
            body.sharedMesh=workingBody;
        }
        public void ApplyBodyMask()
        {
            EnsureCache();if(!workingBody)return;
            for(int sub=0;sub<sourceTriangles.Length;sub++)
            {
                var kept=new List<int>(sourceTriangles[sub].Length);var triangles=sourceTriangles[sub];
                for(int i=0;i<triangles.Length;i+=3)
                {
                    int any=faceAny[sub][i/3],all=faceAll[sub][i/3];
                    bool hidden=(any&activeSlots)!=0||(all!=0&&(all&activeSlots)==all);
                    if(!hidden){kept.Add(triangles[i]);kept.Add(triangles[i+1]);kept.Add(triangles[i+2]);}
                }
                workingBody.SetTriangles(kept,sub,false);
            }
            workingBody.bounds=fullBodySource.bounds;ApplyWaistTuck();
        }
        void ApplyWaistTuck()
        {
            if(!body||!body.sharedMesh)return;
            for(int i=0;i<body.sharedMesh.blendShapeCount;i++)if(body.sharedMesh.GetBlendShapeName(i).Contains("BeltTuck"))body.SetBlendShapeWeight(i,(activeSlots&(1<<4))!=0?100:0);
        }
        public void SetSlotEnabled(int slot,bool equipped)
        {
            if(slot<0||slot>=9)throw new ArgumentOutOfRangeException(nameof(slot));
            slots[slot].gameObject.SetActive(equipped);
            if(equipped)activeSlots|=1<<slot;else activeSlots&=~(1<<slot);
            ApplyBodyMask();
        }
        public void SetSlotTier(int slot,int itemId)
        {
            if(slot<0||slot>=9)throw new ArgumentOutOfRangeException(nameof(slot));
            var binding=Array.Find(tiers,t=>t.itemId==itemId)??throw new ArgumentException("Unknown plate item id "+itemId);
            slots[slot].sharedMesh=familySlotMeshes[binding.meshFamily*9+slot];slots[slot].sharedMaterial=binding.material;equippedTierIds[slot]=itemId;
            SetSlotEnabled(slot,true);
        }
        public void EquipWholeTier(int itemId)
        {
            var binding=Array.Find(tiers,t=>t.itemId==itemId)??throw new ArgumentException("Unknown plate item id "+itemId);
            for(int slot=0;slot<9;slot++)
            {
                slots[slot].sharedMesh=familySlotMeshes[binding.meshFamily*9+slot];slots[slot].sharedMaterial=binding.material;equippedTierIds[slot]=itemId;slots[slot].gameObject.SetActive(true);
            }
            activeSlots=511;ApplyBodyMask();
        }
        public void UseSavedFullSetPreview()
        {
            if(body)body.sharedMesh=fullSetBodyPreview;
            if(workingBody){if(Application.isPlaying)Destroy(workingBody);else DestroyImmediate(workingBody);workingBody=null;}
            activeSlots=511;ApplyWaistTuck();
        }
        void OnDestroy()
        {
            if(workingBody){if(Application.isPlaying)Destroy(workingBody);else DestroyImmediate(workingBody);}
        }
    }
}
