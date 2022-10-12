using System;
using System.Reflection;

namespace MTool.ServiceLocation.Runtime
{
    public partial class ServiceContainer
    {
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		#endregion
		
		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		#endregion
		
		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		#endregion
		
		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

        public void Register<T>(string key , object instance)
        {
            ThrowIfDispose();
            Register(typeof(T) , key , instance);
        }

        public void Unregister<T>()
        {
            var serviceType = typeof(T);
            ServiceRegistration registration = new ServiceRegistration(serviceType, null);
            if (this._registry.ContainsKey(registration))
                this._registry.Remove(registration);
        }

        public void Register(Type serviceType, string key, object instance)
        {
            ThrowIfDispose();
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            if (instance == null)
                throw new ArgumentNullException("instance");

            if (!serviceType.IsInstanceOfType(instance))
            {
                throw new InvalidOperationException($"The instance(name : {instance.GetType().Name}) is not assignable to the serviceType (name : {serviceType.Name}) reference.");
            }

            ServiceRegistration registration = new ServiceRegistration(serviceType , key);

            if (this._registry.TryGetValue(registration,out var entry))
            {
                throw new InvalidOperationException($"The service that type is \"{serviceType.Name}\" is already registed in container .");
            }

            object CreateInstance(ServiceContainer container) => instance;

            RegisterInternal(registration,CreateInstance);
        }


        public void Register<T>(string key , Func<T> create)
        {
            ThrowIfDispose();
            if (create == null)
                throw new ArgumentNullException("create");

            var serviceType = typeof(T);

            ServiceRegistration registration = new ServiceRegistration(serviceType, key);

            if (this._registry.TryGetValue(registration, out var entry))
            {
                throw new InvalidOperationException($"The service that type is \"{serviceType.Name}\" is already registed in container .");
            }

            object CreateInstanceFunc(ServiceContainer container) => create();

            RegisterInternal(registration , CreateInstanceFunc);
        }


        private void RegisterInternal(ServiceRegistration registration , Func<ServiceContainer, object> createInstance)
        {
            var entry = new ServiceEntry(createInstance);

            this._registry[registration] = entry;
        }

        #endregion

    }
}
