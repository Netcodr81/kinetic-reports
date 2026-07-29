using BenchmarkDotNet.Running;

// Run all benchmarks in the assembly
BenchmarkRunner.Run(typeof(Program).Assembly);
