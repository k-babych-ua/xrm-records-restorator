using Microsoft.Xrm.Sdk;
using System.Collections.Generic;

namespace Xrm.RecordsRestorator.Plugin.Wrappers
{
    internal class AttributeWrapper
    {
        public object Value { get; private set; }

        public string Key { get; private set; }

        public AttributeWrapper(KeyValuePair<string, object> keyValue) : this(keyValue.Key, keyValue.Value) 
        {
        }

        public AttributeWrapper(string key, object value)
        {
            Key = key;

            Value = value switch
            {
                EntityReference entityReference => new EntityReferenceWrapper(entityReference),
                OptionSetValue optionSetValue => new OptionSetValueWrapper(optionSetValue),
                Money money => new MoneyWrapper(money),
                _ => value
            };
        }

        public override string ToString() => Value.ToString();
    }
}
