using System.Windows.Forms;

namespace TrackFolderChange.Support
{
    public class FormManipulator
    {
        public static DialogResult ShowWarning(string message)
        {
            return MessageBox.Show(message,
                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        }

        public static void ShowInformation(string message)
        {
            MessageBox.Show(message, @"Done", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, @"Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        public static void ShowMessage(string message)
        {
            MessageBox.Show(message, @"Done", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
