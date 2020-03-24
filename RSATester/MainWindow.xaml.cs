using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace RSATester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PrivateKeyTextBox.Text) || string.IsNullOrWhiteSpace(PublicKeyTextBox.Text))
                return;

            var rsa = new RSACryptoServiceProvider();

            using (var fs = new FileStream(PrivateKeyTextBox.Text, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(rsa.ToXmlString(true));
                }
            }

            using (var fs = new FileStream(PublicKeyTextBox.Text, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(rsa.ToXmlString(false));
                }
            }

            MessageBox.Show("Generate successfully!");
        }

        private void SelectPrivateKey_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "XML|*.xml";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PrivateKeyTextBox.Text = ofd.FileName;
            }
        }

        private void SelectPublicKey_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "XML|*.xml";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PublicKeyTextBox.Text = ofd.FileName;
            }
        }

        private HashAlgorithm GetHashAlgorithm()
        {
            if (Md5RadioButton.IsChecked.Value)
                return new MD5CryptoServiceProvider();

            if (SHA1RadioButton.IsChecked.Value)
                return new SHA1CryptoServiceProvider();

            if (SHA256RadioButton.IsChecked.Value)
                return new SHA256CryptoServiceProvider();

            return null;
        }

        private string GetPrivateKey()
        {
            if (string.IsNullOrWhiteSpace(PrivateKeyTextBox.Text))
                return null;

            using (var fs = new FileStream(PrivateKeyTextBox.Text, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private string GetPublicKey()
        {
            if (string.IsNullOrWhiteSpace(PrivateKeyTextBox.Text))
                return null;

            using (var fs = new FileStream(PublicKeyTextBox.Text, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OriginalText.Text))
                return;

            var privateKey = GetPrivateKey();
            if (string.IsNullOrWhiteSpace(privateKey))
                return;

            var hash = GetHashAlgorithm();
            if (hash == null)
                return;

            var data = Encoding.UTF8.GetBytes(OriginalText.Text);
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);

            try
            {
                SignText.Text = Convert.ToBase64String(rsa.SignData(data, hash));
                MessageBox.Show("Sign successfully!");
                return;
            }
            catch { }

            MessageBox.Show("Sign failed!");
        }

        private void Verify_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OriginalText.Text))
                return;

            var publicKey = GetPublicKey();
            if (string.IsNullOrWhiteSpace(publicKey))
                return;

            var hash = GetHashAlgorithm();
            if (hash == null)
                return;

            var data = Encoding.UTF8.GetBytes(OriginalText.Text);
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);

            try
            {
                var result = rsa.VerifyData(data, hash, Convert.FromBase64String(SignText.Text));
                if (result)
                {
                    MessageBox.Show("Verify successfully!");
                    return;
                }
            }
            catch
            {
            }

            MessageBox.Show("Verification failed!");
        }

        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OriginalText.Text))
                return;

            var publicKey = GetPublicKey();
            if (string.IsNullOrWhiteSpace(publicKey))
                return;

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);

            try
            {
                EncryptedText.Text =
                    Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(OriginalText.Text), false));

                MessageBox.Show("Encrypt successfully!");
                return;
            }
            catch { }

            MessageBox.Show("Encrypt failed!");
        }

        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EncryptedText.Text))
                return;

            var privateKey = GetPrivateKey();
            if (string.IsNullOrWhiteSpace(privateKey))
                return;

            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);

            try
            {
                DecryptedText.Text =
                    Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(EncryptedText.Text), false));

                MessageBox.Show("Decrypt successfully!");
                return;
            }
            catch
            {
            }

            MessageBox.Show("Decrypt failed!");
        }
    }
}

