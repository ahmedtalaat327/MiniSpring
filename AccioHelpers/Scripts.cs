﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;


namespace AccioOracleKit
{
    public static class Scripts
    {
        /// <summary>
        /// Set server params to check current connection to DB.
        /// </summary>
        /// <param name="dbServerParams">All params to connect to local or remote IP device holding the DB</param>
        /// <returns></returns>
        public static OracleConnection TestConnection(string[] dbServerParams, bool autoClose = false)
        {
            //params ex:
            //[0] = IP = 127.0.0.1
            //[1] = Port = 1521
            //[2] = user = store //this should be deep coded here not in config file for security.
            //[3] = pass = store //this should be deep coded here not in config file for security.
            //test connectio to oracle
            string oradb = "Data Source =" + dbServerParams[0] + ":" + dbServerParams[1] + " / orcl; User Id = " + dbServerParams[2] + "; password = " + dbServerParams[3] + ";";

            OracleConnection conn = new OracleConnection(oradb);
            try
            {
                conn.Open();

            }
            catch (Exception orExc)
            {
                //MessageBox.Show(orExc.Message, "Database connection error!");
                Console.WriteLine(orExc.Message.ToString());
                return null;
            }

            Console.Write("Connected to Oracle" + conn.ServerVersion);
            // Close and Dispose OracleConnection object
            if (autoClose)
            {
                conn.Close();
                conn.Dispose();
                Console.Write("Disconnected");
            }
            return conn;
        }
        /// <summary>
        /// This function mainly made for general purpose to query from any table.
        /// </summary>
        /// <param name="oraConn">Object that holds connection</param>
        /// <param name="tablename">Current table to fetch query from</param>
        /// <param name="choosenFields">Fields to collect all data from</param>
        /// <param name="values">Values to compare with</param>
        /// <param name="oper">Cmpare operations</param>
        /// <returns></returns>
        public static OracleCommand FetchMyData(OracleConnection oraConn, string tablename, string[] choosenFields, string[] whereFields, string[] values, string oper, string seper, bool ordered = false, string orderBy = null)
        {

            OracleCommand cmd = new OracleCommand();
            string sqlQueryStatement = "select ";
            string select = "";
            if (whereFields.Length <= 0)
            {

                for (int x = 0; x < choosenFields.Length; x++)
                {
                    sqlQueryStatement += choosenFields[x] + " ,";
                }
                sqlQueryStatement = sqlQueryStatement.Substring(0, sqlQueryStatement.Length - 1);
                sqlQueryStatement += "from " + tablename;
                 select = sqlQueryStatement;
            }

            else
            {
                for (int x = 0; x < choosenFields.Length; x++)
                {
                    sqlQueryStatement += choosenFields[x] + " ,";
                }
                sqlQueryStatement = sqlQueryStatement.Substring(0, sqlQueryStatement.Length - 1);
                sqlQueryStatement += "from " + tablename + " where";

                select = WherePartQueryTxt(sqlQueryStatement, whereFields, oper, seper);


                //we need to repacle each ? by values in it's own order one by one..
                select = ReplaceWithMyVals(select, values);
            }

            if (ordered && orderBy != null)
            {
                select += " order by " + orderBy;
            }
            cmd.CommandText = select;




            cmd.Connection = oraConn;


            return cmd;
        }
        /// <summary>
        /// Speed way to re-use defintion data tables after where in sql statements.
        /// </summary>
        /// <param name="sqlOldCommand">Old sql string without any where</param>
        /// <param name="fields">Fields names</param>
        /// <param name="oper">operator</param>
        /// <param name="seper">seperator</param>
        /// <returns></returns>
        private static string WherePartQueryTxt(string sqlOldCommand, string[] fields, string oper, string seper)
        {

            string sqlStatement = sqlOldCommand;

            for (int x = 0; x < fields.Length; x++)
            {
                sqlStatement += " " + fields[x] + " " + oper + "? " + seper;
            }

            sqlStatement = sqlStatement.Substring(0, sqlStatement.Length - seper.Length);

            return sqlStatement;
        }
        /// <summary>
        /// This method is a replacement for setting values in oracle sql satements.
        /// and setsring in java language!
        /// </summary>
        /// <param name="oldSelect">Old sql string including where with or without ?</param>
        /// <param name="vals">Replacable vals instead of ?</param>
        /// <returns></returns>
        private static string ReplaceWithMyVals(string oldSelect, string[] vals)
        {
            List<string> listofData = new List<string>();

            int pointer = 0;

            for (int c = 0; c < oldSelect.Length; c++)
            {
                if (oldSelect[c] == '?')
                {
                    listofData.Add(oldSelect.Substring(pointer, c - pointer));
                    pointer = c + 1;

                }
            }

            oldSelect = "";

            for (int i = 0; i < listofData.Count; i++)
            {
                oldSelect += listofData[i];
                oldSelect += vals[i];
            }


            return oldSelect;
        }

        /// <summary>
        /// Generic Func to add new row to existing database
        /// </summary>
        /// <param name="oraConn">Current object that carries connection tunnel</param>
        /// <param name="tablename">The table in which data row will be inserted</param>
        /// <param name="values">Each value that must be inserted</param>
        /// <returns></returns>
        public static int InsertMyDataRow(OracleConnection oraConn, string tablename, string[] values)
        {
            OracleCommand cmd = new OracleCommand();

            string addRowQueryStatement = "insert into " + tablename + " values(";

            for (int p = 0; p < values.Length; p++)
            {
                if (p != values.Length - 1)
                    addRowQueryStatement += "?,";
                else
                    addRowQueryStatement += "?";

            }

            /*
            List<OracleParameter> paramsOra = new List<OracleParameter>();

            int i = 0;
            foreach(OracleDbType param in oracleDbTypes) {
                paramsOra.Add(new OracleParameter() { OracleDbType = param, Value = values[i] });
                i++;
            }

            foreach(OracleParameter param in paramsOra) { cmd.Parameters.Add(param); }
            try
            {
                cmd.ExecuteNonQuery();
                return 0;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex}");
            }
            */
            addRowQueryStatement = ReplaceWithMyVals(addRowQueryStatement, values);
            addRowQueryStatement += ")";
            cmd.CommandText = addRowQueryStatement;
            cmd.Connection = oraConn;
            try
            {
                int aff = cmd.ExecuteNonQuery();
                return aff;
            }
            catch (Exception e)
            {
                return -1;
            }


        }
        /// <summary>
        /// Get the highest no in int columns
        /// </summary>
        /// <param name="oraConn">Curent connection object</param>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name in that table</param>
        /// <returns></returns>
        public static int GetHighestNOofRow(OracleConnection oraConn, string tableName, string columnName)
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = oraConn;

            string getCountQueryStatement = "select max(" + columnName + ") from " + tableName;
            cmd.CommandText = getCountQueryStatement;

            try
            {
                OracleDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {


                    while (dr.Read())
                    {
                        return Int32.Parse(dr[0].ToString());
                    }
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                Console.WriteLine(e.Message.ToString());
            }

            return -1;
        }
        /// <summary>
        /// Update selected row depending on one condition or more conditions.
        /// </summary>
        /// <param name="oraConn">Current connection object</param>
        /// <param name="tablename">Table name</param>
        /// <param name="updatedFields">Choosen columns names</param>
        /// <param name="updateValues">New values to be replaced</param>
        /// <param name="whereFields">Condition columns names</param>
        /// <param name="whereValues">Condition Values</param>
        /// <param name="oper">Operator used in comparison in condition form</param>
        /// <param name="seper">seperator</param>
        /// <returns></returns>
        public static int EditMyDataRow(OracleConnection oraConn, string tablename, string[] updatedFields, string[] updateValues, string[] whereFields, string[] whereValues, string oper, string seper)
        {
            OracleCommand cmd = new OracleCommand();

            cmd.Connection = oraConn;

            string editRowQueryStatement = "update " + tablename + " set ";

            //for loop for putting columns names
            string sqlWithWherePart = WherePartQueryTxt(editRowQueryStatement, updatedFields, "=", ",");
            //for loop for putting values in set update part instead of ?
            string sqlWithValsInWherePart = ReplaceWithMyVals(sqlWithWherePart, updateValues);


            //add where portion ..
            sqlWithValsInWherePart += " where ";

            //for loop for putting where and where fields
            string sqlWithWherePart_Real_One = WherePartQueryTxt(sqlWithValsInWherePart, whereFields, oper, seper);

            //for loop for putting values in after where update part instead of ?
            string sqlWithValsInWherePart_Real_One = ReplaceWithMyVals(sqlWithWherePart_Real_One, whereValues);


            string completeSQL = sqlWithValsInWherePart_Real_One;

            cmd.CommandText = completeSQL;

            try
            {
                int aff = cmd.ExecuteNonQuery();
                return aff;
            }
            catch (Exception)
            {
                return -1;
            }







        }
        /// <summary>
        /// Delete selected row depending on a condition or more conditions.[still beta...]
        /// </summary>
        /// <param name="oraConn">current connection object</param>
        /// <param name="tablename">current table name we make operations on it</param>
        /// <param name="whereFields">fields choosen</param>
        /// <param name="whereValues">values for fields choosen before</param>
        /// <param name="seper"> a seperator</param>
        /// <returns></returns>
        public static int DeleteMyDataRow(OracleConnection oraConn, string tablename, string[] whereFields, string[] whereValues, string oper, string seper)
        {

            OracleCommand cmd = new OracleCommand();

            cmd.Connection = oraConn;

            string deleteRowQueryStatement = "delete from " + tablename + " where ";

            //for loop for putting columns names
            string sqlWithWherePart = WherePartQueryTxt(deleteRowQueryStatement, whereFields, "=", ",");
            //for loop for putting values in set update part instead of ?
            string sqlWithValsInWherePart = ReplaceWithMyVals(sqlWithWherePart, whereValues);

            string completeSQL = sqlWithValsInWherePart;

            cmd.CommandText = completeSQL;

            try
            {
                int aff = cmd.ExecuteNonQuery();
                return aff;
            }
            catch (Exception)
            {
                return -1;
            }



        }
        /// <summary>
        /// similar to select func but with counter instead of rows data
        /// </summary>
        /// <param name="oraConn">connection object</param>
        /// <param name="tablename">table to perform select on</param>
        /// <param name="choosenFields">choosen fileds in columns titles</param>
        /// <param name="whereFields">where chosen fields</param>
        /// <param name="values">values for where</param>
        /// <param name="oper">operator</param>
        /// <param name="seper">seperator</param>
        /// <param name="ordered">orderoed or not</param>
        /// <param name="orderBy">by what filed name on column you wan ordered</param>
        /// <returns></returns>
        public static OracleCommand GetCountConditionedWithFields(OracleConnection oraConn, string tablename, string[] choosenFields, string[] whereFields, string[] values, string oper, string seper, bool ordered = false, string orderBy = null)
        {

            OracleCommand cmd = new OracleCommand();
            string sqlQueryStatement = "select COUNT(*) AS count";
            string select = "";
            if (whereFields.Length <= 0)
            {

                for (int x = 0; x < choosenFields.Length; x++)
                {
                    sqlQueryStatement += choosenFields[x] + " ,";
                }
                sqlQueryStatement = sqlQueryStatement.Substring(0, sqlQueryStatement.Length - 1);
                sqlQueryStatement += "from " + tablename;
                select = sqlQueryStatement;
            }

            else
            {
                for (int x = 0; x < choosenFields.Length; x++)
                {
                    sqlQueryStatement += choosenFields[x] + " ,";
                }
                sqlQueryStatement = sqlQueryStatement.Substring(0, sqlQueryStatement.Length - 1);
                sqlQueryStatement += "from " + tablename + " where";

                select = WherePartQueryTxt(sqlQueryStatement, whereFields, oper, seper);


                //we need to repacle each ? by values in it's own order one by one..
                select = ReplaceWithMyVals(select, values);
            }

            if (ordered && orderBy != null)
            {
                select += " order by " + orderBy;
            }
            cmd.CommandText = select;




            cmd.Connection = oraConn;


            return cmd;
        }
        /////////////////////////////////////////////////////Developped Area for Advanced Search Only/////////////////////////////////////

        public static OracleCommand FetchMyDataFullConstrainted(OracleConnection oraConn, string tablename, string[] choosenFields, string[] whereFields, string[] values, string[] oper, string seper, bool ordered = false, string orderBy = null)
        {

            OracleCommand cmd = new OracleCommand();
            string sqlQueryStatement = "select ";
            for (int x = 0; x < choosenFields.Length; x++)
            {
                sqlQueryStatement += choosenFields[x] + " ,";
            }
            sqlQueryStatement = sqlQueryStatement.Substring(0, sqlQueryStatement.Length - 1);
            sqlQueryStatement += "from " + tablename + " where";

            string select = WherePartQueryTxtMultiOperators(sqlQueryStatement, whereFields, oper, seper);


            //we need to repacle each ? by values in it's own order one by one..
            select = ReplaceWithMyVals(select, values);

            if (ordered && orderBy != null)
            {
                select += " order by " + orderBy;
            }
            cmd.CommandText = select;




            cmd.Connection = oraConn;


            return cmd;
        }

        private static string WherePartQueryTxtMultiOperators(string sqlOldCommand, string[] fields, string[] oper, string seper)
        {

            string sqlStatement = sqlOldCommand;

            for (int x = 0; x < fields.Length; x++)
            {
                sqlStatement += " " + fields[x] + " " + oper[x] + "? " + seper;
            }

            sqlStatement = sqlStatement.Substring(0, sqlStatement.Length - seper.Length);

            return sqlStatement;
        }
    }
}
