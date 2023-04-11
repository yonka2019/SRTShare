using MaterialSkin;
using SRTShareLib;
using SRTShareLib.PcapManager;
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
    // 8 Settings
    public partial class Settings : Form
    {
        private const int WAIT_BEFORE_CLOSE = 500;  // ms

        private static readonly AppSettings s = AppSettings.Default;
        private readonly Regex XX_Port_Regex;
        private readonly Regex IP_Regex;
        private bool hostname = false;

        public Settings()
        {
            InitializeComponent();

            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Indigo500, Primary.Indigo700, Primary.Indigo100, Accent.Blue200, TextShade.WHITE);

            EncryptionCBox.Items.AddRange(Enum.GetValues(typeof(EncryptionType)).Cast<object>().ToArray());  // load encryption options

            XX_Port_Regex = new Regex(@"^(.+):(\d{1,5})$");  // '[XX]:[PORT]' (GROUP 1, GROUP 2)
            IP_Regex = new Regex(@"^^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})$");  // '[XX.XX.XX.XX]' (IS GROUP 1)
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
            ServerIpPortTB.Text = $"{s.ServerIP}:{s.ServerPORT}";
            EncryptionCBox.SelectedItem = Enum.Parse(typeof(EncryptionType), s.Encryption);
            IntialPSNNum.Value = s.InitialPSN;
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
            Regex.Split(ServerIpPortTB.Text, @".+:");
            string[] ipPort = ServerIpPortTB.Text.Split(':');

            s.ServerIP = ipPort[0];
            s.ServerPORT = Convert.ToUInt16(ipPort[1]);

            s.Encryption = EncryptionCBox.SelectedItem.ToString();
            s.InitialPSN = (int)IntialPSNNum.Value;
            s.AutoQualityControl = autoQualityControlCB.Checked;
            s.AudioTransmission = audioTransCB.Checked;
            s.RetransmissionMode = retrModeCB.Checked;
            s.DataPercentLossRequired = (int)DataLossRequiredNum.Value;
            s.DecreaseQualityBy = (int)DecreaseQualityNum.Value;

            s.Save();
        }

        private async void SaveExitButton_Click(object sender, EventArgs e)
        {
            (string ip_OR_hostname, int port) = ExtractXX_Port(ServerIpPortTB.Text);
            if (ip_OR_hostname == null)  // can't extract server hostname/ip -> bad input
            {
                SetError("Must be IP/hostname:port");
                return;
            }
            else if (port == 0)
            {
                SetError("Bad port");
                return;
            }
            else  // 'ip/hostname:port' need to check if it's hostname
            {
                if (!IsIP(ip_OR_hostname))  // hostname [not IP, XX.XX.XX.XX]
                {

                    string hostnameIP = NetworkManager.DnsRequest(ip_OR_hostname);

                    if (hostnameIP == null)  // can't find IP, an issue could be occured because a wrong hostname, or some issues with the DNS request..
                    {
                        SetError("Bad hostname");
                        return;
                    }
                    else  // IP found
                    {
                        // If the given server ip is the client external ip, it means that the server is in the same subnet within the clients' subnet. And he should input the local one to avoid loop in the server

                        /* -- Full explanation --
                         * The loop can occur if the server tries to respond to the client by sending the response back to the client's external IP address, which is the same as the server IP address received in the request.

                            In this scenario, the response from the server will be sent to the default gateway (router) instead of being sent directly to the client. The router will then forward the response back to the client, but since the response has the same IP address as the original request, the server will again receive the response and send it back to the router. This process will repeat indefinitely, creating a loop.
                         */
                        if (hostnameIP == NetworkManager.PublicIp)
                        {
                            SetError("Specify the local IP of the server");
                            return;
                        }
                        else  // found IP address, and the ip is good (not the same as the public one)
                        {
                            ServerIpPortTB.Text = $"{hostnameIP}:{port}";
                            hostname = true;
                        }
                    }
                }
            }

            // finally, the textbox must be IP:Port in order to save the settings (even if it was hostname)
            SaveSettings();

            SaveExitButton.ForeColor = System.Drawing.Color.Green;
            SaveExitButton.Font = new System.Drawing.Font(SaveExitButton.Font.Name, SaveExitButton.Font.Size, System.Drawing.FontStyle.Bold);
            SaveExitButton.Text = "Saved";

            if (hostname)
                await Task.Delay(WAIT_BEFORE_CLOSE);
            else
                await Task.Delay(WAIT_BEFORE_CLOSE / 5);

            Close();
        }

        /// <summary>
        /// Extracts server hostname/ip & port from text which should be 'hostname/ip:port' if text wrong -> returns (null, 0)
        /// </summary>
        /// <param name="text">text from ip_port text box</param>
        /// <returns>('hostname/ip', port) OR (null, 0) (if wrong)</returns>
        private (string, int) ExtractXX_Port(string text)
        {
            Match XX_Port_Match = XX_Port_Regex.Match(text);

            if (XX_Port_Match.Success)
            {
                //              HOSTNAME / IP                                   PORT
                int extractedPort = Convert.ToInt32(XX_Port_Match.Groups[2].Value);

                if (extractedPort > 0 && extractedPort < 65536 && XX_Port_Match.Groups[2].Value[0] != '0')
                    return (XX_Port_Match.Groups[1].Value, extractedPort);
                else
                    return (XX_Port_Match.Groups[1].Value, 0);
            }
            else
                return (null, 0);
        }

        private bool IsIP(string text)
        {
            Match IP_Match = IP_Regex.Match(text);
            return IP_Match.Success;
        }

        private void ServerIpPortTB_TextChanged(object sender, EventArgs e)
        {
            if (ServerIpPortTB.GetErrorState())
            {
                ServerIpPortTB.SetErrorState(false);
                ServerIpPortTB.ErrorMessage = "null";
            }
        }

        private void SetError(string message)
        {
            ServerIpPortTB.SetErrorState(true);
            ServerIpPortTB.ErrorMessage = message;
        }
    }
}
