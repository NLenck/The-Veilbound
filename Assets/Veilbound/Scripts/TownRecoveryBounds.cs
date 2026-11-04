using System.Collections.Generic;using UnityEngine;
namespace Veilbound {public partial class VeilGame {
static readonly Dictionary<string,Rect> TownRecoveryUV=new Dictionary<string,Rect>{
{"Recovery/blacksmith:0",new Rect(0.21679688f,0.06738281f,0.67122396f,0.89355469f)},
{"Recovery/home:0",new Rect(0.09250399f,0.02232855f,0.87161085f,0.94338118f)},
{"Recovery/housing:0",new Rect(0.02018229f,0.55664062f,0.30143229f,0.39453125f)},
{"Recovery/housing:1",new Rect(0.33463542f,0.55664062f,0.33007812f,0.38769531f)},
{"Recovery/housing:2",new Rect(0.67447917f,0.55664062f,0.31250000f,0.42187500f)},
{"Recovery/housing:3",new Rect(0.02604167f,0.05371094f,0.28580729f,0.50292969f)},
{"Recovery/housing:4",new Rect(0.36197917f,0.04687500f,0.29882812f,0.50976562f)},
{"Recovery/housing:5",new Rect(0.72330729f,0.03515625f,0.25781250f,0.52148438f)},
{"Recovery/hunters:0",new Rect(0.04361979f,0.11718750f,0.91341146f,0.75390625f)},
{"Recovery/library:0",new Rect(0.06901042f,0.10937500f,0.91015625f,0.82617188f)},
{"Recovery/mine:0",new Rect(0.02929688f,0.12304688f,0.94856771f,0.74902344f)},
{"Recovery/quarry:0",new Rect(0.07682292f,0.04882812f,0.88151042f,0.89843750f)},
{"Recovery/timber:0",new Rect(0.09505208f,0.14453125f,0.87239583f,0.72656250f)},
{"Recovery/weaver:0",new Rect(0.20898438f,0.10644531f,0.67968750f,0.84570312f)},
};
}}