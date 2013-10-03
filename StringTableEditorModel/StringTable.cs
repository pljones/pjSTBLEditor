using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using s3pi.Interfaces;

namespace StringTableEditorModel
{
    public class StringTable : IEnumerable<KeyValuePair<ulong, string>>
    {
        public const uint STBLResourceType = 0x220557DA;
        static bool MatchSTBLResourceType(IResourceKey rk) { return rk.ResourceType == STBLResourceType; }

        Resource resource;
        StblResource.StblResource _stbl { get { return resource.IResource as StblResource.StblResource; } }
        Package _package;

        public event EventHandler ResourceChanged;
        protected void OnResourceChanged(object sender, EventArgs e) { if (ResourceChanged != null) ResourceChanged(sender, e); }

        private StringTable() { }

        public static StringTable New(Package _package, Language language, ulong? iid = null, string name = null)
        {
            StringTable st = new StringTable();

            ulong liid = (iid.HasValue ? iid.Value : (Package.NewInstance() >> 8)) | (((ulong)language) << 56);
            if (name == null) name = "Strings_" + language.ToString() + "_" + liid.ToString("x16");

            st.resource = _package.NewResource(STBLResourceType, 0, liid, name);
            st._package = _package;

            return st;
        }

        public static StringTable Load(Package _package, Language language, ulong iid)
        {
            StringTable st = new StringTable();

            ulong liid = iid | (((ulong)language) << 56);
            st.resource = _package.FindResource(x => x.ResourceType == STBLResourceType && x.Instance == liid);
            if (st.resource.IResource != null)
            {
                st.resource.IResource.ResourceChanged += new EventHandler(st.OnResourceChanged);
                st._package = _package;
                return st;
            }

            return null;
        }

        public void Commit()
        {
            _package.SaveResource(resource);
        }

        public void Add(ulong guid, string value)
        {
            _stbl.Add(guid, value);
        }

        public bool Remove(ulong guid)
        {
            return _stbl.Remove(guid);
        }

        public string this[ulong guid]
        {
            get
            {
                if (_stbl.ContainsKey(guid))
                    return _stbl[guid];
                else
                    return null;
            }
            set
            {
                if (_stbl.ContainsKey(guid))
                    _stbl[guid] = value;
                else
                    Add(guid, value);
            }
        }

        public ulong? Match(string needle)
        {
            foreach (var kvp in _stbl)
            {
                if (kvp.Value.IndexOf(needle) != -1)
                    return kvp.Key;
            }
            return null;
        }

        public ulong? Match(Regex needle)
        {
            foreach (var kvp in _stbl)
            {
                Match match = needle.Match(kvp.Value);
                if (match.Success)
                    return kvp.Key;
            }
            return null;
        }

        public Language Language { get { return (Language)(resource.IResourceIndexEntry.Instance >> 56); } }

        #region IEnumerable<KeyValuePair<ulong, string>>
        public IEnumerator<KeyValuePair<ulong, string>> GetEnumerator()
        {
            return _stbl.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_stbl).GetEnumerator();
        }
        #endregion
    }
}
