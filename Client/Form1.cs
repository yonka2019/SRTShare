using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Client
{
    public partial class Form1 : Form
    {
        Task screen = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e) //Connect
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            var client = new TcpClient();

            try
            {
                client.Connect(IPAddress.Parse(textBox1.Text), 40001);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Invalid IP address", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (client.Connected)
            {
                var stream = client.GetStream();
                var formatter = new BinaryFormatter();
                button1.Visible = false;


                screen = Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var image = ConvertToBitmap(formatter.Deserialize(stream) as Stream);
                            pictureBox1.Image = image;
                        }

                        catch (Exception ex)
                        {
                            pictureBox1.Image = null;
                            MessageBox.Show("Server ended the stream", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        }
                    }
                });

            }

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        public Bitmap ConvertToBitmap(Stream stream)
        {
            Bitmap bitmap;

            Image image = Image.FromStream(stream);
            bitmap = new Bitmap(image);

            return bitmap;
        }
    }
}
