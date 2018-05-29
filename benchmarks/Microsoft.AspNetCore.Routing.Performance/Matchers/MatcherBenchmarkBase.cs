// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Routing.Performance.Matchers
{
    public abstract class MatcherBenchmarkBase
    {
        // The older routing implementations retrieve services when they first execute.
        protected static IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            return services.BuildServiceProvider();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected static void Validate(Endpoint expected, Endpoint actual)
        {
            if (!object.ReferenceEquals(expected, actual))
            {
                throw new InvalidOperationException("OH NO");
            }
        }
    }
}
