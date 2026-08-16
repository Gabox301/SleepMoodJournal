using Xunit;

// Algunos tests comparten el reloj estático de AppTime (UtcNowProvider):
// sin paralelismo entre clases para que los resultados sean deterministas.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
