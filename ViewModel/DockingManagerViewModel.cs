
using AccioOracleKit;
using Microcharts;
using MiniSpring.Views.Pages.ChartsPages.ViewModel;
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
        private Chart View_body = null;
        #endregion
        #region Public props
        public string Title { get; set; } = "no_title";
        public string Details { get; set; } = "no_detials";
        //when set this property you must update the property inside the vmcentral - docking this for feeding the basepagevm
        public required string NameInRecord { get; set; }
        public bool ActiveViewChart { get; set; } = false;
        //does not support auto property changed
        public Chart MyChart { get {
                return View_body;
            }
            set {
                if (View_body != value)
                {
                   
                    View_body = value;
                    OnPropertyChanged(nameof(MyChart));
                }
            }
        }

        /// <summary>
        /// Loading flag for prgress bar visible or not
        /// </summary>
        public bool Loading { get { return WaitingProgress; } }
        public CheckRulesPhase CheckRPhase { get; set; }
        /// <summary>
        /// Current progress bar state
        /// </summary>
        public bool WaitingProgress { get; set; }
        #endregion
        #region Private Property

        private List<MiniSpring.Views.Pages.ChartsPages.ViewModel.Rule> RuleRecords = new System.Collections.Generic.List<MiniSpring.Views.Pages.ChartsPages.ViewModel.Rule>();
        #endregion

        #region Commnands
        public ICommand CheckViewStateOnRules { get; set; }
        #endregion
        public ChartBoard()
        {

            ActiveViewChart = false;

            CheckViewStateOnRules = new RelyCommand(async () => await FindMyActiveView());

            CheckViewStateOnRules.Execute(true);



        }

        /// <summary>
        /// this func is mainly depend on collecting all records from rules table
        /// depending on dept_id from logged user what ever admin or not 
        /// then save the results in list created for specific purpose.
        /// </summary>
        private async Task CollectFromRules()
        {
            RuleRecords.Clear();



            await Task.Delay(1);


            var sqlCMD = Scripts.FetchMyData(VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn,
           "rules",
           new string[] { "rule_id", "rule_view", "dept_id", "rule_level" }, new string[] { "dept_id" }, new string[] { VMCentral.DockingManagerViewModel.loggedUser.DepartmentId.ToString() }, "=", "and", true, "rule_id");

            try
            {
                OracleDataReader dr = sqlCMD.ExecuteReader();

                while (dr.Read())
                {
                    RuleRecords.Add(
                        new MiniSpring.Views.Pages.ChartsPages.ViewModel.Rule
                        {
                            ViewName = dr["rule_view"].ToString(),
                            Level = dr["rule_level"].ToString()

                        });
                }
            }
            catch (Exception xorcl)
            {
                //for debug purposes
                Console.WriteLine(xorcl.Message);
                //Connection error for somereason so aggresive close that connection
                VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Dispose(); VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Close();

            }


        }
        /// <summary>
        /// Compare all records we have
        /// </summary>
        /// <param name="tag_name"></param>
        /// <returns></returns>
        private async Task CompareCurrentViews()
        {
            await Task.Delay(12);

            foreach (var rule in RuleRecords)
            {
                if (rule.ViewName.Contains(NameInRecord) && rule.Level == VMCentral.DockingManagerViewModel.loggedUser.UserAuthLevel)
                {
                    ActiveViewChart = true;
                    OnPropertyChanged(nameof(ActiveViewChart));
                }
                if (rule.ViewName.Contains("all") && rule.Level == VMCentral.DockingManagerViewModel.loggedUser.UserAuthLevel)
                {
                    ActiveViewChart = true;
                    OnPropertyChanged(nameof(ActiveViewChart));

                }


            }

        }
        /// <summary>
        /// Procedure..
        /// </summary>
        /// <returns></returns>
        public async Task FindMyActiveView()
        {

            CheckRPhase = CheckRulesPhase.InChecking;

            await RunCommand(() => this.WaitingProgress, async () =>
            {
                await CollectFromRules();
                await CompareCurrentViews();
            });

        }

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

        public ICommand RefreshAllChartsCollectedCommnads { get; set; }
        #endregion

        #endregion
        public DockingManagerViewModel()
        {
            Loading = false; PreivilagesScored = "Groupe: ";

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
            if (_firstLoad)
                MyAppOnlyObjctConn = await GetOracleConnection(false);

            return MyAppOnlyObjctConn;


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
                catch (Exception rt)
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
                new string[] { "rule_id", "rule_view", "dept_id", "rule_level" }, new string[] { "dept_id", "rule_level" }, new string[] { VMCentral.DockingManagerViewModel.loggedUser.DepartmentId.ToString(), $"'{VMCentral.DockingManagerViewModel.loggedUser.UserAuthLevel}'" }, "=", "and", true, "rule_id");

                 try
                 {
                     OracleDataReader dr = sqlCMD.ExecuteReader();

                     while (dr.Read())
                     {

                         var m_rule = new MiniSpring.Views.Pages.ChartsPages.ViewModel.Rule
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

                    var perc = 100 * (float.Parse(VMCentral.UsersStatisticsViewModel.AllCounters[k]) / all);
                    entriess.Add(new ChartEntry(perc) { Label = $"{VMCentral.UsersStatisticsViewModel.AllDepartments[k].Name}", ValueLabel = $"{VMCentral.UsersStatisticsViewModel.AllCounters[k]}", Color = SKColor.Parse($"{colorNames[k]}") });
                }



                VMCentral.DockingManagerViewModel.OverViewSubBoard[0].MyChart = new DonutChart()
                {
                    Entries = entriess.ToArray(),
                    BackgroundColor = SKColor.Parse("ac99ea"),
                    LabelTextSize = 22,
                    LabelColor = SKColor.Parse("#5a11b6"),


                };


                foreach (var brd in VMCentral.DockingManagerViewModel.OverViewSubBoard.ToList())
                {
                    if (!brd.ActiveViewChart)
                        VMCentral.DockingManagerViewModel.OverViewSubBoard.Remove(brd);
                }


                RefreshAllChartsCollectedCommnads =
                new RelyCommand(async () => { await
                 Task.Run(() => { 
                VMCentral.UsersStatisticsViewModel.GetCountingAndStoreThem.Execute(true);
                 });
                });
            });
        }
    }
}

