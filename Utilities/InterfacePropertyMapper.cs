using System.Reflection;

namespace Utils
{
    /// <summary>
    /// Class to assist with mapping interface implementations onto other objects implementing the same interface
    /// </summary>
    public static class InterfacePropertyMapper
    {
        /// <summary>
        /// Uses reflection to map an implementation of an interface onto a concrete implementation of that interface
        /// </summary>
        /// <typeparam name="TInterface">Interface of the objects to map</typeparam>
        /// <typeparam name="TConcrete">Concrete implementation of the interface</typeparam>
        /// <param name="obj">Concrete object to map onto</param>
        /// <param name="concreteObject">Implementation of the Interface to map from</param>
        /// <returns>obj with mapped values from the interface</returns>
        public static TInterface Map<TInterface, TConcrete>(TConcrete obj, TInterface concreteObject)
            where TConcrete : class, TInterface
            where TInterface : class
        {
            if (concreteObject == null)
                return null;

            var props = typeof(TInterface).GetCachedHeirarchialProperties();
            foreach (PropertyInfo prop in props)
            {
                // use the interface property setter instead of the implementation - we already know this one, don't go looking
                //  for the implementation getters/setters on each property
                // 
                // this seems to be faster
                prop.SetValue(obj, prop.GetValue(concreteObject));
                //obj.SetPropertyValue(prop.Name, concreteObject.GetPropertyValue(prop.Name));
            }

            return obj;
        }
    }
}
