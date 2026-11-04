using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    // Planar footprints match the arena's grounded props. Small swept steps prevent dodge tunnelling.
    public sealed class ArenaCollision
    {
        public struct Obstacle { public Vector3 centre; public float radius; }
        public readonly List<Obstacle> obstacles=new List<Obstacle>();
        public struct Wall {public Vector3 a,b;public float radius,minX,maxX,minZ,maxZ;}
        public readonly List<Wall> walls=new List<Wall>();
        public void AddWall(Vector3 a,Vector3 b,float radius){a.y=b.y=0;walls.Add(new Wall{a=a,b=b,radius=radius,minX=Mathf.Min(a.x,b.x),maxX=Mathf.Max(a.x,b.x),minZ=Mathf.Min(a.z,b.z),maxZ=Mathf.Max(a.z,b.z)});}
        static Vector3 Nearest(Vector3 a,Vector3 b,Vector3 p){Vector3 d=b-a;return a+d*(d.sqrMagnitude>.000001f?Mathf.Clamp01(Vector3.Dot(p-a,d)/d.sqrMagnitude):0);}
        static bool WallBounds(Wall w,float minX,float maxX,float minZ,float maxZ,float margin)=>maxX+margin>=w.minX&&minX-margin<=w.maxX&&maxZ+margin>=w.minZ&&minZ-margin<=w.maxZ;
        static float CrossXZ(Vector3 a,Vector3 b)=>a.x*b.z-a.z*b.x;
        static float SegmentDistanceSq(Vector3 a,Vector3 b,Vector3 c,Vector3 d){Vector3 u=b-a,v=d-c;float cross=CrossXZ(u,v);if(Mathf.Abs(cross)>.000001f){float s=CrossXZ(c-a,v)/cross,t=CrossXZ(c-a,u)/cross;if(s>=0&&s<=1&&t>=0&&t<=1)return 0;}return Mathf.Min(Mathf.Min((a-Nearest(c,d,a)).sqrMagnitude,(b-Nearest(c,d,b)).sqrMagnitude),Mathf.Min((c-Nearest(a,b,c)).sqrMagnitude,(d-Nearest(a,b,d)).sqrMagnitude));}

        public const float Boundary=50;
        // Enemies emerge through the outer cloud; the player stops before its inner edge.
        public const float PlayerBoundary=42;
        public void Add(Vector3 p,float radius) { p.y=0;obstacles.Add(new Obstacle{centre=p,radius=radius}); }
        public bool Free(Vector3 p,float radius)
        {
            p.y=0;if(p.magnitude>Boundary-radius)return false;
            foreach(var o in obstacles)if((p-o.centre).sqrMagnitude<(radius+o.radius)*(radius+o.radius))return false;
            foreach(var w in walls){float r=radius+w.radius;if(WallBounds(w,p.x,p.x,p.z,p.z,r)&&(p-Nearest(w.a,w.b,p)).sqrMagnitude<r*r)return false;}
            return true;
        }
        public Vector3 Resolve(Vector3 p,float radius,float boundary=Boundary)
        {
            p.y=0;
            for(int pass=0;pass<12;pass++)
            {
                p=Vector3.ClampMagnitude(p,boundary-radius-.005f);
                bool changed=false;
                foreach(var o in obstacles)
                {
                    Vector3 d=p-o.centre;float min=radius+o.radius+.005f;
                    if(d.sqrMagnitude>=min*min)continue;
                    p=o.centre+(d.sqrMagnitude>.00001f?d.normalized:Vector3.right)*min;changed=true;
                }
                foreach(var w in walls){float min=radius+w.radius+.005f;if(!WallBounds(w,p.x,p.x,p.z,p.z,min))continue;Vector3 nearest=Nearest(w.a,w.b,p),diff=p-nearest;if(diff.sqrMagnitude>=min*min)continue;Vector3 normal=diff.sqrMagnitude>.000001f?diff.normalized:new Vector3(-(w.b-w.a).z,0,(w.b-w.a).x).normalized;p=nearest+normal*min;changed=true;}
                if(!changed)break;
            }
            return p;
        }
        public Vector3 Move(Vector3 p,Vector3 delta,float radius,float boundary=Boundary)
        {
            delta.y=0;int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.12f));Vector3 step=delta/steps;
            for(int i=0;i<steps;i++)p=Resolve(p+step,radius,boundary);
            return p;
        }
        public bool ClearLine(Vector3 a,Vector3 b,float radius=0)
        {
            a.y=b.y=0;Vector3 d=b-a;float length=d.sqrMagnitude;
            foreach(var o in obstacles)
            {
                float t=length>.00001f?Mathf.Clamp01(Vector3.Dot(o.centre-a,d)/length):0;
                if((o.centre-(a+d*t)).sqrMagnitude<(o.radius+radius)*(o.radius+radius))return false;
            }
            float minX=Mathf.Min(a.x,b.x),maxX=Mathf.Max(a.x,b.x),minZ=Mathf.Min(a.z,b.z),maxZ=Mathf.Max(a.z,b.z);
            foreach(var w in walls){float r=radius+w.radius;if(WallBounds(w,minX,maxX,minZ,maxZ,r)&&SegmentDistanceSq(a,b,w.a,w.b)<r*r)return false;}
            return true;
        }
        public Vector3 Steer(Vector3 p,Vector3 target,float distance,float radius,int handedness)
        {
            Vector3 desired=(target-p).normalized;
            if(ClearLine(p,p+desired*(radius+1),radius))return Move(p,desired*distance,radius);
            // Try both sides, with a stable preference to avoid left/right oscillation at a pillar.
            Vector3 best=p;float score=float.NegativeInfinity;
            foreach(float angle in new[]{0f,35f,-35f,70f,-70f,105f,-105f,140f,-140f})
            {
                Vector3 dir=Quaternion.Euler(0,angle*handedness,0)*desired;
                if(!ClearLine(p,p+dir*.65f,radius))continue;
                Vector3 candidate=Move(p,dir*distance,radius);
                float value=Vector3.Dot(dir,desired)+((angle>=0)?.08f:0);
                if((candidate-p).magnitude>distance*.5f && value>score){best=candidate;score=value;}
            }
            return best;
        }
    }
}

