using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.XPath;

namespace NoteSharp.WinForm.Forms
{
    public partial class MainForm : Form
    {
        private string _openedFileName;

        private string OpenedFileName
        {
            get => _openedFileName;
            set
            {
                _openedFileName = value;
                if (value != null)
                    Text = Path.GetFileName(_openedFileName);
            }
        }

        public bool PendingChanges { get; set; }

        public MainForm()
        {
            InitializeComponent();
        }

        private void ToolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inputTextBox.Undo();
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inputTextBox.Redo();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(inputTextBox.SelectedText))
            {
                inputTextBox.SelectCurrentLine();
            }
            inputTextBox.Cut();
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(inputTextBox.SelectedText))
            {
                inputTextBox.SelectCurrentLine();
            }
            inputTextBox.Copy();
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inputTextBox.Paste();
        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inputTextBox.SelectAll();
        }

        private void WordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch (wordWrapToolStripMenuItem.CheckState)
            {
                case CheckState.Checked:
                    wordWrapToolStripMenuItem.CheckState = CheckState.Unchecked;
                    inputTextBox.WordWrap = false;
                    break;

                case CheckState.Unchecked:
                    wordWrapToolStripMenuItem.CheckState = CheckState.Checked;
                    inputTextBox.WordWrap = true;
                    break;

                case CheckState.Indeterminate:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fontDialog = new FontDialog
            {
                Font = inputTextBox.SelectionFont,
                Color = inputTextBox.SelectionColor,
                ShowColor = true
            };

            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                inputTextBox.SelectionFont = fontDialog.Font;
                inputTextBox.SelectionColor = fontDialog.Color;
            }
        }

        private void ColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
            {
                Color = inputTextBox.SelectionColor
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                inputTextBox.SelectionColor = colorDialog.Color;
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void SaveAs()
        {
            var fileDialog = new SaveFileDialog
            {
                AddExtension = true,
                Filter = "Rich Text File (*.rtf)|*.rtf|Text file (*.txt)|*.txt"
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            if (fileDialog.FileName is null)
                return;

            OpenedFileName = fileDialog.FileName;

            Save();
        }

        private void Save()
        {
            var ext = Path.GetExtension(OpenedFileName);
            if (ext == ".rtf")
                inputTextBox.SaveFile(OpenedFileName);
            else if (ext == ".txt")
                File.WriteAllText(OpenedFileName, inputTextBox.Text);

            PendingChanges = false;
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenedFileName != null)
            {
                Save();
            }
            else
            {
                SaveAs();
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "Rich Text File (*.rtf)|*.rtf|Text file (*.txt)|*.txt"
            };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            var fileName = fileDialog.FileName;

            if (fileName is null)
                return;

            OpenedFileName = fileName;

            var ext = Path.GetExtension(fileName);

            if (ext == ".rtf")
                inputTextBox.LoadFile(fileName);
            else if (ext == ".txt")
                inputTextBox.Text = File.ReadAllText(fileName);

            PendingChanges = false;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PendingChanges)
                return;

            var result = MessageBox.Show("Do you want to save changes?", "Pending changes not saved",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            switch (result)
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    if (_openedFileName is null)
                        SaveAs();
                    else
                        Save();
                    break;

                case DialogResult.Cancel:
                case DialogResult.None:
                case DialogResult.Abort:
                case DialogResult.Ignore:
                    e.Cancel = true;
                    break;

                case DialogResult.No:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InputTextBox_TextChanged(object sender, EventArgs e)
        {
            PendingChanges = true;
        }
    }

    internal static class RichTextBoxExt
    {
        public static void SelectCurrentLine(this RichTextBox self)
        {
            var charIndex = self.GetFirstCharIndexOfCurrentLine();
            var lineIndex = self.GetLineFromCharIndex(charIndex);

            var lineLength = self.Lines[lineIndex].Length;

            self.Select(charIndex, lineLength);
        }
    }
}