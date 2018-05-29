// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Routing.Performance.Matchers
{
    public class TreeRouterMatcher : Matcher
    {
        public static MatcherBuilder CreateBuilder() => new Builder();

        private TreeRouter _inner;

        private TreeRouterMatcher(TreeRouter inner)
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
            private readonly TreeRouteBuilder _inner;

            public Builder()
            {
                _inner = new TreeRouteBuilder(
                    NullLoggerFactory.Instance,
                    new DefaultObjectPool<UriBuildingContext>(new UriBuilderContextPooledObjectPolicy()),
                    new DefaultInlineConstraintResolver(Options.Create(new RouteOptions())));
            }

            public override void AddEntry(string pattern, Endpoint endpoint)
            {
                var handler = new RouteHandler(c =>
                {
                    c.Features.Set<IEndpointFeature>(new EndpointFeature() { Endpoint = endpoint, });
                    return Task.CompletedTask;
                });
                _inner.MapInbound(handler, TemplateParser.Parse(pattern), "default", 0);
            }

            public override Matcher Build()
            {
                return new TreeRouterMatcher(_inner.Build());
            }
        }
    }
}
