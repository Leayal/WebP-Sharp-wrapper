using System;
using System.Collections.Generic;
using System.Reflection;

namespace WebPWrapper.LowLevel
{
    class DelegateIdentity : IEquatable<DelegateIdentity>
    {
        public Type DelegateType { get; }
        public string FunctionName { get; }

        public DelegateIdentity(Type _delegatetype, string _functionName)
        {
            this.DelegateType = _delegatetype;
            this.FunctionName = _functionName;
        }

        public override bool Equals(object obj)
        {
            if (obj is DelegateIdentity identifier)
            {
                return this.Equals(identifier);
            }
            return false;
        }

        public bool Equals(DelegateIdentity other) => ((this.DelegateType == other.DelegateType) && (this.FunctionName.Equals(other.FunctionName)));

        public override string ToString() => this.FunctionName + ';' + this.DelegateType.ToString();

        public override int GetHashCode() => (this.DelegateType.GetHashCode() ^ this.FunctionName.GetHashCode());
    }

    class DelegateIdentifier : IEqualityComparer<DelegateIdentity>
    {
        private StringComparer functionNameComparer;
        public DelegateIdentifier(StringComparer comparer)
        {
            this.functionNameComparer = comparer;
        }

        public DelegateIdentifier() : this(StringComparer.Ordinal) { }

        public bool Equals(DelegateIdentity x, DelegateIdentity y)
        {
            if (functionNameComparer.Equals(x.FunctionName, y.FunctionName))
                if (x.DelegateType == y.DelegateType)
                    return true;
            return false;
        }

        public int GetHashCode(DelegateIdentity obj) => obj.GetHashCode();
    }
}
