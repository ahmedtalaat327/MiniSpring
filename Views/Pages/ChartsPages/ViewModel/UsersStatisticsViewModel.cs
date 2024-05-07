using AccioOracleKit;
using Oracle.ManagedDataAccess.Client;
using Spring.Data;
using Spring.StaticVM;
using Spring.ViewModel.Base;
using Spring.ViewModel.Command;
using System.Windows.Input;

namespace MiniSpring.Views.Pages.ChartsPages.ViewModel
{
    public enum LoadDataPhase
    {
        Non = 0,
        InLoad = 1
       
    }
    public class UsersStatisticsViewModel : BaseViewModel
    {
        #region Public Properties
        public List<Department> AllDepartments = new List<Department>();

        public List<string> AllCounters = new List<string>();
        public bool LoadingChartData { get; set; }
        public LoadDataPhase LoadPhase { get; set; }
        public bool Automated { get; set; }
        //DispatcherTimer AutoUpdate { get; set; }
        #endregion
        #region Commands
        public ICommand GetCountingAndStoreThem { get; set; }
        #endregion
        public UsersStatisticsViewModel()
        {
            LoadingChartData = false;
            Automated = true;

            GetCountingAndStoreThem = new RelyCommand(async () => await MeasureMyData());

            /*
            //  DispatcherTimer setup
            AutoUpdate = new System.Windows.Threading.DispatcherTimer();
            AutoUpdate.Tick += new EventHandler(
                (e, r) =>
                {

                    GetCountingAndStoreThem.Execute(true);



                }

                );
            AutoUpdate.Interval = TimeSpan.FromSeconds(60);

            if (Automated)
                AutoUpdate.Start();
            */

           

        }

        /// <summary>
        /// Load updated values for VM properties
        /// </summary>
        /// <returns></returns>
        private async Task MeasureMyData()
        {
            LoadPhase = LoadDataPhase.InLoad;
            await RunCommand(() => LoadingChartData, async () =>
            {
                //this check happens only her if the connection is not established due to error in start of the app
                if (VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn == null)
                    return;

                await Task.Delay(12);
                AllDepartments.Clear(); AllCounters.Clear();
                AllDepartments = await ReadAllDepartments(VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn);

                foreach (var dept in AllDepartments)
                {
                    AllCounters.Add(
                        await GetEachDeptCounting(VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn, dept.Id)
                        ); ;
                }

            });
          
        }
        /// <summary>
        /// Read all depts from sql-exec
        /// </summary>
        /// <param name="myOpenedTunnel"></param>
        /// <returns></returns>
        private Task<List<Department>> ReadAllDepartments(OracleConnection myOpenedTunnel)
        {


            return Task.Run(() =>
            {

                var sqlCMD = Scripts.FetchMyData(myOpenedTunnel, "departments", new string[] { "dept_id", "dept_name" }, new string[] { "dept_id" }, new string[] { "-1" }, ">", "and");
                OracleDataReader dr = null;
                List<Department> depts = new List<Department>();
                try
                {
                    dr = sqlCMD.ExecuteReader();
                    if (dr.HasRows)
                    {


                        while (dr.Read())
                        {
                            depts.Add(new Department() { Id = Int32.Parse(dr["dept_id"].ToString()), Name = dr["dept_name"].ToString() });

                        }
                    }
                }
                catch (Exception xorcl)
                {
                    //for debug purposes
                    Console.WriteLine(xorcl.Message);
                    //Connection error for somereason so aggresive close that connection
                    VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Dispose(); VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Close();

                }









                return depts;

            });
        }

        private Task<string> GetEachDeptCounting(OracleConnection myOpenedTunnel, int dept_id)
        {
            return Task.Run(() =>
            {
                int count = 0;
                var sqlCMD = Scripts.GetCountConditionedWithFields(myOpenedTunnel, "users", new string[] { "user_id" }, new string[] { "dept_id" }, new string[] { dept_id.ToString() }, "=", "and");
                OracleDataReader dr = null;

                try
                {
                    dr = sqlCMD.ExecuteReader();
                    if (dr.HasRows)
                    {


                        while (dr.Read())
                        {
                            count = Int32.Parse(dr[0].ToString());

                        }
                    }
                }
                catch (Exception xorcl)
                {
                    //for debug purposes
                    Console.WriteLine(xorcl.Message);
                    //Connection error for somereason so aggresive close that connection
                    VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Dispose(); VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn.Close();

                }



                return count.ToString();

            });
        }
    }
}
