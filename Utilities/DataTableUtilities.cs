using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Utils
{
    public static class DataTableUtilities
    {
        /// <summary>
        /// Extension to create a datatable from an object by reflection
        /// Please see <see cref="TypeToDataTable"/> and <see cref="FillFromObject"/> for more information
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static DataTable CreateTableFrom(this object obj, string TableName = "", bool includeObjectRow = true)
        {
            if (obj == null)
                return null;

            DataTable result = TypeToDataTable(obj.GetType(), TableName);

            if (includeObjectRow)
            {
                DataRow dr = result.NewRow();

                dr.FillFromObject(obj);

                result.Rows.Add(dr);
            }

            return result;
        }

        /// <summary>
        /// Extension to create a datatable from a dictionary by parsing keys and values
        /// Please see <see cref="DictionaryToDataTable"/> and <see cref="FillFromDictionary"/> for more information
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static DataTable CreateTableFrom<TKey, TValue>(this IDictionary<TKey, TValue> dict, string TableName = "")
        {
            if (dict == null)
                return null;

            DataTable result = DictionaryToDataTable(dict, TableName);

            DataRow dr = result.NewRow();

            dr.FillFromDictionary(dict);

            result.Rows.Add(dr);

            return result;
        }

        /// <summary>
        /// Fetches an empty datatable for a particular type with column names for each property of the type
        /// </summary>
        /// <param name="objectType">Type to reflect</param>
        /// <param name="tableName">Optionally specify a table name</param>
        /// <returns>Empty datatable with type properties as columns</returns>
        public static DataTable TypeToDataTable(Type objectType, string tableName = "")
        {
            DataTable result = new DataTable();

            // use table name if provided otherwise use the name of the type
            result.TableName = string.IsNullOrEmpty(tableName) ? objectType.Name.ToLower() : tableName;

            var props = objectType.GetCachedHeirarchialProperties();
            foreach (PropertyInfo prop in props)
            {
                if (!result.Columns.Contains(prop.Name))
                {
                    result.Columns.Add(prop.Name, prop.PropertyType);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a datatable from a dictionary
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TValue">Dictionary item value type</typeparam>
        /// <param name="dictionary">Dictionary to turn into datatable</param>
        /// <param name="tableName">Name of the table to return</param>
        /// <returns></returns>
        public static DataTable DictionaryToDataTable<TKey, TValue>(IDictionary<TKey, TValue> dictionary, string tableName = "")
        {
            DataTable result = new DataTable();

            // use table name if provided
            result.TableName = tableName;

            var keys = dictionary.Keys;
            foreach (TKey key in keys)
            {
                if (!result.Columns.Contains(key.ToString()))
                {
                    result.Columns.Add(key.ToString(), typeof(TValue));
                }
            }

            return result;
        }

        /// <summary>
        /// Fills a datarow with the properties of the object - assumes all columns exist for each property
        /// Please use <see cref="TypeToDataTable"/> to create the empty table
        /// </summary>
        /// <param name="dr">row to insert into</param>
        /// <param name="obj">object to fill data from</param>
        /// <returns>Self with it's rows filled with the values of the object</returns>
        public static DataRow FillFromObject(this DataRow dr, object obj)
        {
            var props = obj.GetType().GetCachedHeirarchialProperties();
            foreach (PropertyInfo prop in props)
            {
                dr[prop.Name] = prop.GetValue(obj);
            }

            return dr;
        }

        /// <summary>
        /// Fills a datarow with the properties of the type - assumes all columns exist for each property
        /// Please use <see cref="TypeToDataTable"/> to create the empty table
        /// </summary>
        /// <typeparam name="T">Type to reflect to find properties</typeparam>
        /// <param name="dr">DataRow to fill</param>
        /// <param name="obj">Object to pull values from</param>
        /// <returns></returns>
        public static DataRow FillFromObject<T>(this DataRow dr, T obj) where T : class
        {
            var props = typeof(T).GetCachedHeirarchialProperties();
            foreach (PropertyInfo prop in props)
            {
                dr[prop.Name] = prop.GetValue(obj);
            }

            return dr;
        }

        /// <summary>
        /// Fills a DataRow with values from a dictionary - assumes columns are existing for the dictionary
        /// Please see <see cref="DictionaryToDataTable" /> for creation of the table
        /// </summary>
        /// <typeparam name="TKey">Key type of the dictionary</typeparam>
        /// <typeparam name="TValue">Type of the values in the dictionary</typeparam>
        /// <param name="dr">datarow to set values from</param>
        /// <param name="dictionary">dictionary to pull values from</param>
        /// <returns>self with values set</returns>
        public static DataRow FillFromDictionary<TKey, TValue>(this DataRow dr, IDictionary<TKey, TValue> dictionary)
        {
            foreach (TKey key in dictionary.Keys)
            {
                dr[key.ToString()] = dictionary[key];
            }

            return dr;
        }
    }
}
