using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Transport.Sync
{
    public static class FromBO
    {
        public static DataTable getViewPersonCard()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(Program.ConnectFrom))
            {
                con.Open();
                string sql = "select * from view_personcard";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static DataTable getViewDeleCard()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(Program.ConnectFrom))
            {
                con.Open();
                string sql = "select * from view_delecard";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static DataTable getViewte210()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(Program.ConnectFrom))
            {
                con.Open();
                string sql = "select * from view_te210";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
    }
}
