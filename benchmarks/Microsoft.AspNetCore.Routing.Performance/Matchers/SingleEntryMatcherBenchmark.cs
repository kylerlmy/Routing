// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Performance.Matchers
{
    public class SingleEntryMatcherBenchmark : MatcherBenchmarkBase
    {
        private readonly Endpoint PlaintextEndpoint = new Endpoint();

        private Matcher _baseline;
        private Matcher _dfa;
        private Matcher _instruction;
        private Matcher _route;
        private Matcher _tree;

        private HttpContext _httpContext;

        [GlobalSetup]
        public void Setup()
        {
            _baseline = SetupMatcher(BaselineMatcher.CreateBuilder());
            _dfa = SetupMatcher(DfaMatcher.CreateBuilder());
            _instruction = SetupMatcher(InstructionMatcher.CreateBuilder());
            _route = SetupMatcher(RouteMatcher.CreateBuilder());
            _tree = SetupMatcher(TreeRouterMatcher.CreateBuilder());

            _httpContext = new DefaultHttpContext();
            _httpContext.RequestServices = CreateServices();
            _httpContext.Request.Path = "/plaintext";
        }

        private Matcher SetupMatcher(MatcherBuilder builder)
        {
            builder.AddEntry("/plaintext", PlaintextEndpoint);
            return builder.Build();
        }

        [Benchmark(Baseline = true)]
        public async Task Baseline()
        {
            var endpoint = await _baseline.MatchAsync(_httpContext);
            Validate(PlaintextEndpoint, endpoint);
        }

        [Benchmark]
        public async Task Dfa()
        {
            var endpoint = await _dfa.MatchAsync(_httpContext);
            Validate(PlaintextEndpoint, endpoint);
        }

        [Benchmark]
        public async Task Instruction()
        {
            var endpoint = await _instruction.MatchAsync(_httpContext);
            Validate(PlaintextEndpoint, endpoint);
        }

        [Benchmark]
        public async Task LegacyRoute()
        {
            var endpoint = await _route.MatchAsync(_httpContext);
            Validate(PlaintextEndpoint, endpoint);
        }

        [Benchmark]
        public async Task LegacyTreeRouter()
        {
            var endpoint = await _tree.MatchAsync(_httpContext);
            Validate(PlaintextEndpoint, endpoint);
        }
    }
}
