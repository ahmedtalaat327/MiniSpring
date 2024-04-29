/////////////////this is a pure VM file can be attatchd to any UI or FrameWork!////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////////
using AccioOracleKit;
using Oracle.ManagedDataAccess.Client;
using Spring.Data;
using Spring.ViewModel.Base;
using Spring.ViewModel.Command;
using System.Windows.Input;
using Spring.StaticVM;

namespace Spring.ViewModel
{
    public class LoginViewModel : BaseViewModel
    {
        #region ENUM for Phases
        /// <summary>
        /// Progress bar porperty phases set
        /// <see cref="WaitingProgress"/> <see cref="CurrentWait"/>
        /// </summary>
        public enum LoginVMLoadingPhase
        {
            Non, //reset
            ConnectionCheckWaiting,//first relycommand
            LoginCkeckWaiting//second relycommand
        }
        #endregion
        #region Poperties
        /// <summary>
        /// Property for input username
        /// </summary>
        public string CurrentUserName { get; set; } = "pipo";
        /// <summary>
        /// property for input passpord
        /// </summary>
        public string CurrentPassword { get; set; } = "pip@1234";
        /// <summary>
        /// Loading flag for prgress bar visible or not
        /// </summary>
        public bool Loading { get { return WaitingProgress; } }
        /// <summary>
        /// Current progress bar state
        /// </summary>
        public bool WaitingProgress { get; set; }
        /// <summary>
        /// Is logging in valid or not
        /// </summary>
        public bool ValidLogin { get; set; } = false;
        /// <summary>
        /// Determines whether the user is active or termminated
        /// </summary>
        public bool ValidSession { get; set; } = true; 
        /// <summary>
        /// Is Connection to DB is valid or not
        /// </summary>
        public bool ValidConnection { get; set; } = false;
        /// <summary>
        /// Current local user
        /// </summary>
        private User UserLocal { get { return new User { Id = 0 , DepartmentId = 0 , FullName = "" , UserName = CurrentUserName,Password = CurrentPassword, LastSeen = new DateTime(),TelNo = 11111111, UserAuthLevel="any",
         UserInSession = "no"}; } }
        /// <summary>
        /// All users currently avilable in database
        /// </summary>
        private List<User> UsersInAllDataBase = new List<User>(); 
        /// <summary>
        /// Dummy User to hold info to another way MVVM
        /// </summary>
        public static User UserLogged { get; set; }
        /// <summary>
        /// Visible flag to determine whether if the current form is visile or not
        /// </summary>
        public bool Visible { get { return ValidLogin ? false : true; }  }
        /// <summary>
        /// this for helping wait property when changing in VIEW 
        /// </summary>
        public LoginVMLoadingPhase CurrentWait { get; set; } = LoginVMLoadingPhase.Non;
        /// <summary>
        /// Shows any error
        /// </summary>
        public string ErrorDescription { get; set; } 
        #endregion


        #region Commands
        /// <summary>
        /// First commnad to beloaded
        /// </summary>
        public ICommand CheckConnectivityCommand { get; set; }
        /// <summary>
        /// Our unique way to handle login comparison
        /// </summary>
        public ICommand LoginCommand { get; set; }
        #endregion


      
        #region Constructor
        /// <summary>
        /// Constructor of VM
        /// </summary>
        /// <param name="contol">This is the Vuew UI conroler May change to another thing</param>
        public LoginViewModel()
        {
 
            CheckConnectivityCommand = new RelyCommand(async () => await CheckConndctionToServer());

            LoginCommand = new RelyCommand(async () => await Login());

        }
        #endregion


        #region Procedural methods
        /// <summary>
        /// Check whether connection valid or not?
        /// </summary>
        private async Task CheckConndctionToServer()
        {
            CurrentWait = LoginVMLoadingPhase.ConnectionCheckWaiting;
            await RunCommand(() => this.WaitingProgress, async () =>
            {
               
                //test oracle db connection
                if (await VMCentral.DockingManagerViewModel.ReadyMyDatabaseConn() == null)
                {
                    ValidConnection = false;
                   
                }
                else
                {
                    ValidConnection = true;
                   
                }


            });

               


               
        }
       
        /// <summary>
        /// All login methods step by step efficient way to get the cardential comparing
        /// </summary>
        public async Task Login()
        {
            if (!ValidConnection)
                return;

            CurrentWait = LoginVMLoadingPhase.LoginCkeckWaiting;

            await RunCommand(() =>  this.WaitingProgress, async () =>
            {
                try
                {
                     
                    UsersInAllDataBase = await LoadUsersFromDataBase(VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn);
                }
                catch (Exception ex)
                {
                    //Connection error for somereason so aggresive close that connection
                    VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Dispose(); VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Close();
                }
                finally
                {

                    //    VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Dispose(); VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Close();


                    if (await ComparisonProcedure(UsersInAllDataBase))
                    {

                        ValidLogin = true;

                        VMCentral.DockingManagerViewModel.WindowVisible = true;
                        VMCentral.DockingManagerViewModel.loggedUser = UserLogged;

                    }
                    else
                    {
                        ValidLogin = false;
                      
                    }
                }
            
            });

            
           
        }

        /// <summary>
        /// This is a brilliant task method can return all Users as instance.
        /// </summary>
        /// <param name="myOpenedTunnel">Current connection object</param>
        /// <returns></returns>
        private Task<List<User>> LoadUsersFromDataBase(OracleConnection myOpenedTunnel)
        {
           

          List<User> usersRemote = new List<User>();

          

            return Task.Run(() =>
            {

               
                var sqlCMD = Scripts.FetchMyData(myOpenedTunnel, "users", new string[] { "user_id", "user_name", "user_password", "user_auth", "user_full_name" ,"dept_id", "user_session" }, new string[] { "user_id", "user_auth" }, new string[] { "999", "'power'" }, "!=", "and");

                try
                {
                    OracleDataReader dr = sqlCMD.ExecuteReader();


                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {


                            usersRemote.Add(
                                new User()
                                {
                                    Id = Int32.Parse(dr["user_id"].ToString()),
                                    UserName = dr["user_name"].ToString(),
                                    Password = dr["user_password"].ToString(),
                                    FullName = dr["user_full_name"].ToString(),
                                    UserAuthLevel = dr["user_auth"].ToString(),
                                    DepartmentId = Int32.Parse(dr["dept_id"].ToString()),
                                    UserInSession = dr["user_session"].ToString()

                                });



                        }
                    }
                }
                catch (Exception xorcl)
                {
                    ErrorDescription = xorcl.Message;
                    //for debug purposes
                    Console.WriteLine(xorcl.Message);
                    //Connection error for somereason so aggresive close that connection
                    VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Dispose(); VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Close();

                }


                return usersRemote;
            });


        }
        /// <summary>
        /// Compare netween logged in user and the usrs in the database
        /// </summary>
        /// <param name="allUsers">List full of all uers from DB</param>
        /// <returns></returns>
        private Task<bool> ComparisonProcedure(List<User> allUsers)
        {

            bool feedBack = false;
          
            return Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(UserLocal.UserName) || string.IsNullOrEmpty(UserLocal.Password))
                {
                    feedBack = false;
                }
                else
                {
                    foreach (User usr in allUsers)
                    {
                        if (usr.UserName.Equals(UserLocal.UserName) && usr.Password == UserLocal.Password)
                        {
                            if (await CheckUserSessionCase(usr))
                            {
                                UserLogged = usr;
                                ValidSession = true;
                                feedBack = true;
                            }
                            else
                            {
                                ValidSession = false;
                                feedBack = false;
                            }
                            
                            break;
                        }
                        else
                        {
                            feedBack = false;
                        }
                    }
                }



                return feedBack;
            });
              

        }
        /// <summary>
        /// second layer of security to check if passed user is active or terminated.
        /// </summary>
        /// <param name="currentPassedUser">passed login user object</param>
        /// <returns></returns>
        private Task<bool> CheckUserSessionCase(User currentPassedUser)
        {
            bool feedBack = true;

            return Task.Run(() =>
            {

                if (currentPassedUser.UserInSession.Equals("ter") || currentPassedUser.UserInSession.Equals("off"))
                    feedBack = false;

                return feedBack;
            });
        }
            #endregion
        }
}
