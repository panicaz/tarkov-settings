using System;
using System.Net.Http;
using System.Windows.Forms;
using tarkov_settings.Setting;
using tarkov_settings.GPU;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace tarkov_settings
{

    public partial class MainForm : Form
    {
        private ProcessMonitor pMonitor = ProcessMonitor.Instance;
        private IGPU gpu = GPUDevice.Instance;
        private AppSetting appSetting;

        private bool minimizeOnStart = false;


 
        // DLL libraries used to manage hotkeys
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int AllMapHotkey = 1;
        const int InterchangeMapHotkey = 2;
        const int DefaultHotkey = 3;
        const int GammaUp = 4;
        const int GammaDown = 5;
        const int ForceApply = 6;



        public MainForm()
        {
            InitializeComponent();

            RegisterHotKey(this.Handle, AllMapHotkey, 1, (int)Keys.NumPad1);
            RegisterHotKey(this.Handle, InterchangeMapHotkey, 1, (int)Keys.NumPad2);
            RegisterHotKey(this.Handle, DefaultHotkey, 1, (int)Keys.NumPad3);
            RegisterHotKey(this.Handle, GammaUp, 1, (int)Keys.Up);
            RegisterHotKey(this.Handle, GammaDown, 1, (int)Keys.Down);
            RegisterHotKey(this.Handle, ForceApply, 1, (int)Keys.NumPad0);

            #region Load App Settings
            // Load Settings
            appSetting = AppSetting.Load();

            Brightness = appSetting.brightness;
            Contrast = appSetting.contrast;
            Gamma = appSetting.gamma;
            DVL = appSetting.saturation;
            minimizeOnStart = appSetting.minimizeOnStart;
            this.minimizeStartCheckBox.Checked = minimizeOnStart;
            Prof1Brigtness = appSetting.prof1Brightness;
            Prof1Contrast = appSetting.prof1Contrast;
            Prof1Gamma = appSetting.prof1Gamma;
            Prof2Brigtness = appSetting.prof2Brightness;
            Prof2Contrast = appSetting.prof2Contrast;
            Prof2Gamma = appSetting.prof2Gamma;

            #endregion

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = String.Format("Tarkov Settings {0}", version);
            _ = new UpdateNotifier(version);

            // Saturation Initialize
            if (gpu.Vendor != GPUVendor.NVIDIA)
                DVLGroupBox.Enabled = false;

            #region Initialize Display
            // Initialize Display Dropdown
            foreach (string display in Display.displays)
            {
                DisplayCombo.Items.Add(display);
            }
            
            if(DisplayCombo.FindString(appSetting.display) != -1)
                DisplayCombo.SelectedIndex = DisplayCombo.FindString(appSetting.display);

            Display.Primary = (string)DisplayCombo.SelectedItem;
            #endregion

            // Initialize Process Monitor
            pMonitor.Parent = this;
            foreach (string pTarget in appSetting.pTargets)
            {
                pMonitor.Add(pTarget.ToLower());
            }
            pMonitor.Init();
        }

        #region BCGS Getter/Setter
        public double Brightness
        {
            get => BrightnessBar.Value / 100.0;
            set => BrightnessBar.Value = (int)(value * 100);
        }

        public double Contrast
        {
            get => ContrastBar.Value / 100.0;
            set => ContrastBar.Value = (int)(value * 100);
        }

        public double Gamma
        {
            get => GammaBar.Value / 100.0;
            set => GammaBar.Value = (int)(value * 100);
        }

        public int DVL
        {
            get => DVLBar.Value;
            set => DVLBar.Value = value;
        }

        public int Prof1Brigtness
        {
            get => int.Parse(profile1BrightnessText.Text);
            set => profile1BrightnessText.Text = value.ToString();
        }

        public int Prof1Contrast
        {
            get => int.Parse(profile1ContrastText.Text);
            set => profile1ContrastText.Text = value.ToString();
        }

        public int Prof1Gamma
        {
            get => int.Parse(profile1GammaText.Text);
            set => profile1GammaText.Text = value.ToString();
        }

        public int Prof2Brigtness
        {
            get => int.Parse(profile2BrightnessText.Text);
            set => profile2BrightnessText.Text = value.ToString();
        }

        public int Prof2Contrast
        {
            get => int.Parse(profile2ContrastText.Text);
            set => profile2ContrastText.Text = value.ToString();
        }

        public int Prof2Gamma
        {
            get => int.Parse(profile2GammaText.Text);
            set => profile2GammaText.Text = value.ToString();
        }

        public (double, double, double, int) GetColorValue()
        {
            return (
                BrightnessBar.Value / 100.0,
                ContrastBar.Value / 100.0,
                GammaBar.Value / 100.0,
                DVLBar.Value
                );
        }
        #endregion

        public bool IsEnabled { get=> this.enableToolStripMenuItem.Checked;}

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (minimizeOnStart)
            {
                this.Visible = false;
                this.ShowInTaskbar = false;
                this.trayIcon.ShowBalloonTip(
                    2500,
                    "Tarkov Settings Initailized!",
                    "Check out tray to modify your color setting",
                    ToolTipIcon.Info
                    );
            }
        }

        #region Control Event Handlers
        private void ColorLabel_DClick(object sender, EventArgs e)
        {
            var label = sender as Label;
            
            if (label.Equals(BrightnessLabel))
            {
                BrightnessBar.Value = 50;
            }
            else if (label.Equals(ContrastLabel))
            {
                ContrastBar.Value = 50;
            }
            else if (label.Equals(GammaLabel))
            {
                GammaBar.Value = 100;
            }
            else if (label.Equals(DVLLabel))
            {
                DVLBar.Value = 0;
            }
        }
        private readonly ColorController cController = ColorController.Instance;
        public void Init()
        {
            // Init ColorController
            cController.Init();
        }

        private void AllMapButtonClick(object sender, EventArgs e)
        {
            try
            {
                int profile1Brightness = int.Parse(profile1BrightnessText.Text);
                int profile1Contrast = int.Parse(profile1ContrastText.Text);
                int profile1Gamma = int.Parse(profile1GammaText.Text);
                BrightnessBar.Value = profile1Brightness;
                ContrastBar.Value = profile1Contrast;
                GammaBar.Value = profile1Gamma;
                DVLBar.Value = 10;
            }
            catch
            {
                MessageBox.Show("Invalid number entered or adjustment range exceeded");
            }
        }

        private void InterchangeMapButtonClick(object sender, EventArgs e)
        {
            try
            {
                int profile2Brightness = int.Parse(profile2BrightnessText.Text);
                int profile2Contrast = int.Parse(profile2ContrastText.Text);
                int profile2Gamma = int.Parse(profile2GammaText.Text);
                BrightnessBar.Value = profile2Brightness;
                ContrastBar.Value = profile2Contrast;
                GammaBar.Value = profile2Gamma;
                DVLBar.Value = 10;
            }
            catch
            {
                MessageBox.Show("Invalid number entered or adjustment range exceeded");
            }
        }

        private void DefaultValuesButtonClick(object sender, EventArgs e)
        {
            BrightnessBar.Value = 50;
            ContrastBar.Value = 50;
            GammaBar.Value = 100;
            DVLBar.Value = 0;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == AllMapHotkey)
            {
                try
                {
                    int profile1Brightness = int.Parse(profile1BrightnessText.Text);
                    int profile1Contrast = int.Parse(profile1ContrastText.Text);
                    int profile1Gamma = int.Parse(profile1GammaText.Text);
                    BrightnessBar.Value = profile1Brightness;
                    ContrastBar.Value = profile1Contrast;
                    GammaBar.Value = profile1Gamma;
                    DVLBar.Value = 10;

                    cController.ChangeColorRamp(brightness: BrightnessBar.Value / 100.0,
                                contrast: ContrastBar.Value / 100.0,
                                gamma: GammaBar.Value / 100.0,
                                reset: false);
                    cController.DVL = 10;                
                }
                catch
                {
                    MessageBox.Show("Invalid number or adjustment range exceeded");
                }
            }


            if (m.Msg == 0x0312 && m.WParam.ToInt32() == InterchangeMapHotkey)
            {
                try
                {
                    int profile2Brightness = int.Parse(profile2BrightnessText.Text);
                    int profile2Contrast = int.Parse(profile2ContrastText.Text);
                    int profile2Gamma = int.Parse(profile2GammaText.Text);
                    BrightnessBar.Value = profile2Brightness;
                    ContrastBar.Value = profile2Contrast;
                    GammaBar.Value = profile2Gamma;
                    DVLBar.Value = 10;

                    cController.ChangeColorRamp(brightness: BrightnessBar.Value / 100.0,
                                contrast: ContrastBar.Value / 100.0,
                                gamma: GammaBar.Value / 100.0,
                                reset: false);
                    cController.DVL = 10;
                }
                catch
                {
                    MessageBox.Show("Invalid number or adjustment range exceeded");
                }
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == DefaultHotkey)
            {
                BrightnessBar.Value = 50;
                ContrastBar.Value = 50;
                GammaBar.Value = 100;
                DVLBar.Value = 0;

                cController.ChangeColorRamp(brightness: 0.5,
                                            contrast: 0.50,
                                            gamma: 1.00,
                                            reset: false);
                cController.DVL = 10;
            }

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == GammaUp)
            {
                if (GammaBar.Value < 280)
                {
                    GammaBar.Value = GammaBar.Value + 10;
                    cController.ChangeColorRamp(brightness: BrightnessBar.Value / 100.0,
                                                contrast: ContrastBar.Value / 100.0,
                                                gamma: GammaBar.Value / 100.0,
                                                reset: false);
                }
            }

                if (m.Msg == 0x0312 && m.WParam.ToInt32() == GammaDown)
            {
                if (GammaBar.Value > 40)
                {
                    GammaBar.Value = GammaBar.Value - 10;
                    cController.ChangeColorRamp(brightness: BrightnessBar.Value / 100.0,
                                                contrast: ContrastBar.Value / 100.0,
                                                gamma: GammaBar.Value / 100.0,
                                                reset: false);
                }

            }

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == ForceApply)
            {
                cController.ChangeColorRamp(brightness: BrightnessBar.Value / 100.0,
                                            contrast: ContrastBar.Value / 100.0,
                                            gamma: GammaBar.Value / 100.0,
                                            reset: false);
                cController.DVL = DVLBar.Value;
            }

            base.WndProc(ref m);
        }

        private void TrackBar_ValueChanged(object sender, EventArgs e)
        {
            var trackBar = sender as System.Windows.Forms.TrackBar;

            if (trackBar.Equals(BrightnessBar))
            {
                BrightnessText.Text = (BrightnessBar.Value / 100.0).ToString("0.00");
            }
            else if (trackBar.Equals(ContrastBar))
            {
                ContrastText.Text = (ContrastBar.Value / 100.0).ToString("0.00");
            }
            else if (trackBar.Equals(GammaBar))
            {
                GammaText.Text = (GammaBar.Value / 100.0).ToString("0.00");
            }
            else if (trackBar.Equals(DVLBar))
            {
                DVLText.Text = DVLBar.Value.ToString();
            }
        }
        private void DisplayCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            string selectedDisplay = (string)DisplayCombo.SelectedItem;
            Display.Primary = selectedDisplay;

            if(Display.Primary != selectedDisplay)
            {
                DisplayCombo.SelectedIndex = DisplayCombo.FindString(Display.Primary);
            }
        }
        #endregion

        private void ShowForm(object sender, EventArgs e)
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
        }

        private void ExitFormClicked(object sender, EventArgs e)
        {
            appSetting.brightness = Brightness;
            appSetting.contrast = Contrast;
            appSetting.gamma = Gamma;
            appSetting.saturation = DVL;
            appSetting.display = (string)DisplayCombo.SelectedItem;
            appSetting.minimizeOnStart = minimizeOnStart;
            appSetting.prof1Brightness = Prof1Brigtness;
            appSetting.prof1Contrast = Prof1Contrast;
            appSetting.prof1Gamma = Prof1Gamma;
            appSetting.prof2Brightness = Prof2Brigtness;
            appSetting.prof2Contrast = Prof2Contrast;
            appSetting.prof2Gamma = Prof2Gamma;
            appSetting.Save();

            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                Console.WriteLine(e.CloseReason);
                this.trayIcon.Dispose();
                Console.WriteLine("[mainForm] Closing pMonitor");
                pMonitor.Close();
            }
        }

        private void CheckOnMinimizeToTray(object sender, EventArgs e)
        {
            this.minimizeOnStart = this.minimizeStartCheckBox.Checked;
        }

        private void forceApplyButton_Click(object sender, EventArgs e)
        {
            cController.ChangeColorRamp(brightness: BrightnessBar.Value / 100.0,
                            contrast: ContrastBar.Value / 100.0,
                            gamma: GammaBar.Value / 100.0,
                            reset: false);
            cController.DVL = DVLBar.Value;
        }
    }
}
