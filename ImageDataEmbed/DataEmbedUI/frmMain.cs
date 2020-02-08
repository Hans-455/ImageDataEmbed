using ImageDataEmbed.Code.Image;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataEmbedUI
{
    public partial class frmMain : Form
    {
        private PngDataEmbed embedObj;
        public frmMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (opnImage.ShowDialog() == DialogResult.OK)
            {
                embedObj = new PngDataEmbed(opnImage.FileName);
                lblStorageSize.Text = $"Can Store : {embedObj.BitStorage.ToString()} Bits";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (opnFile.ShowDialog() == DialogResult.OK)
            {
                byte[] data = File.ReadAllBytes(opnFile.FileName);
                embedObj.EmbedData(data);

                if (savImage.ShowDialog() == DialogResult.OK)
                {
                    embedObj.SaveImage(savImage.FileName);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            byte[] data = embedObj.ExtractData();

            if (savImage.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(savImage.FileName, data);
            }

        }
    }
}
