namespace RXDKNeighborhood.Models
{
    public class XboxItem
    {
        public string Name { get; set; }

        public string IpAddress { get; set; }

        public XboxItem(string name, string ipAddress)
        {
            Name = name;
            IpAddress = ipAddress;
        }
    }
}
