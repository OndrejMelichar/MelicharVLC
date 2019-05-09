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

namespace MelicharVLC
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DirectoryInfo vlcLibDirectory;
        private VlcControl control;

        private bool isPaused = false;
        private string URI = "https://pspcr-live.ssl.cdn.cra.cz/live.pscr/smil:snemovna2/index.m3u8";

        public MainWindow()
        {
            InitializeComponent();
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
        }

        /* dodáno */

        /* /dodáno */


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

                this.control.SourceProvider.MediaPlayer.Log += (_, args) =>
                {
                    string message = $"libVlc : {args.Level} {args.Message} @ {args.Module}";
                    System.Diagnostics.Debug.WriteLine(message);
                };
                
                this.control.SourceProvider.MediaPlayer.Play(new Uri(this.URI));
                this.control.SourceProvider.MediaPlayer.EndReached += (s, e) => this.videoEnded();
            }
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
