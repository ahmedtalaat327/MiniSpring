using AccioOracleKit;
using Oracle.ManagedDataAccess.Client;
using Spring.AccioHelpers;
using Spring.StaticVM;
using System;
using System.ComponentModel;


namespace Spring.Data
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public int TelNo { get; set; }
        public DateTime LastSeen { get; set; }
        public string UserInSession { get; set; }
        public string UserAuthLevel { get; set; }

        private int deptId = 0;
        [Browsable(false)]
        public int DepartmentId { get { return deptId; } set { deptId = value; } }

        public string DepartmentName { get { return User.GetDeptName(deptId).Name; } }

        /// <summary>
        /// This function is used by many other classes main goal here to convert Id from [int] to [String] in fastway
        /// we made it static to not create many objects in memory.
        /// </summary>
        /// <param name="deptId">int no that represents PK for real string name</param>
        /// <returns>department model object</returns>
        public static Department GetDeptName(int deptId)
        {
            var sqlCMD = Scripts.FetchMyData(VMCentral.DockingManagerViewModel.MyAppOnlyObjctConn, "departments", new string[] { "dept_name" }, new string[] { "dept_id" }, new string[] { $"{deptId}" }, "=", "and");

            Department dept = new Department();
            try
            {
            
                OracleDataReader dr = sqlCMD.ExecuteReader();

                if (dr.HasRows)
                {


                    while (dr.Read())
                    {
                        dept.Name = dr["dept_name"].ToString();
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

            return dept;
        }
    }
    public class Rule
    {
        public string ViewName { get; set; } = "my_name";
        public string Level { get; set; } = "super_user";
    }
}
