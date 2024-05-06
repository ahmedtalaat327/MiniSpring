using AccioOracleKit;
using Oracle.ManagedDataAccess.Client;
using Spring.StaticVM;
using Spring.ViewModel.Base;
using Spring.ViewModel.Command;
using System.Windows.Input;

namespace MiniSpring.Views.Pages.ChartsPages.ViewModel
{
    
     
     public class Rule
     {

         public string ViewName { get; set; } = "my_name";
         public string Level { get; set; } = "super_user";
     }

   
     public class BasePageViewModel : BaseViewModel
     {


            #region Private Property
            private List<Rule> RuleRecords = new System.Collections.Generic.List<Rule>();
            #endregion
            #region Public Property
            public bool ActiveView { get; set; } = false;

            /// <summary>
            /// Loading flag for prgress bar visible or not
            /// </summary>
            public bool Loading { get { return WaitingProgress; } }
            /// <summary>
            /// Current progress bar state
            /// </summary>
            public bool WaitingProgress { get; set; }
            #endregion
            #region Commnands
            public ICommand CheckViewStateOnRules { get; set; }
            #endregion
            public BasePageViewModel()
            {

                ActiveView = false;

                CheckViewStateOnRules = new RelyCommand(async () => await FindMyActiveView());
            }
            /// <summary>
            /// this func is mainly depend on collecting all records from rules table
            /// depending on dept_id from logged user what ever admin or not 
            /// then save the results in list created for specific purpose.
            /// </summary>
            private async Task CollectFromRules()
            {
                RuleRecords.Clear();


                await RunCommand(() => this.WaitingProgress, async () =>
                {
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
                                new Rule
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
                });

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
                    if (rule.ViewName.Contains(VMCentral.DockingManagerViewModel.ViewName) && rule.Level == VMCentral.DockingManagerViewModel.loggedUser.UserAuthLevel)
                    {
                        ActiveView = true;
                        OnPropertyChanged(nameof(ActiveView));
                    }
                    if (rule.ViewName.Contains("all") && rule.Level == VMCentral.DockingManagerViewModel.loggedUser.UserAuthLevel)
                    {
                        ActiveView = true;
                        OnPropertyChanged(nameof(ActiveView));

                    }


                }
            }
            /// <summary>
            /// Procedure..
            /// </summary>
            /// <returns></returns>
            private async Task FindMyActiveView()
            {

                await CollectFromRules();
                await CompareCurrentViews();

            }
        }
    
      }
