using AccioOracleKit;
using Oracle.ManagedDataAccess.Client;



namespace Spring.AccioHelpers
{
    public static class AccioEasyHelpers
    {
        
       
        

        /// <summary>
        /// Test connectivity to database 
        /// </summary>
        /// <param name="autoclose">show if automatic connection needs to be closed or not</param>
        /// <returns></returns>
        public static OracleConnection ReadParamsThenConnectToDB(bool autoclose)
        {   
            return Scripts.TestConnection(new[] { "192.168.212.34", "1521", "store", "store" }, autoclose);
        }
     
        /// <summary>
        /// This funcs to reconnect with new configurations only MobDevices
        /// </summary>
        /// <param name="autoclose"></param>
        /// <param name="infoToConn"></param>
        /// <returns></returns>
        public static OracleConnection NewConfigsToConnectToDB(bool autoclose, string infoToConn) {


            if (!infoToConn.Contains(":"))
            {
                return ReadParamsThenConnectToDB(false);
            }
            else
            {
                string ip = "";
                string port = "";

                string dumOb = infoToConn;

                ip = dumOb.Substring(0, infoToConn.IndexOf(':'));

                port = infoToConn.Substring(ip.Length + 1, infoToConn.Length - ip.Length - 1);

                return Scripts.TestConnection(new[] { $"{ip}", $"{port}", "store", "store" }, autoclose);
            }
        }
        
    }
    
}
