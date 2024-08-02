using System.Reflection;
using FGR.Common.Attributes;
using FGR.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FGR.Common.Implementation
{
    public static class RepositoryImplementator
    {
        public static IServiceCollection AddPlainRepositories(
            this IServiceCollection services,
            IEnumerable<Type> interfaces,
            Assembly implemetingAssembly,
            Func<Type, Type, Type> implementationFunc)
        {
            ActionPlainRepositories(services, interfaces, implemetingAssembly, implementationFunc);
            return services;
        }

        public static void ActionPlainRepositories(
            IServiceCollection services,
            IEnumerable<Type> interfaces,
            Assembly implemetingAssembly,
            Func<Type, Type, Type> implementationFunc)
        {
            var implementableInterfaces = interfaces
                ?.Where(i => i.CustomAttributes
                ?.Any(a => a.AttributeType.Equals(typeof(RepositoryInterfaceAttribute)))
                ?? false);

            var implementingTypes = implemetingAssembly.GetTypes()
                ?.Where(t => t.GetInterfaces()
                    ?.Any(i => implementableInterfaces?.Contains(i) ?? false)
                    ?? false)
                ?.SelectMany(t => t.GetInterfaces()
                        ?.Where(i => implementableInterfaces?.Contains(i) ?? false)
                        ?.Select(e => ((Type i, Type t)?)(i: e, t))
                        ?? [])
                ?.Where(e => e != null);

            implementableInterfaces
                ?.ToList()
                ?.ForEach(i => services.AppendImplementation(i, implementationFunc, implementingTypes));
        }

        private static Type? GetTypeByItInterface(Type i, IEnumerable<(Type i, Type t)?>? interfaceAndTypePairs) =>
            interfaceAndTypePairs?.Where(p => p?.i == i)?.FirstOrDefault()?.t;

        private static void AppendImplementation(
            this IServiceCollection services,
            Type interfaceToImplement,
            Func<Type, Type, Type> implementationFunc,
            IEnumerable<(Type i, Type t)?>? interfaceAndTypePairs)
        {
            var implementationType = GetTypeByItInterface(interfaceToImplement, interfaceAndTypePairs);
            if (implementationType is null) return;
            var repositoryImplementation = implementationFunc(interfaceToImplement, implementationType);
            services.AddImplementedType(interfaceToImplement, implementationType);
            services.AddImplementedRepositoryType(interfaceToImplement, repositoryImplementation);
        }

        private static void AddImplementedType(
            this IServiceCollection services,
            Type interfaceToImplement,
            Type implementation) => services.AddScoped(interfaceToImplement, implementation);

        private static void AddImplementedRepositoryType(
            this IServiceCollection services,
            Type interfaceToImplement,
            Type implementation)
        {
            var baseInterfaceType = typeof(IRepository<>);
            Type[] typeArgs = [interfaceToImplement];
            var implementedInterface = baseInterfaceType.MakeGenericType(typeArgs);

            services.AddScoped(implementedInterface, implementation);
            if (implementedInterface.GetInterfaces().Contains(typeof(IRepository)))
                services.AddScoped(typeof(IRepository), implementation);
        }
    }
}
