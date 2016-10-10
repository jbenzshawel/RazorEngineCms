using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace RazorEngineCms.App_Classes
{
    public class DataHelper : IDisposable
    {
        private const string CONN_STRING = @"Data Source =.\SQLEXPRESS; Initial Catalog = RazorEngineCms.ApplicationContext; Integrated Security = True";

        public DataHelper()
        {

        }

        public DataTable GetData(string procName)
        {
            return GetData(procName, null);
        }

        public DataTable GetData(string procName, List<SqlParameter> paramz = null)
        {
            var dataTable = new DataTable();

            if (!string.IsNullOrEmpty(procName))
            {
                using (var sqlCon = new SqlConnection(CONN_STRING))
                {
                    var sqlCmd = new SqlCommand
                    {
                        Connection = sqlCon,
                        CommandType = CommandType.StoredProcedure,
                        CommandText = procName
                    };

                    if (paramz != null && paramz.Count > 0)
                    {
                        foreach(var param in paramz)
                        {
                            sqlCmd.Parameters.Add(param);
                        }
                   }

                    try
                    {
                        sqlCon.Open();
                        var reader = sqlCmd.ExecuteReader();
                        var columns = new List<string>();
                        // get columns returned 
                        if (reader.VisibleFieldCount > 0)
                        {
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                var colName = reader.GetName(i);
                                columns.Add(colName);
                                dataTable.Columns.Add(new DataColumn
                                {
                                    ColumnName = colName,
                                    DataType = typeof(string)
                                });
                            }
                        }


                        if (columns.Count > 0)
                        {
                            while (reader.Read())
                            {
                                var newRow = dataTable.NewRow();
                                foreach (var column in columns)
                                {
                                    if (reader[column] != DBNull.Value)
                                    {

                                        newRow[column] = reader[column].ToString();
                                    }
                                }
                                dataTable.Rows.Add(newRow);
                            }
                        }
                    } // end try
                    catch (Exception ex)
                    {
                        
                    }
                    finally
                    {
                        if(sqlCon.State != ConnectionState.Closed)
                            sqlCon.Close();
                    }
                  
                }
            }

            return dataTable;
        }

        public void Dispose()
        {
            
        }
    }
}