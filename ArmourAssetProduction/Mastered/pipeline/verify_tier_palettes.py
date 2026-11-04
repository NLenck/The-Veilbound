import json,struct,io
from pathlib import Path
from PIL import Image
R=Path(r'C:\The Veilbound\ArmourAssetProduction\Mastered');tiers=json.loads((R/'tier_mapping.json').read_text());results=[]
for gender in ['Male','Female']:
 for tier in tiers:
  path=R/f'exports/tiers/{gender}/{tier["tier"]}_{gender}.glb';raw=path.read_bytes();assert raw[:4]==b'glTF';pos=12;chunks={}
  while pos<len(raw):
   size,kind=struct.unpack_from('<II',raw,pos);chunks[kind]=raw[pos+8:pos+8+size];pos+=8+size
  scene=json.loads(chunks[0x4E4F534A]);binary=chunks[0x004E4942];materials=scene['materials'];assert len(materials)==1
  tex=materials[0]['pbrMetallicRoughness']['baseColorTexture']['index'];image=scene['images'][scene['textures'][tex]['source']];view=scene['bufferViews'][image['bufferView']];start=view.get('byteOffset',0);im=Image.open(io.BytesIO(binary[start:start+view['byteLength']])).convert('RGB')
  expected=lambda key:tuple(int(tier[key][i:i+2],16) for i in (1,3,5))
  pixels={'cape':im.getpixel((10*64+32,32)),'belt':im.getpixel((15*64+32,32)),'visor':im.getpixel((14*64+32,32))}
  expected_count=sum(o['triangles'] for o in json.loads((R/f'qa/fitting_{gender}.json').read_text())['objects'] if o['tier']==tier['mesh_family'])
  count=sum(scene['accessors'][p['indices']]['count']//3 for m in scene['meshes'] for p in m['primitives']);bones=len(scene['skins'][0]['joints'])
  metal_rgb=im.getpixel((32,32));expected_metal=tuple(round(v*255) for v in tier['metal_color'])
  passed=metal_rgb==expected_metal and pixels['cape']==expected('cape_color') and pixels['belt']==expected('belt_cloth_color') and max(pixels['visor'])<10 and bones==52 and count==expected_count and len(scene['meshes'])==20
  results.append(dict(gender=gender,tier=tier['tier'],file=str(path),triangles=count,bones=bones,materials=len(materials),metal_rgb=metal_rgb,cape_rgb=pixels['cape'],belt_rgb=pixels['belt'],visor_rgb=pixels['visor'],passed=passed));assert passed
(R/'qa/tier_palette_verification.json').write_text(json.dumps(results,indent=2));print('TIER_PALETTE_EXPORT_CHECKS',len(results),'PASS')
