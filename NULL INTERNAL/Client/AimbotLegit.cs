using System.Numerics;

namespace Client
{
    internal static class AimbotLegit
    {
        private static CancellationTokenSource _cts;
        private static Entity _currentTarget;
        private static uint _originalAimTargetValue;

        private static readonly Random _random = new Random();
        private static DateTime _keyHeldStartTime = DateTime.MinValue;
        private static bool _isAimKeyHeld = false;

        private static DateTime _lastTargetLockTime = DateTime.MinValue;
        private const int TargetLockDurationMs = 300;

        public static Entity CurrentTarget => _currentTarget;

        public static void Work()
        {
            if (_cts != null)
                return;

            _cts = new CancellationTokenSource();
            Task.Run(() => Loop(_cts.Token));
        }

        public static void Stop()
        {
            _cts?.Cancel();
            _cts = null;
            ReleaseAimAndRestore();
        }

        private static async Task Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!Config.AimbotLegit || !Core.HaveMatrix)
                    {
                        ReleaseAimAndRestore();
                        await Task.Delay(200, token);
                        continue;
                    }

                    bool isAimKeyPressedNow = (WinAPI.GetAsyncKeyState(Config.AimbotKey) & 0x8000) != 0;

                    if (isAimKeyPressedNow)
                    {
                        if (!_isAimKeyHeld)
                            _keyHeldStartTime = DateTime.Now;

                        float effectiveDelay = Math.Max(120f, 0);

                        if ((DateTime.Now - _keyHeldStartTime).TotalMilliseconds >= effectiveDelay)
                        {
                            if (_currentTarget == null || _currentTarget.IsDead)
                                FindAndSetNewTarget();

                            if (_currentTarget != null && !_currentTarget.IsDead)
                                PerformSmoothedAim();
                        }
                    }
                    else
                    {
                        ReleaseAimAndRestore();
                    }

                    _isAimKeyHeld = isAimKeyPressedNow;

                    int delay = Math.Clamp(10, 5, 30);
                    await Task.Delay(delay, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ReleaseAimAndRestore();
                    await Task.Delay(100, token);
                }
            }

            ReleaseAimAndRestore();
        }

        private static void FindAndSetNewTarget()
        {
            if (_currentTarget != null && !_currentTarget.IsDead)
            {
                if ((DateTime.Now - _lastTargetLockTime).TotalMilliseconds < TargetLockDurationMs &&
                    IsCrosshairNearTarget(_currentTarget))
                    return;
            }

            _currentTarget = FindBestTarget();
            _originalAimTargetValue = 0;
            _lastTargetLockTime = DateTime.Now;
        }

        private static void PerformSmoothedAim()
        {
            if (_currentTarget == null || _currentTarget.IsDead) return;

            if (!IsCrosshairNearTarget(_currentTarget))
            {
                ReleaseAimAndRestore();
                return;
            }

            Vector3 aimPoint = _random.NextDouble() < 0.25
                ? _currentTarget.Head
                : _currentTarget.Head;

            Vector2 targetScreen = W2S.WorldToScreen(Core.CameraMatrix, aimPoint, Core.Width, Core.Height);
            if (float.IsNaN(targetScreen.X) || float.IsNaN(targetScreen.Y)) return;

            Vector2 centerScreen = new(Core.Width / 2f, Core.Height / 2f);

            float smoothFactor = Math.Clamp(Config.Smooth, 1f, 10f);
            Vector2 smoothed = Vector2.Lerp(centerScreen, targetScreen, 1f / smoothFactor);

            if (Vector2.Distance(centerScreen, smoothed) > Config.AimFovCircle)
                return;

            SetAimTargetTransform();
        }

        private static void SetAimTargetTransform()
        {
            if (_currentTarget == null) return;

            nuint aimTargetAddress = _currentTarget.Address + Offsets.HeadCollider;
            nuint sourceTransformAddress = _currentTarget.Address + Offsets.AimbotVisible;

            try
            {
                if (_originalAimTargetValue == 0 &&
                    InternalMemory.Read<uint>(aimTargetAddress, out uint currentValue))
                {
                    _originalAimTargetValue = currentValue;
                }

                if (InternalMemory.Read<uint>(sourceTransformAddress, out uint visibleTransform) && visibleTransform != 0)
                {
                    InternalMemory.Write(aimTargetAddress, visibleTransform);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void ReleaseAimAndRestore()
        {
            try
            {
                if (_currentTarget != null && _originalAimTargetValue != 0)
                {
                    nuint aimTargetAddress = _currentTarget.Address + Offsets.HeadCollider;

                    if (InternalMemory.Read<uint>(aimTargetAddress, out uint currentValue) &&
                        currentValue != _originalAimTargetValue)
                    {
                        InternalMemory.Write(aimTargetAddress, _originalAimTargetValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AimRestore] {ex.Message}");
            }

            _currentTarget = null;
            _originalAimTargetValue = 0;
        }

        private static Entity FindBestTarget()
        {
            Entity bestTarget = null;
            float closestDist = float.MaxValue;
            Vector2 centerScreen = new(Core.Width / 2f, Core.Height / 2f);

            foreach (var entity in Core.Entities.Values.ToList())
            {
                if (entity == null || entity.Address == 0 || entity.IsDead)
                    continue;
                if (Config.IgnoreKnocked && entity.IsKnocked)
                    continue;

                var screenPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (float.IsNaN(screenPos.X) || float.IsNaN(screenPos.Y))
                    continue;

                // Giới hạn trong FOV
                float dist2D = Vector2.Distance(centerScreen, screenPos);
                if (dist2D > Config.AimFovCircle)
                    continue;

                // Ưu tiên gần tâm hơn
                if (dist2D < closestDist)
                {
                    closestDist = dist2D;
                    bestTarget = entity;
                }
            }

            return bestTarget;
        }

        private static bool IsCrosshairNearTarget(Entity target)
        {
            if (target == null) return false;

            var screenPos = W2S.WorldToScreen(Core.CameraMatrix, target.Head, Core.Width, Core.Height);
            if (float.IsNaN(screenPos.X) || float.IsNaN(screenPos.Y)) return false;

            Vector2 centerScreen = new(Core.Width / 2f, Core.Height / 2f);
            return Vector2.Distance(centerScreen, screenPos) <= Config.AimFovCircle;
        }
    }
}