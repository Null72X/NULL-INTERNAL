using System.Collections.Concurrent;
using System.Numerics;

namespace Client
{
    internal static class Core
    {
        public static uint GameInstance;
        internal static IntPtr Handle;
        internal static uint LibUnity;
        internal static int Width = -1;
        internal static int Height = -1;
        internal static bool IsSpectating = true;
        internal static bool HaveMatrix = false;
        internal static Matrix4x4 CameraMatrix;
        internal static ulong LocalPlayer;
        internal static Vector3 LocalMainCamera;
        public static ConcurrentDictionary<long, Entity> Entities = new();
    }
}
