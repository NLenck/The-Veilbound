using UnityEngine;
namespace Veilbound {public partial class VeilGame {
 Texture2D lightEquipmentAtlas,processedMaterialAtlas;
 void DrawGearArtwork(Rect area,CraftedGear item){
  if(item==null)return;InitSmithArt();int metal=item.slot==9?WeaponRules.Material(item.item):Metals.Index(item.item);
  if(item.slot==9||metal>=0){SmithIcon(area,item.slot==9?8:item.slot,item.item<0?new Color(.62f,.42f,.23f):SmithTint(metal));return;}
  if(!lightEquipmentAtlas)lightEquipmentAtlas=Resources.Load<Texture2D>("InventoryUI/LeatherClothIcons");if(!lightEquipmentAtlas)return;int material=Gear.Material(item.item),slot=item.slot;int[] leather={0,7,1,2,5,6,3,4,14},cloth={8,8,9,10,13,10,11,12,LightGear.FabricIndex(Gear.Tier(item.item))==0?15:14};int cell=(material==2?leather:cloth)[slot];Rect fit=new Rect(area.center.x-Mathf.Min(area.width,area.height)/2,area.center.y-Mathf.Min(area.width,area.height)/2,Mathf.Min(area.width,area.height),Mathf.Min(area.width,area.height));GUI.DrawTextureWithTexCoords(fit,lightEquipmentAtlas,new Rect(cell%4*.25f,1-(cell/4+1)*.25f,.25f,.25f));
 }
 Texture2D ProcessedMaterialTexture(string name){
  if(!name.StartsWith("processed-"))return null;if(!processedMaterialAtlas)processedMaterialAtlas=Resources.Load<Texture2D>("InventoryUI/ProcessedMaterials");if(!processedMaterialAtlas)return null;int index;if(!int.TryParse(name.Substring(name.LastIndexOf('-')+1),out index))return null;int source=name.StartsWith("processed-leather-")?0:1;
  var previous=RenderTexture.active;var target=RenderTexture.GetTemporary(128,128,0,RenderTextureFormat.ARGB32);Graphics.Blit(processedMaterialAtlas,target,new Vector2(.2f,.5f),new Vector2(Mathf.Clamp(index,0,4)*.2f,source==0?.5f:0));RenderTexture.active=target;var texture=new Texture2D(128,128,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear};texture.ReadPixels(new Rect(0,0,128,128),0,0);texture.Apply(false,true);RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);return texture;
 }
 void DrawProcessedMaterial(Rect area,int source,int index){var texture=MaterialIcon("processed-"+(source==0?"leather-":"cloth-")+Mathf.Clamp(index,0,4));if(texture)GUI.DrawTexture(area,texture,ScaleMode.ScaleToFit);}
}}
