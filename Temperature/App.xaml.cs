namespace Temperature
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Set the shell you defined as the main page
            MainPage = new AppShell();
        }
    }

}