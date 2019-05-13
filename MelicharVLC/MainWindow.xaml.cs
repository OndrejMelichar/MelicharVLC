using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Vlc.DotNet.Wpf;
using System.Threading;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;

namespace MelicharVLC
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private string previousChecksum = "";
        private DirectoryInfo vlcLibDirectory;
        private VlcControl control;
        private bool isPaused = false;

        //private string URI = "https://pspcr-live.ssl.cdn.cra.cz/live.pscr/smil:snemovna2/index.m3u8";
        private string URI = @"D:\Ondřej Melichar\Moje data\škola\3. ročník\VAH\videa\Miloš Zeman - TV spot od Filipa Renče plná verze.mp4";

        public MainWindow()
        {
            InitializeComponent();

            this.setTimer();
            this.setPlayer();
        }

        private void setTimer()
        {
            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(60);
            this.timer.Tick += timerTick;
            this.timer.Start();
        }

        private void setPlayer()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            this.vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            this.control?.Dispose();
            this.control = new VlcControl();
            this.ControlContainer.Content = this.control;
            this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
            this.control.SourceProvider.MediaPlayer.Play(new Uri(this.URI));
            this.control.SourceProvider.MediaPlayer.EndReached += (s, e) => this.videoEnded();
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
            string html = await webAPIActions.GET_HTML("https://utils.ssl.cdn.cra.cz/live-streaming/clients/pspcr/player-new.php");
            await Task.Run(() => nonAsyncTickBlock(html));
        }

        private void nonAsyncTickBlock(string html)
        {
            string actualChecksum = this.getStringChecksum(html);

            if (!this.previousChecksum.Equals(actualChecksum))
            {
                // přenos (stream videa) začal
                //this.timer.Stop();
            }

            this.previousChecksum = actualChecksum;
        }

        private string getStringChecksum(string inputString)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(inputString))
                ).Replace("-", String.Empty);
            }

            return hash;
        }


        /* WPF EVENTY */

        private void playPauseButtonClick(object sender, RoutedEventArgs e)
        {
            if (playPauseButton.Content.ToString() == "Přehrát")
            {
                playPauseButton.Content = "Pozastavit";
                this.OnPlayButtonClick();
            }
            else if (playPauseButton.Content.ToString() == "Pozastavit")
            {
                playPauseButton.Content = "Přehrát";
                this.OnPauseButtonClick();
            }
        }

        private void OnPlayButtonClick()
        {
            if (this.isPaused)
            {
                this.isPaused = false;
                this.control.SourceProvider.MediaPlayer.Play();
            }
            else
            {
                this.control?.Dispose();
                this.control = new VlcControl();
                this.ControlContainer.Content = this.control;
                this.control.SourceProvider.CreatePlayer(this.vlcLibDirectory);
                this.control.SourceProvider.MediaPlayer.Play(new Uri(this.URI));
                this.control.SourceProvider.MediaPlayer.EndReached += (s, e) => this.videoEnded();
            }

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            //cmd.StandardInput.WriteLine("cd " + System.IO.Directory.GetCurrentDirectory() + @"\MelicharVLC.exe");
            cmd.StandardInput.WriteLine("--waveout-float32"); //--waveout-float32 //echo Oscar
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            volumeLabel.Content = cmd.StandardOutput.ReadToEnd();
        }

        private void OnPauseButtonClick()
        {
            this.isPaused = true;
            this.control.SourceProvider.MediaPlayer.Pause();
        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            this.control?.Dispose();
            this.control = null;
            playPauseButton.Content = "Přehrát";
            this.isPaused = false;
        }

        private void OnPlusButtonClick(object sender, RoutedEventArgs e)
        {
            this.addTime(30 * 1000);
        }

        private void OnMinusButtonClick(object sender, RoutedEventArgs e)
        {
            this.removeTime(5 * 1000);
        }

        private void videoEnded()
        {
            
        }


        /* POMOCNÉ METODY */
        private long getTotalTime()
        {
            if (this.control == null)
            {
                return 0;
            }

            return this.control.SourceProvider.MediaPlayer.Length; //v milisekundách
        }

        private long getActualTime()
        {
            if (this.control == null)
            {
                return 0;
            }

            return this.control.SourceProvider.MediaPlayer.Time; //v milisekundách
        }

        private void setActualTime(long newTime)
        {
            if (this.control != null)
            {
                long totalTime = this.getTotalTime();
                long actualTime = this.getActualTime();

                if (newTime < 0)
                {
                    newTime = 0;
                }

                if (newTime > totalTime)
                {
                    newTime = totalTime;
                }

                this.control.SourceProvider.MediaPlayer.Time = newTime;
            }
        }

        private void addTime(long addTime)
        {
            if (this.control != null)
            {
                long actualTime = this.getActualTime();
                this.setActualTime(actualTime + addTime);
            }
        }

        private void removeTime(long removeTime)
        {
            if (this.control != null)
            {
                long actualTime = this.getActualTime();
                this.setActualTime(actualTime - removeTime);
            }
        }


        /* OSTATNÍ */

        protected override void OnClosing(CancelEventArgs e)
        {
            this.control?.Dispose();
            base.OnClosing(e);
        }

        
    }
}
