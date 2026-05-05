using System.ComponentModel;
using HuntingDog;

namespace HuntingDog.Config
{
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string _key;
        public LocalizedDisplayNameAttribute(string key) : base(key) { _key = key; }
        public override string DisplayName => Loc.T(_key);
    }

    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _key;
        public LocalizedDescriptionAttribute(string key) : base(key) { _key = key; }
        public override string Description => Loc.T(_key);
    }
}
