
using AccioOracleKit;
using Oracle.ManagedDataAccess.Client;
using Spring.AccioHelpers;
using Spring.Data;
using Spring.StaticVM;
using Spring.ViewModel.Base;
using Spring.ViewModel.Command;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using static Spring.ViewModel.LoginViewModel;


namespace Spring.ViewModel
{
    public class DockingManagerViewModel : BaseViewModel
    {

        #region ENUM for Phases
        /// <summary>
        /// Progress bar porperty phases set
        /// <see cref="WaitingProgress"/> <see cref="CurrentWait"/>
        /// </summary>
        public enum LogoutVMLoadingPhase
        {
            Non, //reset
            TerminatingCheckWaiting,//ConnTermination relycommand
        }
        #endregion
        #region Fields
        private bool _firstLoad = true;
        #endregion
        #region Properties
        /// <summary>
        /// flag to detrmine wheter the window is visible or not
        /// </summary>
        public bool WindowVisible { get; set; } = false;
        /// <summary>
        /// Logged user currently
        /// </summary>
        public User loggedUser { get; set; } = new User() { FullName = "xxx.xxx.xx" };
        /// <summary>
        /// Name on the account button
        /// </summary>
        public string NameBannser { get { return loggedUser.FullName; } }
        /// <summary>
        /// Detiremines whether progress bar is shown or not also determines if loading happens or not.
        /// </summary>
        public bool Loading { get; set; }
        /// <summary>
        /// the current object opened and closed depend on params will be loaded onLoad only
        /// </summary>
        public OracleConnection MyAppOnlyObjctConn { get; set; }
        /// <summary>
        /// this for helping wait property when changing in VIEW 
        /// </summary>
        public LogoutVMLoadingPhase CurrentWait { get; set; } = LogoutVMLoadingPhase.Non;
        /// <summary>
        /// detemines if we are out or not
        /// </summary>
        public bool SuccessLogOut { get; set; } = false;

        /// <summary>
        /// this is related to how pages will be managed
        /// </summary>
        public string ViewName { get; set; } = "none";
        public string PreivilagesScored { get; set; } = "Groupe: "; 
        #region commands
        /// <summary>
        /// Our unique way to handle login comparison
        /// </summary>
        public ICommand LogoutCommand { get; set; }
        public ICommand FetchAllRulesGroupes { get; set; }
        #endregion

        #endregion
        public DockingManagerViewModel()
        {
            Loading = false;


            LogoutCommand = new RelyCommand(async () => { await SignOutFromServerSQL(); });

            FetchAllRulesGroupes = new RelyCommand(async () => { await GetRuleViews(); });
        }
        /// <summary>
        /// check current tunnle conn
        /// </summary>
        /// <returns></returns>
        public async Task<OracleConnection> ReadyMyDatabaseConn()
        {
                if(_firstLoad)
                MyAppOnlyObjctConn = await GetOracleConnection(false);

                return  MyAppOnlyObjctConn;
            

        }
        /// <summary>
        /// Get the oracle Connection
        /// </summary>
        /// <param name="closeOrNot">Flag to close the connection or not before using in other commands statements</param>
        /// <returns></returns>
        private Task<OracleConnection> GetOracleConnection(bool closeOrNot)
        {
            return Task.Run(() =>
            {
                _firstLoad = false;
                return AccioEasyHelpers.ReadParamsThenConnectToDB(closeOrNot);
            });
        }
        /// <summary>
        /// this procedure to end the current connection object
        /// </summary>
        /// <returns></returns>
        private Task TerminateCurrentConn()
        {
            return Task.Run(() =>
            {
                MyAppOnlyObjctConn.Close();
                MyAppOnlyObjctConn.Dispose();
            });
        }
        /// <summary>
        /// Command to sign out
        /// </summary>
        /// <returns></returns>
        public async Task SignOutFromServerSQL()
        {
            CurrentWait = LogoutVMLoadingPhase.TerminatingCheckWaiting;

            await RunCommand(() => this.Loading, async () =>
            {
                try
                {
                    await TerminateCurrentConn();
                    SuccessLogOut = true;
                }
                catch(Exception rt)
                {
                    SuccessLogOut = false;

                }
                /*
                //test oracle db connection
                if (await VMCentral.DockingManagerViewModel.ReadyMyDatabaseConn() == null)
                {
                  //  ValidConnection = false;

                }
                else
                {
                  //  ValidConnection = true;

                }
                */


            });


        }



        private async Task GetRuleViews()
        {
           await RunCommand(() => this.Loading, async () =>
            {
                await Task.Delay(1);


                var sqlCMD = Scripts.FetchMyData(VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn,
               "rules",
               new string[] { "rule_id", "rule_view", "dept_id", "rule_level" }, new string[] { "dept_id","rule_level" }, new string[] { VMCentral.DockingManagerViewModel.loggedUser.DepartmentId.ToString(), $"'{VMCentral.DockingManagerViewModel.loggedUser.UserAuthLevel}'" }, "=", "and", true, "rule_id");

                try
                {
                    OracleDataReader dr = sqlCMD.ExecuteReader();

                    while (dr.Read())
                    {
                       
                          var m_rule =  new Rule
                            {
                                ViewName = dr["rule_view"].ToString(),
                                Level = dr["rule_level"].ToString()

                            };
                        PreivilagesScored += $" {m_rule.ViewName}";
                    }


                    
                }
                catch (Exception xorcl)
                {
                    //for debug purposes
                    Console.WriteLine(xorcl.Message);
                    //Connection error for somereason so aggresive close that connection
                    VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Dispose(); VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Close();

                }
            });

        }
    }
}

