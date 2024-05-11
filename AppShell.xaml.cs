using MiniSpring.Views.Pages;
using Spring.StaticVM;

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


            this.Navigating += AppShell_Navigating;
        }

        private void AppShell_Navigating(object? sender, ShellNavigatingEventArgs e)
        {
            base.OnNavigating(e);

            if (e.Source == ShellNavigationSource.Pop)
                {
                    e.Cancel();
                }
                
           
        }

    }
}
