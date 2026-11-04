using System.Collections.Generic;
using UnityEngine;

namespace Veilbound
{
    // Shared Dijkstra flow fields route whole waves around prop clusters, with separate body clearances.
    public sealed class ArenaNavigation
    {
        readonly int N;const float Cell=.75f;readonly float Origin;
        readonly ArenaCollision collision;
        readonly float[] radii={.25f,.44f,.74f,1.12f};
        readonly bool[][] open=new bool[4][];
        readonly int[][] cost=new int[4][];
        readonly byte[][] connections=new byte[4][];
        readonly int[] dx={1,-1,0,0,1,1,-1,-1},dz={0,0,1,-1,1,-1,1,-1};
        readonly List<int> heap=new List<int>();
        readonly List<int> heapCost=new List<int>();
        Vector3 goal;float clock;int lastCell=-1;
        public ArenaNavigation(ArenaCollision collision)
        {
            this.collision=collision;N=Mathf.CeilToInt((collision.Radius+1)*2/Cell)+1;Origin=-(N-1)*Cell*.5f;
            for(int g=0;g<4;g++)
            {
                open[g]=new bool[N*N];cost[g]=new int[N*N];connections[g]=new byte[N*N];
                for(int i=0;i<N*N;i++)open[g][i]=collision.Free(Point(i),radii[g]+.035f);
                for(int i=0;i<N*N;i++)if(open[g][i])for(int d=0;d<8;d++)
                    if(GridNeighbour(g,i,d,out int next)&&collision.ClearLine(Point(i),Point(next),radii[g]+.025f))connections[g][i]|=(byte)(1<<d);
            }
            Tick(Vector3.zero,1,true);
        }
        int Group(float radius)=>radius<=.26f?0:radius<=.45f?1:radius<=.75f?2:3;
        int Index(Vector3 p)=>Mathf.Clamp(Mathf.RoundToInt((p.x-Origin)/Cell),0,N-1)*N+Mathf.Clamp(Mathf.RoundToInt((p.z-Origin)/Cell),0,N-1);
        Vector3 Point(int i)=>new Vector3(Origin+(i/N)*Cell,0,Origin+(i%N)*Cell);
        void Push(int id,int value)
        {
            int i=heap.Count;heap.Add(id);heapCost.Add(value);
            while(i>0){int parent=(i-1)/2;if(heapCost[parent]<=value)break;heap[i]=heap[parent];heapCost[i]=heapCost[parent];i=parent;}
            heap[i]=id;heapCost[i]=value;
        }
        int Pop(out int value)
        {
            int id=heap[0];value=heapCost[0];int n=heap.Count-1,last=heap[n],v=heapCost[n];heap.RemoveAt(n);heapCost.RemoveAt(n);
            if(n>0){int i=0;while(i*2+1<n){int child=i*2+1;if(child+1<n&&heapCost[child+1]<heapCost[child])child++;if(heapCost[child]>=v)break;heap[i]=heap[child];heapCost[i]=heapCost[child];i=child;}heap[i]=last;heapCost[i]=v;}return id;
        }
        int NearestOpen(int g,Vector3 p)
        {
            int centre=Index(p);if(open[g][centre])return centre;
            float best=float.MaxValue;int found=-1;
            for(int x=-5;x<=5;x++)for(int z=-5;z<=5;z++)
            {int a=centre/N+x,b=centre%N+z;if(a<0||b<0||a>=N||b>=N)continue;int id=a*N+b;if(open[g][id]&&(Point(id)-p).sqrMagnitude<best){found=id;best=(Point(id)-p).sqrMagnitude;}}
            return found;
        }
        bool GridNeighbour(int g,int current,int direction,out int next)
        {
            int x=current/N,z=current%N,a=x+dx[direction],b=z+dz[direction];next=0;
            if(a<0||b<0||a>=N||b>=N)return false;next=a*N+b;if(!open[g][next])return false;
            if(direction>=4&&(!open[g][a*N+z]||!open[g][x*N+b]))return false;return true;
        }
        bool Neighbour(int g,int current,int direction,out int next)
        {next=0;if((connections[g][current]&(1<<direction))==0)return false;return GridNeighbour(g,current,direction,out next);}
        public void Tick(Vector3 target,float dt,bool force=false)
        {
            clock-=dt;goal=target;int cell=Index(target);
            if(!force&&(clock>0||cell==lastCell))return;
            clock=.35f;lastCell=cell;
            for(int g=0;g<4;g++)
            {
                for(int i=0;i<N*N;i++)cost[g][i]=int.MaxValue;
                int start=NearestOpen(g,target);if(start<0)continue;heap.Clear();heapCost.Clear();cost[g][start]=0;Push(start,0);
                while(heap.Count>0)
                {
                    int id=Pop(out int value);if(value!=cost[g][id])continue;
                    for(int d=0;d<8;d++)if(Neighbour(g,id,d,out int next))
                    {int candidate=value+(d<4?10:14);if(candidate<cost[g][next]){cost[g][next]=candidate;Push(next,candidate);}}
                }
            }
        }
        public bool Reachable(Vector3 p,float radius)
        {int g=Group(radius),id=NearestOpen(g,p);return id>=0&&cost[g][id]<int.MaxValue&&collision.ClearLine(p,Point(id),radius);}
        public Vector3 Move(Vector3 p,float distance,float radius)
        {
            if(collision.ClearLine(p,goal,radius))return collision.Move(p,(goal-p).normalized*distance,radius);
            int g=Group(radius),centre=Index(p),best=-1;float score=float.MaxValue;
            for(int x=-2;x<=2;x++)for(int z=-2;z<=2;z++)
            {
                int a=centre/N+x,b=centre%N+z;if(a<0||b<0||a>=N||b>=N)continue;int id=a*N+b;
                if(!open[g][id]||cost[g][id]==int.MaxValue)continue;Vector3 candidate=Point(id);
                float s=cost[g][id]+Vector3.Distance(p,candidate)*10/Cell;
                if(s<score&&collision.ClearLine(p,candidate,radius)){score=s;best=id;}
            }
            if(best<0)return collision.Steer(p,goal,distance,radius,1);
            // Follow several downhill cells when they remain visible, smoothing the grid path.
            int cursor=best;Vector3 waypoint=Point(best);
            for(int step=0;step<5;step++)
            {
                int next=cursor;
                for(int d=0;d<8;d++)if(Neighbour(g,cursor,d,out int n)&&cost[g][n]<cost[g][next])next=n;
                if(next==cursor||!collision.ClearLine(p,Point(next),radius))break;
                waypoint=Point(next);cursor=next;
            }
            return collision.Move(p,(waypoint-p).normalized*Mathf.Min(distance,Vector3.Distance(p,waypoint)),radius);
        }
    }
}
