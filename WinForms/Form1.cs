/***************************************************************************
 *  Copyright (C) 2011 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
 *                                                                         *
 *  This is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  This program is distributed in the hope that it will be useful,        *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.  *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using StringTableEditorModel;
using StringTableEditorController;

namespace StringTableEditorView
{
    /// <summary>
    /// The Form is responsible for managing the Filename
    /// </summary>
    public partial class Form1 : Form
    {
        PresenterController _PresenterController;
        public Form1(PresenterController pc)
        {
            InitializeComponent();

            _PresenterController = pc;
            _PresenterController.FilenameChanged += new EventHandler<PresenterController.StringEventArgs>(_PresenterController_FilenameChanged);
            _PresenterController.IsDirtyChanged += new EventHandler(_PresenterController_IsDirtyChanged);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            closeToolStripMenuItem_Click(null, null);
            if (_PresenterController.CommitRequired) e.Cancel = true;
        }

        void _PresenterController_IsDirtyChanged(object sender, EventArgs e)
        {
            if (this.Text.StartsWith("* "))
                this.Text = this.Text.Substring(2);
            else
                this.Text = "* " + this.Text;
        }

        void _PresenterController_FilenameChanged(object sender, PresenterController.StringEventArgs e)
        {
            string filename = e.Value;
            bool dirty = this.Text.StartsWith("* ");
            if (filename == null || filename.Length == 0)
                this.Text = "String Table Editor";
            else
                this.Text = "String Table Editor: " + System.IO.Path.GetFileName(filename);
            if (dirty)
                this.Text = "* " + this.Text;

            openFileDialog1.FileName = filename;
            saveFileDialog1.FileName = filename;
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            newToolStripMenuItem.Enabled = _PresenterController.FileNewEnabled;
            openToolStripMenuItem.Enabled = _PresenterController.FileOpenEnabled;
            saveToolStripMenuItem.Enabled = _PresenterController.FileSaveEnabled;
            saveAsToolStripMenuItem.Enabled = _PresenterController.FileSaveAsEnabled;
            saveCopyAsToolStripMenuItem.Enabled = _PresenterController.FileSaveCopyAsEnabled;
            closeToolStripMenuItem.Enabled = _PresenterController.FileCloseEnabled;
            exitToolStripMenuItem.Enabled = _PresenterController.FileExitEnabled;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save whatever we have but keep any filename
            if (!Commit()) return;

            _PresenterController.DoFileNew();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save whatever we have but keep any filename
            if (!Commit()) return;

            // Get the proposed filename from the user
            openFileDialog1.FilterIndex = 1;
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr != DialogResult.OK) return;

            // See if there are any objections to using it (like it's a bad package, for example)
            string message;
            if (!_PresenterController.IsValidFilename(openFileDialog1.FileName, false, out message))
            {
                CopyableMessageBox.Show(message, "Open file...", CopyableMessageBoxButtons.OK, CopyableMessageBoxIcon.Stop);
                return;
            }

            _PresenterController.DoFileOpen(openFileDialog1.FileName);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_PresenterController.FileSaveEnabled)
                return;

            if (_PresenterController.MustSaveAs)
            {
                saveAsToolStripMenuItem_Click(null, null);
            }
            else
            {
                _PresenterController.DoFileSave();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_PresenterController.FileSaveAsEnabled)
                return;

            string filename = SaveToNewFilename("Save as...");
            if (filename == null) return;

            // Now work from the new filename
            _PresenterController.DoFileOpen(filename);
        }

        private void saveCopyAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_PresenterController.FileSaveCopyAsEnabled)
                return;

            SaveToNewFilename("Save copy as...");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_PresenterController.FileCloseEnabled)
                return;

            // Save whatever we have but keep any filename
            if (!Commit()) return;

            _PresenterController.DoFileClose();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.ExitCode = 0;
            this.Close();
        }

        bool Commit()
        {
            if (_PresenterController.CommitRequired)
            {
                int i = CopyableMessageBox.Show(
                    "You have unsaved changes - do you want to save before you continue?\n\n" +
                    "Press \"Yes\" to save and continue.\n" +
                    "Press \"No\" to continue without saving.\n" +
                    "Press \"Cancel\" to stop this action without saving.",
                    "Save?", CopyableMessageBoxButtons.YesNoCancel, CopyableMessageBoxIcon.Question);
                if (i == 2) return false; // Cancel
                if (i == 0) // Yes -> Save
                {
                    saveToolStripMenuItem_Click(null, null);
                    // If we still have unsaved changes, the user must have cancelled somewhere
                    if (_PresenterController.CommitRequired) return false;
                }
                // fall through on "No" or successful save
            }

            return true;
        }

        string SaveToNewFilename(string prompt)
        {
            // Get the filename from the user
            saveFileDialog1.FilterIndex = 1;
            DialogResult dr = saveFileDialog1.ShowDialog();
            if (dr != DialogResult.OK) return null;

            // See if there are any objections to using it
            string message;
            if (!_PresenterController.IsValidFilename(saveFileDialog1.FileName, true, out message))
            {
                CopyableMessageBox.Show(message, prompt, CopyableMessageBoxButtons.OK, CopyableMessageBoxIcon.Stop);
                return null;
            }

            // Write out to the new filename
            _PresenterController.DoFileSaveAs(saveFileDialog1.FileName);

            return saveFileDialog1.FileName;
        }
    }
}
