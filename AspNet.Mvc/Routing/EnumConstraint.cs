using System;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace AspNet.Mvc.Common.Routing
{
    /// <summary>
    /// Custom constraint for AspNet.Mvc Attribute Routing that maps string values in URL to specified enum.
    /// <para />
    /// [Route("/{enumValue:enum(MyNamespace.MyEnum)}")]
    /// </summary>
    public class EnumConstraint : IRouteConstraint
    {
        private readonly string[] _validOptions;

        public EnumConstraint(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                throw new ArgumentException($"Type {typeName} was not found", nameof(typeName));
            }

            _validOptions = Enum.GetNames(type)
                .Select(n => n.ToLowerInvariant())
                .ToArray();
        }

        public bool Match(HttpContextBase httpContext, Route route,
            string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            object value;
            if (values.TryGetValue(parameterName, out value) && value != null)
            {
                return _validOptions.Contains(value.ToString(), StringComparer.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
