using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using s3pi.Interfaces;

namespace StringTableEditorModel
{
    public class NameMap
    {
        public const uint NMAPResourceType = 0x0166038C;
        static bool MatchNMAPResourceType(IResourceKey rk) { return rk.ResourceType == NMAPResourceType; }

        Resource resource;
        NameMapResource.NameMapResource _nmap { get { return resource.IResource as NameMapResource.NameMapResource; } }
        Package _package;

        public event EventHandler ResourceChanged;
        protected void OnResourceChanged(object sender, EventArgs e) { if (ResourceChanged != null) ResourceChanged(sender, e); }

        private NameMap() { }

        public static NameMap New(Package _package, ulong? iid = null)
        {
            NameMap nm = new NameMap();

            nm.resource = _package.NewResource(NMAPResourceType, 0, iid.HasValue ? iid.Value : Package.NewInstance());
            nm._package = _package;

            return nm;
        }

        public static NameMap Load(Package _package)
        {
            NameMap nm = new NameMap();

            nm.resource = _package.FindResource(MatchNMAPResourceType);
            nm._package = _package;

            return nm.resource.IResource == null ? null : nm;
        }

        public void Commit()
        {
            _package.SaveResource(resource);
        }

        public void Add(ulong iid, string name)
        {
            _nmap.Add(iid, name);
        }

        public string this[ulong iid]
        {
            get
            {
                if (_nmap.ContainsKey(iid))
                    return _nmap[iid];
                else
                    return null;
            }
            set
            {
                if (_nmap.ContainsKey(iid))
                    _nmap[iid] = value;
                else
                    Add(iid, value);
            }
        }
    }
}
