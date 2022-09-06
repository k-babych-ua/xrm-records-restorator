namespace Xrm.RecordsRestorator.Plugin.Model
{
    internal class EntityItem
    {
        public string Name { get; set; }

        public string LogicalName { get; set; }

        public string PrimaryKey { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
