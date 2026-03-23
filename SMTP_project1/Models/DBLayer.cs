
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMTP_project1.Models
{
    public class DBLayer
    {
        SqlConnection constr;
        public DBLayer(IConfiguration config)
        {
            constr = new SqlConnection(config.GetConnectionString("constr"));
        }
        
        public int ExecuteQuery(string procname, SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(procname, constr);
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null) 
            {
                cmd.Parameters.AddRange(parameters);
            }
            cmd.Connection.Open();
            int res = cmd.ExecuteNonQuery();
            cmd.Connection.Close();
            return res;
        }

        public DataTable table(string procname, SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(procname, constr);
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }
            DataTable dt = new DataTable();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);
            return dt;
        }
    }
}
