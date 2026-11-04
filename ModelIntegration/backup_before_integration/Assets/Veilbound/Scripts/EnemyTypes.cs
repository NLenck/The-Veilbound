namespace Veilbound
{
    public enum HuskKind { Standard,Lobber,Brute,Small,Volatile }
    public static class EnemyTypes
    {
        public static float Health(int kind)=>kind==1?1.8f:kind==2?2:kind==3?.5f:1;
        public static float Speed(int kind)=>kind==1?.8f:kind==2?.55f:kind==3?1.4f:1;
        public static float Experience(int kind)=>kind==2?1.6f:kind==3?.7f:1;
        public static float Damage(int kind)=>kind==2?1.5f:kind==3?.5f:1;
        public static float Radius(int kind)=>kind==2?.72f:kind==3?.24f:.43f;
    }
}
