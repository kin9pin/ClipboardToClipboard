using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ClipboardToClipboard
{
    public partial class MainForm : Form
    {
        // дескриптор окна
        private IntPtr nextClipboardViewer;
        
        // История нашего буфера
        private IList<Buffer> stackClipBoardHistory = new List<Buffer>();

        private bool isPasting = false;
        private bool isInitializing = false;
        public String historyPath; // путь к файлу истории

        // Константы
        private const int WM_DRAWCLIPBOARD = 0x308;
        private const int WM_CHANGECBCHAIN = 0x030D;
        private const byte VK_CONTROL = 0x11;
        private const byte KEYEVENTF_KEYUP = 0x2;

        // Подключение библиотек WIN
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
        
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        public MainForm()
        {
            InitializeComponent();

            historyPath = Application.UserAppDataPath + "\\history.dat";

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            nextClipboardViewer = (IntPtr)SetClipboardViewer((IntPtr)this.Handle);
            Program.EventPressCtrlAltV += new Program.EventCtrlAltVHandler(ShowMainWindow);

            LoadSettings();
            LoadHistoryBuffer();
            isInitializing = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.ShowInTaskbar)
            {
                e.Cancel = true;
                HideMainWindow();
            }
        }

        private void HideMainWindow()
        {
            this.ShowInTaskbar = false;
            this.Hide();
        }
        
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearHistory();
            ReloadListBoxClipboard();
        }

        private void ClearHistory()
        {
            if (Clipboard.ContainsText())
                Clipboard.Clear();

            stackClipBoardHistory.Clear();
            SaveHistoryToFile();
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;

            if (nextClipboardViewer == IntPtr.Zero)
                nextClipboardViewer = (IntPtr)SetClipboardViewer((IntPtr)this.Handle);

            this.Activate();
            this.Focus();
            listBoxClipboard.Focus();
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            isInitializing = false;
            nextClipboardViewer = (IntPtr)SetClipboardViewer((IntPtr)this.Handle);
            isInitializing = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch(keyData)
            {
                case Keys.Escape:
                    if (textBoxSearch.Focused && textBoxSearch.TextValue.Trim() != String.Empty)
                        textBoxSearch.TextValue = String.Empty;
                    else
                        HideMainWindow();
                    break;

                case Keys.F | Keys.Control:
                    textBoxSearch.Focus();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Метод для реагирование на изменение вбуфере обмена и т.д.
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    if (!isPasting)
                        ClipboardChanged();
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);

                    m.Result = IntPtr.Zero;
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        // Реагируем на обновление буфера обмена
        private void ClipboardChanged()
        {
            try
            {
                if (!isPasting && Clipboard.ContainsText() && Clipboard.GetText().Length > 0)
                {
                    Buffer buffer = new Buffer(Clipboard.GetText());
                    if (!stackClipBoardHistory.Take(Properties.Settings.Default.HistorySize).Contains(buffer))
                        AddToHistoryBuffer(buffer);
                    else
                        StandUpBuffer(buffer);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка перехвата значения из буфера обмена",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHistoryBuffer()
        {
            if (File.Exists(historyPath))
            {
                String xmlString = String.Empty;

                try
                {
                    using (StreamReader stream = new StreamReader(historyPath))
                    {
                        xmlString = stream.ReadToEnd();
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Не удалось загрузить историю буферов из файла", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (xmlString != String.Empty)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlString);

                    foreach (XmlElement ruleElement in doc.DocumentElement.ChildNodes)
                    {
                        string comment = ruleElement.GetAttribute("Comment");
                        string value = ruleElement.GetAttribute("Value");

                        Buffer buffer = new Buffer(value, comment);
                        if (!stackClipBoardHistory.Contains(buffer))
                            stackClipBoardHistory.Add(buffer);
                        else
                            stackClipBoardHistory.Where(p => p.Equals(buffer)).First().Comment = buffer.Comment;
                    }
                }

                int maxCountHistoryItems = Properties.Settings.Default.HistorySize;
                while(maxCountHistoryItems < stackClipBoardHistory.Count)
                {
                    stackClipBoardHistory.RemoveAt(maxCountHistoryItems);
                }

                ReloadListBoxClipboard();
            }
        }

        private void AddToHistoryBuffer(Buffer buffer)
        {
            stackClipBoardHistory.Insert(0, buffer);

            if(isInitializing)
                notifyIcon.ShowBalloonTip(0, "Добавлен текст", buffer.ShortValue, ToolTipIcon.None);

            int maxCountHistoryItems = Properties.Settings.Default.HistorySize;
            while (maxCountHistoryItems < stackClipBoardHistory.Count)
            {
                stackClipBoardHistory.RemoveAt(maxCountHistoryItems);
            }

            if (isInitializing)
                SaveHistoryToFile();

            ReloadListBoxClipboard();
        }

        private void SaveHistoryToFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(historyPath, false, Encoding.UTF8))
                {
                    XmlDocument document = new XmlDocument();
                    XmlElement xmlBuffers = document.CreateElement("Buffers");

                    foreach (Buffer buffer in stackClipBoardHistory)
                    {
                        XmlElement newBuffer = document.CreateElement("Buffer");

                        XmlAttribute value = document.CreateAttribute("Value");
                        value.Value = buffer.LongValue;

                        XmlAttribute comment = document.CreateAttribute("Comment");
                        comment.Value = buffer.Comment;

                        newBuffer.Attributes.Append(value);
                        newBuffer.Attributes.Append(comment);
                        
                        xmlBuffers.AppendChild(newBuffer);
                    }

                    XmlElement root = document.CreateElement("root");
                    root.AppendChild(xmlBuffers);

                    writer.WriteLine(root.InnerXml);
                    writer.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Не удалось сохранить историю буферов в файле", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReloadListBoxClipboard()
        {
            listBoxClipboard.BeginUpdate();

            listBoxClipboard.DataSource = null;
            listBoxClipboard.DisplayMember = "ShortValue";
            listBoxClipboard.ValueMember = "LongValue";

            IList<Buffer> result = stackClipBoardHistory.Where(p => p.LongValue.ToUpper().Contains(textBoxSearch.TextValue.ToUpper())).ToList();
            listBoxClipboard.DataSource = result;

            if (listBoxClipboard.Items.Count > 0)
                listBoxClipboard.SelectedIndex = 0;

            listBoxClipboard.EndUpdate();

            buttonClear.Enabled = stackClipBoardHistory.Count != 0;
            buttonRemove.Enabled = listBoxClipboard.SelectedIndex != -1;
        }

        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyData)
            {
                case Keys.Enter:
                    PasteSelectedBuffer();
                break;
            }
        }

        private void listBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PasteSelectedBuffer();
        } 

        private void PasteSelectedBuffer()
        {
            if (listBoxClipboard.SelectedIndex != -1)
            {
                Buffer selectBuffer = listBoxClipboard.SelectedItem as Buffer;

                StandUpBuffer(selectBuffer);

                lock (this)
                {
                    isPasting = true;
                    this.Hide();
                    Clipboard.SetText(selectBuffer.LongValue);

                    //эмулируем нажатие Ctrl+V
                    SendCtrlhotKey('V');

                    isPasting = false;
                }
            }
        }

        private void StandUpBuffer(Buffer buffer)
        {
            stackClipBoardHistory.Remove(buffer);
            stackClipBoardHistory.Insert(0, buffer);
            ReloadListBoxClipboard();
        }

        private static void SendCtrlhotKey(char key)
        {
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event((byte)key, 0, 0, 0);
            keybd_event((byte)key, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMainWindow();

            SettingForm form = new SettingForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadSettings();
                LoadHistoryBuffer();
            }
        }

        private void LoadSettings()
        {
            this.Opacity = Properties.Settings.Default.Opacity / 100d;
        }

        private void listBoxClipboard_MouseMove(object sender, MouseEventArgs e)
        {
            int index = listBoxClipboard.IndexFromPoint(e.Location);

            if (index != -1 && index < listBoxClipboard.Items.Count)
            {
                Buffer buffer = listBoxClipboard.Items[index] as Buffer;
                
                toolTip.ToolTipTitle = buffer.Comment;

                if (toolTip.GetToolTip(listBoxClipboard) != buffer.LongValue)
                    toolTip.SetToolTip(listBoxClipboard, buffer.LongValue);
            }
            else
            {
                toolTip.ToolTipTitle = String.Empty;
                toolTip.SetToolTip(this.listBoxClipboard, string.Empty);
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            RemoveFromHistory();
        }

        private void RemoveFromHistory()
        {
            int selectedIndex = listBoxClipboard.SelectedIndex;
            if (selectedIndex != -1)
            {
                stackClipBoardHistory.RemoveAt(selectedIndex);
                ReloadListBoxClipboard();
                SaveHistoryToFile();
            }
        }

        private void listBoxClipboard_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listBoxClipboard.SelectedIndex;

            if (selectedIndex != -1 && this.Visible)
            {
                Rectangle rec = listBoxClipboard.GetItemRectangle(selectedIndex);
                Point point = new Point(rec.X + 12, rec.Y + listBoxClipboard.ItemHeight + 6);

                Buffer buffer = listBoxClipboard.Items[selectedIndex] as Buffer;
                toolTip.ToolTipTitle = buffer.Comment;
                toolTip.Show(buffer.LongValue, listBoxClipboard, point, 5000);
            }
            else
            {
                toolTip.ToolTipTitle = String.Empty;
                toolTip.SetToolTip(this.listBoxClipboard, string.Empty);
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveFromHistory();
        }

        private void changeCommentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeComment();
        }

        private void ChangeComment()
        {
            int selectedIndex = listBoxClipboard.SelectedIndex;

            if (selectedIndex != -1)
            {
                Buffer buffer = listBoxClipboard.Items[selectedIndex] as Buffer;
                ChangeCommentForm form = new ChangeCommentForm(buffer.Comment);

                form.StartPosition = this.Visible ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    buffer.Comment = form.Comment;
                    SaveHistoryToFile();
                }
            }
        }

        private void contextMenuStripListbox_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            changeCommentToolStripMenuItem.Enabled = listBoxClipboard.SelectedIndex != -1;
            removeToolStripMenuItem.Enabled = listBoxClipboard.SelectedIndex != -1;
        }

        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            ChangeComment();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Enter:
                    listBoxClipboard.Focus();
                    break;
            }        
        }

        private void textBoxSearch_TextValueChanged(object sender, EventArgs e)
        {
            ReloadListBoxClipboard();
        }
    }
}
