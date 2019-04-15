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

namespace MelicharVLC
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DirectoryInfo vlcLibDirectory;
        private VlcControl control;

        public MainWindow()
        {
            InitializeComponent();

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            vlcLibDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
        }

        /* WPF EVENTY */
        private void OnPlayButtonClick(object sender, RoutedEventArgs e)
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

            control.SourceProvider.MediaPlayer.Play(new Uri("http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_surround-fix.avi")); //TODO: vybrené video k přehrání
        }

        private void OnPauseButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnStopButtonClick(object sender, RoutedEventArgs e)
        {
            this.control?.Dispose();
            this.control = null;
        }

        private void OnPlusButtonClick(object sender, RoutedEventArgs e)
        {
            this.addTime(30 * 1000);
        }

        private void OnMinusButtonClick(object sender, RoutedEventArgs e)
        {
            this.removeTime(5 * 1000);
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
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
