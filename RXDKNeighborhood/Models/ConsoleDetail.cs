namespace RXDKNeighborhood.Models
{
    public class ConsoleDetail
    {
        public string Name { get; set; }
     
        public string IpAddress { get; set; }

        public ConsoleDetail()
        {
            Name = string.Empty;
            IpAddress = string.Empty;
        }

        public ConsoleDetail(string name, string ipAddress)
        {
            Name = name;
            IpAddress = ipAddress;
        }
    }
}
