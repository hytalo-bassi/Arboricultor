using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace LSystemTree.Core.Grammar
{
    /// <summary>
    /// Represents a sequence of L-system symbols. Immutable after construction.
    /// Designed for high-frequency string rewriting with minimal GC pressure.
    /// </summary>
    public sealed class LWord : IReadOnlyList<LSymbol>, IEquatable<LWord>
    {
        private readonly LSymbol[] _symbols;

        public int Count => _symbols.Length;
        public LSymbol this[int index] => _symbols[index];

        /// <summary>
        /// Creates a new immutable word from a sequence of symbols.
        /// Defensive copy ensures external mutations cannot affect the word.
        /// </summary>
        public LWord(IReadOnlyList<LSymbol> symbols)
        {
            if (symbols == null) throw new ArgumentNullException(nameof(symbols));
            _symbols = new LSymbol[symbols.Count];
            for (int i = 0; i < symbols.Count; i++)
                _symbols[i] = symbols[i];
        }

        /// <summary>
        /// Internal constructor used by <see cref="Builder"/> to avoid double-allocation.
        /// </summary>
        internal LWord(LSymbol[] symbols) => _symbols = symbols;

        public IEnumerator<LSymbol> GetEnumerator() => ((IEnumerable<LSymbol>)_symbols).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Exposes the underlying buffer as a read-only span for fast, allocation-free iteration.
        /// Critical for the turtle interpreter hot path.
        /// </summary>
        public ReadOnlySpan<LSymbol> AsSpan() => _symbols.AsSpan();

        /// <summary>
        /// Returns a shallow copy of this word. Useful for branching or snapshotting.
        /// </summary>
        public LWord Clone()
        {
            var copy = new LSymbol[_symbols.Length];
            Array.Copy(_symbols, copy, _symbols.Length);
            return new LWord(copy);
        }

        /// <summary>
        /// Compact debug string showing only letter identifiers. 
        /// Use <see cref="AsSpan"/> or enumeration for full parameter data.
        /// </summary>
        public override string ToString()
        {
            if (_symbols.Length == 0) return string.Empty;
            
            var sb = new StringBuilder(_symbols.Length * 2);
            foreach (var sym in _symbols)
                sb.Append(sym.Letter);
            return sb.ToString();
        }

        public bool Equals(LWord? other)
        {
            if (other is null || Count != other.Count) return false;
            for (int i = 0; i < Count; i++)
                if (!_symbols[i].Equals(other._symbols[i])) return false;
            return true;
        }

        public override bool Equals(object? obj) => Equals(obj as LWord);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var sym in _symbols)
                    hash = (hash * 397) ^ sym.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// High-performance builder for L-system expansion. Uses ArrayPool to minimize allocations.
        /// Must be disposed or have <see cref="ToWord"/> called to return buffers to the pool.
        /// </summary>
        public sealed class Builder : IDisposable
        {
            private LSymbol[] _buffer;
            private int _count;
            private bool _isDisposed;

            public Builder(int initialCapacity = 64)
            {
                if (initialCapacity <= 0) initialCapacity = 64;
                _buffer = ArrayPool<LSymbol>.Shared.Rent(initialCapacity);
                _count = 0;
                _isDisposed = false;
            }

            public int Count => _count;

            public void Add(LSymbol symbol)
            {
                EnsureCapacity(1);
                _buffer[_count++] = symbol;
            }

            public void AddRange(IReadOnlyList<LSymbol> symbols)
            {
                if (symbols == null) throw new ArgumentNullException(nameof(symbols));
                int len = symbols.Count;
                if (len == 0) return;
                EnsureCapacity(len);
                for (int i = 0; i < len; i++)
                    _buffer[_count + i] = symbols[i];
                _count += len;
            }

            public void AddRange(ReadOnlySpan<LSymbol> span)
            {
                if (span.IsEmpty) return;
                EnsureCapacity(span.Length);
                span.CopyTo(_buffer.AsSpan(_count));
                _count += span.Length;
            }

            public void Clear() => _count = 0;

            /// <summary>
            /// Finalizes the builder and returns a new immutable LWord.
            /// The builder is unusable after this call.
            /// </summary>
            public LWord ToWord()
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(Builder));
                _isDisposed = true;

                if (_count == 0)
                {
                    ArrayPool<LSymbol>.Shared.Return(_buffer, true);
                    return new LWord(Array.Empty<LSymbol>());
                }

                var word = new LSymbol[_count];
                Array.Copy(_buffer, word, _count);
                ArrayPool<LSymbol>.Shared.Return(_buffer, true);
                _buffer = null!;
                return new LWord(word);
            }

            private void EnsureCapacity(int additional)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(Builder));
                int needed = _count + additional;
                if (needed <= _buffer.Length) return;

                int newSize = Math.Max(_buffer.Length * 2, needed);
                var newBuffer = ArrayPool<LSymbol>.Shared.Rent(newSize);
                
                if (_count > 0)
                {
                    Array.Copy(_buffer, newBuffer, _count);
                    ArrayPool<LSymbol>.Shared.Return(_buffer, true);
                }
                _buffer = newBuffer;
            }

            public void Dispose()
            {
                if (!_isDisposed && _buffer != null)
                {
                    ArrayPool<LSymbol>.Shared.Return(_buffer, true);
                    _isDisposed = true;
                }
            }
        }
    }
}