using MaterialSkin;
using SRTShareLib.SRTManager.Encryption;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using AppSettings = Client.Properties.Settings;

namespace Client
{
    public partial class Settings : Form
    {
        // 8 Settings
        private static readonly AppSettings s = AppSettings.Default;
        private readonly Regex X_Port_Regex;
        private readonly Regex IP_Regex;

        public Settings()
        {
            InitializeComponent();

            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Indigo500, Primary.Indigo700, Primary.Indigo100, Accent.Blue200, TextShade.WHITE);

            EncryptionCBox.Items.AddRange(Enum.GetValues(typeof(EncryptionType)).Cast<object>().ToArray());  // load encryption options

            IP_Port_Regex = new Regex(@"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d{1,5})$");
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            RestoreSettings();
        }

        /// <summary>
        /// Restore settings from application config
        /// </summary>
        private void RestoreSettings()
        {
            ServerIpPortTB.Text = s.ServerIP_PORT;
            EncryptionCBox.SelectedItem = Enum.Parse(typeof(EncryptionType), s.Encryption);
            IntialPSNNum.Value = s.IntialPSN;
            autoQualityControlCB.Checked = s.AutoQualityControl;
            audioTransCB.Checked = s.AudioTransmission;
            retrModeCB.Checked = s.RetransmissionMode;
            DataLossRequiredNum.Value = s.DataPercentLossRequired;
            DecreaseQualityNum.Value = s.DecreaseQualityBy;
        }

        /// <summary>
        /// Save settings to application config
        /// </summary>
        private void SaveSettings()
        {
            s.ServerIP_PORT = ServerIpPortTB.Text;
            s.Encryption = EncryptionCBox.SelectedItem.ToString();
            s.IntialPSN = (int)IntialPSNNum.Value;
            s.AutoQualityControl = autoQualityControlCB.Checked;
            s.AudioTransmission = audioTransCB.Checked;
            s.RetransmissionMode = retrModeCB.Checked;
            s.DataPercentLossRequired = (int)DataLossRequiredNum.Value;
            s.DecreaseQualityBy = (int)DecreaseQualityNum.Value;
        }

        private async void SaveExitButton_Click(object sender, EventArgs e)
        {
            (string ip, ushort port) = ExtractServerIP_Port(ServerIpPortTB.Text);
            if (ip == null)  // can't extract server ip -> maybe it's a hostname?
            {
                return;
            }

            SaveSettings();

            SaveExitButton.Text = "Saved";
            await Task.Delay(50);
            Close();
        }

        /// <summary>
        /// Extracts server ip & port from text which should be 'ip:port' if text wrong -> returns (null, 0)
        /// </summary>
        /// <param name="text">text from ip_port text box</param>
        /// <returns>('ip', port) OR (null, 0) (if wrong)</returns>
        private (string, ushort) ExtractServerIP_Port(string text)
        {
            Match regexMatch = IP_Port_Regex.Match(text);

            if (regexMatch.Success)
            {
                return (regexMatch.Groups[1].Value, Convert.ToUInt16(regexMatch.Groups[2].Value));
            }
            else
                return (null, 0);
        }

        private void ServerIpPortTB_TextChanged(object sender, EventArgs e)
        {
            if (ServerIpPortTB.GetErrorState())
            {
                ServerIpPortTB.SetErrorState(false);
                ServerIpPortTB.ErrorMessage = "null";
            }
        }
    }
}
