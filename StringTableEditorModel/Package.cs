using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using s3pi.Interfaces;
using s3pi.WrapperDealer;

namespace StringTableEditorModel
{
    public class Package
    {
        IPackage _package;
        string _filename;

        NameMap _nmap = null;
        NameMap nmap
        {
            get
            {
                if (_nmap == null && _package != null)
                    _nmap = NameMap.Load(this);

                return _nmap;
            }
        }

        public event EventHandler ResourceChanged;
        protected void OnResourceChanged(object sender, EventArgs e) { if (ResourceChanged != null) ResourceChanged(sender, e); }

        private Package() { }

        public static Package New()
        {
            Package p = new Package();
            p._package = s3pi.Package.Package.NewPackage(0);
            return p;
        }

        public static Package Open(string path)
        {
            Package p = new Package();
            p._package = s3pi.Package.Package.OpenPackage(0, path, true);
            p._filename = path;
            return p;
        }

        public void Save()
        {
            if (_nmap != null)
                _nmap.Commit();

            _package.SavePackage();
        }

        public void SaveAs(string path)
        {
            if (_nmap != null)
                _nmap.Commit();

            if (_package != null)
                _package.SaveAs(path);
        }

        public void Close()
        {
            s3pi.Package.Package.ClosePackage(0, _package);
            _package = null;
            _filename = null;
            _nmap = null;
        }

        public string Filename { get { return _filename; } }

        public Resource NewResource(uint resourceType, uint resourceGroup, ulong instance, string name = null)
        {
            IResource res = WrapperDealer.CreateNewResource(0, "0x" + resourceType.ToString("X8"));
            if (res == null) return default(Resource);

            IResourceIndexEntry rie = _package.AddResource(new ResourceKey { ResourceType = resourceType, ResourceGroup = resourceGroup, Instance = instance }, res.Stream, true);

            if (name != null)
                ResourceName(rie.Instance, name);

            return GetResource(rie);
        }

        public Resource GetResource(uint resourceType, uint resourceGroup, ulong instance)
        {
            IResourceIndexEntry rie = _package.Find(x =>
                x.ResourceType == resourceType && x.ResourceGroup == resourceGroup && x.Instance == instance);
            if (rie == null) return default(Resource);

            return GetResource(rie);
        }

        public Resource GetResource(IResourceIndexEntry rie)
        {
            IResource res = WrapperDealer.GetResource(0, _package, rie);
            if (res != null)
                res.ResourceChanged += new EventHandler(OnResourceChanged);
            return new Resource { IResourceIndexEntry = rie, IResource = res };
        }

        public static ulong NewInstance()
        {
            DateTime now = DateTime.UtcNow;
            string name =
                now.Millisecond.ToString("X") +
                now.Second.ToString("X") + now.Minute.ToString("X") +
                now.Hour.ToString("X") + now.ToString("tt") +
                now.ToString("dddd") + now.Day.ToString("X") +
                now.ToString("MMMM") + now.Month.ToString("X") +
                now.Year.ToString("X") + now.ToString("g") +
                ""
                ;
            return FNV64.GetHash(name);
        }

        public string ResourceName(Resource resource) { return nmap == null ? null : nmap[resource.IResourceIndexEntry.Instance]; }

        public void ResourceName(Resource resource, string name) { ResourceName(resource.IResourceIndexEntry.Instance, name); }

        public void ResourceName(ulong iid, string name)
        {
            if (nmap == null && _package != null)
                _nmap = NameMap.New(this, NewInstance());

            if (nmap != null)
                nmap.Add(iid, name);
        }

        public Resource FindResource(Predicate<IResourceKey> Match)
        {
            IResourceIndexEntry rie = _package.Find(Match);
            if (rie == null) return default(Resource);
            return GetResource(rie);
        }

        public List<IResourceIndexEntry> FindAll(Predicate<IResourceKey> Match)
        {
            return _package.FindAll(Match);
        }

        public void SaveResource(Resource resource)
        {
            _package.ReplaceResource(resource.IResourceIndexEntry, resource.IResource);
        }
    }
}
