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

using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Aspectus.HelperFunctions
{
    /// <summary>
    /// Extension methods
    /// </summary>
    internal static class ExtensionMethods
    {
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
            if (collection is null || predicate is null)
                return false;
            if (items is null || items.Length == 0)
                return true;
            var ReturnValue = false;
            for (int I = 0, ItemsLength = items.Length; I < ItemsLength; I++)
            {
                T? Item = items[I];
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
        public static bool AddIf<T>(this ICollection<T> collection, Predicate<T> predicate, IEnumerable<T> items) => collection is not null && predicate is not null && (items?.Any() != true || collection.AddIf(predicate, [.. items]));

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
        public static bool AddIfUnique<T>(this ICollection<T> collection, Func<T, T, bool> predicate, params T[] items) => collection is not null && predicate is not null && (items is null || collection.AddIf(x => !collection.Any(y => predicate(x, y)), items));

        /// <summary>
        /// Adds an item to the collection if it isn't already in the collection
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="collection">Collection to add to</param>
        /// <param name="items">Items to add to the collection</param>
        /// <returns>True if it is added, false otherwise</returns>
        public static bool AddIfUnique<T>(this ICollection<T> collection, IEnumerable<T> items) => collection is not null && (items is null || collection.AddIf(x => !collection.Contains(x), items));

        /// <summary>
        /// Adds an item to the collection if it isn't already in the collection
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="collection">Collection to add to</param>
        /// <param name="items">Items to add to the collection</param>
        /// <returns>True if it is added, false otherwise</returns>
        public static bool AddIfUnique<T>(this ICollection<T> collection, params T[] items) => collection is not null && (items is null || collection.AddIf(x => !collection.Contains(x), items));

        /// <summary>
        /// Does an AppendFormat and then an AppendLine on the StringBuilder
        /// </summary>
        /// <param name="builder">Builder object</param>
        /// <param name="provider">Format provider (CultureInfo) to use</param>
        /// <param name="format">Format string</param>
        /// <param name="objects">Objects to format</param>
        /// <returns>The StringBuilder passed in</returns>
        public static StringBuilder? AppendLineFormat(this StringBuilder builder, IFormatProvider provider, string format, params object[] objects)
        {
            if (builder is null || string.IsNullOrEmpty(format))
                return builder;
            objects ??= [];
            provider ??= CultureInfo.InvariantCulture;
            return builder.AppendFormat(provider, format, objects).AppendLine();
        }

        /// <summary>
        /// Does an AppendFormat and then an AppendLine on the StringBuilder
        /// </summary>
        /// <param name="builder">Builder object</param>
        /// <param name="format">Format string</param>
        /// <param name="objects">Objects to format</param>
        /// <returns>The StringBuilder passed in</returns>
        public static StringBuilder? AppendLineFormat(this StringBuilder builder, string format, params object[] objects) => builder.AppendLineFormat(CultureInfo.InvariantCulture, format, objects);

        /// <summary>
        /// Does an action for each item in the IEnumerable
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="list">IEnumerable to iterate over</param>
        /// <param name="action">Action to do</param>
        /// <returns>The original list</returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list is null)
                return [];
            if (action is null)
                return list;
            foreach (T? Item in list)
                action(Item);
            return list;
        }

        /// <summary>
        /// Does a function for each item in the IEnumerable, returning a list of the results
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <param name="list">IEnumerable to iterate over</param>
        /// <param name="function">Function to do</param>
        /// <returns>The resulting list</returns>
        public static IEnumerable<TResult> ForEach<T, TResult>(this IEnumerable<T> list, Func<T, TResult> function)
        {
            if (list is null || function is null)
                return [];
            var ReturnList = new List<TResult>(list.Count());
            foreach (T? Item in list)
                ReturnList.Add(function(Item));
            return ReturnList;
        }

        /// <summary>
        /// Returns the type's name (Actual C# name, not the funky version from the Name property)
        /// </summary>
        /// <param name="objectType">Type to get the name of</param>
        /// <param name="objectPool">The object pool.</param>
        /// <returns>string name of the type</returns>
        public static string GetName(this Type objectType, ObjectPool<StringBuilder>? objectPool)
        {
            if (objectType is null)
                return string.Empty;
            StringBuilder Output = objectPool?.Get() ?? new StringBuilder();
            if (objectType.Name == "Void")
            {
                objectPool?.Return(Output);
                return "void";
            }
            else
            {
                _ = Output.Append(objectType.DeclaringType is null ? objectType.Namespace : objectType.DeclaringType.GetName(objectPool))
                    .Append('.');
                if (objectType.Name.Contains('`', StringComparison.Ordinal))
                {
                    Type[] GenericTypes = objectType.GetGenericArguments();
                    _ = Output.Append(objectType.Name, 0, objectType.Name.IndexOf('`'))
                        .Append('<');
                    var Seperator = string.Empty;
                    foreach (Type GenericType in GenericTypes)
                    {
                        _ = Output.Append(Seperator).Append(GenericType.GetName(objectPool));
                        Seperator = ",";
                    }
                    _ = Output.Append('>');
                }
                else
                {
                    _ = Output.Append(objectType.Name);
                }
            }
            var ReturnValue = Output.ToString().Replace("&", string.Empty, StringComparison.Ordinal);
            objectPool?.Return(Output);
            return ReturnValue;
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
        /// Converts the list to a string where each item is seperated by the Seperator
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="list">List to convert</param>
        /// <param name="itemOutput">
        /// Used to convert the item to a string (defaults to calling ToString)
        /// </param>
        /// <param name="seperator">Seperator to use between items (defaults to ,)</param>
        /// <returns>The string version of the list</returns>
        public static string ToString<T>(this IEnumerable<T> list, Func<T, string>? itemOutput = null, string seperator = ",")
        {
            if (list?.Any() != true)
            {
                return string.Empty;
            }

            seperator ??= string.Empty;
            itemOutput ??= DefaultToStringConverter;
            return string.Join(seperator, list.Select(itemOutput));
        }

        /// <summary>
        /// Default to string converter.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The value as a string</returns>
        private static string DefaultToStringConverter<TValue>(TValue value) => value?.ToString() ?? string.Empty;
    }
}