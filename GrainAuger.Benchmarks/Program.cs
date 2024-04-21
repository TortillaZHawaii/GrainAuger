using BenchmarkDotNet.Running;
using GrainAuger.Benchmarks.ProcessChains;

var results = BenchmarkRunner.Run<ProcessChainsBenchmark>();
