using Microsoft.Xrm.Sdk;

namespace Xrm.RecordsRestorator.Plugin.Wrappers
{
    internal class MoneyWrapper
    {
        public decimal Value { get; set; }

        public MoneyWrapper(Money money)
        {
            Value = money.Value;
        }

        public override string ToString() => Value.ToString();
    }
}
