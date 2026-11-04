using System.Collections.Generic;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
struct HousingRefPlot {public Vector2 ground;public float width,height;public HousingRefPlot(float x,float y,float w,float h){ground=new Vector2(x,y);width=w;height=h;}}
static readonly HousingRefPlot[][] HousingReferencePlots={
new[]{new HousingRefPlot(591,378,91,60),new HousingRefPlot(692,444,90,65),new HousingRefPlot(533,510,91,62),new HousingRefPlot(609,635,122,87),new HousingRefPlot(1069,445,87,61),new HousingRefPlot(1172,539,112,77),new HousingRefPlot(757,704,108,73),new HousingRefPlot(1044,681,108,81)},
new[]{new HousingRefPlot(605,393,95,68),new HousingRefPlot(692,444,87,67),new HousingRefPlot(544,514,103,70),new HousingRefPlot(588,622,93,75),new HousingRefPlot(664,647,96,71),new HousingRefPlot(1075,445,103,69),new HousingRefPlot(1171,530,96,79),new HousingRefPlot(748,704,107,76),new HousingRefPlot(992,703,102,84),new HousingRefPlot(1089,662,99,79)},
new[]{new HousingRefPlot(576,376,73,54),new HousingRefPlot(654,385,70,56),new HousingRefPlot(709,440,81,65),new HousingRefPlot(499,508,78,69),new HousingRefPlot(601,506,77,66),new HousingRefPlot(596,626,97,77),new HousingRefPlot(1069,437,98,61),new HousingRefPlot(1171,530,93,80),new HousingRefPlot(758,707,103,85),new HousingRefPlot(993,700,101,80),new HousingRefPlot(1109,680,95,72)},
new[]{new HousingRefPlot(576,376,69,69),new HousingRefPlot(657,386,65,64),new HousingRefPlot(703,440,73,75),new HousingRefPlot(507,515,76,84),new HousingRefPlot(596,501,77,73),new HousingRefPlot(612,627,84,86),new HousingRefPlot(1015,438,71,78),new HousingRefPlot(1092,438,72,73),new HousingRefPlot(1146,515,80,81),new HousingRefPlot(1226,556,64,70),new HousingRefPlot(760,686,90,88),new HousingRefPlot(985,686,82,84),new HousingRefPlot(1068,659,87,81)},
new[]{new HousingRefPlot(568,381,73,77),new HousingRefPlot(652,397,75,74),new HousingRefPlot(709,444,76,87),new HousingRefPlot(506,517,86,87),new HousingRefPlot(598,504,81,82),new HousingRefPlot(566,628,80,88),new HousingRefPlot(654,647,89,92),new HousingRefPlot(1019,434,71,87),new HousingRefPlot(1100,445,74,76),new HousingRefPlot(1150,522,87,87),new HousingRefPlot(1229,564,77,80),new HousingRefPlot(752,710,89,99),new HousingRefPlot(957,709,107,99),new HousingRefPlot(1070,665,91,86)},
new[]{new HousingRefPlot(568,382,74,92),new HousingRefPlot(650,390,69,87),new HousingRefPlot(668,440,65,85),new HousingRefPlot(730,440,65,88),new HousingRefPlot(506,514,76,91),new HousingRefPlot(596,510,73,86),new HousingRefPlot(571,626,83,100),new HousingRefPlot(653,644,80,93),new HousingRefPlot(1017,439,70,92),new HousingRefPlot(1101,438,69,87),new HousingRefPlot(1143,517,77,93),new HousingRefPlot(1220,561,72,90),new HousingRefPlot(712,702,80,96),new HousingRefPlot(792,682,75,94),new HousingRefPlot(928,710,78,96),new HousingRefPlot(1006,689,75,96),new HousingRefPlot(1088,671,76,95)},
};
static readonly Dictionary<string,Rect> HousingReferenceUV=new Dictionary<string,Rect>{
{"Recovery/HousingRefs/tier0:0",new Rect(0.01888021f,0.05371094f,0.96354167f,0.82128906f)},
{"Recovery/HousingRefs/tier1:0",new Rect(0.11588542f,0.04980469f,0.79687500f,0.86914062f)},
{"Recovery/HousingRefs/tier2:0",new Rect(0.16861979f,0.02539062f,0.72656250f,0.97070312f)},
{"Recovery/HousingRefs/tier3:0",new Rect(0.13256705f,0.01824212f,0.79386973f,0.96849088f)},
{"Recovery/HousingRefs/tier4:0",new Rect(0.05650155f,0.04105090f,0.94117647f,0.95566502f)},
{"Recovery/HousingRefs/tier5:0",new Rect(0.16887417f,0.04915515f,0.71523179f,0.91397849f)},
};
static readonly Vector2[] HousingGrovePoints={new Vector2(526,374),new Vector2(681,360),new Vector2(649,329),new Vector2(631,421),new Vector2(761,420),new Vector2(450,496),new Vector2(641,486),new Vector2(516,605),new Vector2(689,613),new Vector2(982,412),new Vector2(1136,413),new Vector2(1091,500),new Vector2(1273,557),new Vector2(679,674),new Vector2(817,703),new Vector2(897,664),new Vector2(1145,648),new Vector2(1119,699)};
void CollectReferenceHousing(int tier){int i=0;foreach(var home in HousingReferencePlots[tier]){AddTownLayer("home "+i++,"Recovery/HousingRefs/tier"+tier,0,home.ground.x,home.ground.y,home.width,home.height,true);}foreach(var point in HousingGrovePoints)AddPine("housing grove",point.x,point.y,20);}
void DrawReferenceHousingPortrait(Rect r){int tier=Mathf.Clamp(city.housingTier,0,5);DrawRecoveryPortrait(r,"HousingRefs/tier"+tier,0);}
}}
