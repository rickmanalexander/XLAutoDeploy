using System.Windows.Forms;

namespace XLAutoDeploy.Utilities
{
    public static class MessageBoxDisplay
    {
        public static DialogResult DisplayMessage(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (var form = new Form() { TopMost = true })
            {
                return MessageBox.Show(form, message, caption, buttons, icon);
            }
        }
    }
}
