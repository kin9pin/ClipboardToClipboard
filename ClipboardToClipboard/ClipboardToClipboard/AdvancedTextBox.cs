using System;
using System.Windows.Forms;

namespace ClipboardToClipboard
{
    public partial class AdvancedTextBox : UserControl
    {
        public new event KeyEventHandler KeyDown;
        public event EventHandler TextValueChanged;

        public new bool Focused
        {
            get
            {
                return textBox.Focused;
            }
        }

        public string Placeholder
        {
            get
            {
                return labelPlaceholder.Text;
            }
            set
            {
                labelPlaceholder.Text = value;
            }
        }

        public string TextValue
        {
            get
            {
                return textBox.Text;
            }
            set
            {
                textBox.Text = value;
            }
        }

        public AdvancedTextBox()
        {
            InitializeComponent();

            KeyDown = (s, e) => { };
            TextValueChanged = (s, e) => { };
            
            Placeholder = "Поиск";
            labelPlaceholder.Visible = true;

            textBox.GotFocus += textBox_GotFocus;
            textBox.LostFocus += textBox_LostFocus;
        }

        private void AdvancedTextBox_Load(object sender, EventArgs e)
        {
            MoveControl();
        }

        private void AdvancedTextBox_SizeChanged(object sender, EventArgs e)
        {
            MoveControl();
        }

        private void MoveControl()
        {
            this.Height = textBox.Height;            
        }

        private void UpdatePlaceholder()
        {
            if (textBox.Text.Trim() == String.Empty)
                labelPlaceholder.Visible = true;
            else
                labelPlaceholder.Visible = false;
        }

        private void labelPlaceholder_Click(object sender, EventArgs e)
        {
            textBox.Focus();
        }

        void textBox_GotFocus(object sender, EventArgs e)
        {
            labelPlaceholder.Visible = false;
        }

        void textBox_LostFocus(object sender, EventArgs e)
        {
            UpdatePlaceholder();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyDown(this, e);
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextValueChanged(this, e);
        }
    }
}
