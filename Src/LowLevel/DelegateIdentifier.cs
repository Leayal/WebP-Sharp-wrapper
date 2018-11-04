using System;
using System.Collections.Generic;

namespace WebPWrapper.WPF.LowLevel
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
            if (obj is DelegateIdentifier identifier)
            {
                return this.Equals(identifier);
            }
            return false;
        }

        public bool Equals(DelegateIdentity other)
        {
            return ((this.DelegateType == other.DelegateType) &&
                   (this.FunctionName == other.FunctionName));
        }

        public override string ToString()
        {
            return this.FunctionName + ';' + this.DelegateType.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = 1328913212;
            hashCode = hashCode * -1521134295 + this.DelegateType.GetHashCode();
            hashCode = hashCode * -1521134295 + this.FunctionName.GetHashCode();
            return hashCode;
        }
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

        public int GetHashCode(DelegateIdentity obj)
        {
            return obj.GetHashCode();
        }
    }
}
