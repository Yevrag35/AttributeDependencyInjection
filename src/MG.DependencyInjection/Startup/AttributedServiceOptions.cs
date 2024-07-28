using MG.DependencyInjection.Attributes;
using MG.DependencyInjection.Internal;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace MG.DependencyInjection.Startup
{
    public sealed class AttributedServiceOptions
    {
        private Action<IAddServiceTypeExclusions>? _exclusionAction;
        private ActOnReferencer? _referencerAction;

        public Assembly[] AssembliesToScan { private get; set; }
        public IConfiguration Configuration { internal get; set; }
        public bool IncludeNonAttributedAssembliesInScan { get; set; }

        internal AttributedServiceOptions()
        {
            this.AssembliesToScan = Array.Empty<Assembly>();
            this.Configuration = null!;
        }

        public AttributedServiceOptions AddExclusions(Action<IAddServiceTypeExclusions> configureExclusions)
        {
            _exclusionAction = configureExclusions;
            return this;
        }
        internal IEnumerable<Assembly> GetAssemblies()
        {
            if (_referencerAction is not null)
            {
                Referencer.LoadAll(_referencerAction);
            }

            Assembly[] allAssemblies = this.AssembliesToScan.Length > 0
                ? this.AssembliesToScan
                : AppDomain.CurrentDomain.GetAssemblies();

            return !this.IncludeNonAttributedAssembliesInScan
                ? allAssemblies.Where(IsAttributeServicableAssembly)
                : allAssemblies.Where(IsServicableAssembly);
        }
        internal IServiceTypeExclusions GetServiceTypeExclusions()
        {
            return ServiceTypeExclusions.ConfigureFromAction(_exclusionAction);
        }
        public AttributedServiceOptions LoadReferences(ActOnReferencer action)
        {
            _referencerAction = action;
            return this;
        }

        private static bool IsAttributeServicableAssembly(Assembly assembly)
        {
            return IsServicableAssembly(assembly) && assembly.IsDefined(typeof(DependencyAssemblyAttribute), inherit: false);
        }
        private static bool IsServicableAssembly(Assembly assembly)
        {
            return !assembly.IsDynamic;
        }
    }
}
