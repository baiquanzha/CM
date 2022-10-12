using System;
using System.Collections.Generic;

namespace MTool.Framework
{
    public static class FrameworkEntry
    {
        private static List<FrameworkModule> frameworkModules = new List<FrameworkModule>(16);

        public static void Update(float elapseTime, float realElapseTime)
        {
            foreach (FrameworkModule module in frameworkModules)
            {
                module.Update(elapseTime, realElapseTime);
            }
        }

        public static void LateUpdate()
        {
            foreach (FrameworkModule module in frameworkModules)
            {
                module.LateUpdate();
            }
        }

        public static void ShutDown()
        {
            foreach (FrameworkModule module in frameworkModules)
            {
                module.Shutdown();
            }
            frameworkModules.Clear();
        }

        public static T GetModule<T>() where T : FrameworkModule
        {
            Type type = typeof(T);
            foreach (FrameworkModule module in frameworkModules)
            {
                if (module.GetType() == type)
                {
                    return module as T;
                }
            }
            return CreateModule<T>();
        }

        private static T CreateModule<T>() where T : FrameworkModule
        {
            Type type = typeof(T);
            T module = Activator.CreateInstance<T>();
            frameworkModules.Add(module);
            module.Init();
            return module;
        }
    }
}