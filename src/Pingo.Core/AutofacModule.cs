using Autofac;

namespace Pingo.Core
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Global>().SingleInstance();
        }
    }
}
