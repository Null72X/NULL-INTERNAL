using AotForms;
using Guna.UI2.WinForms;
using ImGuiNET;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Client
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer holdTimer;
        private bool keyIsDown = false;
        private Keys? boundKeyEnum = null;
        private DateTime holdStartTime;
        private bool _isListening = false;
        private string _boundKey = null;
        IntPtr mainHandle;
        bool formHidden = true;
        private GlobalKeyboardHook _hook;

        public Form1(IntPtr handle)
        {
            InitializeComponent();

            mainHandle = handle;
            shayanComboBox2.DataSource = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            shayanComboBox2.SelectedItem = Keys.LButton;

            holdTimer = new System.Windows.Forms.Timer();
            holdTimer.Interval = 50;
            holdTimer.Tick += (s, e) => WhileHolding();

            _hook = new GlobalKeyboardHook();
            _hook.KeyPressed += Hook_KeyPressed;
            _hook.KeyDown += Hook_KeyDown;
            _hook.KeyUp += Hook_KeyUp;

            this.FormClosing += (s, e) => _hook.Dispose();
        }

        private void Hook_KeyPressed(Keys key)
        {
            if (key == Keys.F1)
            {
                Core.Entities = new();
                InternalMemory.Cache = new();
            }

            if (key == Keys.F2)
            {
                shayanCheckBox27.Checked = !shayanCheckBox27.Checked;

                if (shayanCheckBox27.Checked)
                {
                    Config.SpeedTimer = true;
                }
                else
                {
                    Config.SpeedTimer = false;

                    SpeedTimer.RestoreNormalSpeed();
                }
            }

            if (key == Keys.F12)
            {
                this.Visible = !formHidden;
                formHidden = !formHidden;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new(378, 460);
            this.TopMost = true;
            this.Text = "NULL INTERNAL";
            FakeLagFileCheck();
            guna2Button1.ForeColor = Color.Red;
            shayanComboBox1.Items.Add("AimBotVisible");
            shayanComboBox1.Items.Add("AimBotHex");
            shayanComboBox1.Items.Add("AimBotRage");
            shayanComboBox1.SelectedIndex = 0;

            shayanComboBox1.SelectedIndexChanged += (s, e) =>
            {
                Config.AimbotMode = shayanComboBox1.SelectedItem.ToString();
            };
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            guna2Panel1.Visible = true;
            guna2Panel3.Visible = false;
            guna2Panel2.Visible = false;
            guna2Panel1.Location = new Point(64, 42);
            guna2Button1.ForeColor = Color.Red;
            guna2Button2.ForeColor = Color.DarkRed;
            guna2Button3.ForeColor = Color.DarkRed;
            guna2Button1.BorderColor = Color.Red;
            guna2Button2.BorderColor = Color.DarkRed;
            guna2Button3.BorderColor = Color.DarkRed;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2Panel1.Visible = false;
            guna2Panel2.Visible = true;
            guna2Panel3.Visible = false;
            guna2Panel2.Location = new Point(64, 42);
            guna2Button1.ForeColor = Color.DarkRed;
            guna2Button2.ForeColor = Color.Red;
            guna2Button3.ForeColor = Color.DarkRed;
            guna2Button1.BorderColor = Color.DarkRed;
            guna2Button2.BorderColor = Color.Red;
            guna2Button3.BorderColor = Color.DarkRed;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2Panel1.Visible = false;
            guna2Panel2.Visible = false;
            guna2Panel3.Visible = true;
            guna2Panel3.Location = new Point(64, 42);
            guna2Button1.ForeColor = Color.DarkRed;
            guna2Button2.ForeColor = Color.DarkRed;
            guna2Button3.ForeColor = Color.Red;
            guna2Button1.BorderColor = Color.DarkRed;
            guna2Button2.BorderColor = Color.DarkRed;
            guna2Button3.BorderColor = Color.Red;
        }

        static IntPtr FindRenderWindow(IntPtr parent)
        {
            IntPtr renderWindow = IntPtr.Zero;
            WinAPI.EnumChildWindows(parent, (hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                WinAPI.GetWindowText(hWnd, sb, sb.Capacity);
                string windowName = sb.ToString();
                if (!string.IsNullOrEmpty(windowName))
                {
                    if (windowName != "HD-Player")
                    {
                        renderWindow = hWnd;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return renderWindow;
        }

        private async Task<bool> StartCheatAsync()
        {
            var processes = Process.GetProcessesByName("HD-Player");
            if (processes.Length != 1)
                throw new Exception("HD-Player process not found or multiple instances");

            var process = processes[0];
            var mainModulePath = Path.GetDirectoryName(process.MainModule.FileName);
            var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");

            if (!File.Exists(adbPath))
                throw new FileNotFoundException("HD-Adb.exe not found");

            var adb = new Adb(adbPath);

            await adb.Kill();

            if (!await adb.Start())
                throw new Exception("ADB start failed");

            string pkg = "com.dts.freefireth";
            string lib = "libil2cpp.so";

            var moduleAddr = await adb.FindModule(pkg, lib);
            if (moduleAddr == 0)
                throw new Exception("libil2cpp.so not found");

            Offsets.Il2Cpp = moduleAddr;
            Core.Handle = FindRenderWindow(mainHandle);

            var esp = new ESP();
            await esp.Start();

            new Thread(Data.Work) { IsBackground = true }.Start();
            new Thread(Aimbot.Work) { IsBackground = true }.Start();
            new Thread(AimbotLegit.Work) { IsBackground = true }.Start();
            new Thread(SilentAim.Work) { IsBackground = true }.Start();
            SpeedTimer.Work();

            this.TopMost = false;
            return true;
        }

        private async void shayanCheckBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (!shayanCheckBox6.Checked)
                return;

            shayanCheckBox6.CheckedColor = Color.Yellow;

            int retryCount = 0;
            const int maxRetries = 10;

            while (retryCount < maxRetries)
            {
                try
                {
                    await StartCheatAsync();
                    shayanCheckBox6.CheckedColor = Color.Red;
                    return;
                }
                catch
                {
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        MessageBox.Show(
                            "Failed to start after multiple attempts.\nPlease restart BlueStacks and try again.",
                            "Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        return;
                    }

                    await Task.Delay(1000);
                }
            }
        }

        private void shayanCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Config.AimBot = shayanCheckBox1.Checked;

            if (shayanCheckBox1.Checked)
            {
                shayanCheckBox24.Checked = false;
                shayanCheckBox29.Checked = false;

                Config.SilentAim = false;
                Config.AimbotLegit = false;
            }
        }

        private void shayanCheckBox24_CheckedChanged(object sender, EventArgs e)
        {
            Config.SilentAim = shayanCheckBox24.Checked;

            if (shayanCheckBox24.Checked)
            {
                shayanCheckBox1.Checked = false;
                shayanCheckBox29.Checked = false;

                Config.AimBot = false;
                Config.AimbotLegit = false;
            }
        }

        private void shayanComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.AimbotMode = shayanComboBox1.SelectedItem.ToString();
        }

        private void shayanCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            Config.IgnoreKnocked = shayanCheckBox2.Checked;
        }

        private void shayanCheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            Config.AimFov = shayanCheckBox3.Checked;
        }

        private void shayanCheckBox4_CheckedChanged(object sender, EventArgs e)
        {
            Config.NoRecoil = shayanCheckBox4.Checked;
        }

        private void shayanCheckBox5_CheckedChanged(object sender, EventArgs e)
        {
            Config.UnlimitedAmmo = shayanCheckBox5.Checked;
        }

        private void shayanSlider1_Click(object sender, EventArgs e)
        {
            var baseFOV = shayanSlider1.Value;
            Config.AimFovCircle = baseFOV;

            float windowWidth = ImGui.GetIO().DisplaySize.X;
            float baseWidth = 1920.0f;

            float scaleFactor = windowWidth / baseWidth;
            float scaledFOV = baseFOV * scaleFactor;

            shayanSlider1.Text = $"{baseFOV}";
        }

        private void shayanSlider2_Click(object sender, EventArgs e)
        {
            var distance = shayanSlider2.Value;

            shayanSlider2.Text = $"{distance}";

            Config.Smooth = distance;
        }

        private void shayanSlider3_Click(object sender, EventArgs e)
        {
            var distance = shayanSlider3.Value;

            shayanSlider3.Text = $"{distance}";

            Config.AimBotMaxDistance = distance;
        }

        private void shayanComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config.AimbotKey = (Keys)shayanComboBox2.SelectedItem;
        }

        private void shayanCheckBox11_CheckedChanged(object sender, EventArgs e)
        {
            var processes = Process.GetProcessesByName("HD-Player");
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        void SetStreamMode(bool state)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (state)
                {
                    SetWindowDisplayAffinity(form.Handle, WDA_EXCLUDEFROMCAPTURE);

                }
                else
                {
                    SetWindowDisplayAffinity(form.Handle, WDA_NONE);
                    ShowInAltTab(form.Handle);
                }

                form.Activated -= Form_Activated;
                form.Deactivate -= Form_Deactivated;

                if (state)
                {
                    form.Activated += Form_Activated;
                    form.Deactivate += Form_Deactivated;
                }
            }
        }

        private void Form_Activated(object sender, EventArgs e)
        {
            if (Config.StreamMode)
                HideFromAltTab(((Form)sender).Handle);
        }

        private void Form_Deactivated(object sender, EventArgs e)
        {
            if (Config.StreamMode)
                HideFromAltTab(((Form)sender).Handle);
        }

        void HideFromAltTab(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            exStyle &= ~WS_EX_APPWINDOW;
            exStyle |= WS_EX_TOOLWINDOW;
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);
        }

        void ShowInAltTab(IntPtr hWnd)
        {
            int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            exStyle |= WS_EX_APPWINDOW;
            exStyle &= ~WS_EX_TOOLWINDOW;
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);
        }

        [DllImport("user32.dll")]
        static extern uint SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
        [DllImport("user32.dll")]

        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_APPWINDOW = 0x00040000;

        const uint WDA_NONE = 0x00000000;
        const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

        private void shayanCheckBox10_CheckedChanged(object sender, EventArgs e)
        {
            bool state = shayanCheckBox10.Checked;
            SetStreamMode(state);
            Config.StreamMode = state;
        }

        private void shayanCheckBox9_CheckedChanged(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void shayanCheckBox22_CheckedChanged(object sender, EventArgs e)
        {
            Core.Entities = new();
            InternalMemory.Cache = new();
        }

        private void UpdateEntities()
        {
            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsTeam != Bool.False) continue;

                TreeNode entityNode = new TreeNode(entity.Name);

                entityNode.Nodes.Add(new TreeNode($"IsKnown: {entity.IsKnown}"));
                entityNode.Nodes.Add(new TreeNode($"IsTeam: {entity.IsTeam}"));
                entityNode.Nodes.Add(new TreeNode($"Head: {entity.Head}"));
                entityNode.Nodes.Add(new TreeNode($"Root: {entity.Root}"));
                entityNode.Nodes.Add(new TreeNode($"Health: {entity.Health}"));
                entityNode.Nodes.Add(new TreeNode($"IsDead: {entity.IsDead}"));
                entityNode.Nodes.Add(new TreeNode($"IsKnocked: {entity.IsKnocked}"));
            }
            Thread.Sleep(1000);
        }

        private void shayanCheckBox23_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEntities();
        }

        private void shayanCheckBox26_CheckedChanged(object sender, EventArgs e)
        {
            Config.InfinitySkyler = shayanCheckBox26.Checked;
        }

        private void shayanCheckBox12_CheckedChanged(object sender, EventArgs e)
        {
            if (shayanCheckBox12.Checked)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            Application.Exit();

            var processes = Process.GetProcessesByName("HD-Player");
            foreach (var process in processes)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        private void shayanCheckBox27_CheckedChanged(object sender, EventArgs e)
        {
            if (shayanCheckBox27.Checked)
            {
                Config.SpeedTimer = true;
            }
            else
            {
                Config.SpeedTimer = false;

                SpeedTimer.RestoreNormalSpeed();
            }
        }

        private void FakeLagFileCheck()
        {
            try
            {
                string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string targetFolder = Path.Combine(roaming, "Fake Lag Dlls");

                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                Assembly asm = Assembly.GetExecutingAssembly();
                var resources = new (string resourceName, string outName)[]
                {
                    ("Client.WinDivert.dll", "WinDivert.dll"),
                    ("Client.WinDivert64.sys", "WinDivert64.sys")
                };

                bool allExist = true;
                foreach (var r in resources)
                {
                    string outPath = Path.Combine(targetFolder, r.outName);
                    if (!File.Exists(outPath))
                    {
                        allExist = false;
                        break;
                    }
                }

                if (allExist)
                {
                    return;
                }

                foreach (var r in resources)
                {
                    string outPath = Path.Combine(targetFolder, r.outName);
                    if (!File.Exists(outPath))
                    {
                        using (Stream rs = asm.GetManifestResourceStream(r.resourceName))
                        {
                            if (rs == null)
                            {
                                MessageBox.Show("Resource not found for Fake Lag");
                                continue;
                            }
                            using (FileStream fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
                            {
                                rs.CopyTo(fs);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Fake Lag");
            }
        }
        private void OnPressStart()
        {
            holdStartTime = DateTime.Now;
        }

        private void WhileHolding()
        {
            FakeLag.Start();
            double elapsedSeconds = (DateTime.Now - holdStartTime).TotalSeconds;
            FakeLagStatus.Text = $"FakeLag Started : {elapsedSeconds:F1} sec";
        }

        private void OnRelease()
        {
            FakeLag.Stop();
            FakeLagStatus.Text = "FakeLag Stopped";
        }

        private void Hook_KeyDown(Keys key)
        {
            if (boundKeyEnum.HasValue && key == boundKeyEnum.Value && !keyIsDown)
            {
                keyIsDown = true;
                OnPressStart();
                holdTimer.Start();
                return;
            }

            if (_isListening)
            {
                _boundKey = key.ToString();
                boundKeyEnum = key;
                _isListening = false;
                guna2Button10.Text = _boundKey;
                return;
            }
        }

        private void Hook_KeyUp(Keys key)
        {
            if (boundKeyEnum.HasValue && key == boundKeyEnum.Value && keyIsDown)
            {
                keyIsDown = false;
                holdTimer.Stop();
                OnRelease();
            }
        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {
            _isListening = true;
            guna2Button10.Text = "Press key";
        }



        private void shayanCheckBox7_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPLine = shayanCheckBox7.Checked;
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2Button5.FillColor = picker.Color;
                Config.ESPLineColor = picker.Color;
            }
        }

        private void shayanCheckBox16_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPBox = shayanCheckBox16.Checked;
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2Button6.FillColor = picker.Color;
                Config.ESPBoxColor = picker.Color;
            }
        }

        private void shayanCheckBox15_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPCornerbox = shayanCheckBox15.Checked;
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2Button8.FillColor = picker.Color;
                Config.ESPCornerboxColor = picker.Color;
            }
        }

        private void shayanCheckBox14_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPFillBox = shayanCheckBox14.Checked;
        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2Button9.FillColor = picker.Color;
                Config.ESPFillBoxColor = picker.Color;
            }
        }

        private void shayanCheckBox13_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPSkeleton = shayanCheckBox13.Checked;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            var picker = new ColorDialog();
            var result = picker.ShowDialog();

            if (result == DialogResult.OK)
            {
                guna2Button4.FillColor = picker.Color;
                Config.ESPSkeletonColor = picker.Color;
            }
        }

        private void shayanCheckBox8_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPHealth = shayanCheckBox8.Checked;
        }

        private void shayanCheckBox20_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPHealthText = shayanCheckBox20.Checked;
        }

        private void shayanCheckBox19_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPDistance = shayanCheckBox19.Checked;
        }

        private void shayanCheckBox18_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPName = shayanCheckBox18.Checked;
        }

        private void shayanCheckBox17_CheckedChanged(object sender, EventArgs e)
        {
            Config.MiniMap = shayanCheckBox17.Checked;
        }

        private void shayanCheckBox25_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPWeapon = shayanCheckBox25.Checked;
        }

        private void shayanCheckBox28_CheckedChanged(object sender, EventArgs e)
        {
            Config.ESPLevel = shayanCheckBox28.Checked;
        }

        private void shayanCheckBox21_CheckedChanged(object sender, EventArgs e)
        {
            Config.FovNearMe = shayanCheckBox21.Checked;
        }

        private void shayanCheckBox29_CheckedChanged(object sender, EventArgs e)
        {
            Config.AimbotLegit = shayanCheckBox29.Checked;

            if (shayanCheckBox29.Checked)
            {
                shayanCheckBox1.Checked = false;
                shayanCheckBox24.Checked = false;

                Config.AimBot = false;
                Config.SilentAim = false;
            }
        }

        private void shayanCheckBox30_CheckedChanged(object sender, EventArgs e)
        {
            Config.SpeedJump = shayanCheckBox30.Checked;
        }

        private void shayanCheckBox34_CheckedChanged(object sender, EventArgs e)
        {
            Config.CameraHack = shayanCheckBox34.Checked;
        }

        private void shayanCheckBox33_CheckedChanged(object sender, EventArgs e)
        {
            Config.MedkitRunning = shayanCheckBox33.Checked;
        }

        private void shayanCheckBox32_CheckedChanged(object sender, EventArgs e)
        {
            Config.FastMediKit = shayanCheckBox32.Checked;
        }

        private void shayanCheckBox31_CheckedChanged(object sender, EventArgs e)
        {
            Config.UnderCamera = shayanCheckBox31.Checked;
        }
    }

    public class GlobalKeyboardHook : IDisposable
    {
        public event Action<Keys> KeyPressed;
        public event Action<Keys> KeyDown;
        public event Action<Keys> KeyUp;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public GlobalKeyboardHook()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            Application.ApplicationExit += (s, e) => Dispose();
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(
                    WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    KeyDown?.Invoke(key);
                    KeyPressed?.Invoke(key); 
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    KeyUp?.Invoke(key);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public void Dispose()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }
    }
}

