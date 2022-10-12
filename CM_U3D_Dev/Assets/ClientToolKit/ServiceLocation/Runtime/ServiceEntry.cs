using System;

namespace MTool.ServiceLocation.Runtime
{
#if ENABLE_NUNIT_TEST
    public sealed class ServiceEntry
#else
    internal class ServiceEntry
#endif
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public Func<ServiceContainer, object> CreateInstanceFunc { private set; get; }
        public object Instances { private set; get; }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public ServiceEntry(Func<ServiceContainer,object> crateInstance)
        {
            this.CreateInstanceFunc = crateInstance;
            this.ThrowIfInvalid();
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void ThrowIfInvalid()
        {
            if (CreateInstanceFunc == null)
            {
                if (CreateInstanceFunc == null)
                    throw new ArgumentException($"CreateInstanceFunc");
            }
        }

        public object CreateInstance(ServiceContainer container)
        {
            return this.Instances = CreateInstanceFunc?.Invoke(container);
        }

        #endregion

    }
}

