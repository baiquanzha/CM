using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using MTool.LoggerModule.Runtime;

namespace MTool.ServiceLocation.Runtime
{
    public partial class ServiceContainer : IServiceLocator , IDisposable
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static Lazy<ILogger> s_mLogger = new Lazy<ILogger>(()=>LoggerManager.GetLogger("ServiceContainer"));

        private const int _defaultCapacity = 4;

        private readonly Dictionary<ServiceRegistration, ServiceEntry> _registry;

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public bool IsDisposed { private set; get; }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public ServiceContainer()
        {
            this._registry = new Dictionary<ServiceRegistration, ServiceEntry>(_defaultCapacity);
            this.IsDisposed = false;
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------


        private void ThrowIfDispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        public object GetService(Type serviceType)
        {
            ThrowIfDispose();
            return GetInstance(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            ThrowIfDispose();
            return GetInstance(serviceType,null);
        }

        public object GetInstance(Type serviceType, string key)
        {
            ThrowIfDispose();
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            return GetInstanceInternal(serviceType , key);
        }

        private object GetInstanceInternal(Type serviceType , string key)
        {
            var registration = new ServiceRegistration(serviceType,key);
            if (this._registry.TryGetValue(registration, out var entry))
            {
                return entry.Instances?? entry.CreateInstance(this);
            }

            s_mLogger.Value?.Warn($"The instance you want to get is not registed in the current container .");

            return null;
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            ThrowIfDispose();
            var registrations = GetServiceRegistrations(serviceType);

            var results = new List<object>();
            foreach (var registration in registrations)
            {
                var entry = _registry[registration];

                results.Add(entry.Instances??entry.CreateInstance(this));
            }

            return results;
        }

        public TService GetInstance<TService>()
        {
            ThrowIfDispose();
            return (TService)GetInstance(typeof(TService));
        }

        public TService GetInstance<TService>(string key)
        {
            ThrowIfDispose();
            return (TService)GetInstance(typeof(TService),key);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            ThrowIfDispose();
            var registrations = GetServiceRegistrations<TService>();

            var results = new List<TService>();
            foreach (var registration in registrations)
            {
                var entry = _registry[registration];
                var service = entry.Instances ?? entry.CreateInstance(this);
                results.Add((TService)service);
            }

            return results;
        }

        private IEnumerable<ServiceRegistration> GetServiceRegistrations<TService>()
        {
            return GetServiceRegistrations(typeof(TService));
        }

        private IEnumerable<ServiceRegistration> GetServiceRegistrations(Type serviceType)
        {
            return _registry.Keys.Where(x=>x.Type == serviceType).ToArray();
        }

        public void Dispose()
        {
            foreach (var kvp in _registry)
            {
                var entry = kvp.Value;
                var instance = entry.Instances;
                if (instance != null && instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _registry.Clear();

            IsDisposed = true;
        }

        #endregion



    }
}
