// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Performance.Matchers
{
    public class BaselineMatcher : Matcher
    {
        public static MatcherBuilder CreateBuilder() => new Builder();

        private readonly (string pattern, Endpoint endpoint)[] _entries;

        private BaselineMatcher((string pattern, Endpoint endpoint)[] entries)
        {
            _entries = entries;
        }

        public override Task<Endpoint> MatchAsync(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var path = httpContext.Request.Path.Value;
            for (var i = 0; i < _entries.Length; i++)
            {
                if (string.Equals(_entries[i].pattern, path, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(_entries[i].endpoint);
                }
            }

            return null;
        }

        private class Builder : MatcherBuilder
        {
            private List<(string pattern, Endpoint endpoint)> _entries = new List<(string pattern, Endpoint endpoint)>();

            public override void AddEntry(string pattern, Endpoint endpoint)
            {
                _entries.Add((pattern, endpoint)); 
            }

            public override Matcher Build()
            {
                return new BaselineMatcher(_entries.ToArray());
            }
        }
    }
}
