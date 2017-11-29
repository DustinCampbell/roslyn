// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Remote.Serialization
{
    internal static class SerializationKindExtensions
    {
        private static readonly string[] s_strings;

        static SerializationKindExtensions()
        {
            var fields = typeof(SerializationKind).GetTypeInfo().DeclaredFields.Where(f => f.IsStatic);

            var maxValue = fields.Max(f => (int)f.GetValue(null));
            s_strings = new string[maxValue + 1];

            foreach (var field in fields)
            {
                var value = (int)field.GetValue(null);
                s_strings[value] = field.Name;
            }
        }

        public static string ToStringFast(this SerializationKind kind)
            => s_strings[(int)kind];
    }
}
