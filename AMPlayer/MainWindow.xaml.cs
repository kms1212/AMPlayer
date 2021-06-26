using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Windows.Forms.Integration;
using Microsoft.Win32;
using System.Threading;
using MahApps.Metro.IconPacks;

namespace AMPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Process mediaPlayer;
        private Dictionary<String, String> playerProperty;

        private OpenFileDialog dialog;
        private DebugForm debugForm;

        private string args;

        private bool isVideoPlaying = false;
        private bool isVideoPlayerLoaded = false;
        private bool isSeeking = false;
        private bool isFullscreen = false;

        private event EventHandler VideoLoaded;

        private System.Windows.Forms.Label osdLabel;

        private WindowState windowState;

        public MainWindow()
        {
            InitializeComponent();

            osdLabel = new System.Windows.Forms.Label();
            osdLabel.Font = new System.Drawing.Font("", 20);
            osdLabel.ForeColor = System.Drawing.Color.White;
            osdLabel.BackColor = System.Drawing.Color.Black;
            osdLabel.AutoSize = true;
            osdLabel.Location = new System.Drawing.Point(30, 30);
            osdLabel.Visible = false;
            mpParent.Controls.Add(osdLabel);
            int osdLabelIndex = mpParent.Controls.IndexOf(osdLabel);
            mpParent.Controls[osdLabelIndex].BringToFront();

            mediaPlayer = new Process();
            mediaPlayer.StartInfo.FileName = settings.Default.Player;
            mediaPlayer.StartInfo.UseShellExecute = false;
            mediaPlayer.StartInfo.CreateNoWindow = true;
            mediaPlayer.StartInfo.RedirectStandardInput = true;
            mediaPlayer.StartInfo.RedirectStandardOutput = true;
            mediaPlayer.Exited += MediaPlayer_Exited;
            mediaPlayer.OutputDataReceived += MediaPlayer_OutputDataReceived;
            VideoLoaded = MediaOutput_VideoLoaded;

            dialog = new OpenFileDialog();

            debugForm = new DebugForm();
            debugForm.Show();

            lengthLabel.Content = "-:--:--";
            elapsedTimeLabel.Content = "-:--:--";

            playerProperty = new Dictionary<String, String>();
        }

        void StartPlayer()
        {
            if (isVideoPlayerLoaded)
                StopPlayer();

            args = ReplaceArgumentMacro(settings.Default.PlayerArgs);
            mediaPlayer.StartInfo.Arguments = args;
            mediaPlayer.Start();

            isVideoPlayerLoaded = true;
            isVideoPlaying = true;
            seekBar.IsEnabled = true;
            mainGrid.RowDefinitions[1].Height = new GridLength(60);

            mediaPlayer.BeginOutputReadLine();

            RefreshButtonImage();

            ShowMessageOSD("Play", 3);
        }

        private void MediaPlayer_Exited(object sender, EventArgs e)
        {
            playerProperty.Clear();
            mediaPlayer.CancelOutputRead();

            isVideoPlayerLoaded = false;
            isVideoPlaying = false;

            RefreshButtonImage();

            this.Dispatcher.Invoke(new Action(delegate ()
            {
                seekBar.Value = 0;
                seekBar.Maximum = 1;
                seekBar.IsEnabled = false;

                lengthLabel.Content = "-:--:--";
                elapsedTimeLabel.Content = "-:--:--";
            }));

            ShowMessageOSD("Stop", 3);
        }

        private void ShowMessageOSD(string text, int s_duration)
        {
            Thread t = new Thread(new ThreadStart(delegate
            {
                try
                {
                    this.Dispatcher.Invoke(new Action(async delegate ()
                    {
                        osdLabel.Visible = true;
                        osdLabel.Text = text;
                        for (int i = 0; i < s_duration * 2; i++)
                        {
                            await Task.Delay(500);
                            if (osdLabel.Text != text)
                                return;
                        }
                        if (osdLabel.Text == text)
                            osdLabel.Visible = false;
                    }));
                }
                catch (Exception) { }
            }));
            t.Start();
        }

        private void MediaOutput_VideoLoaded(object sender, EventArgs e)
        {
            int length = (int)double.Parse(playerProperty["ID_LENGTH"]);
            int lHour = length / 3600;
            int lMin = length % 3600 / 60;
            int lSec = length % 60;

            this.Dispatcher.Invoke(new Action(delegate ()
            {
                this.lengthLabel.Content = String.Format("{0:0}:{1:00}:{2:00}", lHour, lMin, lSec);
                this.seekBar.Maximum = length;
            }));
        }

        private void MediaPlayer_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            string strMessage = e.Data;

            if (isVideoPlaying)
            {
                try
                {
                    if (strMessage != "")
                    {
                        debugForm.Invoke(new Action(delegate
                        {
                            debugForm.frameInfo.Text = strMessage;
                        }));
                        if (strMessage.StartsWith("A:"))
                        {
                            int elapsedTime = int.Parse(strMessage.Split(':')[1].Split('.')[0]);
                            int tHour = elapsedTime / 3600;
                            int tMin = elapsedTime % 3600 / 60;
                            int tSec = elapsedTime % 60;

                            this.Dispatcher.Invoke(new Action(delegate ()
                            {
                                if (!isSeeking && seekBar.Maximum != 0)
                                {
                                    elapsedTimeLabel.Content = String.Format("{0:0}:{1:00}:{2:00}", tHour, tMin, tSec);
                                    seekBar.Value = elapsedTime;
                                }
                            }));
                        }
                        else if (strMessage.StartsWith("V:"))
                        {

                            int elapsedTime = int.Parse(strMessage.Split(':')[1].Split('.')[0]);
                            int tHour = elapsedTime / 3600;
                            int tMin = elapsedTime % 3600 / 60;
                            int tSec = elapsedTime % 60;

                            this.Dispatcher.Invoke(new Action(delegate ()
                            {
                                if (!isSeeking && seekBar.Maximum != 0)
                                {
                                    elapsedTimeLabel.Content = String.Format("{0:0}:{1:00}:{2:00}", tHour, tMin, tSec);
                                    seekBar.Value = elapsedTime;
                                }
                            }));
                        }
                        else
                        {
                            Console.WriteLine(strMessage);
                            
                            debugForm.Invoke(new Action(delegate
                            {
                                debugForm.debugLog.Text += "CORE > " + strMessage + "\n";
                            }));

                            if (strMessage.StartsWith("ID_") && strMessage.Contains('='))
                            {
                                string[] keyValue = strMessage.Split('=');

                                if (!playerProperty.ContainsKey(keyValue[0]))
                                    playerProperty.Add(keyValue[0], keyValue[1]);
                            }
                            else if (strMessage.StartsWith("Starting playback..."))
                            {
                                VideoLoaded(this, new EventArgs());
                            }
                            else if (strMessage.StartsWith("Exiting..."))
                            {
                                debugForm.Invoke(new Action(delegate
                                {
                                    StopPlayer();
                                }));
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        String ReplaceArgumentMacro(String orig)
        {
            StringBuilder res = new StringBuilder();
            res.Append(orig);
            res.Replace("$(OutputHandle)", mediaPlayerOutput.Handle.ToString());
            res.Replace("$(Path)", dialog.FileName);
            return res.ToString();
        }

        private void HideBarButton_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.RowDefinitions[1].Height = new GridLength(0);
        }

        private void MediaPlayerOutput_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                mainGrid.RowDefinitions[1].Height = new GridLength(60);
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (isVideoPlayerLoaded)
                {
                    VideoPauseResume();
                }
                else
                {
                    if (dialog.ShowDialog() == true)
                    {
                        StartPlayer();
                    }
                }
            }
        }

        bool SendCommand(string cmd)
        {
            try
            {
                if (mediaPlayer != null && mediaPlayer.HasExited == false)
                {
                    debugForm.Invoke(new Action(delegate
                    {
                        debugForm.debugLog.Text += "AMPlayer > " + cmd + "\n";
                    }));
                    mediaPlayer.StandardInput.Write(cmd + "\n");
                    return true;
                }
                else
                    return false;
            }
            catch { return false; }
        }

        public void RefreshButtonImage()
        {
            PackIconFontAwesome icon = new PackIconFontAwesome();
            icon.Width = 15;
            icon.Height = 15;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            icon.VerticalAlignment = VerticalAlignment.Center;

            if (isVideoPlaying)
            {
                icon.Kind = PackIconFontAwesomeKind.PauseSolid;
                playPauseButton.Content = icon;
            }
            else
            {
                icon.Kind = PackIconFontAwesomeKind.PlaySolid;
                playPauseButton.Content = icon;
            }
        }

        private void StopPlayer()
        {
            try
            {
                SendCommand("quit");
                if (!mediaPlayer.WaitForExit(1000))
                {
                    mediaPlayer.Kill();
                    MediaPlayer_Exited(this, new EventArgs());
                }
            }
            catch { }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            debugForm.Invoke(new Action(delegate
            {
                StopPlayer();
            }));
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopPlayer();
            debugForm.CloseForm();
        }

        private void VideoPauseResume()
        {
            isVideoPlaying = !isVideoPlaying;
            ShowMessageOSD(isVideoPlaying ? "Resume" : "Pause", 3);
            RefreshButtonImage();
            SendCommand("pause");
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isVideoPlayerLoaded)
            {
                VideoPauseResume();
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isVideoPlayerLoaded)
            {
                SendCommand("set_property volume " + ((int)volumeSlider.Value).ToString());
            }
        }

        private void SeekBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isSeeking = true;
                Seek((int)((float)e.GetPosition(seekBar).X / seekBar.ActualWidth * seekBar.Maximum));
            }
        }

        private void SeekBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                isSeeking = false;
        }

        private void SeekBar_MouseLeave(object sender, MouseEventArgs e)
        {
            isSeeking = false;
        }

        private void SeekBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isSeeking = true;
                Seek((int)((float)e.GetPosition(seekBar).X / seekBar.ActualWidth * seekBar.Maximum));
            }
        }

        private void Seek(int position)
        {
            if (isSeeking && isVideoPlayerLoaded && (position < seekBar.Maximum) && (position > seekBar.Minimum))
            {
                int tHour = position / 3600;
                int tMin = position % 3600 / 60;
                int tSec = position % 60;

                ShowMessageOSD(String.Format("{0:0}:{1:00}:{2:00}", tHour, tMin, tSec), 1);
                if (position < 0 || position > seekBar.Maximum)
                    return;
                seekBar.Value = seekBar.Maximum;
                seekBar.Value = position;

                SendCommand("set_property time_pos " + position.ToString());

                isVideoPlaying = true;
                if (!isVideoPlaying)
                    RefreshButtonImage();
            }
        }

        private void FullscreenButton_Click(object sender, RoutedEventArgs e)
        {
            PackIconMaterial icon = new PackIconMaterial();
            icon.Width = 12;
            icon.Height = 12;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            icon.VerticalAlignment = VerticalAlignment.Center;

            if (isFullscreen)
            {
                mainGrid.RowDefinitions[1].Height = new GridLength(60);

                WindowState = windowState;
                this.ResizeMode = ResizeMode.CanResizeWithGrip;
                this.IgnoreTaskbarOnMaximize = false;
                ShowTitleBar = true;

                this.UseNoneWindowStyle = false;
                this.WindowStyle = WindowStyle.SingleBorderWindow;

                isFullscreen = false;

                icon.Kind = PackIconMaterialKind.Fullscreen;
                fullscreenButton.Content = icon;
            }
            else
            {
                mainGrid.RowDefinitions[1].Height = new GridLength(0);

                windowState = this.WindowState;
                this.WindowState = WindowState.Maximized;
                this.ResizeMode = ResizeMode.NoResize;
                this.IgnoreTaskbarOnMaximize = true;
                ShowTitleBar = false;

                this.WindowStyle = WindowStyle.None;

                isFullscreen = true;

                icon.Kind = PackIconMaterialKind.FullscreenExit;
                fullscreenButton.Content = icon;
            }
        }

        private void openFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dialog.ShowDialog() == true)
            {
                StartPlayer();
            }
        }

        private void closeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
