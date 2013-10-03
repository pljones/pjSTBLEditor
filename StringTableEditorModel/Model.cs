using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using s3pi.Interfaces;
using StblResource;

namespace StringTableEditorModel
{
    public class Model : List<StringTableSet>, IModel
    {
        #region Package
        Package _package;

        public event EventHandler IsDirtyChanged;
        protected void OnIsDirtyChanged() { if (IsDirtyChanged != null) IsDirtyChanged(this, EventArgs.Empty); }

        bool _isDirty;
        public bool IsDirty { get { return _isDirty; } private set { if (_isDirty != value) { _isDirty = value; OnIsDirtyChanged(); } } }

        void _package_ResourceChanged(object sender, EventArgs e) { IsDirty = true; }

        public bool IsValidFilename(string filename, bool save, out string message)
        {
            if (filename == null) { message = "Invalid filename supplied - null"; return false; }

            if (filename.Length == 0) { message = "Invalid filename supplied - zero length"; return false; }

            try
            {
                string full = Path.GetFullPath(filename);
            }
            catch (Exception e)
            {
                message = "There was a serious problem trying to access the file.\n\n" + e.ToString();
                return false;
            }

            if (!save)
            {
                try
                {
                    Model test = new Model();
                    test.Open(filename);

                    int numSTBLs = test.Count;

                    test.Close();

                    if (numSTBLs == 0)
                    {
                        message = "The selected package contains no String Table Sets.";
                        return false;
                    }
                }
                catch (InvalidDataException e)
                {
                    message = "The selected file did not appear to be a valid Sims3 package.\n" +
                        "This could be because it is a protected file ('DBPP'),\n" +
                        "because it is a Sims2 or Spore package (major version below '2')\n" +
                        "or because the package is corrupt.\n\n" + e.Message;
                    return false;
                }
                catch (Exception e)
                {
                    message = "There was a serious problem trying to read the file.\n\n" + e.ToString();
                    return false;
                }
            }

            message = "";
            return true;
        }

        public void New()
        {
            _package = Package.New();
            _package.ResourceChanged += new EventHandler(_package_ResourceChanged);

            IsDirty = false;
        }

        public void Open(string filename)
        {
            _package = Package.Open(filename);
            _package.ResourceChanged += new EventHandler(_package_ResourceChanged);

            foreach (ulong iid in StringTableSet.FindAll(_package))
                this.Add(StringTableSet.Load(_package, iid));

            IsDirty = false;
        }

        public void Close()
        {
            Clear();
            if (_package != null)
            {
                _package.ResourceChanged -= new EventHandler(_package_ResourceChanged);
                _package.Close();
                _package = null;
            }
            IsDirty = false;
        }

        public void Save()
        {
            foreach (StringTableSet sts in this)
                sts.Commit();

            if (_package != null)
                _package.Save();
            IsDirty = false;
        }

        public void SaveAs(string filename)
        {
            foreach (StringTableSet sts in this)
                sts.Commit();

            if (_package != null)
                _package.SaveAs(filename);
        }

        public string Filename
        {
            get { return _package == null ? null : _package.Filename; }
        }
        #endregion

        public IEnumerable<Language> Languages(ulong iid)
        {
            StringTableSet sts = this[iid];
            return sts == null ? null : sts.Languages;
        }

        public void Add(ulong iid)
        {
            if (this[iid] == null)
                // "Load" creates a new StringTableSet instance that's empty if there aren't any of that iid in the package.
                this.Add(StringTableSet.Load(_package, iid));
        }

        public StringTableSet this[ulong iid] { get { return this.Find(x => x.IID == iid); } }
    }
}
