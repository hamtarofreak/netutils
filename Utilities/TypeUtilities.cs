using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;

namespace Utils
{
    public static class TypeUtilities
    {
            private static IEnumerable<PropertyInfo> GetHeirarchialProperties(Type type)
            {
                if (type.IsInterface)
                {
                    var propertyInfos = new List<PropertyInfo>();

                    var considered = new List<Type>();
                    var queue = new Queue<Type>();
                    considered.Add(type);
                    queue.Enqueue(type);
                    while (queue.Count > 0)
                    {
                        var subType = queue.Dequeue();
                        foreach (var subInterface in subType.GetInterfaces())
                        {
                            if (considered.Contains(subInterface)) continue;

                            considered.Add(subInterface);
                            queue.Enqueue(subInterface);
                        }

                        var typeProperties = subType.GetProperties(
                            BindingFlags.FlattenHierarchy
                            | BindingFlags.Public
                            | BindingFlags.Instance);

                        var newPropertyInfos = typeProperties
                            .Where(x => !propertyInfos.Contains(x));

                        propertyInfos.InsertRange(0, newPropertyInfos);
                    }

                    return propertyInfos;
                }

                return type.GetProperties(BindingFlags.FlattenHierarchy
                    | BindingFlags.Public | BindingFlags.Instance);
            }

            /// <summary>
            /// Gets the heirarchial properties of a Type and returns them
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public static IEnumerable<PropertyInfo> GetCachedHeirarchialProperties(this Type type)
            {
                var cacheKey = string.Format("HeirrchlPrpFTyp:{0}", type.Name);
                var result = HostingEnvironment.Cache.Get(cacheKey) as IEnumerable<PropertyInfo>;
                if (result == null)
                {
                    result = GetHeirarchialProperties(type);
                    if (result != null)
                        HostingEnvironment.Cache.Insert(cacheKey, result);
                }

                return result;
            }

            /// <summary>
            /// Determines whether the type in question is a numeric type
            /// </summary>
            /// <param name="t"></param>
            /// <returns></returns>
            public static bool IsNumeric(this Type t)
            {
                switch (Type.GetTypeCode(t))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Single:
                        return true;
                    default:
                        return false;
                }
            }
    }
}
