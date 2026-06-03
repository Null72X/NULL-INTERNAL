using System.Numerics;

namespace Client
{
    internal static class Aimbot
    {
        private static Entity _HexCurrentTarget = null;
        private static bool _HexFirstSnapDone = false;
        private static long _lastSnap;

        internal static void Work()
        {
            while (true)
            {
                if (!Config.AimBot)
                {
                    Thread.Sleep(10);
                    continue;
                }

                bool keyHeld =
                    (WinAPI.GetAsyncKeyState(Config.AimbotKey) & 0x8000) != 0;

                if (!keyHeld)
                {
                    _HexFirstSnapDone = false;
                    _HexCurrentTarget = null;

                    Thread.Sleep(10);
                    continue;
                }

                Entity target = FindBestTarget();

                if (target != null)
                {
                    switch (Config.AimbotMode)
                    {
                        case "AimBotRage":
                            {
                                var aimRotation =
                                    MathUtils.GetRotationToLocation(
                                        target.Head,
                                        0.1f,
                                        Core.LocalMainCamera);

                                InternalMemory.Write(
                                    Core.LocalPlayer + Offsets.AimRotation,
                                    aimRotation);
                                Thread.Sleep(0);
                                break;
                            }

                        case "AimBotVisible":
                            {
                                AimAtTarget(target);
                                break;
                            }

                        case "AimBotHex":
                            {
                                ExecuteHexAimbot(target);
                                break;
                            }
                    }
                }

                Thread.Sleep(5);
            }
        }

        private static void ExecuteHexAimbot(Entity target)
        {
            if (target == null || target.Address == 0)
                return;

            bool isNewTarget = (_HexCurrentTarget != target);

            bool timeout =
                Environment.TickCount64 - _lastSnap > 500;

            if (isNewTarget || timeout)
            {
                _HexCurrentTarget = target;
                _HexFirstSnapDone = false;
            }

            Quaternion perfectRotation =
                MathUtils.GetRotationToLocation(
                    target.Head,
                    0.1f,
                    Core.LocalMainCamera);

            float smoothFactor =
                Math.Clamp(Config.Smooth / 100f, 0f, 1f);

            if (!_HexFirstSnapDone)
            {
                InternalMemory.Write(
                    Core.LocalPlayer + Offsets.AimRotation,
                    perfectRotation);

                _HexFirstSnapDone = true;
                _lastSnap = Environment.TickCount64;
            }
            else if (smoothFactor >= 0.99f)
            {
                InternalMemory.Write(
                    Core.LocalPlayer + Offsets.AimRotation,
                    perfectRotation);
            }
            else
            {
                if (InternalMemory.Read(
                    Core.LocalPlayer + Offsets.AimRotation,
                    out Quaternion currentRotation))
                {
                    Quaternion newRotation =
                        Quaternion.Slerp(
                            currentRotation,
                            perfectRotation,
                            smoothFactor);

                    InternalMemory.Write(
                        Core.LocalPlayer + Offsets.AimRotation,
                        newRotation);
                }
            }

            if (InternalMemory.Read(
                target.Address + Offsets.AimbotVisible,
                out uint headCollider))
            {
                if (headCollider != 0)
                {
                    InternalMemory.Write(
                        target.Address + Offsets.HeadCollider,
                        headCollider);
                }
            }
        }

        private static Entity FindBestTarget()
        {
            Entity bestTarget = null;

            float closestDistance = float.MaxValue;

            float screenCenterX = Core.Width * 0.5f;
            float screenCenterY = Core.Height * 0.5f;

            foreach (Entity entity in Core.Entities.Values)
            {
                if (entity == null)
                    continue;

                if (entity.IsDead)
                    continue;

                if (Config.IgnoreKnocked && entity.IsKnocked)
                    continue;

                Vector2 head2D =
                    W2S.WorldToScreen(
                        Core.CameraMatrix,
                        entity.Head,
                        Core.Width,
                        Core.Height);

                if (head2D.X <= 1f || head2D.Y <= 1f)
                    continue;

                float distance =
                    Vector3.Distance(
                        Core.LocalMainCamera,
                        entity.Head);

                if (distance > Config.AimBotMaxDistance)
                    continue;

                float dx = head2D.X - screenCenterX;
                float dy = head2D.Y - screenCenterY;

                float crosshairDistance =
                    MathF.Sqrt(dx * dx + dy * dy);

                if (crosshairDistance <= Config.AimFovCircle &&
                    crosshairDistance < closestDistance)
                {
                    closestDistance = crosshairDistance;
                    bestTarget = entity;
                }
            }

            return bestTarget;
        }

        private static void AimAtTarget(Entity target)
        {
            if (target == null || target.Address == 0)
                return;

            if (InternalMemory.Read(
                target.Address + Offsets.AimbotVisible,
                out uint headCollider))
            {
                if (headCollider != 0)
                {
                    InternalMemory.Write(
                        target.Address + Offsets.HeadCollider,
                        headCollider);
                }
            }
        }
    }

}