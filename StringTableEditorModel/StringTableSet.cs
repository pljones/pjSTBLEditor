using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StringTableEditorModel
{
    public class StringTableSet
    {
        static ulong defaultGUID = System.Security.Cryptography.FNV64.GetHash("default");

        Dictionary<Language, StringTable> _stringTables = new Dictionary<Language, StringTable>();
        Dictionary<ulong, Dictionary<Language, string>> _stringTableSet = new Dictionary<ulong, Dictionary<Language, string>>();
        ulong _iid;
        Package _package;

        public event EventHandler ResourceChanged;
        protected void OnResourceChanged(object sender, EventArgs e) { if (ResourceChanged != null) ResourceChanged(sender, e); }

        private StringTableSet() { }
        private StringTableSet(Package package, ulong iid) { _iid = iid; _package = package; }

        public static StringTableSet New(Package package)
        {
            StringTableSet sts = new StringTableSet(package, Package.NewInstance() >> 8);

            return sts;
        }

        public static StringTableSet Load(Package package, ulong iid)
        {
            if ((iid >> 56) != 0)
                throw new ArgumentException("Instance ID must not contain language to load StringTableSet");

            StringTableSet sts = new StringTableSet(package, iid);
            
            foreach (Language l in Enum.GetValues(typeof(Language)))
            {
                StringTable stl = StringTable.Load(package, l, iid);

                if (stl != null)
                {
                    stl.ResourceChanged += new EventHandler(sts.OnResourceChanged);
                    sts.Add(l, stl);
                }
            }

            return sts;
        }

        public void Commit()
        {
            foreach (StringTable st in this._stringTables.Values)
                st.Commit();
        }

        public static List<ulong> FindAll(Package package)
        {
            List<ulong> seen = new List<ulong>();

            foreach (var rie in package.FindAll(x => x.ResourceType == StringTable.STBLResourceType))
            {
                ulong iid = rie.Instance & 0x00FFFFFFFFFFFFFF;
                if (!seen.Contains(iid))
                    seen.Add(iid);
            }

            return seen;
        }

        public string this[ulong guid, Language language]
        {
            get
            {
                if (_stringTableSet.ContainsKey(guid))
                {
                    if (_stringTableSet[guid].ContainsKey(language))
                    {
                        return _stringTableSet[guid][language];
                    }
                }
                return null;
            }
            set
            {
                if (_stringTableSet.ContainsKey(guid) && _stringTableSet[guid].ContainsKey(language))
                {
                    if (_stringTableSet[guid][language] != value)
                    {
                        _stringTableSet[guid][language] = value;
                        _stringTables[language][guid] = value;
                    }
                }
                else
                {
                    Add(guid, language, value);
                }
            }
        }

        public void Add(Language language, StringTable stringTable)
        {
            stringTable.ResourceChanged += new EventHandler(OnResourceChanged);
            _stringTables.Add(language, stringTable);
            foreach (KeyValuePair<ulong, string> kvp in stringTable)
                this[kvp.Key, stringTable.Language] = kvp.Value;
        }

        public void Add(ulong guid, Language language, string value)
        {
            if (!_stringTableSet.ContainsKey(guid))
            {
                _stringTableSet.Add(guid, new Dictionary<Language, string>());
            }
            if (!_stringTableSet[guid].ContainsKey(language))
            {
                _stringTableSet[guid].Add(language, value);
            }
            else
                _stringTableSet[guid].Add(language, value);

            if (!_stringTables.ContainsKey(language))
            {
                StringTable stl = StringTable.New(_package, language, _iid);
                stl.ResourceChanged += new EventHandler(OnResourceChanged);
                _stringTables.Add(language, stl);
            }
            _stringTables[language][guid] = value;
        }

        public bool Remove(ulong guid)
        {
            bool removed = false;

            if (_stringTableSet.ContainsKey(guid))
                removed |= _stringTableSet.Remove(guid);

            foreach (StringTable stl in _stringTables.Values)
                removed |= stl.Remove(guid);

            return removed;
        }

        public ulong IID { get { return _iid; } }

        public IEnumerable<Language> Languages
        {
            get
            {
                return _stringTables.Keys;
            }
        }

        public IEnumerable<ulong> GUIDs { get { return _stringTableSet.Keys; } }
    }
}
