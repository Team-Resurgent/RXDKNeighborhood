namespace RXDKNeighborhood.ViewModels
{
    public enum ConsoleItemType
    {
        AddXbox,
        XboxOriginal
    }

    public class ConsoleItem
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public ConsoleItemType Type { get; set; }

        public bool HasDelete => Type != ConsoleItemType.AddXbox;

        public ConsoleItem()
        {
            Type = ConsoleItemType.AddXbox;
            Name = string.Empty;
            Description = string.Empty;
            ImageUrl = string.Empty;
        }
    }
}