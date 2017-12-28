// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Routing.Performance
{
    public class ReproRoutingBenchmark
    {
        private const int NumberOfRequestTypes = 3;
        private const int Iterations = 100;

        private readonly IRouter _treeRouter;
        private readonly RequestEntry[] _requests;

        public ReproRoutingBenchmark()
        {
            var handler = new RouteHandler((next) => Task.FromResult<object>(null));

            var treeBuilder = new TreeRouteBuilder(
                NullLoggerFactory.Instance,
                UrlEncoder.Default,
                new DefaultObjectPool<UriBuildingContext>(new UriBuilderContextPooledObjectPolicy(UrlEncoder.Default)),
                new DefaultInlineConstraintResolver(new OptionsManager<RouteOptions>(new OptionsFactory<RouteOptions>(Enumerable.Empty<IConfigureOptions<RouteOptions>>(), Enumerable.Empty<IPostConfigureOptions<RouteOptions>>()))));

            var random = new Random(Seed: 42); // Be predictable

            var letters = new char[26 * 2];          
            for (var i = 0; i < 26; i++)
            {
                letters[i] = (char)('a' + i);
            }
            for (var i = 0; i < 26; i++)
            {
                letters[i + 26] = (char)('A' + i);
            }

            var routes = new string[110];
            for (var i = 0; i < routes.Length; i++)
            {
                // Generate a random route of the format `storedprocedure/[a-zA-Z]8-10`
                var builder = new StringBuilder();
                builder.Append("storedprocedure/");

                var letterCount = random.Next(8, 11);
                for (var j = 0; j < letterCount; j++)
                {
                    var c = letters[random.Next(0, 26 * 2)];
                    builder.Append(c);
                }

                routes[i] = builder.ToString();
            }

            for (var i = 0; i < routes.Length; i++)
            {
                treeBuilder.MapInbound(handler, TemplateParser.Parse(routes[i]), "default", 0);
            }

            _treeRouter = treeBuilder.Build();

            // Two of these will won't match any of these routes, but will exercise different paths
            _requests = new RequestEntry[NumberOfRequestTypes];

            _requests[0].HttpContext = new DefaultHttpContext();
            _requests[0].HttpContext.Request.Path = "/api/Widgets/5";
            _requests[0].IsMatch = false;
            _requests[0].Values = new RouteValueDictionary();

            _requests[1].HttpContext = new DefaultHttpContext();
            _requests[1].HttpContext.Request.Path = "/" + routes[routes.Length - 1] + "/foo";
            _requests[1].IsMatch = false;
            _requests[1].Values = new RouteValueDictionary();

            _requests[2].HttpContext = new DefaultHttpContext();
            _requests[2].HttpContext.Request.Path = "/" + routes[routes.Length - 1];
            _requests[2].IsMatch = true;
            _requests[2].Values = new RouteValueDictionary();
        }

        [Benchmark(Description = "Attribute Routing (many routes)", OperationsPerInvoke = Iterations * NumberOfRequestTypes)]
        public async Task AttributeRouting()
        {
            for (var i = 0; i < Iterations; i++)
            {
                for (var j = 0; j < _requests.Length; j++)
                {
                    var context = new RouteContext(_requests[j].HttpContext);

                    await _treeRouter.RouteAsync(context);

                    Verify(context, j);
                }
            }
        }

        private void Verify(RouteContext context, int i)
        {
            if (_requests[i].IsMatch)
            {
                if (context.Handler == null)
                {
                    throw new InvalidOperationException($"Failed {i}");
                }

                var values = _requests[i].Values;
                if (values.Count != context.RouteData.Values.Count)
                {
                    throw new InvalidOperationException($"Failed {i}");
                }
            }
            else
            {
                if (context.Handler != null)
                {
                    throw new InvalidOperationException($"Failed {i}");
                }

                if (context.RouteData.Values.Count != 0)
                {
                    throw new InvalidOperationException($"Failed {i}");
                }
            }
        }

        private struct RequestEntry
        {
            public HttpContext HttpContext;
            public bool IsMatch;
            public RouteValueDictionary Values;
        }
    }
}