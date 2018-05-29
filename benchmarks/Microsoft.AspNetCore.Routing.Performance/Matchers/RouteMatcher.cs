// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Routing.Performance.Matchers
{
    public class RouteMatcher : Matcher
    {
        public static MatcherBuilder CreateBuilder() => new Builder();

        private IRouter _inner;

        private RouteMatcher(IRouter inner)
        {
            _inner = inner;
        }

        public override async Task<Endpoint> MatchAsync(HttpContext httpContext)
        {
            var context = new RouteContext(httpContext);
            await _inner.RouteAsync(context);

            if (context.Handler != null)
            {
                await context.Handler(httpContext);
            }

            return httpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        }

        private class Builder : MatcherBuilder
        {
            private readonly RouteCollection _routes = new RouteCollection();
            private readonly IInlineConstraintResolver _constraintResolver;

            public Builder()
            {
                _constraintResolver = new DefaultInlineConstraintResolver(Options.Create(new RouteOptions()));
            }

            public override void AddEntry(string pattern, Endpoint endpoint)
            {
                var handler = new RouteHandler(c =>
                {
                    c.Features.Set<IEndpointFeature>(new EndpointFeature() { Endpoint = endpoint, });
                    return Task.CompletedTask;
                });
                _routes.Add(new Route(handler, pattern, _constraintResolver));
            }

            public override Matcher Build()
            {
                return new RouteMatcher(_routes);
            }
        }
    }
}
