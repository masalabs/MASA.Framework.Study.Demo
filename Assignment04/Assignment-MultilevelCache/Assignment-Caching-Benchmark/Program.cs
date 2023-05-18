// See https://aka.ms/new-console-template for more information


using Assignment_Caching_Benchmark;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

var config = DefaultConfig.Instance
    .AddValidator(ExecutionValidator.FailOnError)
    .WithOptions(ConfigOptions.DisableOptimizationsValidator);
BenchmarkRunner.Run<Benchmarks>(config);

Console.ReadLine();
