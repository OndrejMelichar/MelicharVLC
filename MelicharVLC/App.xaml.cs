using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MelicharVLC
{
    /// <summary>
    /// Interakční logika pro App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private bool _isExit;

        public DispatcherTimer timer;
        private string checkURI = "https://student.sps-prosek.cz/~melicon16/jsonDBzaloha.json"; //https://utils.ssl.cdn.cra.cz/live-streaming/clients/pspcr/player-new.php //https://student.sps-prosek.cz/~melicon16/jsonDBzaloha.json
        private string previousChecksum = "";
        private StreamStates streamState = StreamStates.initialization;
        private double timerFastInterval = 3;
        private double timerSlowInterval = 10;

        private string debug = "";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            _notifyIcon.Icon = MelicharVLC.Properties.Resources.MyIcon;
            _notifyIcon.Visible = true;

            CreateContextMenu();
            this.setTimer();
        }

        private void setTimer()
        {
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(3);
            this.timer.Tick += timerTick;
            this.timer.Start();
        }

        void timerTick(object sender, EventArgs e)
        {
            Task.Run(async () => {
                await this.timerTickAsync();
            }).ConfigureAwait(true);
        }

        private async Task timerTickAsync()
        {
            WebAPIActions webAPIActions = new WebAPIActions();
            string html = await webAPIActions.GET_HTML(this.checkURI);
            await Task.Run(() => nonAsyncTickBlock(html));
        }

        private void nonAsyncTickBlock(string html)
        {
            this.debug += "|";
            string actualChecksum = this.getStringChecksum(html);

            if (!this.previousChecksum.Equals(actualChecksum))
            {
                if (this.streamState == StreamStates.streamEnded)
                {
                    this.streamState = StreamStates.streamStarted;
                    this.timer.Interval = TimeSpan.FromSeconds(this.timerSlowInterval);

                    Application.Current.Dispatcher.Invoke(new Action(() => {
                        ShowMainWindow();
                    }));
                    
                    this.debug += "Z ";
                }
                else if (this.streamState == StreamStates.streamStarted)
                {
                    this.streamState = StreamStates.streamEnded;
                    this.timer.Interval = TimeSpan.FromSeconds(this.timerFastInterval);
                    this.debug += "S ";
                }

                if (this.streamState == StreamStates.initialization)
                {
                    this.streamState = StreamStates.streamEnded;
                    this.debug += "0 ";
                }

                this.previousChecksum = actualChecksum;
            }
        }

        private string getStringChecksum(string inputString)
        {
            string hash;
            using (System.Security.Cryptography.SHA1 crypt = System.Security.Cryptography.SHA1.Create())
            {
                hash = BitConverter.ToString(
                  crypt.ComputeHash(Encoding.UTF8.GetBytes(inputString))
                ).Replace("-", String.Empty);
            }

            return hash;
        }



        /* metody pro funkčnost TrayIcon */
        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Zobrazit okno přenosu").Click += (s, e) => ShowMainWindow();
            _notifyIcon.ContextMenuStrip.Items.Add("Ukončit").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            _isExit = true;
            MainWindow.Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                MainWindow.Hide(); // A hidden window can be shown again, a closed one not
            }
        }
    }

    enum StreamStates
    {
        initialization, streamStarted, streamEnded
    }
}
