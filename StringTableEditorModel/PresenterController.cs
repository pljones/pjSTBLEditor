using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using StringTableEditorModel;
using System.Xml;

namespace StringTableEditorController
{
    public class PresenterController
    {
        public PresenterController()
        {
            Model = null;
            OnLanguagesChanged(this, EventArgs.Empty);
            _SelectedLanguage = _DefaultLanguage;
            OnSelectedLanguageChanged();
            OnShowDefaultLanguageChanged();
        }

        #region File menu
        public bool FileNewEnabled { get { return true; } }
        public bool FileOpenEnabled { get { return true; } }
        public bool FileSaveEnabled { get { return ModelIsDirty; } }
        public bool FileSaveAsEnabled { get { return HasValue; } }
        public bool FileSaveCopyAsEnabled { get { return HasValue; } }
        public bool FileCloseEnabled { get { return HasValue; } }
        public bool FileExitEnabled { get { return true; } }

        public void DoFileNew()
        {
            DoFileClose();

            Model = new StringTableEditorModel.Model();
            Model.New();

            OnStringTableSetsChanged();
            if (StringTableSets.Count() > 0)
                SelectedStringTableSet = 0;
        }

        public void DoFileOpen(string filename)
        {
            DoFileClose();

            Filename = filename;
            Model = new StringTableEditorModel.Model();
            Model.Open(filename);

            OnStringTableSetsChanged();
            if (StringTableSets.Count() > 0)
                SelectedStringTableSet = 0;
        }

        public void DoFileSave()
        {
            Model.Save();
        }

        public void DoFileSaveAs(string filename)
        {
            Model.SaveAs(filename);
        }

        public void DoFileClose()
        {
            SelectedStringSet = -1;
            SelectedStringTableSet = -1;
            if (Model != null)
            {
                Model.Close();
                Model = null;
            }
            Filename = null;

            OnStringTableSetsChanged();
        }

        #endregion

        #region Export menu
        public bool ExportCurrentEnabled { get { return _SelectedStringTableSet >= 0 && _SelectedLanguage.HasValue && _SelectedStringSet >= 0; } }
        public bool ExportMarkedEnabled { get { return _SelectedStringTableSet >= 0 && _SelectedLanguage.HasValue && _SelectedStringSet >= 0 && _SelectedGUIDs.Count > 0; } }
        public bool ExportChangedEnabled { get { return _SelectedStringTableSet >= 0 && _SelectedLanguage.HasValue && _SelectedStringSet >= 0 && _ChangedGUIDs.Count > 0; } }
        public bool ExportLanguageEnabled { get { return _SelectedStringTableSet >= 0 && _SelectedLanguage.HasValue; } }
        public bool ExportSTSCurrentEnabled { get { return _SelectedStringSet >= 0 && _SelectedStringSet >= 0; } }
        public bool ExportSTSMarkedEnabled { get { return _SelectedStringTableSet >= 0 && _SelectedStringSet >= 0 && _SelectedGUIDs.Count > 0; } }
        public bool ExportSTSChangedEnabled { get { return _SelectedStringTableSet >= 0 && _SelectedStringSet >= 0 && _ChangedGUIDs.Count > 0; } }
        public bool ExportSTSLanguageEnabled { get { return _SelectedStringTableSet >= 0; } }
        public bool ExportAllEnabled { get { return HasValue && Model.Count() > 0; } }

        public void DoExportCurrent(string filename) { if (ExportCurrentEnabled) DoExport(filename, x => x == _CurrentStringTableSet.IID, x => _CurrentGUID == x, x => x == _SelectedLanguage.Value); }
        public void DoExportMarked(string filename) { if (ExportMarkedEnabled) DoExport(filename, x => x == _CurrentStringTableSet.IID, x => _SelectedGUIDs.Contains(x), x => x == _SelectedLanguage.Value); }
        public void DoExportChanged(string filename) { if (ExportChangedEnabled)  DoExport(filename, x => x == _CurrentStringTableSet.IID, x => _ChangedGUIDs.Contains(x), x => x == _SelectedLanguage.Value); }
        public void DoExportLanguage(string filename) { if (ExportLanguageEnabled) DoExport(filename, x => x == _CurrentStringTableSet.IID, x => true, x => x == _SelectedLanguage.Value); }
        public void DoExportSTSCurrent(string filename) { if (ExportSTSMarkedEnabled) DoExport(filename, x => x == _CurrentStringTableSet.IID, x => _CurrentGUID == x, x => true); }
        public void DoExportSTSMarked(string filename) { if (ExportSTSMarkedEnabled) DoExport(filename, x => x == _CurrentStringTableSet.IID, x => _SelectedGUIDs.Contains(x), x => true); }
        public void DoExportSTSChanged(string filename) { if (ExportSTSChangedEnabled) DoExport(filename, x => x == _CurrentStringTableSet.IID, x => _ChangedGUIDs.Contains(x), x => true); }
        public void DoExportSTSLanguage(string filename) { if (ExportSTSLanguageEnabled) DoExport(filename, x => x == _CurrentStringTableSet.IID, x => true, x => true); }
        public void DoExportAll(string filename) { if (ExportAllEnabled) DoExport(filename, x => true, x => true, x => true); }

        void DoExport(string filename, Predicate<ulong> matchIID, Predicate<ulong> matchGUID, Predicate<Language> matchLanguage)
        {
            XmlDocument xdoc = new XmlDocument();

            XmlElement root = xdoc.CreateElement("pjstbleditorexport");
            if (Model.Filename != null) root.SetAttribute("sourcePackage", System.IO.Path.GetFileName(Model.Filename));
            root.SetAttribute("exportTimestamp", DateTime.UtcNow.ToString("R"));// or O; rfc822 looks nicer, though

            foreach (StringTableSet sts in Model.Where((x, i) => matchIID(x.IID)))
            {
                XmlElement xiid = xdoc.CreateElement("iid");
                xiid.InnerText = "0x" + sts.IID.ToString("X16");

                XmlElement xss = xdoc.CreateElement("stringSets");
                foreach (ulong guid in sts.GUIDs.Where((x, i) => matchGUID(x)))
                {
                    foreach (Language language in ((Language[])Enum.GetValues(typeof(Language))).Where((x, i) => matchLanguage(x)))
                    {
                        XmlElement xguid = xdoc.CreateElement("guid");
                        xguid.InnerText = "0x" + guid.ToString("X16");

                        XmlElement xlanguage = xdoc.CreateElement("language");
                        xlanguage.InnerText = language.ToString();

                        XmlElement xvalue = xdoc.CreateElement("value");
                        xvalue.InnerText = sts[guid, language];

                        XmlElement stringSet = xdoc.CreateElement("stringSet");
                        stringSet.AppendChild(xguid);
                        stringSet.AppendChild(xlanguage);
                        stringSet.AppendChild(xvalue);

                        xss.AppendChild(stringSet);
                    }
                }

                XmlElement xsts = xdoc.CreateElement("StringTableSet");
                xsts.AppendChild(xiid);
                xsts.AppendChild(xss);

                root.AppendChild(xsts);
            }

            xdoc.AppendChild(root);

            using (System.IO.Stream sw = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                XmlTextWriter xtw = new XmlTextWriter(sw, null);
                xtw.Formatting = Formatting.Indented;
                xdoc.WriteContentTo(xtw);
                xtw.Flush();
                sw.Close();
            }
        }
        ulong? _CurrentGUID
        {
            get
            {
                if (_SelectedStringSet >= 0 && _CurrentStringTableSet != null)
                {
                    return _CurrentStringTableSet.GUIDs
                        .Where((u, i) => i == _SelectedStringSet)
                        .Cast<Nullable<ulong>>()
                        .FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        #region Import menu
        public bool ImportStringsEnabled { get { return HasValue; } }
        public void DoImportStrings(string filename)
        {
            System.Xml.XPath.XPathDocument xdoc = new System.Xml.XPath.XPathDocument(filename);
            System.Xml.XPath.XPathNavigator nav = xdoc.CreateNavigator();

            StringTableSet previousSTS = _CurrentStringTableSet;
            SelectedStringTableSet = -1;
            System.Xml.XPath.XPathNodeIterator xsts = nav.Select("/pjstbleditorexport/StringTableSet");
            while (xsts.MoveNext())
            {
                ulong iid = Package.NewInstance() >> 8;
                System.Xml.XPath.XPathNavigator xiid = xsts.Current.SelectSingleNode("iid");
                if (xiid != null)
                {
                    if (xiid.Value.ToLower().StartsWith("0x"))
                    ulong.TryParse(xiid.Value.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out iid);
                }

                Model.Add(iid);
                StringTableSet sts = Model[iid];

                System.Xml.XPath.XPathNodeIterator stringSets = xsts.Current.Select("stringSets/stringSet");
                while (stringSets.MoveNext())
                {
                    ulong guid;
                    System.Xml.XPath.XPathNavigator xguid = stringSets.Current.SelectSingleNode("guid");
                    if (xguid == null)
                        continue;
                    if (!xguid.Value.ToLower().StartsWith("0x"))
                        continue;
                    if (!ulong.TryParse(xguid.Value.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out guid))
                        continue;

                    Language language;
                    System.Xml.XPath.XPathNavigator xlanguage = stringSets.Current.SelectSingleNode("language");
                    if (xlanguage == null)
                        continue;
                    if (!Enum.TryParse<Language>(xlanguage.Value, out language))
                        continue;

                    string value;
                    System.Xml.XPath.XPathNavigator xvalue = stringSets.Current.SelectSingleNode("value");
                    if (xvalue == null)
                        continue;
                    value = xvalue.Value;

                    sts[guid, language] = value;
                }
            }
            OnStringTableSetsChanged();
            SelectedStringTableSet = Model.Count() - 1;
        }
        #endregion

        #region Settings menu
        public void DoSettingsDefaultLanguage(string language) { DefaultLanguage = language; }

        public event EventHandler DefaultLanguageChanged;
        protected void OnDefaultLanguageChanged() { if (DefaultLanguageChanged != null) DefaultLanguageChanged(this, EventArgs.Empty); }
        Language _DefaultLanguage = Language.ENG_US;
        public string DefaultLanguage
        {
            get { return _DefaultLanguage.ToString(); }
            set
            {
                Language res = (Language)Enum.Parse(typeof(Language), value);
                if (_DefaultLanguage != res)
                {
                    _DefaultLanguage = res;
                    OnDefaultLanguageChanged();
                    if (_ShowDefaultLanguage)
                        OnStringSetsChanged();
                }
            }
        }

        public bool SettingsExportEmptyStringsEnabled { get { return true; } }

        public void DoSettingsExportEmptyStrings() { ExportEmptyStrings = !ExportEmptyStrings; }
        public event EventHandler ExportEmptyStringsChanged;
        protected void OnExportEmptyStringsChanged() { if (ExportEmptyStringsChanged != null) ExportEmptyStringsChanged(this, EventArgs.Empty); }
        bool _ExportEmptyStrings = true;
        public bool ExportEmptyStrings
        {
            get { return _ExportEmptyStrings; }
            set
            {
                if (_ExportEmptyStrings != value)
                {
                    _ExportEmptyStrings = value;
                    OnExportEmptyStringsChanged();
                }
            }
        }
        #endregion

        #region Show Default Language checkbox
        public event EventHandler ShowDefaultLanguageChanged;
        protected void OnShowDefaultLanguageChanged() { if (ShowDefaultLanguageChanged != null) ShowDefaultLanguageChanged(this, EventArgs.Empty); }
        bool _ShowDefaultLanguage;
        public bool ShowDefaultLanguage
        {
            get { return _ShowDefaultLanguage; }
            set
            {
                if (_ShowDefaultLanguage != value)
                {
                    int selectedStringSet = _SelectedStringSet;
                    SelectedStringSet = -1;

                    _ShowDefaultLanguage = value;
                    OnShowDefaultLanguageChanged();
                    OnStringSetsChanged();

                    SelectedStringSet = selectedStringSet;
                }
            }
        }
        #endregion

        #region Add/Delete String buttons
        public bool AddStringEnabled { get { return Model != null && _SelectedStringTableSet >= 0; } }
        public bool DeleteStringEnabled { get { return Model != null && _SelectedStringTableSet >= 0 && _SelectedStringSet >= 0; } }

        public void DoAddString()
        {
            if (!AddStringEnabled) return;

            ulong guid = Package.NewInstance();
            _CurrentStringTableSet[guid, _DefaultLanguage] = "";
            OnStringSetsChanged();
            SelectedStringSet = _CurrentStringTableSet.GUIDs.ToList().IndexOf(guid);
        }

        public void DoDeleteString()
        {
            if (!DeleteStringEnabled) return;

            ulong guid = _CurrentGUID.Value;
            int index = _SelectedStringSet;
            SelectedStringSet = -1;
            if (!_CurrentStringTableSet.Remove(guid))
                SelectedStringSet = index;
            else
            {
                OnStringSetsChanged();
                if (_CurrentStringTableSet.GUIDs.Count() > index)
                    SelectedStringSet = index;
                else
                    SelectedStringSet = index - 1;
            }
        }
        #endregion

        // ---------------------

        #region Filename
        public class StringEventArgs : EventArgs { public string Value { get; set; } }
        public event EventHandler<StringEventArgs> FilenameChanged;
        protected void OnFilenameChanged(string filename) { if (FilenameChanged != null) FilenameChanged(this, new StringEventArgs { Value = filename, }); }
        string _Filename;
        string Filename { get { return _Filename; } set { if (_Filename != value) { _Filename = value; OnFilenameChanged(_Filename); } } }
        bool HasFilename { get { return Filename != null && Filename != ""; } }
        public bool MustSaveAs { get { return !HasFilename; } }

        public bool IsValidFilename(string filename, bool save, out string message)
        {
            return (new StringTableEditorModel.Model()).IsValidFilename(filename, save, out message);
        }
        #endregion

        #region Model
        IModel _model;
        IModel Model
        {
            get { return _model; }
            set
            {
                if (_model != value)
                {
                    bool isDirty = _model == null ? false : _model.IsDirty;
                    _model = value;
                    if (_model != null)
                        _model.IsDirtyChanged += new EventHandler(OnIsDirtyChanged);
                    if (isDirty != (_model == null ? false : _model.IsDirty))
                        OnIsDirtyChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler IsDirtyChanged;
        protected void OnIsDirtyChanged(object sender, EventArgs e) { if (IsDirtyChanged != null) IsDirtyChanged(this, EventArgs.Empty); }
        bool ModelIsDirty { get { return Model != null && Model.IsDirty; } }
        public bool CommitRequired { get { return ModelIsDirty; } }

        bool HasValue { get { return Model != null; } }
        #endregion

        #region Model StringTableSets
        public event EventHandler StringTableSetsChanged;
        protected void OnStringTableSetsChanged() { if (StringTableSetsChanged != null) StringTableSetsChanged(this, EventArgs.Empty); }
        public IEnumerable<string> StringTableSets
        {
            get
            {
                return Model == null ? null : Model.Select(x => x.IID).Select(x => "0x" + x.ToString("X16"));
            }
        }

        public event EventHandler SelectedStringTableSetChanged;
        protected void OnSelectedStringTableSetChanged(object sender, EventArgs e) { if (SelectedStringTableSetChanged != null) SelectedStringTableSetChanged(this, EventArgs.Empty); }
        StringTableSet _CurrentStringTableSet;
        int _SelectedStringTableSet = -1;
        public int SelectedStringTableSet
        {
            get { return _SelectedStringTableSet; }
            set
            {
                if (_SelectedStringTableSet != value)
                {
                    _SelectedGUIDs = new List<ulong>();
                    _ChangedGUIDs = new List<ulong>();

                    SelectedStringSet = -1;

                    _SelectedStringTableSet = value;
                    _CurrentStringTableSet = _SelectedStringTableSet >= 0 ? Model.ElementAtOrDefault(_SelectedStringTableSet) : null;
                    OnSelectedStringTableSetChanged(this, EventArgs.Empty);
                    OnStringSetsChanged();

                    if (_CurrentStringTableSet != null)
                    {
                        System.Collections.IList ss = StringSets as System.Collections.IList;
                        if (ss != null && ss.Count > 0)
                            SelectedStringSet = 0;
                    }
                }
            }
        }
        #endregion

        #region Model languages
        public event EventHandler LanguagesChanged;
        protected void OnLanguagesChanged(object sender, EventArgs e) { if (LanguagesChanged != null) LanguagesChanged(this, EventArgs.Empty); }
        public IEnumerable<string> Languages { get { return Enum.GetNames(typeof(Language)); } }

        public event EventHandler SelectedLanguageChanged;
        protected void OnSelectedLanguageChanged() { if (SelectedLanguageChanged != null) SelectedLanguageChanged(this, EventArgs.Empty); }
        Language? _SelectedLanguage = null;
        public int SelectedLanguage
        {
            get { return _SelectedLanguage.HasValue ? (int)_SelectedLanguage.Value : -1; }
            set
            {
                if (value >= 0 && Enum.IsDefined(typeof(Language), value))
                {
                    if (_SelectedLanguage != (Language)value)
                    {
                        int selectedStringSet = _SelectedStringSet;
                        SelectedStringSet = -1;

                        _SelectedLanguage = (Language)value;
                        OnSelectedLanguageChanged();
                        OnStringSetsChanged();

                        SelectedStringSet = selectedStringSet;
                    }
                }
                else if (_SelectedLanguage.HasValue)
                {
                    SelectedStringSet = -1;
                    _SelectedLanguage = null;
                    OnSelectedLanguageChanged();
                    OnStringSetsChanged();
                }
            }
        }
        #endregion

        #region Model String Sets (GUID/String pairs)
        public event EventHandler StringSetsChanged;
        protected void OnStringSetsChanged() { if (StringSetsChanged != null) StringSetsChanged(this, EventArgs.Empty); }
        List<ulong> _ChangedGUIDs;
        List<ulong> _SelectedGUIDs;
        public System.Collections.IEnumerable StringSets
        {
            get
            {
                if (Model == null || _CurrentStringTableSet == null || !_SelectedLanguage.HasValue) return null;

                System.Collections.IList res = _ShowDefaultLanguage
                    ? (System.Collections.IList)new List<RowFormatCurrentDefault>()
                    : new List<RowFormatCurrent>();

                if (_ShowDefaultLanguage)
                {
                    foreach (ulong guid in _CurrentStringTableSet.GUIDs)
                        res.Add(new RowFormatCurrentDefault(this, guid));
                }
                else
                {
                    foreach (ulong guid in _CurrentStringTableSet.GUIDs)
                        res.Add(new RowFormatCurrent(this, guid));
                }

                return res;
            }
        }
        public class RowFormatCurrent : INotifyPropertyChanged
        {
            protected PresenterController _Parent;
            protected ulong _Guid;
            public RowFormatCurrent(PresenterController parent, ulong guid) { _Parent = parent; _Guid = guid; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string property) { if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(property)); } }

            public bool Changed { get { return _Parent._ChangedGUIDs.Contains(_Guid); } }
            public bool Selected
            {
                get { return _Parent._SelectedGUIDs.Contains(_Guid); }
                set
                {
                    if (_Parent.Model == null || _Parent._CurrentStringTableSet == null) return;

                    if (!_Parent._CurrentStringTableSet.GUIDs.Contains(_Guid)) return;

                    if (value) { if (!_Parent._SelectedGUIDs.Contains(_Guid)) _Parent._SelectedGUIDs.Add(_Guid); }
                    else { if (_Parent._SelectedGUIDs.Contains(_Guid)) _Parent._SelectedGUIDs.Remove(_Guid); }
                    OnPropertyChanged("Selected");
                }
            }
            public string Guid { get { return "0x" + _Guid.ToString("X16"); } }
            public string Current
            {
                get
                {
                    if (_Parent.Model == null || _Parent._CurrentStringTableSet == null || !_Parent._SelectedLanguage.HasValue) return null;
                    return _Parent._CurrentStringTableSet[_Guid, _Parent._SelectedLanguage.Value];
                }
                set
                {
                    if (_Parent.Model == null || _Parent._CurrentStringTableSet == null || !_Parent._SelectedLanguage.HasValue) return;

                    if (_Parent._CurrentStringTableSet[_Guid, _Parent._SelectedLanguage.Value] != value)
                    {
                        _Parent._CurrentStringTableSet[_Guid, _Parent._SelectedLanguage.Value] = value;
                        OnPropertyChanged("Current");

                        if (!_Parent._ChangedGUIDs.Contains(_Guid))
                        {
                            _Parent._ChangedGUIDs.Add(_Guid);
                            OnPropertyChanged("Changed");
                        }
                    }
                }
            }
        }
        public class RowFormatCurrentDefault : RowFormatCurrent
        {
            public RowFormatCurrentDefault(PresenterController parent, ulong _Guid) : base(parent, _Guid) { }

            public string Default
            {
                get
                {
                    if (_Parent.Model == null || _Parent._CurrentStringTableSet == null) return null;
                    return _Parent._CurrentStringTableSet[_Guid, _Parent._DefaultLanguage];
                }
            }
        }

        public event EventHandler SelectedStringSetChanged;
        protected void OnSelectedStringSetChanged(object sender, EventArgs e) { if (SelectedStringSetChanged != null) SelectedStringSetChanged(this, EventArgs.Empty); }
        int _SelectedStringSet = -1;
        public int SelectedStringSet
        {
            get { return _SelectedStringSet; }
            set
            {
                if (_SelectedStringSet != value)
                {
                    _SelectedStringSet = value;
                    OnSelectedStringSetChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion
    }
}
