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

using Aspectus.CodeGen;
using Aspectus.Interfaces;
using Canister.Interfaces;

namespace Aspectus.Module
{
    /// <summary>
    /// AOP module
    /// </summary>
    public class AOPModule : IModule
    {
        /// <summary>
        /// Order to run it in
        /// </summary>
        public int Order => 2;

        /// <summary>
        /// Loads the module
        /// </summary>
        /// <param name="bootstrapper">Bootstrapper to register with</param>
        public void Load(IBootstrapper bootstrapper)
        {
            if (bootstrapper == null)
                return;
            bootstrapper.Register(new Compiler());
            bootstrapper.RegisterAll<IAspect>();
            bootstrapper.RegisterAll<IAOPModule>();
            bootstrapper.Register(new Aspectus(bootstrapper.Resolve<Compiler>(), bootstrapper.ResolveAll<IAspect>(), bootstrapper.ResolveAll<IAOPModule>()));
        }
    }
}