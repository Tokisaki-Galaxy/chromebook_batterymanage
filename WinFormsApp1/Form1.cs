using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // --- 配置常量 ---
        private const string InstallDir = @"C:\batteryManage";
        private const string AppName = "chromebook_batterymanage.exe";
        private const string ToolName = "ectool.exe";
        private const string ProcessName = "chromebook_batterymanage"; // 不带 .exe
        private const string AutoStartValueName = "ChromebookBatteryManager";
        private const string GithubUrl = "https://github.com/Tokisaki-Galaxy/chromebook_batterymanage";

        private readonly string _fullAppPath = Path.Combine(InstallDir, AppName);

        public Form1()
        {
            InitializeComponent();
            // 在窗体加载时更新UI状态
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 窗体加载时检查状态并更新UI
            UpdateUIState();
        }

        /// <summary>
        /// 核心函数：检查程序状态并更新UI元素
        /// </summary>
        private void UpdateUIState()
        {
            bool isInstalled = File.Exists(_fullAppPath);
            bool isRunning = Process.GetProcessesByName(ProcessName).Any();

            if (isInstalled)
            {
                // 已安装
                installButton.Text = "Uninstall";
                installButton.ForeColor = Color.DarkGreen;

                if (isRunning)
                {
                    runTempButton.Text = "Stop Service";
                    runTempButton.ForeColor = Color.DarkGoldenrod;
                    runTempButton.Enabled = true;
                }
                else
                {
                    runTempButton.Text = "Start Service";
                    runTempButton.ForeColor = Color.DarkGreen;
                    runTempButton.Enabled = true;
                }
            }
            else
            {
                // 未安装
                installButton.Text = "Install and Run";
                installButton.ForeColor = Color.Red;

                runTempButton.Text = "Run Temporarily";
                runTempButton.ForeColor = Color.Red;
                runTempButton.Enabled = true; // 临时运行总是可用的
            }
        }

        #region 按钮点击事件

        // "安装/卸载" 按钮
        private void button1_Click(object sender, EventArgs e)
        {
            if (File.Exists(_fullAppPath))
            {
                UninstallService();
            }
            else
            {
                InstallService();
            }
            UpdateUIState();
        }

        // "临时运行/启动/停止" 按钮
        private void button2_Click(object sender, EventArgs e)
        {
            bool isInstalled = File.Exists(_fullAppPath);
            bool isRunning = Process.GetProcessesByName(ProcessName).Any();

            if (!isInstalled) // 对应 "Run Temporarily"
            {
                RunService(false); // false 表示不设置自启动
            }
            else if (isRunning) // 对应 "Stop Service"
            {
                StopService();
            }
            else // 对应 "Start Service"
            {
                StartService();
            }
            UpdateUIState();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                // 使用 ShellExecute 来打开默认浏览器
                Process.Start(new ProcessStartInfo(GithubUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 核心功能方法

        private void InstallService()
        {
            try
            {
                ExtractResources();
                SetAutoStart(true);
                StartService();
                MessageBox.Show("The service was successfully installed and started！", "congratulation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Installation failed {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 清理可能不完整的安装
                UninstallService();
            }
        }

        private void RunService(bool setAutoStart)
        {
            try
            {
                ExtractResources();
                if (setAutoStart)
                {
                    SetAutoStart(true);
                }
                StartService();
                MessageBox.Show("The service was successfully started！", "congratulation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Start failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void UninstallService()
        {
            try
            {
                StopService();
                SetAutoStart(false);

                // 等待一小段时间确保文件锁已释放
                Thread.Sleep(500);

                if (Directory.Exists(InstallDir))
                {
                    Directory.Delete(InstallDir, true);
                }
                MessageBox.Show("Service successfully uninstalled.", "success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uninstall failed: {ex.Message}", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartService()
        {
            if (Process.GetProcessesByName(ProcessName).Any())
            {
                MessageBox.Show("The service is already running.", "info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!File.Exists(_fullAppPath))
            {
                MessageBox.Show($"Application not found: {_fullAppPath}", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process.Start(_fullAppPath);
        }

        private void StopService()
        {
            var processes = Process.GetProcessesByName(ProcessName);
            if (!processes.Any())
            {
                return; // 已经停止了
            }

            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(); // 等待进程完全终止
                }
                catch (Exception ex)
                {
                    // 忽略错误，例如进程已经自己退出了
                    Debug.WriteLine($"停止进程时出错: {ex.Message}");
                }
            }
        }

        private void ExtractResources()
        {
            if (!Directory.Exists(InstallDir))
            {
                Directory.CreateDirectory(InstallDir);
            }

            // 获取当前程序集
            var assembly = Assembly.GetExecutingAssembly();
            // 注意：资源名称是 "命名空间.文件名"
            string appResourceName = $"{assembly.GetName().Name}.{AppName}";
            string toolResourceName = $"{assembly.GetName().Name}.{ToolName}";

            // 提取主程序
            using (Stream resourceStream = assembly.GetManifestResourceStream(appResourceName))
            {
                if (resourceStream == null) throw new Exception($"The embedded resource could not be found: {appResourceName}");
                using (FileStream fileStream = new FileStream(_fullAppPath, FileMode.Create))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }

            // 提取 ectool.exe
            using (Stream resourceStream = assembly.GetManifestResourceStream(toolResourceName))
            {
                if (resourceStream == null) throw new Exception($"The embedded resource could not be found: {toolResourceName}");
                using (FileStream fileStream = new FileStream(Path.Combine(InstallDir, ToolName), FileMode.Create))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }

        private void SetAutoStart(bool enable)
        {
            // 操作 HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (enable)
                {
                    // 添加引号以处理路径中的空格
                    key.SetValue(AutoStartValueName, $"\"{_fullAppPath}\"");
                }
                else
                {
                    if (key.GetValue(AutoStartValueName) != null)
                    {
                        key.DeleteValue(AutoStartValueName);
                    }
                }
            }
        }

        #endregion
    }
}