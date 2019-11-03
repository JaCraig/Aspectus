/*
Copyright 2016 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspectus.HelperFunctions
{
    /// <summary>
    /// Extension methods
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Adds a list of items to the collection
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection</typeparam>
        /// <param name="collection">Collection</param>
        /// <param name="items">Items to add</param>
        /// <returns>The collection with the added items</returns>
        public static ConcurrentBag<T> Add<T>(this ConcurrentBag<T> collection, IEnumerable<T> items)
        {
            collection = collection ?? new ConcurrentBag<T>();
            if (items == null)
                return collection;
            foreach (var Item in items)
            {
                collection.Add(Item);
            }
            return collection;
        }

        /// <summary>
        /// Adds items to the collection if it passes the predicate test
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="collection">Collection to add to</param>
        /// <param name="predicate">Predicate that an item needs to satisfy in order to be added</param>
        /// <param name="items">Items to add to the collection</param>
        /// <returns>True if any are added, false otherwise</returns>
        public static bool AddIf<T>(this ICollection<T> collection, Predicate<T> predicate, params T[] items)
        {
            if (collection == null || predicate == null)
                return false;
            if (items == null || items.Length == 0)
                return true;
            var ReturnValue = false;
            for (int i = 0, itemsLength = items.Length; i < itemsLength; i++)
            {
                var Item = items[i];
                if (predicate(Item))
                {
                    collection.Add(Item);
                    ReturnValue = true;
                }
            }

            return ReturnValue;
        }

        /// <summary>
        /// Adds an item to the collection if it isn't already in the collection
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="collection">Collection to add to</param>
        /// <param name="predicate">Predicate that an item needs to satisfy in order to be added</param>
        /// <param name="items">Items to add to the collection</param>
        /// <returns>True if it is added, false otherwise</returns>
        public static bool AddIf<T>(this ICollection<T> collection, Predicate<T> predicate, IEnumerable<T> items)
        {
            if (collection == null || predicate == null)
                return false;
            if (items?.Any() != true)
                return true;
            return collection.AddIf(predicate, items.ToArray());
        }

        /// <summary>
        /// Adds an item to the collection if it isn't already in the collection
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="collection">Collection to add to</param>
        /// <param name="predicate">
        /// Predicate used to determine if two values are equal. Return true if they are the same,
        /// false otherwise
        /// </param>
        /// <param name="items">Items to add to the collection</param>
        /// <returns>True if it is added, false otherwise</returns>
        public static bool AddIfUnique<T>(this ICollection<T> collection, Func<T, T, bool> predicate, params T[] items)
        {
            if (collection == null || predicate == null)
                return false;
            if (items == null)
                return true;
            return collection.AddIf(x => !collection.Any(y => predicate(x, y)), items);
        }

        /// <summary>
        /// Adds an item to the collection if it isn't already in the collection
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="collection">Collection to add to</param>
        /// <param name="items">Items to add to the collection</param>
        /// <returns>True if it is added, false otherwise</returns>
        public static bool AddIfUnique<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null)
                return false;
            if (items == null)
                return true;
            return collection.AddIf(x => !collection.Contains(x), items);
        }

        /// <summary>
        /// Adds an item to the collection if it isn't already in the collection
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="collection">Collection to add to</param>
        /// <param name="items">Items to add to the collection</param>
        /// <returns>True if it is added, false otherwise</returns>
        public static bool AddIfUnique<T>(this ICollection<T> collection, params T[] items)
        {
            if (collection == null)
                return false;
            if (items == null)
                return true;
            return collection.AddIf(x => !collection.Contains(x), items);
        }

        /// <summary>
        /// Does an AppendFormat and then an AppendLine on the StringBuilder
        /// </summary>
        /// <param name="builder">Builder object</param>
        /// <param name="provider">Format provider (CultureInfo) to use</param>
        /// <param name="format">Format string</param>
        /// <param name="objects">Objects to format</param>
        /// <returns>The StringBuilder passed in</returns>
        public static StringBuilder AppendLineFormat(this StringBuilder builder, IFormatProvider provider, string format, params object[] objects)
        {
            if (builder == null || string.IsNullOrEmpty(format))
                return builder;
            objects = objects ?? Array.Empty<object>();
            provider = provider ?? CultureInfo.InvariantCulture;
            return builder.AppendFormat(provider, format, objects).AppendLine();
        }

        /// <summary>
        /// Does an AppendFormat and then an AppendLine on the StringBuilder
        /// </summary>
        /// <param name="builder">Builder object</param>
        /// <param name="format">Format string</param>
        /// <param name="objects">Objects to format</param>
        /// <returns>The StringBuilder passed in</returns>
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, params object[] objects) => builder.AppendLineFormat(CultureInfo.InvariantCulture, format, objects);

        /// <summary>
        /// Does an action for each item in the IEnumerable
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="list">IEnumerable to iterate over</param>
        /// <param name="action">Action to do</param>
        /// <returns>The original list</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list == null)
                return Array.Empty<T>();
            if (action == null)
                return list;
            foreach (var Item in list)
                action(Item);
            return list;
        }

        /// <summary>
        /// Does a function for each item in the IEnumerable, returning a list of the results
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <typeparam name="R">Return type</typeparam>
        /// <param name="list">IEnumerable to iterate over</param>
        /// <param name="function">Function to do</param>
        /// <returns>The resulting list</returns>
        public static IEnumerable<R> ForEach<T, R>(this IEnumerable<T> list, Func<T, R> function)
        {
            if (list == null || function == null)
                return Array.Empty<R>();
            var ReturnList = new List<R>(list.Count());
            foreach (var Item in list)
                ReturnList.Add(function(Item));
            return ReturnList;
        }

        /// <summary>
        /// Does an action for each item in the IEnumerable in parallel
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="list">IEnumerable to iterate over</param>
        /// <param name="action">Action to do</param>
        /// <returns>The original list</returns>
        public static IEnumerable<T> ForEachParallel<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list == null)
                return Array.Empty<T>();
            if (action == null)
                return list;
            Parallel.ForEach(list, action);
            return list;
        }

        /// <summary>
        /// Returns the type's name (Actual C# name, not the funky version from the Name property)
        /// </summary>
        /// <param name="objectType">Type to get the name of</param>
        /// <returns>string name of the type</returns>
        public static string GetName(this Type objectType)
        {
            if (objectType == null)
                return "";
            var Output = new StringBuilder();
            if (objectType.Name == "Void")
            {
                Output.Append("void");
            }
            else
            {
                Output.Append(objectType.DeclaringType == null ? objectType.Namespace : objectType.DeclaringType.GetName())
                    .Append(".");
                if (objectType.Name.Contains("`"))
                {
                    var GenericTypes = objectType.GetGenericArguments();
                    Output.Append(objectType.Name, 0, objectType.Name.IndexOf("`", StringComparison.OrdinalIgnoreCase))
                        .Append("<");
                    var Seperator = "";
                    foreach (var GenericType in GenericTypes)
                    {
                        Output.Append(Seperator).Append(GenericType.GetName());
                        Seperator = ",";
                    }
                    Output.Append(">");
                }
                else
                {
                    Output.Append(objectType.Name);
                }
            }
            return Output.ToString().Replace("&", "");
        }

        /// <summary>
        /// Determines if the type has a default constructor
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if it does, false otherwise</returns>
        public static bool HasDefaultConstructor(this Type type)
        {
            return type?
                .GetConstructors()
                .Any(x => x.GetParameters().Length == 0) == true;
        }

        /// <summary>
        /// Takes all of the data in the stream and returns it as an array of bytes
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <returns>A byte array</returns>
        public static byte[] ReadAllBinary(this Stream input)
        {
            if (input == null)
                return Array.Empty<byte>();

            if (input is MemoryStream TempInput)
                return TempInput.ToArray();

            var Buffer = new byte[4096];
            using (var Temp = new MemoryStream())
            {
                while (true)
                {
                    var Count = input.Read(Buffer, 0, Buffer.Length);
                    if (Count <= 0)
                    {
                        return Temp.ToArray();
                    }
                    Temp.Write(Buffer, 0, Count);
                }
            }
        }

        /// <summary>
        /// Converts the list to a string where each item is seperated by the Seperator
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">List to convert</param>
        /// <param name="itemOutput">
        /// Used to convert the item to a string (defaults to calling ToString)
        /// </param>
        /// <param name="seperator">Seperator to use between items (defaults to ,)</param>
        /// <returns>The string version of the list</returns>
        public static string ToString<T>(this IEnumerable<T> list, Func<T, string> itemOutput = null, string seperator = ",")
        {
            if (list?.Any() != true)
            {
                return "";
            }

            seperator = seperator ?? "";
            itemOutput = itemOutput ?? (x => x.ToString());
            return string.Join(seperator, list.Select(itemOutput));
        }
    }
}