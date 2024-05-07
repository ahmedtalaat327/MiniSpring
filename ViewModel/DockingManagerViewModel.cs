
using AccioOracleKit;
using Microcharts;
using Oracle.ManagedDataAccess.Client;
using SkiaSharp;
using Spring.AccioHelpers;
using Spring.Data;
using Spring.StaticVM;
using Spring.ViewModel.Base;
using Spring.ViewModel.Command;
using System.Collections.ObjectModel;
using System.Windows.Input;



namespace Spring.ViewModel
{
    /// <summary>
    /// this class represent the charts board propeeties for each board
    /// </summary>
    public class ChartBoard : BaseViewModel
    {
        #region Private Fields
        private string view_name = "noname";
        private Chart View_body = null;
        #endregion
        #region Public props
        public string Title { get; set; } = "no_title";
        public string Details { get; set; } = "no_detials";
        public required int Count { get; set; } = 0;
        //when set this property you must update the property inside the vmcentral - docking this for feeding the basepagevm
        public required string NameInRecord { get { return view_name; } set { if (view_name != value) StaticVM.VMCentral.DockingManagerViewModel.ViewName = view_name = value; } }
        public bool ActiveViewChart { get => StaticVM.VMCentral.BasePageViewModel.ActiveView; }
        //does not support auto property changed
        public Chart MyChart { get {
                return View_body;
            }
            set { 
                if (View_body != value) View_body = value;
                OnPropertyChanged(nameof(MyChart)); }
        } 
        #endregion

    }
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

        public ObservableCollection<ChartBoard> OverViewSubBoard { get; set; } = new ObservableCollection<ChartBoard>();
        #region commands
        /// <summary>
        /// Our unique way to handle login comparison
        /// </summary>
        public ICommand LogoutCommand { get; set; }
        public ICommand FetchAllRulesGroupes { get; set; }

        public ICommand LoadCurrentChartsCommands { get; set; }
        #endregion

        #endregion
        public DockingManagerViewModel()
        {
            Loading = false;

             
            LogoutCommand = new RelyCommand(async () => { await SignOutFromServerSQL(); });

            FetchAllRulesGroupes = new RelyCommand(async () => { await GetRuleViews(); });

            LoadCurrentChartsCommands = new RelyCommand(async () => { await GetChartsBuild(); });


         
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
        private async Task GetChartsBuild()
        {
            await RunCommand(() => this.Loading, async () =>
            {
                await Task.Delay(1);

           


                /*
                        ChartEntry[] entrs = new ChartEntry[] { 
                       // new ChartEntry(21){Label="HVC",ValueLabel="112",Color = SKColor.Parse("#c64a44")},
                       // new ChartEntry(73){Label="Engineering",ValueLabel="432",Color = SKColor.Parse("#0099ff")},
                       // new ChartEntry(6){Label="IT",ValueLabel="3",Color = SKColor.Parse("#b3db86")},


                        };
                */

              
               


                List<string> colorNames = new List<string>() { "#c64a44", "#0099ff", "#b3db86", "#1e0000", "#601aa9", "#b45f06", "#0c343d"
                                                      , "#CA6F1E", "#0779ff","#13aff12"};

            List<ChartEntry> entriess = new List<ChartEntry>();

            for (int k = 0; k < VMCentral.UsersStatisticsViewModel.AllCounters.Count; k++)
            {
                int all = 0;
                foreach (string co in VMCentral.UsersStatisticsViewModel.AllCounters)
                {
                    all = +Int32.Parse(co);
                }

                var perc = 100*(float.Parse(VMCentral.UsersStatisticsViewModel.AllCounters[k]) / all);
                entriess.Add(new ChartEntry(perc) { Label = $"{VMCentral.UsersStatisticsViewModel.AllDepartments[k].Name}", ValueLabel = $"{VMCentral.UsersStatisticsViewModel.AllCounters[k]}", Color = SKColor.Parse($"{colorNames[k]}") });
            }


            VMCentral.DockingManagerViewModel.OverViewSubBoard[0].MyChart = new DonutChart()
            {
                Entries = entriess.ToArray(),
                BackgroundColor = SKColors.Transparent
            };

            });
        }
    }
}

