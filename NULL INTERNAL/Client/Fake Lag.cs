using System.Runtime.InteropServices;

public static class FakeLag
{
    private const string DllName = "WinDivert.dll";
    private const string SysName = "WinDivert64.sys";

    private static readonly string AppDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fake Lag Dlls");

    private static readonly string DllPath = Path.Combine(AppDataPath, DllName);
    private static readonly string SysPath = Path.Combine(AppDataPath, SysName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr WinDivertOpen(string filter, int layer, short priority, ulong flags);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool WinDivertClose(IntPtr handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool WinDivertRecv(IntPtr handle, byte[] pPacket, int packetLen, int flags, IntPtr pAddr, ref int readLen);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool WinDivertSend(IntPtr handle, byte[] pPacket, int packetLen, int flags, IntPtr pAddr);

    private static IntPtr handle = IntPtr.Zero;
    private static Thread divertThread;
    private static bool isRunning = false;
    private static bool dllLoaded = false;

    public static bool IsRunning => isRunning;
    public static int LagMilliseconds = 5 ;

    static FakeLag()
    {
        Directory.CreateDirectory(AppDataPath);
    }

    public static bool IsDllLoaded() => dllLoaded;

    private static bool LoadWinDivertDll()
    {
        if (dllLoaded) return true;

        try
        {
            if (!File.Exists(DllPath)) throw new FileNotFoundException($"{DllName} not found at {DllPath}");
            if (!File.Exists(SysPath)) throw new FileNotFoundException($"{SysName} not found at {SysPath}");

            IntPtr hModule = LoadLibrary(DllPath);
            if (hModule == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Failed to load {DllName}. Error code: {errorCode}");
            }

            dllLoaded = true;
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static bool Start()
    {
        if (isRunning) return true;

        if (!LoadWinDivertDll())
        {
            return false;
        }

        try
        {
            string filter = "inbound and udp.PayloadLength >= 25";
            handle = WinDivertOpen(filter, 0, 0, 0);
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            isRunning = true;
            divertThread = new Thread(Run) { IsBackground = true, Priority = ThreadPriority.Highest };
            divertThread.Start();

            return true;
        }
        catch (Exception ex)
        {
            Stop();
            return false;
        }
    }

    public static void Stop()
    {
        if (!isRunning) return;

        isRunning = false;

        try
        {
            divertThread?.Join(1);

            if (handle != IntPtr.Zero)
            {
                WinDivertClose(handle);
                handle = IntPtr.Zero;
            }
        }
        catch (Exception ex)
        {

        }
        finally
        {
            divertThread = null;
        }
    }

    private static void Run()
    {
        byte[] packet = new byte[65535];
        IntPtr addr = Marshal.AllocHGlobal(64);
        int readLen = 0;

        try
        {
            while (isRunning)
            {
                bool received = WinDivertRecv(handle, packet, packet.Length, 0, addr, ref readLen);
                if (!received)
                {
                    Thread.SpinWait(1);
                    continue;
                }

                if (LagMilliseconds > 0)
                {
                    Thread.Sleep(LagMilliseconds);
                }

                WinDivertSend(handle, packet, readLen, 0, addr);
            }
        }
        catch (Exception ex)
        {

        }
        finally
        {
            Marshal.FreeHGlobal(addr);
        }
    }
}
