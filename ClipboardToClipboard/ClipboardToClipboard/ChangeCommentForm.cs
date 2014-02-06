using System.Windows.Forms;

namespace ClipboardToClipboard
{
    public partial class ChangeCommentForm : Form
    {
        public string Comment
        {
            get
            {
                return textBox.Text;
            }
        }

        public ChangeCommentForm(string oldComment)
        {
            InitializeComponent();
            textBox.Text = oldComment;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == '\r')
            {
                e.Handled = true;
                DialogResult = DialogResult.OK;
            }
        }
    }
}
