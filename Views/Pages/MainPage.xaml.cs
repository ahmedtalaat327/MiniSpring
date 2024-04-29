using Spring.AccioHelpers;
using Spring.StaticVM;
using Spring.ViewModel;
using System.DirectoryServices.Protocols;

namespace MiniSpring.Views.Pages
{
    public partial class MainPage : ContentPage
    {
        private LoginViewModel loginVMContext = new LoginViewModel();
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = loginVMContext;
            this.loginVMContext.PropertyChanged += loginVMContext_PropertyChanged;
        }

        private void LoginBtn_Clicked(object sender, EventArgs e)
        {
            loginVMContext.LoginCommand.Execute(true);
        }

        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            loginVMContext.CheckConnectivityCommand.Execute(true);
        }
        #region Property Additonal Even Handle [Special to WINFORMS UI and MAUI]
        // when some propwerty change [Reason: to take out any View UI from VM]
        private void loginVMContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //make sure we are in same VM and same property.
            if (e.PropertyName == nameof(this.loginVMContext.WaitingProgress) && this.loginVMContext.GetType() == typeof(LoginViewModel))
            {
                //After Loading property finish in RELYCOMMAND
                if (!this.loginVMContext.WaitingProgress)
                {
                    //Pick which phase we are in
                    if (this.loginVMContext.CurrentWait == LoginViewModel.LoginVMLoadingPhase.ConnectionCheckWaiting)
                    {
                        if (!this.loginVMContext.ValidConnection)
                        {
                            //    new AdvOptions().ShowError_Connection(AdvOptions.GetForm(AdvOptions.GetHandleByTitle("Spring")));
                             DisplayAlert("Connection not set", "Your session is terminated due to error in connection to server", "OK");
                        }
                        else
                        {
                            DisplayAlert("Connection is valid", "Connection to server is successfully done", "OK");
                           
                        }
                        //reset phase of loading fter all logic done!
                        this.loginVMContext.CurrentWait = LoginViewModel.LoginVMLoadingPhase.Non;
                    }
                    //Pick which phase we are in
                    if (this.loginVMContext.CurrentWait == LoginViewModel.LoginVMLoadingPhase.LoginCkeckWaiting)
                    {
                        if (!this.loginVMContext.ValidSession)
                        {
                            DisplayAlert("Login error", "Account is inactive!", "OK");
                        }
                        else if (this.loginVMContext.ValidLogin)
                        {

                            DisplayAlert("You're in!", "Login is successfully done", "OK");
                            VMCentral.DockingManagerViewModel.FetchAllRulesGroupes.Execute(true);
                            Shell.Current.GoToAsync(nameof(OnBoardPage));
                        }
                        else if (!this.loginVMContext.ValidLogin)
                        {
                            DisplayAlert("Login failed", "some of your entries are wriong, Try again!", "OK");
                        }
                        //reset phase of loading fter all logic done!
                        this.loginVMContext.CurrentWait = LoginViewModel.LoginVMLoadingPhase.Non;
                        //reset the value of active account checker if it's changed in any phase
                        this.loginVMContext.ValidSession = true;
                    }
                }
            }
        }
        #endregion

        private async void SettingsBtn_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayPromptAsync("New configs will be set", "Put down the IP for remote server and the port then confirm.", "Test new config","Cancel");
           
            if (result!=null)
            {
                VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn = AccioEasyHelpers.NewConfigsToConnectToDB(false, (string)result);
                loginVMContext.CheckConnectivityCommand.Execute(true);
            }

        }
    }

}
