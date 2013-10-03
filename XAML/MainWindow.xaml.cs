using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;//CopyableMessageBox
using Microsoft.Win32;
using System.ComponentModel;//CommonDialogs
using StringTableEditorController;
using System.Collections;

namespace StringTableEditorView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        PresenterController _PresenterController;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _PresenterController = new PresenterController();

            #region PresenterController events
            _PresenterController.DefaultLanguageChanged += new EventHandler(_PresenterController_DefaultLanguageChanged);
            _PresenterController.ExportEmptyStringsChanged += new EventHandler(_PresenterController_ExportEmptyStringsChanged);

            _PresenterController.FilenameChanged += new EventHandler<PresenterController.StringEventArgs>(_PresenterController_FilenameChanged);
            _PresenterController.IsDirtyChanged += new EventHandler(_PresenterController_IsDirtyChanged);

            _PresenterController.StringTableSetsChanged += new EventHandler(_PresenterController_StringTableSetsChanged);
            _PresenterController.SelectedStringTableSetChanged += new EventHandler(_PresenterController_SelectedStringTableSetChanged);

            _PresenterController.LanguagesChanged += new EventHandler(_PresenterController_LanguagesChanged);
            _PresenterController.SelectedLanguageChanged += new EventHandler(_PresenterController_SelectedLanguageChanged);

            _PresenterController.ShowDefaultLanguageChanged += new EventHandler(_PresenterController_ShowDefaultLanguageChanged);

            _PresenterController.StringSetsChanged += new EventHandler(_PresenterController_StringSetsChanged);
            _PresenterController.SelectedStringSetChanged += new EventHandler(_PresenterController_SelectedStringSetChanged);

            // These event will be raised before the event handler is attached, so resend now.
            _PresenterController_ExportEmptyStringsChanged(null, null);
            _PresenterController_LanguagesChanged(null, null);
            _PresenterController_SelectedLanguageChanged(null, null);
            _PresenterController_ShowDefaultLanguageChanged(null, null);
            #endregion

            #region File menu
            ofdOpenPackage = new OpenFileDialog();
            ofdOpenPackage.FileName = "*.package";
            ofdOpenPackage.Filter = "Sims3 Packages|*.package|All Files|*.*";
            ofdOpenPackage.Title = "Open package";

            sfdSavePackage = new SaveFileDialog();
            sfdSavePackage.DefaultExt = "package";
            sfdSavePackage.FileName = "*.package";
            sfdSavePackage.Filter = "Sims3 Packages|*.package|All Files|*.*";
            sfdSavePackage.Title = "Save package as";

            this.CommandBindings.AddRange(new CommandBinding[] {
                new CommandBinding(ApplicationCommands.New, fileNewExecuted, (x, e) => {e.CanExecute = _PresenterController.FileNewEnabled; e.Handled = true; }),
                new CommandBinding(ApplicationCommands.Open, fileOpenExecuted, (x, e) => {e.CanExecute = _PresenterController.FileOpenEnabled; e.Handled = true; }),
                new CommandBinding(ApplicationCommands.Save, fileSaveExecuted, (x, e) => {e.CanExecute = _PresenterController.FileSaveEnabled; e.Handled = true; }),
                new CommandBinding(ApplicationCommands.SaveAs, fileSaveAsExecuted, (x, e) => {e.CanExecute = _PresenterController.FileSaveAsEnabled; e.Handled = true; }),
                new CommandBinding(fileSaveCopyAs, fileSaveCopyAsExecuted, (x, e) => {e.CanExecute = _PresenterController.FileSaveCopyAsEnabled; e.Handled = true; }),
                new CommandBinding(ApplicationCommands.Close, fileCloseExecuted, (x, e) => {e.CanExecute = _PresenterController.FileCloseEnabled; e.Handled = true; }),
                new CommandBinding(fileExit, fileExitExecuted, (x, e) => {e.CanExecute = _PresenterController.FileExitEnabled; e.Handled = true; }),
            });

            // Additional input bindings
            this.InputBindings.AddRange(new InputBinding[] {
                new KeyBinding(ApplicationCommands.Close, new KeyGesture(Key.W, ModifierKeys.Control)),
                new KeyBinding(fileExit, new KeyGesture(Key.Q, ModifierKeys.Control)),
            });
            #endregion

            #region Export menu
            sfdExportLanguage = new SaveFileDialog();
            sfdExportLanguage.DefaultExt = "xml";
            sfdExportLanguage.FileName = "*.xml";
            sfdExportLanguage.Filter = "XML files|*.xml|All Files|*.*";
            sfdExportLanguage.Title = "Export language(s)";

            this.CommandBindings.AddRange(new CommandBinding[] {
                new CommandBinding(exportCurrent, exportCurrentExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportCurrentEnabled; e.Handled = true; }),
                new CommandBinding(exportMarked, exportMarkedExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportMarkedEnabled; e.Handled = true; }),
                new CommandBinding(exportChanged, exportChangedExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportChangedEnabled; e.Handled = true; }),
                new CommandBinding(exportLanguage, exportLanguageExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportLanguageEnabled; e.Handled = true; }),
                new CommandBinding(exportSTSCurrent, exportSTSCurrentExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportSTSCurrentEnabled; e.Handled = true; }),
                new CommandBinding(exportSTSMarked, exportSTSMarkedExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportSTSMarkedEnabled; e.Handled = true; }),
                new CommandBinding(exportSTSChanged, exportSTSChangedExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportSTSChangedEnabled; e.Handled = true; }),
                new CommandBinding(exportSTSLanguage, exportSTSLanguageExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportSTSLanguageEnabled; e.Handled = true; }),
                new CommandBinding(exportAll, exportAllExecuted, (x, e) => {e.CanExecute = _PresenterController.ExportAllEnabled; e.Handled = true; }),
            });
            #endregion

            #region Import menu
            ofdImportLanguage = new OpenFileDialog();
            ofdImportLanguage.FileName = "*.xml";
            ofdImportLanguage.Filter = "XML files|*.xml|All Files|*.*";
            ofdImportLanguage.Title = "Import languages";

            this.CommandBindings.AddRange(new CommandBinding[] {
                new CommandBinding(importStrings, importStringsExecuted, (x, e) => {e.CanExecute = _PresenterController.ImportStringsEnabled; e.Handled = true; }),
            });
            #endregion

            #region Settings menu
            this.CommandBindings.AddRange(new CommandBinding[] {
                new CommandBinding(settingsExportEmptyStrings, settingsExportEmptyStringsExecuted, (x, e) => {e.CanExecute = _PresenterController.SettingsExportEmptyStringsEnabled; e.Handled = true; }),
            });
            #endregion

            this.CommandBindings.AddRange(new CommandBinding[] {
                new CommandBinding(addString, addStringExecuted, (x, e) => { e.CanExecute = _PresenterController.AddStringEnabled; e.Handled = true; }),
                new CommandBinding(deleteString, deleteStringExecuted,(x, e) => { e.CanExecute = _PresenterController.DeleteStringEnabled; e.Handled = true; }),
            });

        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            fileCloseExecuted(null, null);
            if (_PresenterController.CommitRequired) e.Cancel = true;
        }

        void _PresenterController_FilenameChanged(object sender, PresenterController.StringEventArgs e)
        {
            string filename = e.Value;
            bool dirty = this.Title.StartsWith("* ");
            if (filename == null || filename.Length == 0)
                this.Title = "String Table Editor";
            else
                this.Title = "String Table Editor: " + System.IO.Path.GetFileName(filename);
            if (dirty)
                this.Title = "* " + this.Title;

            //openFileDialog1.FileName = filename;
            //saveFileDialog1.FileName = filename;
        }

        void _PresenterController_IsDirtyChanged(object sender, EventArgs e)
        {
            if (this.Title.StartsWith("* "))
                this.Title = this.Title.Substring(2);
            else
                this.Title = "* " + this.Title;
        }

        #region File menu
        OpenFileDialog ofdOpenPackage;
        SaveFileDialog sfdSavePackage;

        // Additional File menu commands
        public static RoutedCommand fileSaveCopyAs = new RoutedCommand();
        public static RoutedCommand fileExit = new RoutedCommand();

        private void fileNewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Save whatever we have but keep any filename
            if (!Commit()) return;

            _PresenterController.DoFileNew();
        }
        private void fileOpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {             // Save whatever we have but keep any filename
            if (!Commit()) return;

            // Get the proposed filename from the user
            ofdOpenPackage.FilterIndex = 1;
            bool? dr = ofdOpenPackage.ShowDialog();
            if (dr != true) return;

            // See if there are any objections to using it (like it's a bad package, for example)
            string message;
            if (!_PresenterController.IsValidFilename(ofdOpenPackage.FileName, false, out message))
            {
                CopyableMessageBox.Show(message, "Open file...", CopyableMessageBoxButtons.OK, CopyableMessageBoxIcon.Stop);
                return;
            }

            _PresenterController.DoFileOpen(ofdOpenPackage.FileName);
        }
        private void fileSaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_PresenterController.FileSaveEnabled)
                return;

            if (_PresenterController.MustSaveAs)
            {
                fileSaveAsExecuted(null, null);
            }
            else
            {
                _PresenterController.DoFileSave();
            }
        }
        private void fileSaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_PresenterController.FileSaveAsEnabled)
                return;

            string filename = SaveToNewFilename("Save as...");
            if (filename == null) return;

            // Now work from the new filename
            _PresenterController.DoFileOpen(filename);
        }
        private void fileSaveCopyAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_PresenterController.FileSaveCopyAsEnabled)
                return;

            SaveToNewFilename("Save copy as...");
        }
        private void fileCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_PresenterController.FileCloseEnabled)
                return;

            // Save whatever we have but keep any filename
            if (!Commit()) return;

            _PresenterController.DoFileClose();
        }
        private void fileExitExecuted(object sender, ExecutedRoutedEventArgs e)
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
                    fileSaveExecuted(null, null);
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
            sfdSavePackage.FilterIndex = 1;
            bool? dr = sfdSavePackage.ShowDialog();
            if (dr != true) return null;

            // See if there are any objections to using it
            string message;
            if (!_PresenterController.IsValidFilename(sfdSavePackage.FileName, true, out message))
            {
                CopyableMessageBox.Show(message, prompt, CopyableMessageBoxButtons.OK, CopyableMessageBoxIcon.Stop);
                return null;
            }

            // Write out to the new filename
            _PresenterController.DoFileSaveAs(sfdSavePackage.FileName);

            return sfdSavePackage.FileName;
        }
        #endregion

        #region Export menu
        SaveFileDialog sfdExportLanguage;

        public static RoutedCommand exportCurrent = new RoutedCommand();
        public static RoutedCommand exportMarked = new RoutedCommand();
        public static RoutedCommand exportChanged = new RoutedCommand();
        public static RoutedCommand exportLanguage = new RoutedCommand();
        public static RoutedCommand exportSTSCurrent = new RoutedCommand();
        public static RoutedCommand exportSTSMarked = new RoutedCommand();
        public static RoutedCommand exportSTSChanged = new RoutedCommand();
        public static RoutedCommand exportSTSLanguage = new RoutedCommand();
        public static RoutedCommand exportAll = new RoutedCommand();

        private void exportCurrentExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportCurrent); }
        private void exportMarkedExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportMarked); }
        private void exportChangedExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportChanged); }
        private void exportLanguageExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportLanguage); }
        private void exportSTSCurrentExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportSTSCurrent); }
        private void exportSTSMarkedExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportSTSMarked); }
        private void exportSTSChangedExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportSTSChanged); }
        private void exportSTSLanguageExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportSTSLanguage); }
        private void exportAllExecuted(object sender, ExecutedRoutedEventArgs e) { doLanguageExport(_PresenterController.DoExportAll); }

        void doLanguageExport(Action<string> doExport)
        {
            // Get the filename from the user
            sfdExportLanguage.FilterIndex = 1;
            bool? dr = sfdExportLanguage.ShowDialog();
            if (dr != true) return;

            doExport(sfdExportLanguage.FileName);
        }
        #endregion

        #region Import menu
        OpenFileDialog ofdImportLanguage;
        public static RoutedCommand importStrings = new RoutedCommand();

        private void importStringsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // Get the proposed filename from the user
            ofdImportLanguage.FilterIndex = 1;
            bool? dr = ofdImportLanguage.ShowDialog();
            if (dr != true) return;

            _PresenterController.DoImportStrings(ofdImportLanguage.FileName);
        }
        #endregion

        #region Settings menu
        public IEnumerable settingsDefaultLanguage_ItemsSource
        {
            get
            {
                return _PresenterController.Languages.Select(x => new {
                    Header = x.Replace("_", "__"),
                    IsChecked = _PresenterController.DefaultLanguage == x,
                });
            }
        }
        private void settingsDefaultLanguage_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = e.OriginalSource as MenuItem;
            if (mi == null) return;

            _PresenterController.DoSettingsDefaultLanguage((mi.Header as string).Replace("__", "_"));
        }

        void _PresenterController_DefaultLanguageChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("settingsDefaultLanguage_ItemsSource");
        }

        public static RoutedCommand settingsExportEmptyStrings = new RoutedCommand();
        private void settingsExportEmptyStringsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            _PresenterController.DoSettingsExportEmptyStrings();
        }
        public bool settingsExportEmptyStrings_IsChecked { get { return _PresenterController.ExportEmptyStrings; } }
        void _PresenterController_ExportEmptyStringsChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("settingsExportEmptyStrings_IsChecked");
        }
        #endregion

        // ---------

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String property) { if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(property)); } }

        #region StringTableSets ComboBox
        void _PresenterController_StringTableSetsChanged(object sender, EventArgs e)
        {
            PopulateStringTableSets();
        }

        void PopulateStringTableSets()
        {
            OnPropertyChanged("cbStringTableSet_ItemsSource");
            OnPropertyChanged("cbStringTableSet_IsEnabled");
        }

        public IEnumerable<string> cbStringTableSet_ItemsSource { get { return _PresenterController.StringTableSets; } }

        public bool cbStringTableSet_IsEnabled { get { return !cbStringTableSet.Items.IsEmpty; } }

        //--

        void _PresenterController_SelectedStringTableSetChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("cbStringTableSet_SelectedValue");
        }

        public object cbStringTableSet_SelectedValue
        {
            get { return cbStringTableSet.Items.IsEmpty || _PresenterController.SelectedStringTableSet == -1 ? null : (string)cbStringTableSet.Items[_PresenterController.SelectedStringTableSet]; }
            set
            {
                int res = value == null ? -1 : cbStringTableSet.Items.IndexOf(value);
                if (_PresenterController.SelectedStringTableSet != res)
                {
                    _PresenterController.SelectedStringTableSet = res;
                    OnPropertyChanged("cbStringTableSet_SelectedValue");
                }
            }
        }
        #endregion

        #region Languages ComboBox
        void _PresenterController_LanguagesChanged(object sender, EventArgs e)
        {
            PopulateLanguages();
        }

        void PopulateLanguages()
        {
            OnPropertyChanged("cbLanguage_ItemsSource");
            OnPropertyChanged("cbLanguage_IsEnabled");
        }

        public IEnumerable<string> cbLanguage_ItemsSource { get { return _PresenterController.Languages; } }

        public bool cbLanguage_IsEnabled { get { return !cbLanguage.Items.IsEmpty; } }

        // --

        void _PresenterController_SelectedLanguageChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("cbLanguage_SelectedValue");
        }

        public object cbLanguage_SelectedValue
        {
            get { return cbLanguage.Items.IsEmpty || _PresenterController.SelectedLanguage == -1 ? null : cbLanguage.Items[_PresenterController.SelectedLanguage]; }
            set
            {
                int res = value == null ? -1 : cbLanguage.Items.IndexOf(value);
                if (_PresenterController.SelectedLanguage != res)
                {
                    _PresenterController.SelectedLanguage = res;
                    OnPropertyChanged("cbLanguage_SelectedValue");
                }
            }
        }
        #endregion

        #region ShowDefaultLanguage CheckBox
        void PopulateShowDefaultLanguage()
        {
            OnPropertyChanged("ckbShowDefaultLanguage_IsChecked");
        }

        public bool ckbShowDefaultLanguage_IsChecked
        {
            get { return _PresenterController.ShowDefaultLanguage; }
            set
            {
                if (_PresenterController.ShowDefaultLanguage != value)
                {
                    _PresenterController.ShowDefaultLanguage = value;
                    OnPropertyChanged("ckbShowDefaultLanguage_IsChecked");
                }
            }
        }

        void _PresenterController_ShowDefaultLanguageChanged(object sender, EventArgs e) { OnPropertyChanged("ShowDefaultLanguageIsChecked"); }
        #endregion

        #region Add/Delete String buttons
        public static RoutedCommand addString = new RoutedCommand();
        public static RoutedCommand deleteString = new RoutedCommand();

        private void addStringCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = _PresenterController.AddStringEnabled; e.Handled = true; }
        private void deleteStringCanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = _PresenterController.DeleteStringEnabled; e.Handled = true; }

        private void addStringExecuted(object sender, ExecutedRoutedEventArgs e) { _PresenterController.DoAddString(); }
        private void deleteStringExecuted(object sender, ExecutedRoutedEventArgs e) { _PresenterController.DoDeleteString(); }
        #endregion

        #region Strings DataGrid
        void _PresenterController_StringSetsChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("dgStringTable_ItemsSource");
            OnPropertyChanged("dgStringTable_IsEnabled");
        }

        public IEnumerable dgStringTable_ItemsSource { get { return _PresenterController.StringSets; } }

        public bool dgStringTable_IsEnabled { get { return !dgStringTable.Items.IsEmpty; } }

        private void dgStringTable_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Changed") { e.Column.IsReadOnly = true; e.Column.DisplayIndex = 0; e.Column.Header = "*"; e.Column.CanUserResize = false; }
            else if (e.PropertyName == "Selected") { e.Column.DisplayIndex = 1; e.Column.Header = "?"; e.Column.CanUserResize = false; }
            else if (e.PropertyName == "Guid") { e.Column.DisplayIndex = 2; e.Column.CanUserResize = false; }
            else
            {
                e.Column.IsReadOnly = true;
                e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                if (e.PropertyName == "Default")
                {
                    e.Column.Header = String.Format("Default ({0})", _PresenterController.DefaultLanguage);
                }
                else if (e.PropertyName == "Current")
                {
                    e.Column.Header = String.Format("Current ({0})", cbLanguage.SelectedItem as string);
                }
            }
        }

        private void dgStringTable_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell dgc = (e.OriginalSource as DependencyObject).TryFindParent<DataGridCell>();
            if (dgc == null || dgc.Column.SortMemberPath != "Selected") return;

            CheckBox ckb = dgc.Content as CheckBox;
            if (ckb == null) return;

            DataGridRow dgr = (e.OriginalSource as DependencyObject).TryFindParent<DataGridRow>();
            if (dgr == null) return;

            PresenterController.RowFormatCurrent item = dgr.Item as PresenterController.RowFormatCurrent;
            if (item == null) return;

            if (ckb.IsChecked.HasValue) item.Selected = !ckb.IsChecked.Value;

            dgr.IsSelected = true;
            ckb.Focus();
            e.Handled = true;
        }

        // --

        void _PresenterController_SelectedStringSetChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("dgStringTable_SelectedValue");
            OnPropertyChanged("tbCurrentString_IsEnabled");
        }

        public object dgStringTable_SelectedValue
        {
            get { return dgStringTable.Items.IsEmpty || _PresenterController.SelectedStringSet == -1 ? null : dgStringTable.Items[_PresenterController.SelectedStringSet]; }
            set
            {
                int res = value == null ? -1 : dgStringTable.Items.IndexOf(value);
                if (_PresenterController.SelectedStringSet != res)
                {
                    _PresenterController.SelectedStringSet = res;
                    OnPropertyChanged("dgStringTable_SelectedValue");
                    OnPropertyChanged("tbCurrentString_IsEnabled");
                }
            }
        }

        public bool tbCurrentString_IsEnabled { get { return _PresenterController.SelectedStringSet >= 0; } }
        #endregion
    }
}
