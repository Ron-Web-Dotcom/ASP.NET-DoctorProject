using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Security;

/// <summary>
/// Provides a centralised factory for opening SQL connections to the
/// application database (SignUpDB.mdf via LocalDB).
/// The connection string is read from the "SignUpDBConnectionString1" entry
/// in Web.config, keeping credentials out of application code.
/// </summary>
public class connectionManager
{
    /// <summary>
    /// Creates, opens, and returns a new <see cref="SqlConnection"/> to the
    /// application database. The caller is responsible for disposing the
    /// connection (preferably with a <c>using</c> block) to ensure it is
    /// returned to the connection pool promptly.
    /// </summary>
    /// <returns>An open <see cref="SqlConnection"/> ready for use.</returns>
    public static SqlConnection GetMembersConnection()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["SignUpDBConnectionString1"].ConnectionString;
        SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}
