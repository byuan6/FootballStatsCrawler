using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;


namespace ViewPng
{
    public partial class ViewPNG : Form
    {
        public ViewPNG()
        {
            InitializeComponent();
        }

        public string IniFilename { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.IniFilename))
                if (File.Exists(this.IniFilename))
                {
                    pictureBox1.Image = Image.FromFile(this.IniFilename);
                }
        }
    }
}
