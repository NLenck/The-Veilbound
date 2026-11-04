using System.Collections.Generic;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 struct TownSpriteBounds {public Rect uv;public TownSpriteBounds(float x,float y,float w,float h){uv=new Rect(x,y,w,h);}}
 static readonly Dictionary<string,TownSpriteBounds> TownSpriteBoundsTable=new Dictionary<string,TownSpriteBounds>{
{"housing-single:0",new TownSpriteBounds(0.04622396f,0.50000000f,0.26106771f,0.41503906f)},
{"housing-single:1",new TownSpriteBounds(0.33723958f,0.50000000f,0.30989583f,0.43750000f)},
{"housing-single:2",new TownSpriteBounds(0.67773438f,0.50000000f,0.28320312f,0.44531250f)},
{"housing-single:3",new TownSpriteBounds(0.03059896f,0.06835938f,0.27929688f,0.43164062f)},
{"housing-single:4",new TownSpriteBounds(0.34114583f,0.06250000f,0.31640625f,0.43750000f)},
{"housing-single:5",new TownSpriteBounds(0.67643229f,0.06835938f,0.30598958f,0.43164062f)},
{"timber:0",new TownSpriteBounds(0.04817708f,0.57324219f,0.23242188f,0.23632812f)},
{"timber:1",new TownSpriteBounds(0.34049479f,0.57324219f,0.32617188f,0.30078125f)},
{"timber:2",new TownSpriteBounds(0.66666667f,0.56250000f,0.31835938f,0.35937500f)},
{"timber:3",new TownSpriteBounds(0.01106771f,0.13281250f,0.32226562f,0.32031250f)},
{"timber:4",new TownSpriteBounds(0.33333333f,0.08300781f,0.33333333f,0.41406250f)},
{"timber:5",new TownSpriteBounds(0.66666667f,0.08984375f,0.32552083f,0.39257812f)},
{"mine:0",new TownSpriteBounds(0.02018229f,0.60644531f,0.29427083f,0.30371094f)},
{"mine:1",new TownSpriteBounds(0.33723958f,0.50000000f,0.32942708f,0.43554688f)},
{"mine:2",new TownSpriteBounds(0.66666667f,0.50000000f,0.32161458f,0.46875000f)},
{"mine:3",new TownSpriteBounds(0.01106771f,0.07031250f,0.32226562f,0.41894531f)},
{"mine:4",new TownSpriteBounds(0.33333333f,0.07031250f,0.33333333f,0.42968750f)},
{"mine:5",new TownSpriteBounds(0.66666667f,0.06054688f,0.32812500f,0.43945312f)},
{"blacksmith:0",new TownSpriteBounds(0.04361979f,0.50000000f,0.25130208f,0.41308594f)},
{"blacksmith:1",new TownSpriteBounds(0.36002604f,0.50000000f,0.27864583f,0.46386719f)},
{"blacksmith:2",new TownSpriteBounds(0.69010417f,0.50000000f,0.29296875f,0.46582031f)},
{"blacksmith:3",new TownSpriteBounds(0.00846354f,0.08398438f,0.32486979f,0.41601562f)},
{"blacksmith:4",new TownSpriteBounds(0.33333333f,0.05957031f,0.33333333f,0.44042969f)},
{"blacksmith:5",new TownSpriteBounds(0.66666667f,0.04296875f,0.32552083f,0.45703125f)},
{"quarry:0",new TownSpriteBounds(0.02604167f,0.50000000f,0.27734375f,0.34667969f)},
{"quarry:1",new TownSpriteBounds(0.34179688f,0.57226562f,0.30078125f,0.36425781f)},
{"quarry:2",new TownSpriteBounds(0.66861979f,0.50000000f,0.32031250f,0.47167969f)},
{"quarry:3",new TownSpriteBounds(0.00846354f,0.06250000f,0.31575521f,0.43750000f)},
{"quarry:4",new TownSpriteBounds(0.33333333f,0.06250000f,0.33333333f,0.41503906f)},
{"quarry:5",new TownSpriteBounds(0.66666667f,0.03417969f,0.32942708f,0.46582031f)},
{"weaver:0",new TownSpriteBounds(0.00976562f,0.50000000f,0.31575521f,0.43359375f)},
{"weaver:1",new TownSpriteBounds(0.34440104f,0.50000000f,0.32226562f,0.44531250f)},
{"weaver:2",new TownSpriteBounds(0.66666667f,0.50000000f,0.32226562f,0.45996094f)},
{"weaver:3",new TownSpriteBounds(0.01757812f,0.12109375f,0.31575521f,0.37890625f)},
{"weaver:4",new TownSpriteBounds(0.33333333f,0.12890625f,0.33333333f,0.37109375f)},
{"weaver:5",new TownSpriteBounds(0.66666667f,0.06445312f,0.32421875f,0.43554688f)},
{"hunters:0",new TownSpriteBounds(0.02734375f,0.57714844f,0.28450521f,0.29101562f)},
{"hunters:1",new TownSpriteBounds(0.35807292f,0.56835938f,0.30859375f,0.34863281f)},
{"hunters:2",new TownSpriteBounds(0.66666667f,0.56054688f,0.30924479f,0.39355469f)},
{"hunters:3",new TownSpriteBounds(0.01302083f,0.10644531f,0.32031250f,0.35156250f)},
{"hunters:4",new TownSpriteBounds(0.33333333f,0.10351562f,0.33333333f,0.37109375f)},
{"hunters:5",new TownSpriteBounds(0.66666667f,0.08398438f,0.32552083f,0.37890625f)},
{"clothier:0",new TownSpriteBounds(0.00403429f,0.55233291f,0.19616742f,0.24842371f)},
{"clothier:1",new TownSpriteBounds(0.20020171f,0.54476671f,0.19969743f,0.30769231f)},
{"clothier:2",new TownSpriteBounds(0.39989914f,0.54981084f,0.20020171f,0.31021438f)},
{"clothier:3",new TownSpriteBounds(0.60010086f,0.53972257f,0.19969743f,0.32156368f)},
{"clothier:4",new TownSpriteBounds(0.79979829f,0.52332913f,0.19465456f,0.36822194f)},
{"clothier:5",new TownSpriteBounds(0.00806858f,0.09331652f,0.19213313f,0.36948298f)},
{"clothier:6",new TownSpriteBounds(0.20020171f,0.11223203f,0.19969743f,0.33291299f)},
{"clothier:7",new TownSpriteBounds(0.39989914f,0.09331652f,0.20020171f,0.36948298f)},
{"clothier:8",new TownSpriteBounds(0.60010086f,0.09583859f,0.19969743f,0.36822194f)},
{"clothier:9",new TownSpriteBounds(0.79979829f,0.08575032f,0.19314170f,0.41109710f)},
{"services:0",new TownSpriteBounds(0.03710938f,0.50000000f,0.46289062f,0.46093750f)},
{"services:1",new TownSpriteBounds(0.50000000f,0.50000000f,0.48111979f,0.45996094f)},
{"services:2",new TownSpriteBounds(0.01692708f,0.08496094f,0.48307292f,0.41503906f)},
{"services:3",new TownSpriteBounds(0.50000000f,0.04882812f,0.48763021f,0.45117188f)},
};
 static readonly Vector2[] TownHomePlots={new Vector2(0.369000f,0.509000f),new Vector2(0.842000f,0.673000f),new Vector2(0.670000f,0.345000f),new Vector2(0.455000f,0.755000f),new Vector2(0.820500f,0.468000f),new Vector2(0.670000f,0.755000f),new Vector2(0.455000f,0.345000f),new Vector2(0.326000f,0.673000f),new Vector2(0.691500f,0.632000f),new Vector2(0.734500f,0.550000f),new Vector2(0.412000f,0.591000f),new Vector2(0.756000f,0.345000f),new Vector2(0.369000f,0.345000f),new Vector2(0.734500f,0.468000f),new Vector2(0.734500f,0.714000f),new Vector2(0.390500f,0.714000f),new Vector2(0.756000f,0.509000f),new Vector2(0.820500f,0.632000f),new Vector2(0.347500f,0.468000f),new Vector2(0.670000f,0.673000f),new Vector2(0.820500f,0.714000f),new Vector2(0.433500f,0.386000f),new Vector2(0.390500f,0.550000f),new Vector2(0.390500f,0.468000f),new Vector2(0.713000f,0.673000f),new Vector2(0.347500f,0.714000f)};
 static readonly float[] TownBuildingWidths={0.102f,0.105f,0.1f,0.115f,0.102f,0.1f,0f,0.11f,0.095f,0.074f,0.074f};
 static readonly Vector2[][] TownRoads={new[]{new Vector2(0.235000f,0.205000f),new Vector2(0.225000f,0.245000f),new Vector2(0.255000f,0.275000f),new Vector2(0.307000f,0.296000f),new Vector2(0.316000f,0.325000f),new Vector2(0.348000f,0.355000f),new Vector2(0.397000f,0.381000f)},new[]{new Vector2(0.307000f,0.296000f),new Vector2(0.260000f,0.320000f),new Vector2(0.203000f,0.338000f)},new[]{new Vector2(0.130000f,0.377000f),new Vector2(0.195000f,0.410000f),new Vector2(0.300000f,0.443000f),new Vector2(0.370000f,0.415000f),new Vector2(0.404000f,0.388000f),new Vector2(0.459000f,0.456000f)},new[]{new Vector2(0.300000f,0.443000f),new Vector2(0.275000f,0.476000f),new Vector2(0.300000f,0.500000f),new Vector2(0.370000f,0.522000f),new Vector2(0.460000f,0.490000f)},new[]{new Vector2(0.370000f,0.522000f),new Vector2(0.310000f,0.547000f),new Vector2(0.220000f,0.590000f),new Vector2(0.160000f,0.600000f)},new[]{new Vector2(0.310000f,0.547000f),new Vector2(0.330000f,0.580000f),new Vector2(0.360000f,0.610000f),new Vector2(0.395000f,0.645000f),new Vector2(0.450000f,0.680000f),new Vector2(0.540000f,0.710000f)},new[]{new Vector2(0.490000f,0.580000f),new Vector2(0.460000f,0.600000f),new Vector2(0.410000f,0.640000f),new Vector2(0.370000f,0.650000f)},new[]{new Vector2(0.770000f,0.220000f),new Vector2(0.750000f,0.260000f),new Vector2(0.720000f,0.290000f),new Vector2(0.720000f,0.340000f),new Vector2(0.730000f,0.370000f),new Vector2(0.760000f,0.400000f),new Vector2(0.750000f,0.420000f),new Vector2(0.700000f,0.440000f),new Vector2(0.655000f,0.430000f)},new[]{new Vector2(0.600000f,0.370000f),new Vector2(0.640000f,0.360000f),new Vector2(0.680000f,0.360000f),new Vector2(0.720000f,0.360000f)},new[]{new Vector2(0.680000f,0.450000f),new Vector2(0.700000f,0.510000f),new Vector2(0.690000f,0.550000f),new Vector2(0.700000f,0.580000f)},new[]{new Vector2(0.650000f,0.550000f),new Vector2(0.700000f,0.580000f),new Vector2(0.730000f,0.600000f),new Vector2(0.790000f,0.580000f),new Vector2(0.840000f,0.560000f),new Vector2(0.890000f,0.580000f),new Vector2(0.950000f,0.590000f)},new[]{new Vector2(0.730000f,0.600000f),new Vector2(0.770000f,0.640000f),new Vector2(0.780000f,0.665000f),new Vector2(0.740000f,0.680000f),new Vector2(0.680000f,0.700000f),new Vector2(0.600000f,0.720000f),new Vector2(0.560000f,0.730000f)},new[]{new Vector2(0.550000f,0.590000f),new Vector2(0.550000f,0.640000f),new Vector2(0.550000f,0.690000f),new Vector2(0.560000f,0.730000f),new Vector2(0.540000f,0.780000f),new Vector2(0.510000f,0.810000f),new Vector2(0.480000f,0.840000f),new Vector2(0.500000f,0.890000f),new Vector2(0.540000f,0.940000f),new Vector2(0.580000f,1.000000f)}};
 static float TownPerspective(float y)=>.76f+.34f*y;
 static float TownRoadDistance(Vector2 p,Vector2 a,Vector2 b){p=Vector2.Scale(p,new Vector2(1536,1024));a=Vector2.Scale(a,new Vector2(1536,1024));b=Vector2.Scale(b,new Vector2(1536,1024));var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
 static bool TownFootprintClear(Vector2 p,float rx,float ry){for(int t=0;t<16;t++){float angle=t*Mathf.PI/8;var q=p+new Vector2(rx*Mathf.Cos(angle),-ry+ry*Mathf.Sin(angle));foreach(var line in TownRoads)for(int i=1;i<line.Length;i++)if(TownRoadDistance(q,line[i-1],line[i])<9)return false;}return true;}
 Material townGroundContact;
 void DrawTownGroundSprite(Rect map,Vector2 ground,float width,string sheet,int stage,float opacity=1){var tex=TownSheet(sheet);if(!tex)return;var uv=TownSpriteBoundsTable[sheet+":"+stage].uv;float w=map.width*width*TownPerspective(ground.y),h=w*(tex.height*uv.height)/(tex.width*uv.width);var r=new Rect(map.x+ground.x*map.width-w/2,map.y+ground.y*map.height-h,w,h);if(townOffscreenCapture||Event.current.type==EventType.Repaint){if(!townGroundContact)townGroundContact=new Material(Resources.Load<Shader>("TownGroundContact"));var shadow=new Rect(r.center.x-w*.54f,r.yMax-w*.19f,w*1.08f,w*.24f);Graphics.DrawTexture(shadow,Texture2D.whiteTexture,new Rect(0,0,1,1),0,0,0,0,Color.white,townGroundContact);}if(townOffscreenCapture)Graphics.DrawTexture(r,tex,uv,0,0,0,0,new Color(.5f,.5f,.5f,.5f*opacity));else{var old=GUI.color;GUI.color=new Color(old.r,old.g,old.b,old.a*opacity);GUI.DrawTextureWithTexCoords(r,tex,uv,true);GUI.color=old;}}
 }}
