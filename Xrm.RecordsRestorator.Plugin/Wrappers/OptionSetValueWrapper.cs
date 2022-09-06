using Microsoft.Xrm.Sdk;
using System;

namespace Xrm.RecordsRestorator.Plugin.Wrappers
{
    internal class OptionSetValueWrapper
    {
        public string Label { get; private set; }

        public int Value { get; private set; }

        public OptionSetValueWrapper(OptionSetValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Value = value.Value;
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Label) ? Value.ToString() : Label;
    }
}
