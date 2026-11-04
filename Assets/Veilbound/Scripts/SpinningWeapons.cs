using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    public partial class VeilGame
    {
        sealed class WeaponSweep
        {
            public Transform root;
            public float age,damage;
            public int pulses;
        }
        readonly List<WeaponSweep> weaponSweeps=new List<WeaponSweep>();
        Transform oathblade;
        int OathBladeCount=>1+Mathf.FloorToInt(build.Attribute("orbit","projectiles"))+(Ascended("orbit",1)?2:0);
        Vector3 OathPosition=>OathPositionAt(0);
        Vector3 OathPositionAt(int index)=>position+new Vector3(Mathf.Sin(elapsed*2+index*Mathf.PI*2/OathBladeCount),0,Mathf.Cos(elapsed*2+index*Mathf.PI*2/OathBladeCount))*(2.4f*SkillArea("orbit"));
        Transform SpinningSword(Transform parent,Vector3 offset)
        {
            var sword=StarterEquipment.CreateEquippedSword(parent,city);sword.localPosition=offset;
            var tip=WorldArt.Group("Blade trail",sword);tip.localPosition=new Vector3(0,0,1.72f);
            var trail=tip.gameObject.AddComponent<TrailRenderer>();trail.time=.14f;trail.minVertexDistance=.08f;
            trail.startWidth=.075f;trail.endWidth=0;trail.sharedMaterial=WorldArt.Glow(city.equippedSword==2?WorldArt.VeilRed:WorldArt.Gold);
            trail.startColor=new Color(1,1,1,.6f);trail.endColor=new Color(1,1,1,0);return sword;
        }
        void BeginWeaponSweep(float damage)
        {
            if(BeginAscendedWhirl(damage))return;PlayerCast("whirl",position,SkillRadius("whirl"));
            var root=WorldArt.Group("Reaping circle weapon",effects);root.position=position+Vector3.up*1.05f;root.localScale=Vector3.one*SkillArea("whirl");
            SpinningSword(root,new Vector3(0,0,1.65f));
            weaponSweeps.Add(new WeaponSweep{root=root,damage=damage});
        }
        void UpdateCombatWeapons(float dt)
        {
            for(int i=weaponSweeps.Count-1;i>=0;i--)
            {
                var sweep=weaponSweeps[i];sweep.age+=dt;
                sweep.root.position=position+Vector3.up*1.05f;sweep.root.localScale=Vector3.one*SkillArea("whirl");
                sweep.root.rotation=Quaternion.Euler(0,sweep.age*1080,0);
                // Three passes distribute the original attack damage across the visible spin.
                while(sweep.pulses<3 && sweep.age>=sweep.pulses*.3f)
                {sweep.pulses++;AreaDamage(position,SkillRadius("whirl"),sweep.damage/3,false,"whirl");}
                if(sweep.age>=.9f){Destroy(sweep.root.gameObject);weaponSweeps.RemoveAt(i);}
            }
            if(build.Rank("orbit")>0)
            {
                if(!oathblade)
                {
                    oathblade=WorldArt.Group("Orbiting equipped weapon",effects);
                    SpinningSword(oathblade,Vector3.zero);
                }
                oathblade.gameObject.SetActive(!Ascended("orbit",2));oathblade.localScale=Vector3.one*SkillArea("orbit");oathblade.position=OathPosition+Vector3.up*1.05f;
                oathblade.rotation=Quaternion.Euler(0,elapsed*480,0);
            }
        }
        void ClearCombatWeapons(){weaponSweeps.Clear();oathblade=null;}
    }
}


