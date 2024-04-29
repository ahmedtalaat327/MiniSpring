using MiniSpring.Views.Pages;

namespace MiniSpring
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //login page
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            //onboard page
            Routing.RegisterRoute(nameof(OnBoardPage), typeof(OnBoardPage));

        }
    }
}
