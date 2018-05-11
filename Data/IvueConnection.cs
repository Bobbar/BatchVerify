using System.Data;
using System.Data.SqlClient;

namespace BatchVerify.Data
{
    public static class IvueConnection
    {
        private static string connectString = "server=ddad-svr-fs02; database=IntelliVue; trusted_connection=True; Connection Timeout=5";

        private static SqlConnection NewConnection()
        {
            return new SqlConnection(connectString);
        }

        public static SqlCommand ReturnSqlCommand(string query)
        {
            SqlConnection conn = NewConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            return cmd;
        }

        public static DataTable ReturnSqlTable(string query)
        {
            using (SqlConnection conn = NewConnection())
            using (DataTable newTable = new DataTable())
            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                da.SelectCommand = new SqlCommand(query);
                da.SelectCommand.Connection = conn;
                da.Fill(newTable);
                return newTable;
            }
        }

        public static DataTable ReturnSqlTableFromCmd(SqlCommand cmd)
        {
            using (DataTable newTable = new DataTable())
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(newTable);
                cmd.Dispose();
                return newTable;
            }
        }
    }
}