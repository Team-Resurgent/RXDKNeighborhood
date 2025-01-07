namespace RXDKNeighborhood
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ConsolePage), typeof(ConsolePage));
        }
    }
}
