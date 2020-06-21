using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioEncrypt
{
    public partial class Form1 : Form
    {
        OpenFileDialog openDialog = new OpenFileDialog();
        public Form1()
        {
            openDialog.Filter = "Audio Formats (*.wav, *.mp3, *.3gp, *.m4a, *.ogg) | *.wav; *.mp3; *.3gp; *.m4a; *.ogg";

            InitializeComponent();
        }
        public string Filter(string name, bool Decrypt = false)
        {
            switch (Decrypt)
            {
                case false: return name.Replace(".", "_E.");
                case true: return name.Replace(".", "_D.");
                default: return "";
            }
        }
        private void Encrypt_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 5)
            {
                var Key = GeneratePassword(textBox1.Text);
                openDialog.ShowDialog();
                if (openDialog.FileName.Length > 0)
                {
                    EncryptFile(Key, openDialog.FileName, Filter(openDialog.FileName));
                    if (openDialog.FileName.Length > 0)
                        Process.Start(Path.GetDirectoryName(openDialog.FileName));
                }
            }
            else MessageBox.Show("Enter Passwork > 5");
        }
        private void Decrypt_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 5)
            {
                var Key = GeneratePassword(textBox2.Text);
                openDialog.ShowDialog();
                if (openDialog.FileName.Length > 0)
                {
                    DecryptFile(Key, openDialog.FileName, Filter(openDialog.FileName, true));
                    if (openDialog.FileName.Length > 0)
                        Process.Start(Filter(openDialog.FileName, true));
                }
            }
            else MessageBox.Show("Enter Passwork > 5");
        }
        public string GeneratePassword(string password)
        {

            var r = password.Substring(1, 3);
            var k = password;

            var table = Chaos_Table(r, k);
            return table.Substring(int.Parse(table.Substring(password.Length, 2)), 8);
        }
        public string Chaos_Table(string InputKeyR, string InputKeyX)
        {
            double[] x = new double[1001];
            double r;

            x[0] = ChaosAsc(InputKeyX) % 0.31;
            r = ChaosAsc(InputKeyR) % 3.73;
            if (r == 0)
                r += 0.39;
            else if (x[0] == 0)
                x[0] = 0.43;
            for (var i = 0; i <= x.Length - 2; i++)
            {
                if (!x.Contains(x[i] * r * (1 - x[i]) % 1))
                {
                    x[i + 1] = x[i] * r * (1 - x[i]) % 1;
                    r = r + 0.5 % 4;
                }
            }
            string[] StrArray = Array.ConvertAll(x, y => y.ToString());
            return string.Join("", StrArray).Replace("0.", "").Replace("00","0").Replace("00","0");
        }
        public int ChaosAsc(string Key)
        {
            int FullAsc = 0;
            for (var i = 0; i < Key.Length; i++)
                FullAsc += (int)char.Parse(Key.Substring(i, 1));
            return FullAsc;
        }
        private static void EncryptFile(string password, string inputFile, string outputFile)
        {

            try
            {
                // = @"myKey123"; // Your Key Here
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                string cryptFile = outputFile;
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,
                    RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);

                FileStream fsIn = new FileStream(inputFile, FileMode.Open);

                int data;
                while ((data = fsIn.ReadByte()) != -1)
                    cs.WriteByte((byte)data);


                fsIn.Close();
                cs.Close();
                fsCrypt.Close();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.ToString());
            }
        }
        ///<summary>
        /// Steve Lydford - 12/05/2008.
        ///
        /// Decrypts a file using Rijndael algorithm.
        ///</summary>
        ///<param name="inputFile"></param>
        ///<param name="outputFile"></param>
        private static void DecryptFile(string password, string inputFile, string outputFile)
        {

            {
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);

                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,
                    RMCrypto.CreateDecryptor(key, key),
                    CryptoStreamMode.Read);

                FileStream fsOut = new FileStream(outputFile, FileMode.Create);

                int data;
                try
                {
                    while ((data = cs.ReadByte()) != -1)
                        fsOut.WriteByte((byte)data);
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid Pasword");
                }

                try
                {
                    fsOut.Close();
                    cs.Close();
                    fsCrypt.Close();
                }
                catch
                {
                    MessageBox.Show("Invalid Password for this file");
                    Application.Restart();
                }

            }
        }

        
    }
}
