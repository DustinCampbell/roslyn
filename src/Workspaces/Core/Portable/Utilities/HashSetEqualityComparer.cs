// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Roslyn.Utilities
{
    internal class HashSetEqualityComparer<T> : IEqualityComparer<ImmutableHashSet<T>>
    {
        public static HashSetEqualityComparer<T> Instance { get; } = new HashSetEqualityComparer<T>();

        public bool Equals(ImmutableHashSet<T> x, ImmutableHashSet<T> y)
        {
            if (x == y)
            {
                return true;
            }

            if (x.Count != y.Count)
            {
                return false;
            }

            foreach (var v in x)
            {
                if (!y.Contains(v))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(ImmutableHashSet<T> obj)
        {
            var hash = 0;
            foreach (var o in obj)
            {
                unchecked
                {
                    hash += o.GetHashCode();
                }
            }

            return hash;
        }
    }
}
