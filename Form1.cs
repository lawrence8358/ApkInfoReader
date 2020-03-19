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

namespace ApkInfoReader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string apkPath = Properties.Settings.Default.ApkPath;

            if (!string.IsNullOrEmpty(apkPath) && File.Exists(apkPath))  
            {
                textBox_Path.Text = apkPath;
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(apkPath);
                textBox_Info.Text = ReadInfo(apkPath);
            }
            else
            {
                openFileDialog1.InitialDirectory = Application.StartupPath;
            } 
        }

        private void button_Select_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "apk files (*.apk)|*.apk";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(fileName);
                textBox_Path.Text = fileName;
                Properties.Settings.Default.ApkPath = fileName;
                Properties.Settings.Default.Save();

                textBox_Info.Text = ReadInfo(fileName); 
            }
        }
         
        private string ReadInfo(string apkPath)
        {
            if (string.IsNullOrEmpty(apkPath) || !File.Exists(apkPath)) return string.Empty;

            ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(apkPath));
            var filestream = new FileStream(apkPath, FileMode.Open, FileAccess.Read);
            ICSharpCode.SharpZipLib.Zip.ZipFile zipfile = new ICSharpCode.SharpZipLib.Zip.ZipFile(filestream);
            ICSharpCode.SharpZipLib.Zip.ZipEntry item;

            string content = string.Empty;

            while ((item = zip.GetNextEntry()) != null)
            {
                if (item.Name == "AndroidManifest.xml")
                {
                    byte[] bytes = new byte[50 * 1024];

                    Stream strm = zipfile.GetInputStream(item);
                    int size = strm.Read(bytes, 0, bytes.Length);

                    using (BinaryReader s = new BinaryReader(strm))
                    {
                        byte[] bytes2 = new byte[size];
                        Array.Copy(bytes, bytes2, size);
                        AndroidDecompress decompress = new AndroidDecompress();
                        content = decompress.decompressXML(bytes);
                        break;
                    }
                }
            }

            return content;
        }
    }
}
