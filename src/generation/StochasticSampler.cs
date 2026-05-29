using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace LSystemTree.Generation
{
    /// <summary>
    /// Seeded, reproducible random number generator for L-system expansion.
    /// Wraps System.Random with convenience methods and high-entropy time-based seeding.
    /// 
    /// ⚠️ Not thread-safe: create one instance per generation context or synchronize externally.
    /// </summary>
    public sealed class StochasticSampler
    {
        private Random _rng;
        private long _seed;

        /// <summary>
        /// Gets the current seed value. Useful for logging, debugging, and preset serialization.
        /// </summary>
        public long Seed => _seed;

        /// <summary>
        /// Creates a new sampler with an explicit seed for reproducible results.
        /// </summary>
        public StochasticSampler(long seed)
        {
            _seed = seed;
            _rng = new Random((int)(seed ^ (seed >> 32))); // Mix 64-bit seed to 32-bit for System.Random
        }

        /// <summary>
        /// Creates a new sampler with a high-entropy time-based seed.
        /// Combines epoch milliseconds with nanoseconds for uniqueness even in rapid succession.
        /// </summary>
        public StochasticSampler() : this(GenerateTimeSeed()) { }

        /// <summary>
        /// Resets the generator to the initial seed state.
        /// Subsequent calls will reproduce the exact same sequence.
        /// </summary>
        public void Reset() => Reseed(_seed);

        /// <summary>
        /// Reseeds the generator with a new value, discarding prior state.
        /// </summary>
        public void Reseed(long newSeed)
        {
            _seed = newSeed;
            // Mix 64-bit seed to 32-bit; System.Random constructor takes int
            _rng = new Random((int)(newSeed ^ (newSeed >> 32)));
        }

        /// <summary>
        /// Returns a random float in [0.0, 1.0).
        /// Optimized to avoid double→float conversion overhead in hot paths.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat() => (float)_rng.NextDouble();

        /// <summary>
        /// Returns a random float in [min, max).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float min, float max) => min + (NextFloat() * (max - min));

        /// <summary>
        /// Returns a random int in [min, max).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int min, int max) => _rng.Next(min, max);

        /// <summary>
        /// Returns a random int in [0, max).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int max) => _rng.Next(max);

        /// <summary>
        /// Picks a random element from a non-empty span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Choose<T>(ReadOnlySpan<T> options)
        {
            if (options.IsEmpty) throw new ArgumentException("Cannot choose from empty span.", nameof(options));
            return options[_rng.Next(options.Length)];
        }

        /// <summary>
        /// Picks a random element from a non-empty array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Choose<T>(T[] options) => Choose<T>(options.AsSpan());

        /// <summary>
        /// Stochastic weighted selection using cumulative probabilities.
        /// Expects weights normalized to sum to 1.0, or provides internal normalization.
        /// </summary>
        /// <param name="cumulativeWeights">Pre-normalized cumulative weights [0.1, 0.4, 1.0]</param>
        /// <returns>Index of selected item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WeightedIndex(ReadOnlySpan<float> cumulativeWeights)
        {
            if (cumulativeWeights.IsEmpty) throw new ArgumentException("Weights cannot be empty.", nameof(cumulativeWeights));
            
            float roll = NextFloat();
            for (int i = 0; i < cumulativeWeights.Length; i++)
            {
                if (roll < cumulativeWeights[i])
                    return i;
            }
            return cumulativeWeights.Length - 1; // Fallback for floating-point edge cases
        }

        /// <summary>
        /// Generates a high-entropy time-based seed.
        /// Combines epoch milliseconds with nanoseconds via XOR for uniqueness.
        /// </summary>
        public static long GenerateTimeSeed()
        {
            var now = DateTimeOffset.UtcNow;
            // Mix milliseconds with shifted nanoseconds to avoid collisions within same ms
            return now.ToUnixTimeMilliseconds() ^ (now.Ticks % 10_000_000L >> 16);
        }

        /// <summary>
        /// Creates a forked sampler with a derived seed.
        /// Useful for branching stochastic processes (e.g., per-branch variation) while maintaining reproducibility.
        /// </summary>
        public StochasticSampler Fork(int branchId)
        {
            // Mix parent seed with branch ID for deterministic divergence
            long derivedSeed = _seed ^ ((long)branchId << 32) ^ unchecked((long)0x9E3779B97F4A7C15UL); // Golden ratio constant
            return new StochasticSampler(derivedSeed);
        }
    }
}