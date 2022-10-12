namespace MTool.Framework
{
    public abstract class FrameworkModule
    {
        internal abstract void Init();

        internal abstract void Update(float elapseTime, float realElapseTime);

        internal abstract void LateUpdate();

        internal abstract void Shutdown();
    }
}