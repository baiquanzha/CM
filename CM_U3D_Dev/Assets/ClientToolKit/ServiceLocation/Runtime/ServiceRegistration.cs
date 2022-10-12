using System;

namespace MTool.ServiceLocation.Runtime
{
#if ENABLE_NUNIT_TEST
    public struct ServiceRegistration : IEquatable<ServiceRegistration>
#else
    internal class ServiceRegistration : IEquatable<ServiceRegistration>
#endif
    {
        //--------------------------------------------------------------
#region Fields
        //--------------------------------------------------------------


        public readonly Type Type;

        public readonly string Key;

#endregion

        //--------------------------------------------------------------
#region Properties & Events
        //--------------------------------------------------------------
#endregion

        //--------------------------------------------------------------
#region Creation & Cleanup
        //--------------------------------------------------------------

        public ServiceRegistration(Type type , string key)
        {
            this.Type = type;
            this.Key = key;

            this.ThrowIfInvalid();
        }

#endregion

        //--------------------------------------------------------------
#region Methods
        //--------------------------------------------------------------

        private void ThrowIfInvalid()
        {
            if (this.Type == null && string.IsNullOrEmpty(Key))
            {
                if(Type == null)
                    throw new ArgumentException($"Type");
                if (Key == null)
                    throw new ArgumentException($"Key");
            }
        }

        public override bool Equals(object obj)
        {
            return obj is ServiceRegistration && Equals((ServiceRegistration)obj);
        }

        public bool Equals(ServiceRegistration other)
        {
            return other == this;
        }

        public static bool operator ==(ServiceRegistration arg0, ServiceRegistration arg1)
        {
            return arg0.Type == arg1.Type && arg0.Key == arg1.Key;
        }

        public static bool operator !=(ServiceRegistration arg0, ServiceRegistration arg1)
        {
            return !(arg0 == arg1);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;

            if (Type != null)
            {
                hashCode = Type.GetHashCode();
            }

            if(Key != null)
            {
                hashCode ^= Key.GetHashCode();
            }
            return hashCode;
        }


        public override string ToString()
        {
            this.ThrowIfInvalid();

            if (Type == null)
            {
                return $"Key : {Key} , type is null .";
            }

            if (string.IsNullOrEmpty(Key))
            {
                return $"Type : {Type.Name} , key is null .";
            }
            return $"Type : {Type.Name} , key : {Key}.";
        }
#endregion


    }
}
