using System;
using s3pi.Interfaces;

namespace StringTableEditorModel
{
    public class ResourceKey : IResourceKey
    {
        public ulong Instance { get; set; }
        public uint ResourceGroup { get; set; }
        public uint ResourceType { get; set; }

        public bool Equals(IResourceKey x, IResourceKey y) { return x.CompareTo(y) == 0; }

        public int GetHashCode(IResourceKey obj) { return (int)ResourceType ^ (int)ResourceGroup ^ (int)(Instance >> 32) ^ (int)(Instance & 0xFFFFFFFF); }

        public bool Equals(IResourceKey other) { return this.CompareTo(other) == 0; }

        public int CompareTo(IResourceKey other)
        {
            int res = ResourceType.CompareTo(other.ResourceType); if (res != 0) return res;
            res = ResourceGroup.CompareTo(other.ResourceGroup); if (res != 0) return res;
            return Instance.CompareTo(other.Instance);
        }
    }
}
