using AMPlayer.resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace AMPlayer
{
    public partial class PlayerMain : Form
    {
        string args = "";
        string videoPath = null;
        Process mediaPlayer = null;
        public Dictionary<String, String> property;
        StringBuilder playerLog = null;
        DebugForm debugForm;
        XmlNode formPlayermainLocale;

        Image playButtonImagePlay = null;
        Image playButtonImagePause = null;

        bool isVideoPlaying = false;
        bool isPlayerLoaded = false;
        bool isSeeking = false;
        bool isControllerVisible = false;
        bool isFullScreen = false;
        bool isUpdatingVideoInfo = false;

        FormWindowState normalWindowState;

        event EventHandler onSeekBarValueChanged;
        event EventHandler videoLoaded;

        Label osdLabel;

        public PlayerMain()
        {
            InitializeComponent();

            XmlDocument localeFile = new XmlDocument();
            playerLog = new StringBuilder();
            osdLabel = new Label();
            debugForm = new DebugForm();
            property = new Dictionary<String, String>();
            mediaPlayer = new Process();

            if (File.Exists(@".\locale\" + player.Default.Locale + @".xml"))
                localeFile.Load(@".\locale\" + player.Default.Locale + @".xml");
            else
                MessageBox.Show("Could not find the locale file. Starting with embedded locale.", "Warning"
                    , MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            formPlayermainLocale = localeFile.SelectSingleNode("locale/playerForm");

            // Form Title
            this.Text = formPlayermainLocale.SelectSingleNode("formTitle").InnerText;

            // Tool Strip Menu Item Text
            this.fileToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("fileToolStripMenuItem").InnerText;
            this.playToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("playToolStripMenuItem").InnerText;
            this.videoToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("videoToolStripMenuItem").InnerText;
            this.audioToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("audioToolStripMenuItem").InnerText;
            this.subtitlesToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("subtitlesToolStripMenuItem").InnerText;
            this.informationToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("informationToolStripMenuItem").InnerText;
            this.viewToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("viewToolStripMenuItem").InnerText;
            this.optionsToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("optionsToolStripMenuItem").InnerText;
            this.helpToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("helpToolStripMenuItem").InnerText;

            // File Menu
            this.openFileToolStripMenuItem.Text = formPlayermainLocale.SelectSingleNode("openFileToolStripMenuItem").InnerText;

            // Control
            this.elapsedTimeLabel.Text = formPlayermainLocale.SelectSingleNode("timestampNotAvailable").InnerText;
            this.lengthLabel.Text = formPlayermainLocale.SelectSingleNode("timestampNotAvailable").InnerText;

            // Dialog
            this.videoFileDialog.Title = formPlayermainLocale.SelectSingleNode("videoFileDialogTitle").InnerText;
            this.videoFileDialog.Filter = formPlayermainLocale.SelectSingleNode("videoFileDialogFilter").InnerText;

            // Event
            videoLoaded += mediaOutput_VideoLoaded;
            osdLabel.Resize += osdLabel_Resize;

            // Load Player Background Image
            if (File.Exists(@".\theme\" + player.Default.Theme + @"\Select_128x128.png"))
            {
                Bitmap image = new Bitmap(256, 256);
                Graphics g = Graphics.FromImage(image);
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                g.DrawImage(Image.FromFile(@".\theme\" + player.Default.Theme + @"\Select_128x128.png"), 64, 64);
                g.DrawString(formPlayermainLocale.SelectSingleNode("playerBackgroundText").InnerText
                    , new Font(SystemFonts.CaptionFont.FontFamily, 12, FontStyle.Bold), Brushes.Gray, 128, 216, stringFormat);
                this.mediaOutput.BackgroundImage = image;
            }

            // Load Play Button Image
            if (File.Exists(@".\theme\" + player.Default.Theme + @"\PlayBtn_64x64.png"))
            {
                playButtonImagePlay = new Bitmap(Image.FromFile(@".\theme\" + player.Default.Theme + @"\PlayBtn_64x64.png"), this.playButton.Size);
                this.playButton.Image = playButtonImagePlay;
                this.playButton.Text = "";
            }

            // Load Pause Button Image
            if (File.Exists(@".\theme\" + player.Default.Theme + @"\PauseBtn_64x64.png"))
            {
                playButtonImagePause = new Bitmap(Image.FromFile(@".\theme\" + player.Default.Theme + @"\PauseBtn_64x64.png"), this.playButton.Size);
            }

            // Load Prev Button Image
            if (File.Exists(@".\theme\" + player.Default.Theme + @"\PrevBtn_64x64.png"))
            {
                this.prevButton.Image = new Bitmap(Image.FromFile(@".\theme\" + player.Default.Theme + @"\PrevBtn_64x64.png"), this.prevButton.Size);
                this.prevButton.Text = "";
            }

            // Load Next Button Image
            if (File.Exists(@".\theme\" + player.Default.Theme + @"\NextBtn_64x64.png"))
            {
                this.nextButton.Image = new Bitmap(Image.FromFile(@".\theme\" + player.Default.Theme + @"\NextBtn_64x64.png"), this.nextButton.Size);
                this.nextButton.Text = "";
            }

            // Load Stop Button Image
            if (File.Exists(@".\theme\" + player.Default.Theme + @"\StopBtn_64x64.png"))
            {
                this.stopButton.Image = new Bitmap(Image.FromFile(@".\theme\" + player.Default.Theme + @"\StopBtn_64x64.png"), this.stopButton.Size);
                this.stopButton.Text = "";
            }

            // Load Hide Button Image
            if (File.Exists(@".\theme\" + player.Default.Theme + @"\HideBtn_64x64.png"))
            {
                this.hideButton.Image = new Bitmap(Image.FromFile(@".\theme\" + player.Default.Theme + @"\HideBtn_64x64.png"), this.hideButton.Size);
                this.hideButton.Text = "";
            }

            // Load Full Screen Button Image
            if (File.Exists(@".\theme\" + player.Default.Theme + @"\FScreenBtn_64x64.png"))
            {
                this.fullScreenButton.Image = new Bitmap(Image.FromFile(@".\theme\" + player.Default.Theme + @"\FScreenBtn_64x64.png"), this.fullScreenButton.Size);
                this.fullScreenButton.Text = "";
            }

            debugForm.Show();

            onSeekBarValueChanged += seekBar_ValueChanged;

            mediaPlayer.StartInfo.FileName = player.Default.MediaPlayer;
            mediaPlayer.StartInfo.UseShellExecute = false;
            mediaPlayer.StartInfo.CreateNoWindow = true;
            mediaPlayer.StartInfo.RedirectStandardInput = true;
            mediaPlayer.StartInfo.RedirectStandardOutput = true;
            mediaPlayer.Exited += mediaPlayer_Exited;
            mediaPlayer.OutputDataReceived += getPlayerOutput;

            osdLabel.Font = new Font("Arial", 20);
            osdLabel.Text = "test";
            osdLabel.ForeColor = Color.White;
            osdLabel.BackColor = Color.Black;
            osdLabel.AutoSize = true;
            osdLabel.Location = new Point(30, 30);
            osdLabel.Visible = false;
            this.Controls.Add(osdLabel);
            int osdLabelIndex = this.Controls.IndexOf(osdLabel);
            this.Controls[osdLabelIndex].BringToFront();

            tableLayoutPanel1.RowStyles[1].Height = 0;

            if (!File.Exists(player.Default.MediaPlayer))
            {
                MessageBox.Show("Media player not found. Please reinstall the program to fix this problem.", "Error", MessageBoxButtons.OK);
            }
        }

        private void mediaPlayer_Exited(object sender, EventArgs e)
        {
            MessageBox.Show("ff");

            property.Clear();
            mediaPlayer.CancelOutputRead();

            setVideoStatus(false);
            isPlayerLoaded = false;
            isVideoPlaying = false;

            seekBar.Value = 0;
            seekBar.Maximum = 1;
            seekBar.Enabled = false;
            lengthLabel.Text = formPlayermainLocale.SelectSingleNode("timestampNotAvailable").InnerText;
            elapsedTimeLabel.Text = formPlayermainLocale.SelectSingleNode("timestampNotAvailable").InnerText;

            showMessageOSD("Stop", 3);
        }

        private void osdLabel_Resize(object sender, EventArgs e)
        {
            osdLabel.Font = new Font("Arial", this.Size.Height / 25);
        }

        void startPlayer()
        {
            args = replaceArgumentMacro(player.Default.Arguments);
            mediaPlayer.StartInfo.Arguments = args;
            mediaPlayer.Start();

            showMessageOSD("Play", 3);
            isPlayerLoaded = true;
            isVideoPlaying = true;
            isControllerVisible = true;
            seekBar.Enabled = true;
            tableLayoutPanel1.RowStyles[1].Height = 61;

            mediaPlayer.BeginOutputReadLine();
        }

        private void mediaOutput_VideoLoaded(object sender, EventArgs e)
        {
            setVideoStatus(true);

            int length = (int)(double.Parse(property["ID_LENGTH"]));
            int lHour = length / 3600;
            int lMin = length % 3600 / 60;
            int lSec = length % 60;


            Invoke(new MethodInvoker(delegate ()
            {
                this.lengthLabel.Text = String.Format("{0:0}:{1:00}:{2:00}", lHour, lMin, lSec);
                this.seekBar.Maximum = length;
            }));
        }

        async void stopPlayer()
        {
            try
            {
                await TaskEx.WaitUntil(() => !isUpdatingVideoInfo);

                sendCommand("quit");
                mediaPlayer.WaitForExit(1000);
                if (!mediaPlayer.HasExited)
                {
                    mediaPlayer.Kill();
                    mediaPlayer_Exited(this, new EventArgs());
                }
            }
            catch { }
        }

        public void setVideoStatus(bool videoPlaying)
        {
            if (isPlayerLoaded)
            {
                isVideoPlaying = videoPlaying;
                if (videoPlaying)
                {
                    this.playButton.Image = playButtonImagePause;
                }
                else
                {
                    this.playButton.Image = playButtonImagePlay;
                }
            }
        }

        void getVideoInformation()
        {

        }

        bool sendCommand(string cmd)
        {
            try
            {
                if (mediaPlayer != null && mediaPlayer.HasExited == false)
                {
                    debugForm.Invoke((MethodInvoker)delegate
                    {
                        debugForm.textBox1_append("FRONTEND > " + cmd + "\n");
                    });
                    mediaPlayer.StandardInput.Write(cmd + "\n");
                    return true;
                }
                else
                    return false;
            }
            catch { return false; }
        }

        String replaceArgumentMacro(String orig)
        {
            StringBuilder res = new StringBuilder();
            res.Append(orig);
            res.Replace("$(OutputHandle)", ((int)mediaOutput.Handle).ToString());
            res.Replace("$(Path)", videoPath);
            return res.ToString();
        }

        void getPlayerOutput(object sender, DataReceivedEventArgs e)
        {
            isUpdatingVideoInfo = true;
            if (e.Data == null)
                return;
            string strMessage = e.Data;

            if (isVideoPlaying)
            {
                try
                {
                    if (strMessage != "")
                    {
                        if (strMessage.StartsWith("A:"))
                        {
                            int elapsedTime = int.Parse(strMessage.Split(':')[2].Split('.')[0]);
                            int tHour = elapsedTime / 3600;
                            int tMin = elapsedTime % 3600 / 60;
                            int tSec = elapsedTime % 60;

                            Invoke(new MethodInvoker(delegate ()
                            {
                                if (!isSeeking && seekBar.Maximum != 0)
                                {
                                    elapsedTimeLabel.Text = String.Format("{0:0}:{1:00}:{2:00}", tHour, tMin, tSec);
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

                            Invoke(new MethodInvoker(delegate ()
                            {
                                if (!isSeeking && seekBar.Maximum != 0)
                                {
                                    elapsedTimeLabel.Text = String.Format("{0:0}:{1:00}:{2:00}", tHour, tMin, tSec);
                                    seekBar.Value = elapsedTime;
                                }
                            }));
                        }
                        else
                        {
                            playerLog.AppendLine(strMessage);
                            debugForm.Invoke((MethodInvoker)delegate
                            {
                                debugForm.textBox1_append("CORE > " + strMessage + "\n");
                            });

                            if (strMessage.StartsWith("ID_") && strMessage.Contains('='))
                            {
                                string[] keyValue = strMessage.Split('=');

                                if (!property.ContainsKey(keyValue[0]))
                                    property.Add(keyValue[0], keyValue[1]);
                            }
                            else if (strMessage.StartsWith("Starting playback..."))
                            {
                                videoLoaded(this, new EventArgs());
                            }
                            else if (strMessage.StartsWith("Exiting..."))
                            {
                                stopPlayer();
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
            isUpdatingVideoInfo = false;
        }

        private void videoPauseResume()
        {
            showMessageOSD(isVideoPlaying ? "Pause" : "Resume", 3);
            setVideoStatus(!isVideoPlaying);
            sendCommand("pause");
        }

        private void playermain_FormClosed(object sender, FormClosedEventArgs e)
        {
            stopPlayer();
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileWithDialog();
        }

        private async void mediaOutput_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            stopPlayer();
            await TaskEx.WaitUntil(() => mediaPlayer.HasExited);
            videoPath = videoFileDialog.FileName;
            startPlayer();
            //for (int i = 0; i < s.Length; i++)
            //MessageBox.Show(s[i]);
        }

        private void mediaOutput_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void volumeBar_ValueChanged(object sender, EventArgs e)
        {
            if (isPlayerLoaded)
            {
                sendCommand("set_property volume " + volumeBar.Value.ToString());
            }
        }

        private void seekBar_ValueChanged(object sender, EventArgs e)
        {
            if (isPlayerLoaded)
            {
                if (isSeeking)
                    sendCommand("set_property time_pos " + seekBar.Value.ToString());
            }
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (isPlayerLoaded)
            {
                videoPauseResume();
            }
        }

        private void prevButton_Click(object sender, EventArgs e)
        {

        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (isPlayerLoaded)
            {
                stopPlayer();
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {

        }

        private void seekBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isSeeking = true;
                seek((int)((float)((float)e.X / (float)seekBar.Width) * seekBar.Maximum));
            }
        }

        private void seekBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                isSeeking = false;
        }

        private void seekBar_MouseLeave(object sender, EventArgs e)
        {
            isSeeking = false;
        }

        private void seekBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                seek((int)((float)((float)e.X / (float)seekBar.Width) * seekBar.Maximum));
        }

        private void seek(int position)
        {
            if (isSeeking && isPlayerLoaded && (position < seekBar.Maximum) && (position > seekBar.Minimum))
            {
                int tHour = position / 3600;
                int tMin = position % 3600 / 60;
                int tSec = position % 60;

                showMessageOSD(String.Format("{0:0}:{1:00}:{2:00}", tHour, tMin, tSec), 1);
                if (position < 0 || position > seekBar.Maximum)
                    return;
                seekBar.Value = seekBar.Maximum;
                seekBar.Value = position;
                onSeekBarValueChanged(this, new EventArgs());

                if (!isVideoPlaying)
                    setVideoStatus(true);
            }
        }

        private void hideButton_Click(object sender, EventArgs e)
        {
            isControllerVisible = false;
            tableLayoutPanel1.RowStyles[1].Height = 0;
        }

        private void PlayerMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            mediaPlayer.Close();
        }

        private void showMessageOSD(string text, int s_duration)
        {
            Thread t = new Thread(new ThreadStart(delegate
            {
                try
                {
                    Invoke((MethodInvoker)async delegate ()
                    {
                        osdLabel.Text = text;
                        osdLabel.Visible = true;
                        for (int i = 0; i < s_duration * 2; i++)
                        {
                            await Task.Delay(500);
                            if (osdLabel.Text != text)
                                return;
                        }
                        if (osdLabel.Text == text)
                            osdLabel.Visible = false;
                    });
                }
                catch (Exception) { }
            }));
            t.Start();
        }

        private void PlayerMain_Load(object sender, EventArgs e)
        {

        }

        private void mediaOutput_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (isPlayerLoaded)
                {
                    videoPauseResume();
                }
                else
                {
                    if (videoFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        videoPath = videoFileDialog.FileName;
                        startPlayer();
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (!isControllerVisible)
                {
                    isControllerVisible = false;
                    tableLayoutPanel1.RowStyles[1].Height = 61;
                }
            }
        }

        private void fullScreenButton_Click(object sender, EventArgs e)
        {
            if (isFullScreen)
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = normalWindowState;

                menuStrip1.Show();
                isFullScreen = false;
            }
            else
            {
                normalWindowState = this.WindowState;

                isControllerVisible = false;
                tableLayoutPanel1.RowStyles[1].Height = 0;

                menuStrip1.Hide();

                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
                isFullScreen = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.O))
            {
                openFileWithDialog();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.X))
            {
                MessageBox.Show("What the Ctrl+F?");
                return true;
            }
            else if (keyData == Keys.Left)
            {
                if (isPlayerLoaded)
                {
                    isSeeking = true;
                    seek(seekBar.Value - 5);
                    isSeeking = false;
                }
                return true;
            }
            else if (keyData == Keys.Right)
            {
                if(isPlayerLoaded)
                {
                    isSeeking = true;
                    seek(seekBar.Value + 5);
                    isSeeking = false;
                    if (!isPlayerLoaded)
                        MessageBox.Show("Ff");
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        async void openFileWithDialog()
        {
            if (videoFileDialog.ShowDialog() == DialogResult.OK)
            {
                stopPlayer();
                await TaskEx.WaitUntil(() => mediaPlayer.HasExited);
                videoPath = videoFileDialog.FileName;
                startPlayer();
            }
        }
    }
}
