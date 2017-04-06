using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;

namespace Utilities
{

    /// <summary>
    /// Stores some helper methods for dealing with Enums
    /// </summary>
    public class EnumHelpers : EnumClassHelper<Enum> // compile-time constrain to an actual Enum
    {
    }

    public abstract class EnumClassHelper<C> where C : class
    {
        private static IEnumerable<string> GetListOfDescriptionAttributes<T>() where T : struct, C // this effectively restricts T to enums
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            List<string> result = new List<string>();

            var fields = typeof(T).GetFields();
            DescriptionAttribute attr;
            foreach (FieldInfo field in fields)
            {
                attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    result.Add(attr.Description);
                }
            }

            return result;
        }

        private static IDictionary<ulong, string> GetDictionaryOfEnumValueAndDescriptionAttributes<T>() where T : struct, C // this effectively restricts T to enums
        {
            IDictionary<ulong, string> result = new Dictionary<ulong, string>();

            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            // WARNING: BE CAREFUL HERE, THERE ARE OTHER FIELDS ON ENUMS THAT WE WANT TO EXCLUDE ELSE FACE REFLECTION TARGETEXCEPTIONS
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);
            ulong enumVal;
            object enumObjVal;
            DescriptionAttribute attr;
            foreach (FieldInfo field in fields)
            {
                attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    enumObjVal = field.GetValue(null);

                    enumVal = GetEnumValueAsLong<T>((T)enumObjVal);

                    result[enumVal] = attr.Description;
                }
            }

            return result;
        }

        private static IDictionary<ulong, string> GetDictionaryOfEnumValueAndDescriptionAttributesByType(Type enumType)
        {
            IDictionary<ulong, string> result = new Dictionary<ulong, string>();

            var underlyingType = Enum.GetUnderlyingType(enumType);
            // WARNING: BE CAREFUL HERE, THERE ARE OTHER FIELDS ON ENUMS THAT WE WANT TO EXCLUDE ELSE FACE REFLECTION TARGETEXCEPTIONS
            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            ulong enumVal;
            object enumObjVal;
            DescriptionAttribute attr;
            foreach (FieldInfo field in fields)
            {
                attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    enumObjVal = field.GetValue(null);

                    enumVal = GetEnumValueAsLongByType(enumObjVal, enumType);

                    result[enumVal] = attr.Description;
                }
            }

            return result;
        }

        private static IDictionary<ulong, string> GetDictionaryOfEnumValuesAndNames<T>() where T : struct, C
        {
            IDictionary<ulong, string> result = new Dictionary<ulong, string>();

            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            // WARNING: BE CAREFUL HERE, THERE ARE OTHER FIELDS ON ENUMS THAT WE WANT TO EXCLUDE ELSE FACE REFLECTION TARGETEXCEPTIONS
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);
            ulong enumVal;
            object enumObjVal;
            foreach (var field in fields)
            {
                enumObjVal = field.GetValue(null);

                enumVal = GetEnumValueAsLong<T>((T)enumObjVal);

                result[enumVal] = field.Name;
            }

            return result;
        }

        /// <summary>
        /// Get the value of an enum as a long
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the given value is not an enum</exception>
        public static ulong GetEnumValueAsLong<T>(T enumValue) where T : struct, C // enum
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("Method must be called with an enum type.");

            var underlyingType = Enum.GetUnderlyingType(typeof(T));

            var enumObjVal = Convert.ChangeType(enumValue, underlyingType);

            ulong enumVal;

            // hnnnnng runtime types hurt - just double cast the object into a long after it is a system type
            if (underlyingType == typeof(short))
                enumVal = (ulong)(short)enumObjVal;
            else if (underlyingType == typeof(byte))
                enumVal = (ulong)(byte)enumObjVal;
            else if (underlyingType == typeof(int))
                enumVal = (ulong)(int)enumObjVal;
            else if (underlyingType == typeof(long))
                enumVal = (ulong)(long)enumObjVal;
            else if (underlyingType == typeof(ulong))
                enumVal = (ulong)enumObjVal;
            else
                enumVal = (ulong)Convert.ChangeType(enumObjVal, typeof(ulong)); // try something - I don't really know how we made it here
                                                                                // technically you can have string underlying type enums but lets just pretend that's not possible
                                                                                //  really, why would you be using this method in that case anyway?

            return enumVal;
        }

        public static ulong GetEnumValueAsLongByType(object enumValue, Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Method must be called with an enum type.");

            var underlyingType = Enum.GetUnderlyingType(enumType);
            if (enumValue == null || enumValue.ToString() == string.Empty)
                enumValue = 0;
            var enumObjVal = Convert.ChangeType(enumValue, underlyingType);

            ulong enumVal;

            // hnnnnng runtime types hurt - just double cast the object into a long after it is a system type
            if (underlyingType == typeof(short))
                enumVal = (ulong)(short)enumObjVal;
            else if (underlyingType == typeof(byte))
                enumVal = (ulong)(byte)enumObjVal;
            else if (underlyingType == typeof(int))
                enumVal = (ulong)(int)enumObjVal;
            else if (underlyingType == typeof(long))
                enumVal = (ulong)(long)enumObjVal;
            else if (underlyingType == typeof(ulong))
                enumVal = (ulong)enumObjVal;
            else
                enumVal = (ulong)Convert.ChangeType(enumObjVal, typeof(ulong)); // try something - I don't really know how we made it here
                                                                                // technically you can have string underlying type enums but lets just pretend that's not possible
                                                                                //  really, why would you be using this method in that case anyway?

            return enumVal;
        }

        /// <summary>
        /// Parse a string into the the enum underlying type
        /// </summary>
        /// <typeparam name="T">Enum type to get underlying type for</typeparam>
        /// <param name="enumValue">string to parse</param>
        /// <returns></returns>
        public static object ParseStringToEnumUnderlyingType<T>(string enumValue) where T : struct, C //enum
        {
            var underlyingType = Enum.GetUnderlyingType(typeof(T));

            return Convert.ChangeType(enumValue, underlyingType);
        }

        /// <summary>
        /// Gets a list of description attributes from the fields on the enum (by type)
        /// </summary>
        /// <typeparam name="T">Enum type to fetch descriptions for</typeparam>
        /// <param name="theEnum"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetCachedListOfDescriptionAttributes<T>() where T : struct, C // restrict to enums (almost)
        {
            var cacheKey = string.Concat("DscrAttr:", typeof(T).AssemblyQualifiedName, ":", typeof(T).ToString());

            var result = HostingEnvironment.Cache.Get(cacheKey) as IEnumerable<string>;
            if (result == null)
            {
                result = GetListOfDescriptionAttributes<T>();
                if (result != null)
                    HostingEnvironment.Cache.Insert(cacheKey, result);// this can't change during the life of the application - just cache aggressively forever
            }

            return result;
        }

        /// <summary>
        /// Gets a dictionary of enum values and description attributes based on the enum type that is given
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <returns></returns>
        public static IDictionary<ulong, string> GetCachedDictionaryOfEnumValueAndDescriptionAttributes<T>() where T : struct, C // this effectively restricts T to enums
        {
            var cacheKey = string.Concat("DscrAttrDct:", typeof(T).AssemblyQualifiedName, ":", typeof(T).ToString());
            var result = HostingEnvironment.Cache.Get(cacheKey) as IDictionary<ulong, string>;
            if (result == null)
            {
                result = GetDictionaryOfEnumValueAndDescriptionAttributes<T>();
                if (result != null)
                    HostingEnvironment.Cache.Insert(cacheKey, result); // this can't change during the life of the application - just cache aggressively forever
            }

            return result;
        }

        /// <summary>
        /// Gets a dictionary of enum values and description attributes based on the enum type that is given
        /// </summary>
        /// <param name="enumType">Type of the enum</param>
        /// <returns></returns>
        public static IDictionary<ulong, string> GetCachedDictionaryOfEnumValueAndDescriptionAttributesForEnumType(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("This method can only be used on Enum types.");

            var cacheKey = $"DscrAttrDct:{enumType.AssemblyQualifiedName}:{enumType.ToString()}";
            var result = HostingEnvironment.Cache.Get(cacheKey) as IDictionary<ulong, string>;
            if (result == null)
            {
                result = GetDictionaryOfEnumValueAndDescriptionAttributesByType(enumType);
                if (result != null)
                    HostingEnvironment.Cache.Insert(cacheKey, result); // this can't change during the life of the application - just cache aggressively forever
            }

            return result;
        }

        /// <summary>
        /// Gets a dictionary of enum values and names based on the enum type that is given
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <returns></returns>
        public static IDictionary<ulong, string> GetCachedDictionaryOfEnumValuesAndNames<T>() where T : struct, C // this effectively restricts T to enums
        {
            var cacheKey = string.Concat("DscrNmDct:", typeof(T).AssemblyQualifiedName, ":", typeof(T).ToString());
            var result = HostingEnvironment.Cache.Get(cacheKey) as IDictionary<ulong, string>;
            if (result == null)
            {
                result = GetDictionaryOfEnumValuesAndNames<T>();
                if (result != null)
                    HostingEnvironment.Cache.Insert(cacheKey, result); // this can't change during the life of the application - just cache aggressively forever
            }

            return result;
        }

        /// <summary>
        /// Generates a NameValueCollection of enum values and their associated names from an Enum using a Type argument
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns></returns>
        public static NameValueCollection GetNameValueOptionsFromEnum<T>() where T : struct, C // restrict to something like an enum
        {
            Type myType = typeof(T);

            return GetNameValueOptionsFromEnumType(myType);
        }

        /// <summary>
        /// Generates a NameValueCollection of enum values and their associated names from a given enum type
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the Type provided is not an enum type</exception>
        public static NameValueCollection GetNameValueOptionsFromEnumType(Type enumType)
        {
            if ((!enumType.IsEnum))
            {
                throw new ArgumentException("Method must be used on an Enum type");
            }

            NameValueCollection result = new NameValueCollection();
            foreach (ulong val in Enum.GetValues(enumType))
            {
                result.Add(Enum.GetName(enumType, val), val.ToString());
            }

            return result;
        }

        /// <summary>
        /// Gets a name value collection of the enum options with the names being assigned by the description attributes of the enum options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static NameValueCollection GetNameValueOptionsFromEnumByDescriptionAttribute<T>() where T : struct, C
        {
            IDictionary<ulong, string> nameValDict = GetCachedDictionaryOfEnumValueAndDescriptionAttributes<T>();

            NameValueCollection result = new NameValueCollection();
            foreach (var nvp in nameValDict)
            {
                result.Add(nvp.Value, nvp.Key.ToString());
            }

            return result;
        }

        /// <summary>
        /// Get the unique flag values from an enum in an enumerable manner
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetUniqueFlags<T>(T flags) where T : struct, C
        {
            var flag = 1ul;
            Enum enmFlags = flags as Enum;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<T>())
            {
                ulong bits = Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                if (flag == bits && enmFlags.HasFlag(value as Enum))
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Get the unique flag values from an enum by a particular type in an enumerable manner
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static IEnumerable<Enum> GetUniqueFlagsByType(object flags, Type t)
        {
            if (t.IsEnum)
            {
                var flag = 1ul;
                var enmFlags = Enum.ToObject(t, flags) as Enum;
                foreach (var value in Enum.GetValues(t).Cast<Enum>())
                {
                    ulong bits = Convert.ToUInt64(value);
                    while (flag < bits)
                    {
                        flag <<= 1;
                    }

                    if (flag == bits && enmFlags.HasFlag(value as Enum))
                    {
                        yield return value;
                    }
                }
            }
            else
                throw new ArgumentException("This method call only be used on Enum types.");
        }

        /// <summary>
        /// Gets a string of description attribute value(s) from an enum value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">Value to convert to string</param>
        /// <param name="separatorText">Text to inject between values</param>
        /// <returns>String representation of the description attributes associated with the enum value(s)</returns>
        public static string GetEnumDescriptionAttributeStringFromEnumValue<T>(T enumValue, string separatorText = ", ") where T : struct, C
        {
            return GetEnumDescriptionAttributeStringFromEnumValue(typeof(T), enumValue, separatorText);
        }

        /// <summary>
        /// Gets a string of description attribute value(s) from an enum value for a particular type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">Value to convert to string</param>
        /// <param name="separatorText">Text to inject between values</param>
        /// <returns>String representation of the description attributes associated with the enum value(s)</returns>
        public static string GetEnumDescriptionAttributeStringFromEnumValue(Type t, object enumValue, string separatorText = ", ")
        {
            var opts = GetCachedDictionaryOfEnumValueAndDescriptionAttributesForEnumType(t);
            string result = string.Empty;
            // flags
            if (t.GetCustomAttribute<FlagsAttribute>() != null)
            {
                var curValues = GetUniqueFlagsByType(enumValue, t);
                IList<string> stringValues = new List<string>();
                ulong enumVal;
                foreach (var value in curValues)
                {
                    enumVal = GetEnumValueAsLongByType(value, t);
                    if (opts.ContainsKey(enumVal))
                        stringValues.Add(opts[enumVal]);
                }

                result = string.Join(separatorText, stringValues);
            }
            else
            {
                var longVal = GetEnumValueAsLongByType(enumValue, t);
                if (opts.ContainsKey(longVal))
                    result = opts[longVal];
            }

            return result;
        }

        internal static T TransformStringEnumValuesIntoFlaggedEnum<T>(IEnumerable<string> stringEnumValues) where T : struct, C
        {
            var result = new T();
            var enumType = typeof(T);
            T enumValue;
            ulong resultLong = 0ul;
            object enumVal;
            foreach (var val in stringEnumValues)
            {
                enumVal = ParseStringToEnumUnderlyingType<T>(val);
                if (Enum.IsDefined(enumType, enumVal))
                {
                    enumValue = (T)enumVal;
                    resultLong |= GetEnumValueAsLong(enumValue);
                }
            }

            result = (T)Enum.ToObject(enumType, resultLong);

            return result;
        }

        internal static T TransformCommaSeparatedStringEnumValueIntoEnum<T>(string stringEnum) where T : struct, C
        {
            var result = new T();
            var enumType = typeof(T);
            if (string.IsNullOrEmpty(stringEnum))
                return result;

            // flags
            if (enumType.GetCustomAttributes<FlagsAttribute>().Any())
            {
                // split the string and clean it
                var valueParts = stringEnum.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

                return TransformStringEnumValuesIntoFlaggedEnum<T>(valueParts);
            }
            else
            {
                var enumVal = ParseStringToEnumUnderlyingType<T>(stringEnum);
                if (Enum.IsDefined(enumType, enumVal))
                {
                    result = (T)Enum.ToObject(enumType, enumVal);
                }
            }

            return result;
        }
    }
}
