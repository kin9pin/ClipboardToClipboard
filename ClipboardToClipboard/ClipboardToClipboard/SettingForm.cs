using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows.Forms;

namespace ClipboardToClipboard
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            labelVersion.Text += Assembly.GetExecutingAssembly().GetName().Version.ToString();

            numericUpDownHistorySize.Value = Properties.Settings.Default.HistorySize;
            trackBarOpacity.Value = Properties.Settings.Default.Opacity;
            labelOpacity.Text = String.Format("Прозрачность ({0:P0}):", trackBarOpacity.Value / 100d);

            RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
            checkBoxAutoStart.Checked = reg.GetValue(Application.ProductName) != null;
            reg.Close();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            RegistryKey reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true);
            if (reg.GetValue(Application.ProductName) != null)
            {
                try
                {
                    reg.DeleteValue(Application.ProductName);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка удаления программы из автозагрузки", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            if (checkBoxAutoStart.Checked)
                reg.SetValue(Application.ProductName, Application.ExecutablePath);

            reg.Close();

            Properties.Settings.Default.HistorySize = (int)numericUpDownHistorySize.Value;
            Properties.Settings.Default.Opacity = trackBarOpacity.Value;
            Properties.Settings.Default.Save();

            DialogResult = DialogResult.OK;
        }

        private void trackBarOpacity_Scroll(object sender, EventArgs e)
        {
            labelOpacity.Text = String.Format("Прозрачность ({0:P0}):", trackBarOpacity.Value / 100d);
        }
    }
}
