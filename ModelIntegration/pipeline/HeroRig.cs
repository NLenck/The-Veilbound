using UnityEngine;

namespace Veilbound
{
    public sealed class HeroRig
    {
        public readonly Transform root,body,swordPivot,leftLeg,rightLeg;
        public readonly SkinnedMeshRenderer skin;
        readonly Transform[] bones=new Transform[HeroBodyMesh.Rest.Length];
        readonly Transform cape;
        readonly BronzeHeroVisual visual;
        readonly ProductionHeroVisual production;
        public void SetMotion(Vector3 actualVelocity){if(production)production.SetMotion(actualVelocity);}
        readonly Vector3[] rest;
        readonly Material surface;
        readonly Light emberLight;
        readonly HeroGearAnchor[] gearAnchors;
        readonly Renderer[] emberEyes;
        readonly MaterialPropertyBlock eyeBlock=new MaterialPropertyBlock();
        readonly float[] armLengths=new float[4];
        string abilityGesture;float gestureRemaining,gestureDuration;
        public void PlayAbilityGesture(string id) {
            if(id!="lance"&&id!="smite"&&id!="warcry"&&id!="brand"&&id!="bloodthirst"&&id!="rage")return;
            if(gestureRemaining>0&&(abilityGesture=="lance"||abilityGesture=="smite")&&id!="lance"&&id!="smite")return;
            abilityGesture=id;gestureRemaining=gestureDuration=id=="smite"?.5f:.42f;
        }
        public void AdvanceAbilityGesture(float dt){gestureRemaining=Mathf.Max(0,gestureRemaining-Mathf.Max(0,dt));}
        public void ClearAbilityGesture(){gestureRemaining=0;abilityGesture=null;}

        public HeroRig(Transform parent,Settlement city)
        {
            city.EnsureGear();
            if(ProductionHeroVisual.Available) {
                production=ProductionHeroVisual.Create(parent,city);visual=production.facade;root=production.transform;bones=visual.bones;rest=visual.rest;
                body=bones[0];leftLeg=bones[11];rightLeg=bones[14];skin=visual.bodySkin;gearAnchors=System.Array.Empty<HeroGearAnchor>();emberEyes=System.Array.Empty<Renderer>();
                swordPivot=WorldArt.Group("Greatsword grip",root);RoundedArt.Greatsword(swordPivot,city.equippedSword);MetalAppearance.Apply(swordPivot,city.WeaponMetalIndex);production.sword=swordPivot;Animate(0,0,0);return;
            }
            visual=BronzeHeroVisual.Create(parent,city);root=visual.transform;bones=visual.bones;rest=visual.rest;
            body=bones[0];leftLeg=bones[11];rightLeg=bones[14];skin=visual.bodySkin;
            surface=new Material(skin.sharedMaterial);skin.sharedMaterial=surface;
            emberEyes=visual.emberEyes;gearAnchors=root.GetComponentsInChildren<HeroGearAnchor>();
            var glow=WorldArt.Group("Restrained Veilborne ember glow",bones[2]);glow.localPosition=new Vector3(0,-.12f,.18f);
            emberLight=glow.gameObject.AddComponent<Light>();emberLight.type=LightType.Point;emberLight.range=2.3f;
            emberLight.color=new Color(.64f,.035f,.018f);emberLight.shadows=LightShadows.None;emberLight.intensity=.06f;
            var fill=WorldArt.Group("Hero readability fill",root);fill.localPosition=new Vector3(0,1.65f,.65f);var lamp=fill.gameObject.AddComponent<Light>();lamp.type=LightType.Point;lamp.range=2.6f;lamp.intensity=.8f;lamp.color=new Color(1,.83f,.66f);lamp.shadows=LightShadows.None;
            swordPivot=WorldArt.Group("Greatsword grip",root);RoundedArt.Greatsword(swordPivot,city.equippedSword);MetalAppearance.Apply(swordPivot,city.WeaponMetalIndex);
            armLengths[0]=Vector3.Distance(bones[5].position,bones[6].position);armLengths[1]=Vector3.Distance(bones[6].position,bones[7].position);
            armLengths[2]=Vector3.Distance(bones[8].position,bones[9].position);armLengths[3]=Vector3.Distance(bones[9].position,bones[10].position);
            Animate(0,0,0);
        }
        void Aim(Transform bone,Transform child,Vector3 target)
        {bone.rotation=Quaternion.FromToRotation(child.position-bone.position,target-bone.position)*bone.rotation;}
        public void Release(){if(production){production.Release();return;}if(surface)Object.Destroy(surface);if(visual)visual.ReleaseMaterials();}
        public void NeutralPose(float time=0)
        {
            if(production){production.Animate(time,0,0,null,0,true);return;}
            Animate(time,0,0);
            for(int i=5;i<=10;i++)bones[i].localRotation=Quaternion.identity;
            foreach(var anchor in gearAnchors)anchor.Sync();
        }
        public static float VeilPulse(float time)
        {
            float phase=Mathf.Repeat(time+1.7f,9.7f);
            float rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(7.5f,7.9f,phase));
            float fall=Mathf.SmoothStep(0,1,Mathf.InverseLerp(8.55f,9.15f,phase));
            return rise*(1-fall)*(.9f+.1f*Mathf.Sin(time*19));
        }
        void Arm(int start,Vector3 target,Vector3 pole,float upper,float lower)
        {
            Vector3 at=bones[start].position,diff=target-at;float distance=Mathf.Clamp(diff.magnitude,.05f,upper+lower-.002f);Vector3 direction=diff.normalized;
            pole=Vector3.ProjectOnPlane(pole,direction).normalized;
            float c=Mathf.Clamp((upper*upper+distance*distance-lower*lower)/(2*upper*distance),-1,1);
            Vector3 elbow=at+direction*(c*upper)+pole*(Mathf.Sqrt(1-c*c)*upper);
            Aim(bones[start],bones[start+1],elbow);Aim(bones[start+1],bones[start+2],at+direction*distance);
            bones[start+2].rotation=swordPivot.rotation;
        }
        public void Animate(float time,float movement,float swing)
        {
            if(production){float amount=gestureRemaining>0?Mathf.Sin(Mathf.PI*(1-gestureRemaining/gestureDuration)):0;production.Animate(time,movement,swing,abilityGesture,amount);return;}
            for(int i=0;i<bones.Length;i++)bones[i].localRotation=Quaternion.identity;
            float stride=Mathf.Sin(time*10)*25*movement,bob=Mathf.Abs(Mathf.Sin(time*10))*.025f*movement;
            body.localPosition=rest[0]+Vector3.up*bob;
            bones[11].localRotation=Quaternion.Euler(stride,0,-2);bones[14].localRotation=Quaternion.Euler(-stride,0,2);
            bones[12].localRotation=Quaternion.Euler(Mathf.Max(0,-stride)*1.1f,0,0);bones[15].localRotation=Quaternion.Euler(Mathf.Max(0,stride)*1.1f,0,0);
            bones[13].localRotation=Quaternion.Euler(-Mathf.Max(0,-stride)*.45f,0,0);bones[16].localRotation=Quaternion.Euler(-Mathf.Max(0,stride)*.45f,0,0);
            bones[1].localRotation=Quaternion.Euler(movement*4,Mathf.Sin(time*10)*3*movement,0);
            bones[4].localRotation=Quaternion.Euler(-movement*3,0,0);
            swordPivot.localPosition=new Vector3(.015f,1.085f+bob,.27f);
            swordPivot.localRotation=Quaternion.Euler(-43,Mathf.Lerp(-28,74,swing),-3);
            float gesture=gestureRemaining>0?Mathf.Sin(Mathf.PI*(1-gestureRemaining/gestureDuration)):0;
            if(abilityGesture=="smite")swordPivot.localRotation*=Quaternion.Euler(-65*gesture,0,0);
            if(abilityGesture=="warcry"||abilityGesture=="rage"||abilityGesture=="brand"||abilityGesture=="bloodthirst"){bones[1].localRotation*=Quaternion.Euler(-8*gesture,0,0);bones[4].localRotation*=Quaternion.Euler(-7*gesture,0,0);}
            Vector3 up=root.up*.055f;
            Arm(8,swordPivot.TransformPoint(new Vector3(0,0,.015f))+up,root.TransformDirection(new Vector3(1,-.4f,-.35f)),armLengths[2],armLengths[3]);
            Arm(5,swordPivot.TransformPoint(new Vector3(0,0,.17f))+up,root.TransformDirection(new Vector3(-1,-.4f,-.35f)),armLengths[0],armLengths[1]);
            if(abilityGesture=="lance"&&gesture>0){float phase=1-gestureRemaining/gestureDuration;Vector3 target=root.TransformPoint(new Vector3(-.25f,1.35f,Mathf.Lerp(-.25f,.85f,Mathf.SmoothStep(0,1,phase*2))));Arm(5,Vector3.Lerp(swordPivot.TransformPoint(new Vector3(0,0,.17f))+up,target,gesture),root.TransformDirection(new Vector3(-1,-.25f,-.3f)),armLengths[0],armLengths[1]);}
            if(cape)cape.localRotation=Quaternion.Euler(5+movement*12+Mathf.Sin(time*3)*3,0,Mathf.Sin(time*4)*2);
            float ember=VeilPulse(time);surface.SetColor("_EmissionColor",new Color(.9f,.20f,.035f)*(.12f+ember));
            emberLight.intensity=.06f+ember*.42f;
            float eyeFlicker=.045f*Mathf.Sin(time*3.1f)+.025f*Mathf.Sin(time*7.7f+.8f);
            eyeBlock.SetColor("_EmissionColor",new Color(.85f,.16f,.025f)*(.9f+.6f*ember+eyeFlicker));
            eyeBlock.SetFloat("_VeilTime",time);
            foreach(var eye in emberEyes)eye.SetPropertyBlock(eyeBlock);
            foreach(var anchor in gearAnchors)anchor.Sync();
        }
    }
}
