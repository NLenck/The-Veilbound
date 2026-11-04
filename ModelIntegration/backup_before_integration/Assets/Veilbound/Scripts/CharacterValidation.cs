using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Veilbound
{
    public partial class VeilGame
    {
        void CheckCharacter()
        {
            Mesh mesh=hero.skin.sharedMesh;
            Require(mesh!=null && hero.skin.bones.Length==HeroBodyMesh.Rest.Length,"Player uses a skinned body with its skeleton");
            Require(HeroArt.Data!=null&&mesh.vertexCount>20000,"Player loads the authored human anatomy");
            if(Gear.Material(city.Item(2))==3)
            {
                Renderer breastplate=null,underlayer=null,crest=null;
                foreach(var renderer in hero.root.GetComponentsInChildren<Renderer>())
                {
                    if(renderer.name=="Raised steel breastplate with central keel")breastplate=renderer;
                    if(renderer.name=="Separate red vest / right front")underlayer=renderer;
                    if(renderer.name=="Plate rune / Chest / slot 2")crest=renderer;
                }
                Require(breastplate!=null&&underlayer==null,"Plate cuirass replaces the baseline red vest");
                Require(breastplate.transform.IsChildOf(hero.skin.bones[2])&&breastplate.GetComponent<MeshFilter>().sharedMesh.bounds.max.z>.23f,"Raised breastplate is fitted to the chest anchor with its own volume");
                Require(breastplate.sharedMaterial.GetFloat("_Metallic")>.5f&&crest==null,"Foundation plate uses plain steel without the decorative crest");
                bool detailSocket=false;
                foreach(Transform child in breastplate.transform.parent)
                    if(child.name=="Detail socket / "+Gear.Slots[2]+" / "+HeroBodyMesh.Names[2])detailSocket=true;
                Require(detailSocket,"Plate retains an empty detail socket for future equipment variants");
            }
            Require(HeroRig.VeilPulse(1)<.001f&&HeroRig.VeilPulse(6.4f)>.75f&&HeroRig.VeilPulse(8)<.001f,"Ember cracks pulse briefly with quiet intervals");
            var eyeRenderers=new List<Renderer>();
            foreach(var renderer in hero.root.GetComponentsInChildren<Renderer>())if(renderer.name.StartsWith("Ember eye /"))eyeRenderers.Add(renderer);
            Require(eyeRenderers.Count==2,"Two ember eyes are independently anchored to the head");
            var eyeState=new MaterialPropertyBlock();hero.Animate(1,0,0);eyeRenderers[0].GetPropertyBlock(eyeState);
            float quietEye=eyeState.GetColor("_EmissionColor").maxColorComponent;
            hero.Animate(6.4f,0,0);eyeRenderers[0].GetPropertyBlock(eyeState);
            Require(quietEye>.5f&&eyeState.GetColor("_EmissionColor").maxColorComponent>quietEye,"Eyes retain a coal glow between pulses and brighten with the cracks");
            if(city.Item(0)==0)
            {
                Transform hair=null;
                foreach(Transform child in hero.skin.bones[4])if(child.name=="Black swept hair / scalp")hair=child;
                Require(hair!=null&&hair.GetComponent<MeshRenderer>().sharedMaterial.color.maxColorComponent<.11f,"Black swept hair is separate and head anchored");
            }
            var links=new List<int>[mesh.vertexCount];for(int i=0;i<links.Length;i++)links[i]=new List<int>();
            var triangles=mesh.triangles;var counts=new Dictionary<ulong,int>();
            void Edge(int a,int b)
            {
                links[a].Add(b);links[b].Add(a);uint lo=(uint)Mathf.Min(a,b),hi=(uint)Mathf.Max(a,b);ulong key=((ulong)lo<<32)|hi;
                counts[key]=counts.TryGetValue(key,out int n)?n+1:1;
            }
            for(int i=0;i<triangles.Length;i+=3){Edge(triangles[i],triangles[i+1]);Edge(triangles[i+1],triangles[i+2]);Edge(triangles[i+2],triangles[i]);}
            int components=0;var visited=new bool[links.Length];var queue=new Queue<int>();
            for(int v=0;v<links.Length;v++)if(!visited[v])
            {
                components++;visited[v]=true;queue.Enqueue(v);
                while(queue.Count>0){int n=queue.Dequeue();foreach(int next in links[n])if(!visited[next]){visited[next]=true;queue.Enqueue(next);}}
            }
            Require(components==1,"Body mesh is one connected surface, including head, arms and legs");
            foreach(int count in counts.Values)Require(count==2,"Body surface is closed without open joint seams");
            foreach(var w in mesh.boneWeights)Require(Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.0001f,"Skin weights are normalised");
            var posed=new Mesh();hero.Animate(.47f,1,.65f);hero.skin.BakeMesh(posed);
            var anchors=hero.root.GetComponentsInChildren<HeroGearAnchor>();
            Require(anchors.Length==(city.Item(1)==0?0:2),"Each equipped pauldron has its own shoulder anchor");
            foreach(var anchor in anchors)
            {
                Require(Vector3.Distance(anchor.transform.position,anchor.joint.position)<.0001f,"Pauldron anchor stays on its animated shoulder joint");
                Require(Quaternion.Angle(anchor.transform.rotation,Quaternion.Slerp(anchor.chest.rotation,anchor.joint.rotation,.82f))<.05f,"Pauldron follows its shoulder with restrained chest influence");
            }
            foreach(Vector3 v in posed.vertices)Require(!float.IsNaN(v.x)&&!float.IsInfinity(v.x)&&v.sqrMagnitude<25,"Animated body remains finite and bounded");
            Destroy(posed);hero.Animate(0,0,0);
            Debug.Log("VEILBOUND_CONNECTED_CHARACTER_PASS vertices="+mesh.vertexCount);
        }
        void MonsterPortrait(string path,Transform subject,float framing=.88f,float height=.4f,bool back=false)
        {
            var all=subject.GetComponentsInChildren<Transform>();var layers=new int[all.Length];
            Quaternion rotation=subject.rotation;subject.rotation=Quaternion.identity;
            for(int i=0;i<all.Length;i++){layers[i]=all[i].gameObject.layer;all[i].gameObject.layer=30;}
            var obj=new GameObject("Monster review camera");var camera=obj.AddComponent<Camera>();camera.cullingMask=1<<30;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.09f,.10f,.13f);
            camera.orthographic=true;camera.orthographicSize=framing;
            camera.transform.position=subject.position+new Vector3(2.3f,height+1.1f,back?-3:3);
            camera.transform.LookAt(subject.position+Vector3.up*height);
            var target=new RenderTexture(1024,1024,24);camera.targetTexture=target;RenderTexture previous=RenderTexture.active;
            camera.Render();RenderTexture.active=target;
            var texture=new Texture2D(1024,1024,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1024,1024),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;target.Release();Destroy(target);Destroy(texture);Destroy(obj);
            for(int i=0;i<all.Length;i++)all[i].gameObject.layer=layers[i];subject.rotation=rotation;
        }
        void CharacterPortrait(string path,float movement,float swing,Vector3 view,bool neutral=false,float poseTime=.47f,bool faceClose=false,float framing=0,float focusHeight=1.08f)
        {
            Quaternion rotation=hero.root.rotation;hero.root.rotation=Quaternion.identity;
            hero.Animate(poseTime,movement,swing);
            if(neutral){hero.NeutralPose(poseTime);hero.swordPivot.gameObject.SetActive(false);}
            // Explicitly bake this pose: a manual Camera.Render can otherwise use last frame's skinning buffer.
            var baked=new Mesh();hero.skin.BakeMesh(baked);
            var restCoordinates=new List<Vector3>();hero.skin.sharedMesh.GetUVs(1,restCoordinates);baked.SetUVs(1,restCoordinates);
            var preview=WorldArt.Group("Baked portrait pose",hero.root);
            preview.gameObject.AddComponent<MeshFilter>().sharedMesh=baked;
            preview.gameObject.AddComponent<MeshRenderer>().sharedMaterials=hero.skin.sharedMaterials;
            hero.skin.enabled=false;
            var all=hero.root.GetComponentsInChildren<Transform>();var layers=new int[all.Length];
            for(int i=0;i<all.Length;i++){layers[i]=all[i].gameObject.layer;all[i].gameObject.layer=30;}
            var cameraObject=new GameObject("Character review camera");var camera=cameraObject.AddComponent<Camera>();camera.cullingMask=1<<30;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.09f,.10f,.13f);
            camera.orthographic=true;camera.orthographicSize=framing>0?framing:faceClose?.36f:neutral?1.22f:1.48f;
            camera.transform.position=hero.root.position+view;camera.transform.LookAt(hero.root.position+Vector3.up*(faceClose?1.9f:focusHeight));
            var reviewLight=new GameObject("Portrait key light");var key=reviewLight.AddComponent<Light>();key.type=LightType.Directional;
            key.color=new Color(1,.91f,.81f);key.intensity=1.4f;key.cullingMask=1<<30;reviewLight.transform.rotation=Quaternion.Euler(37,-28,0);
            var target=new RenderTexture(1024,1024,24);camera.targetTexture=target;RenderTexture previous=RenderTexture.active;
            camera.Render();RenderTexture.active=target;
            var texture=new Texture2D(1024,1024,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1024,1024),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;target.Release();Destroy(target);Destroy(texture);Destroy(cameraObject);
            key.enabled=false;Destroy(reviewLight);
            for(int i=0;i<all.Length;i++)all[i].gameObject.layer=layers[i];hero.root.rotation=rotation;hero.Animate(0,0,0);
            hero.swordPivot.gameObject.SetActive(true);
            hero.skin.enabled=true;preview.gameObject.SetActive(false);Destroy(preview.gameObject);Destroy(baked);
        }
    }
}
