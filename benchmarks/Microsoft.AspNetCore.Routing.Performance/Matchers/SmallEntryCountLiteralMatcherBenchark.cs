// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Routing.Performance.Matchers
{
    public class SmallEntryCountLiteralMatcherBenchark : MatcherBenchmarkBase
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

        // For this case we're specifically targeting the last entry to hit 'worst case'
        // performance for the matchers that scale linearly.
        private Matcher SetupMatcher(MatcherBuilder builder)
        {
            builder.AddEntry("/another-really-cool-entry", PlaintextEndpoint);
            builder.AddEntry("/Some-Entry", PlaintextEndpoint);
            builder.AddEntry("/a/path/with/more/segments", PlaintextEndpoint);
            builder.AddEntry("/random/name", PlaintextEndpoint);
            builder.AddEntry("/random/name2", PlaintextEndpoint);
            builder.AddEntry("/random/name3", PlaintextEndpoint);
            builder.AddEntry("/random/name4", PlaintextEndpoint);
            builder.AddEntry("/plaintext1", PlaintextEndpoint);
            builder.AddEntry("/plaintext2", PlaintextEndpoint);
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
