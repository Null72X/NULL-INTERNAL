using Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class SpeedTimer
    {
        private static uint LastValidSpeedTimer;
        private static Task TimerTask;
        private static CancellationTokenSource Cts = new();
        private static bool IsRunning;
        private static int FailCount;
        private static float LastWrittenValue = -1f;
        private const float NormalSpeed = 0.033000f;
        private const float MaxSpeed = 1.7f;

        internal static void Work()
        {
            if (IsRunning)
                return;

            IsRunning = true;

            Cts = new CancellationTokenSource();

            FailCount = 0;

            LastWrittenValue = -1f;

            TimerTask = Task.Run(async () =>
            {
                while (!Cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (!Config.SpeedTimer)
                        {
                            await Task.Delay(100, Cts.Token);
                            continue;
                        }

                        ApplySpeedTimer();

                        await Task.Delay(35, Cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        await Task.Delay(100, Cts.Token);
                    }
                }
            }, Cts.Token);
        }

        internal static void Stop()
        {
            if (!IsRunning)
                return;

            try
            {
                Cts.Cancel();

                RestoreNormalSpeed();
            }
            catch
            {
            }

            IsRunning = false;

            FailCount = 0;

            LastWrittenValue = -1f;
        }

        private static void ApplySpeedTimer()
        {
            try
            {
                uint speedTimer = LastValidSpeedTimer;

                // Invalid pointer
                if (speedTimer == 0)
                {
                    FailCount++;

                    if (FailCount > 10)
                    {
                        FailCount = 0;

                        LastValidSpeedTimer = 0;
                    }

                    return;
                }

                FailCount = 0;

                // Stable speed
                float speedValue =
                    NormalSpeed *
                    MaxSpeed;

                // Prevent useless writes
                if (Math.Abs(LastWrittenValue - speedValue) < 0.002f)
                    return;

                // Write memory
                InternalMemory.Write(
                    (ulong)speedTimer + Offsets.FixedDeltaTime,
                    speedValue
                );

                LastWrittenValue = speedValue;
            }
            catch
            {
                FailCount++;
            }
        }

        internal static void PublishGameTimer(uint currentGame)
        {
            try
            {
                if (currentGame == 0)
                    return;

                if (!InternalMemory.Read<uint>(
                    (ulong)currentGame + Offsets.GameTimer,
                    out uint timerPtr))
                    return;

                if (timerPtr == 0)
                    return;

                // Validate pointer
                if (!InternalMemory.Read<float>(
                    (ulong)timerPtr + Offsets.FixedDeltaTime,
                    out _))
                    return;

                LastValidSpeedTimer = timerPtr;
            }
            catch
            {
                LastValidSpeedTimer = 0;
            }
        }

        internal static void RestoreNormalSpeed()
        {
            try
            {
                if (LastValidSpeedTimer == 0)
                    return;

                InternalMemory.Write(
                    (ulong)LastValidSpeedTimer + Offsets.FixedDeltaTime,
                    NormalSpeed
                );

                LastWrittenValue = NormalSpeed;
            }
            catch
            {
            }
        }
    }
}