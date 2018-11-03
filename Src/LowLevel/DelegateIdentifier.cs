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
            return Equals(obj as DelegateIdentity);
        }

        public bool Equals(DelegateIdentity other)
        {
            return ((other != null) &&
                   (this.DelegateType == other.DelegateType) &&
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

        public static bool operator ==(DelegateIdentity identity1, DelegateIdentity identity2)
        {
            if (identity1 != null)
            {
                return identity1.Equals(identity2);
            }
            else if (identity2 != null)
            {
                return identity2.Equals(identity1);
            }
            else
            {
                return true;
            }
        }

        public static bool operator !=(DelegateIdentity identity1, DelegateIdentity identity2)
        {
            return !(identity1 == identity2);
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
