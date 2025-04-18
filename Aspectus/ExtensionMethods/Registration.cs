﻿/*
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

using Aspectus.CodeGen;
using Aspectus.Interfaces;
using Canister.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System.Reflection;

namespace Aspectus.ExtensionMethods
{
    /// <summary>
    /// Registration extension methods
    /// </summary>
    public static class Registration
    {
        /// <summary>
        /// Registers the aspectus library with the bootstrapper.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The bootstrapper</returns>
        public static ICanisterConfiguration? RegisterAspectus(this ICanisterConfiguration? bootstrapper) => bootstrapper?.AddAssembly(typeof(Aspectus).GetTypeInfo().Assembly);

        /// <summary>
        /// Registers the Aspectus services with the provided IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add the services to.</param>
        /// <returns>The IServiceCollection with the registered services.</returns>
        public static IServiceCollection? RegisterAspectus(this IServiceCollection? services)
        {
            if (services.Exists<Aspectus>())
                return services;
            var ObjectPoolProvider = new DefaultObjectPoolProvider();
            return services?.AddTransient<Compiler>()
                ?.AddAllTransient<IAspect>()
                ?.AddAllTransient<IAOPModule>()
                ?.AddSingleton(_ => ObjectPoolProvider.CreateStringBuilderPool())
                ?.AddSingleton<Aspectus>();
        }
    }
}