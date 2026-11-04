using System;
using UnityEngine;

namespace Veilbound
{
    public partial class VeilGame
    {
        int gearSlot=2,gearMaterial=3;
        void Hub()
        {
            if(gearBagOpen){GearBagScreen();return;}if(tab==1){EquipmentPaperDoll();return;}if(tab==2){IndustryScreen();return;}
            city.EnsureGear();
            Box(new Rect(0,0,1600,112),ink);Label(44,22,620,52,"V E I L B O U N D",titleStyle);
            Label(48,77,630,25,$"{CityClock}   /   SETTLEMENT PRODUCTION ×1",smallStyle);
            Box(new Rect(28,137,886,665),panel);
            Button(48,155,274,48,tab==0?"• EXPEDITION":"EXPEDITION",()=>{tab=0;selection=0;});
            Button(333,155,274,48,tab==1?"• LOADOUT":"LOADOUT",()=>{tab=1;selection=0;});
            Button(618,155,274,48,tab==2?"• SETTLEMENT":"SETTLEMENT",()=>{tab=2;selection=0;});Rule(48,223,844);
            if(tab==0)ExpeditionPanel();else if(tab==1)EquipmentPaperDoll();else BuildingPanel();
            Box(new Rect(1110,664,462,138),panel);Label(1134,679,420,28,"THE VEILBORNE");
            Label(1134,719,415,67,$"{SwordName(city.equippedSword)}\n{120+city.GearHealth+city.GearBonus("vitality"):0} health   •   {(city.WeaponDamage+city.blacksmith*2)*(1+city.GearBonus("power")*.01f):0.#} damage\n{city.Protection:P0} protection   •   Cape: {Gear.Name(city.Item(8))}",smallStyle);
            Box(new Rect(0,825,1600,75),ink);Label(42,838,1510,26,notice,smallStyle);
            Label(42,870,1510,24,"WASD / left stick: move   •   Mouse / right stick: aim   •   Space / A / RB: dodge   •   Menus: arrows or D-pad + Enter / A",smallStyle);
        }
        void ExpeditionPanel(){ExpeditionSetupPanel();}
        void BuildingPanel()
        {
            Label(52,243,830,48,"The city endures",titleStyle);
            Label(52,296,830,43,"All building work uses timber and stone. Invest in the workshops for your chosen gear.",smallStyle);
            for(int i=0;i<Settlement.Buildings.Length;i++)
            {
                string id=Settlement.Buildings[i];int n=city.BuildingLevel(id);float x=52+(i%2)*424,y=350+(i/2)*104;
                Label(x,y,408,27,Settlement.BuildingNames[i]+"  /  LEVEL "+n);
                string detail=id=="timber"?$"{city.WoodRate:0} timber / hour":id=="quarry"?$"{city.StoneRate:0} stone / hour":id=="mine"?$"{city.IronRate:0} iron / hour":id=="weaver"?$"{city.ClothRate:0} cloth / hour":id=="leatherworker"?$"{city.LeatherRate:0} leather / hour + leather gear":id=="tailor"?"Cloth pieces & optional capes":$"Weapons, plate & +{n*2} weapon damage";
                Label(x,y+29,408,23,detail,smallStyle);Settlement.BuildingCost(n,out double w,out double stone);
                bool complete=id=="tailor"&&n>=2;
                Button(x,y+57,408,36,complete?"ALL GEAR TIERS UNLOCKED":$"{(n==0?"BUILD":"UPGRADE")}  •  {w:0} wood / {stone:0} stone",()=>Buy(id),!complete);
            }
        }
    }
}
