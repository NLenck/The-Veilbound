using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 // Digitised from the approved 12-library-v3 layout, not the rejected clean rebuild.
 static readonly Vector2[] ApprovedHomeGrounds={new Vector2(574,376),new Vector2(704,438),new Vector2(510,514),new Vector2(610,626),new Vector2(1018,434),new Vector2(1143,520),new Vector2(756,684),new Vector2(968,685),new Vector2(651,383),new Vector2(593,504),new Vector2(1098,435),new Vector2(1218,557),new Vector2(1061,656)};
 static readonly float[] ApprovedHomeWidths={66,67,73,80,67,76,76,78,61,68,65,70,76};
 static Rect TownAspectRect(Rect area){float w=Mathf.Min(area.width,area.height*1.5f),h=w/1.5f;return new Rect(area.center.x-w/2,area.center.y-h/2,w,h);}
}}
