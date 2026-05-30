using System.Numerics;

namespace Client
{
    internal class Entity
    {
        internal uint Address;
        internal bool IsKnown;
        internal Bool IsTeam;

        public Vector3 Head;
        public Vector3 Breast;
        public Vector3 Root;
        public Vector3 Hip;

        public Vector3 LeftBiceps;
        public Vector3 RightBiceps;

        public Vector3 LeftShoulder;
        public Vector3 RightShoulder;

        public Vector3 LeftWristJoint;
        public Vector3 RightWristJoint;

        public Vector3 LeftLeg;
        public Vector3 RightLeg;

        public Vector3 LeftFoot;
        public Vector3 RightFoot;

        public int Level;
        public int WeaponID;
        internal short Health;
        internal bool IsDead;
        internal bool IsKnocked;
        internal string Name;
        internal float Distance;
    }
}
