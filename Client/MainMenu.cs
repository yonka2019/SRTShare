using System;
using System.Windows.Forms;

namespace Client
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {

            LiveStream liveStream = new LiveStream(Properties.Settings.Default.ServerIP,
                Properties.Settings.Default.ServerPORT,
                (SRTShareLib.SRTManager.Encryption.EncryptionType)Enum.Parse(typeof(SRTShareLib.SRTManager.Encryption.EncryptionType), Properties.Settings.Default.Encryption),
                Properties.Settings.Default.InitialPSN,
                Properties.Settings.Default.DataPercentLossRequired,
                Properties.Settings.Default.DecreaseQualityBy,
                Properties.Settings.Default.AutoQualityControl,
                Properties.Settings.Default.AudioTransmission,
                Properties.Settings.Default.RetransmissionMode);

            Hide();
            liveStream.ShowDialog();
            Close();
        }
    }
}
