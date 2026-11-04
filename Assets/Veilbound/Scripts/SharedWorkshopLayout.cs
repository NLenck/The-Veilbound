using UnityEngine;
namespace Veilbound {
 // These are the single layout definitions used by every gathering/crafting screen.
 public static class SharedWorkshopLayout {
  public static readonly Rect Frame=new Rect(46,132,1508,720),Catalog=new Rect(62,350,330,466),Slots=new Rect(408,350,684,270),Orders=new Rect(408,636,684,180),ProcessingDetails=new Rect(1108,350,422,466),EquipmentGrid=new Rect(62,350,1064,466),EquipmentDetails=new Rect(1140,350,390,466);
  public static Rect Tab(int index,int count){float width=1468f/count;return new Rect(64+index*width,305,width-12,35);}
  public static Rect EquipmentCell(int row,int column,int columns){float width=870f/Mathf.Max(8,columns);return new Rect(220+column*width+(width-58)/2,437+row*37,58,35);}
 }
}
