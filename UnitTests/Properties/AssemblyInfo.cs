using System.Reflection;
using Xunit;

// Don't run tests in parallel.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]