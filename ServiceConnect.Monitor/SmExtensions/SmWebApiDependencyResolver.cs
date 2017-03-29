using System.Linq;
using System.Web.Http.Dependencies;
using StructureMap;

namespace ServiceConnect.Monitor.SmExtensions
{
    public sealed class StructureMapWebApiDependencyResolver : IDependencyResolver
    {
        #region Constructors and Destructors
        private IContainer _container;
        /// <summary>
        /// Initializes a new instance of the <see cref="StructureMapWebApiDependencyResolver"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public StructureMapWebApiDependencyResolver(IContainer container)
        {
            _container = container;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The begin scope.
        /// </summary>
        /// <returns>
        /// The System.Web.Http.Dependencies.IDependencyScope.
        /// </returns>
        public IDependencyScope BeginScope()
        {
            var child = _container.GetNestedContainer();
            return new StructureMapWebApiDependencyScope(child);
        }

        #endregion

        public object GetService(System.Type serviceType)
        {
            if (serviceType == null)
                return null;
            try
            {
                if (serviceType.IsAbstract || serviceType.IsInterface)
                    return _container.TryGetInstance(serviceType);

                return _container.GetInstance(serviceType);
            }
            catch
            {
                return null;
            }
        }

        public System.Collections.Generic.IEnumerable<object> GetServices(System.Type serviceType)
        {
            return _container.GetAllInstances<object>().Where(x => x.GetType() == serviceType);

        }

        public void Dispose()
        {
            _container.Dispose();
            _container = null;
        }
    }
}