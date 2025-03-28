using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GotifyWindowsClient
{
    static class Program
    {
        private static NotifyIcon _trayIcon;
        private static ClientWebSocket _webSocket;
        private static bool _isConnected;
        private static readonly Mutex _mutex = new Mutex(true, "{8F6F0AC4-BC3B-4895-BC8F-5BBC8632465C}");

        [STAThread]
        static void Main()
        {
            if (!_mutex.WaitOne(TimeSpan.Zero, true)) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new Form { ShowInTaskbar = false, WindowState = FormWindowState.Minimized };
            InitializeTray();

            // ����ʱ��С��������
            mainForm.Load += async (s, e) =>
            {
                mainForm.Visible = false;
                await ConnectToGotify();
            };

            Application.Run(mainForm);
            _mutex.ReleaseMutex();
        }

        private static void InitializeTray()
        {
            // �ӵ�ǰ�����ļ���ȡͼ��
            var exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var appIcon = Icon.ExtractAssociatedIcon(exePath);
            _trayIcon = new NotifyIcon
            {
                Icon = appIcon,
                Visible = true,
                Text = "Gotify Client",
                ContextMenuStrip = new ContextMenuStrip()
            };

            // ��̬�����������˵��ı�
            UpdateAutoStartMenu();

            _trayIcon.ContextMenuStrip.Items.Add("�˳�", null, (s, e) =>
            {
                _trayIcon.Visible = false;
                Application.Exit();
            });
        }

        private static void UpdateAutoStartMenu()
        {
            const string separatorTag = "AutoStartSeparator";
            var currentText = IsAutoStartEnabled() ? "���ÿ�������" : "���ÿ�������";

            // ����ɲ˵���
            var itemsToRemove = new List<ToolStripItem>();
            foreach (ToolStripItem item in _trayIcon.ContextMenuStrip.Items)
            {
                if (item.Tag?.ToString() == "AutoStart" || item.Tag?.ToString() == separatorTag)
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (var item in itemsToRemove)
            {
                _trayIcon.ContextMenuStrip.Items.Remove(item);
            }

            // �����²˵���
            var autoStartItem = new ToolStripMenuItem(currentText)
            {
                Tag = "AutoStart"
            };
            autoStartItem.Click += ToggleAutoStart;

            // ��������ʶ�ķָ���
            var separator = new ToolStripSeparator { Tag = separatorTag };

            // ���뵽�˵������������˳���ť�ڵײ���
            _trayIcon.ContextMenuStrip.Items.Insert(0, autoStartItem);
            _trayIcon.ContextMenuStrip.Items.Insert(1, separator);
        }

        private static async Task ConnectToGotify()
        {
            var config = ConfigurationManager.AppSettings;
            var serverUrl = config["ServerUrl"] ?? "http://localhost:3000";
            var clientToken = config["ClientToken"];
            var AppIDs = config["AppID"].Split(",");
             
            var wsUrl = serverUrl
                .Replace("http://", "ws://")
                .Replace("https://", "wss://")
                + $"/stream?token={clientToken}";
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (true)
            {
                try
                {
                    using (var ws = new ClientWebSocket())
                    {
                        await ws.ConnectAsync(new Uri(wsUrl), CancellationToken.None);

                        var buffer = new byte[4096];
                        while (ws.State == WebSocketState.Open)
                        {
                            var receivedBuffer = new List<byte>();
                            WebSocketReceiveResult result;
                            do
                            {
                                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                                receivedBuffer.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                            } while (!result.EndOfMessage);

                            var jsonBytes = receivedBuffer.ToArray();
                            Debug.WriteLine($"�յ�ԭʼ����({jsonBytes.Length}�ֽ�)");

                            try
                            {
                                var dd = Encoding.UTF8.GetString(jsonBytes);
                                var message = JsonSerializer.Deserialize<GotifyMessage>(jsonBytes, options);
                                if(AppIDs.Contains(message.Appid.ToString()) )
                                {
                                    var title = string.IsNullOrEmpty(message.Title) ? "�ޱ���" : message.Title;
                                    var content = string.IsNullOrEmpty(message.Content) ? "������" : message.Content;
                                    ShowNotification(title, content);


                                }







                            }
                            catch (JsonException ex)
                            {
                                var rawJson = Encoding.UTF8.GetString(jsonBytes);
                                File.WriteAllText("error.json", rawJson);
                                Debug.WriteLine($"JSON����ʧ��: {ex.Message}\n{rawJson}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"���Ӵ���: {ex.Message}");
                    await Task.Delay(5000);
                }
            }
        }

        private static void ToggleAutoStart(object sender, EventArgs e)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            var appPath = $"\"{Application.ExecutablePath}\"";

            if (IsAutoStartEnabled())
            {
                key.DeleteValue("GotifyTray");
            }
            else
            {
                key.SetValue("GotifyTray", appPath);
            }

            UpdateAutoStartMenu();
        }

        private static bool IsAutoStartEnabled()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
            return key?.GetValue("GotifyTray") != null;
        }

        private static void ShowNotification(string title, string message)
        {
            _trayIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }
    }
}