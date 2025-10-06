namespace RXDKXBDM.Models
{
    public class DriveItem
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public bool CanShowProperties => true;

        public DriveItem(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
