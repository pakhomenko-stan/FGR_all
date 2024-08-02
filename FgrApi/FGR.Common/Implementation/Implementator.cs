using System.Reflection;
using FGR.Common.Attributes;
using FGR.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FGR.Common.Implementation
{
    public static class Implementator
    {
        public static IServiceCollection AddImplementable(this IServiceCollection services, Assembly assembly, string implementableType)
        {
            var implementable = assembly.GetTypes()
                .Where(t => t.CustomAttributes?.Any(a => a.AttributeType.Equals(typeof(ImplementableAttribute))) ?? false);
            var implementableServices = implementable.Where(i => i.GetTypeInfo().CustomAttributes
                    ?.Any(a => a.ConstructorArguments
                        ?.Any(arg => arg.Value?.ToString()?.Equals(implementableType) ?? false) ?? false
                    ) ?? false);
            implementableServices.ToList().ForEach(s => services.Implements(s, implementableType));
            return services;
        }

        public static Func<string, object[], I> GeneralActivator<I>(IServiceProvider s, Assembly assembly) where I : class =>
            (typName, pars) => (I)ActivatorUtilities.CreateInstance(s, GetType<I>(typName, assembly), pars);

        private static Type GetType<I>(string className, Assembly assembly)
        {
            var assemblyTypes = assembly.GetTypes();
            var inheritingTypes = assemblyTypes.Where(t => t.GetInterfaces()
                ?.Where(i => i.FullName == typeof(I).FullName)
                ?.Any() ?? false);
            var typ = inheritingTypes.Where(t => t?.FullName?.Contains(className) ?? false)?.FirstOrDefault();
            if (typ == default) throw new Exception($"Type {className} is not available in current assembly");
            return typ;
        }

        private static void Implements(this IServiceCollection services, Type type, string implementableType)
        {
            var interfacesPreselect = type.GetTypeInfo().ImplementedInterfaces;
            var interfaces = GetInterfaces(interfacesPreselect, implementableType);
            var mainInterface = interfaces?.FirstOrDefault() ?? default;
            if (mainInterface == default) services.AddScoped(type);
            else
            {
                services.AddScoped(mainInterface, type);
            }

            if (type.GetInterfaces().Contains(typeof(IRepository)))
                services.AddScoped(typeof(IRepository), type);
        }

        private static IEnumerable<Type> GetInterfaces(IEnumerable<Type> source, string implementableType)
        {
            if (implementableType != ImplementableType.Repository) return source;
            var attrType = typeof(CustomRepositoryInterfaceAttribute);
            var interfaces = source.Where(i => i.CustomAttributes?.Any(a => a.AttributeType == attrType) ?? false);
            return interfaces;
        }
    }
}
