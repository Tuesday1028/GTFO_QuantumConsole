﻿using Hikaria.QC.Bootstrap;
using Hikaria.QC.Utilities;

namespace Hikaria.QC
{
    /// <summary>
    /// Parser for all types that are generic constructions of a single type.
    /// </summary>
    public abstract class GenericQcParser : IQcParser
    {
        /// <summary>
        /// The incomplete generic type of this parser.
        /// </summary>
        protected abstract Type GenericType { get; }

        private Func<string, Type, object> _recursiveParser;

        protected GenericQcParser()
        {
            if (!GenericType.IsGenericType)
            {
                throw new ArgumentException(QuantumConsoleBootstrap.Localization.Get(61));
            }

            if (GenericType.IsConstructedGenericType)
            {
                throw new ArgumentException(QuantumConsoleBootstrap.Localization.Get(62));
            }
        }

        public virtual int Priority => -500;

        public bool CanParse(Type type)
        {
            return type.IsGenericTypeOf(GenericType);
        }

        public virtual object Parse(string value, Type type, Func<string, Type, object> recursiveParser)
        {
            _recursiveParser = recursiveParser;
            return Parse(value, type);
        }

        protected object ParseRecursive(string value, Type type)
        {
            return _recursiveParser(value, type);
        }

        protected TElement ParseRecursive<TElement>(string value)
        {
            return (TElement)_recursiveParser(value, typeof(TElement));
        }

        public abstract object Parse(string value, Type type);
    }
}