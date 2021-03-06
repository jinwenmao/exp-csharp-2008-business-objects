using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Csla.Properties;

namespace Csla.Data
{
    /// <summary>
    /// Provides an automated way to reuse open
    /// database connections within the context
    /// of a single data portal operation.
    /// </summary>
    /// <remarks>
    /// This type stores the open database connection
    /// in <see cref="Csla.ApplicationContext.LocalContext" />
    /// and uses reference counting through
    /// <see cref="IDisposable" /> to keep the connection
    /// open for reuse by child objects, and to automatically
    /// dispose the connection when the last consumer
    /// has called Dispose."
    /// </remarks>
    public class ConnectionManager : IDisposable
  {
    private static object _lock = new object();
    private IDbConnection _connection;
    private string _connectionString;

    /// <summary>
    /// Gets the ConnectionManager object for the 
    /// specified database.
    /// </summary>
    /// <param name="database">
    /// Database name as shown in the config file.
    /// </param>
    public static ConnectionManager GetManager(string database)
    {
      return GetManager(database, true);
    }

    /// <summary>
    /// Gets the ConnectionManager object for the 
    /// specified database.
    /// </summary>
    /// <param name="database">
    /// The database name or connection string.
    /// </param>
    /// <param name="isDatabaseName">
    /// True to indicate that the connection string
    /// should be retrieved from the config file. If
    /// False, the database parameter is directly 
    /// used as a connection string.
    /// </param>
    /// <returns>ConnectionManager object for the name.</returns>
    public static ConnectionManager GetManager(string database, bool isDatabaseName)
    {
      if (isDatabaseName)
      {
        var connection = ConfigurationManager.ConnectionStrings[database];
        if (connection == null)
          throw new ConfigurationErrorsException(String.Format(Resources.DatabaseNameNotFound, database));

        var conn = ConfigurationManager.ConnectionStrings[database].ConnectionString;
        if (string.IsNullOrEmpty(conn))
          throw new ConfigurationErrorsException(String.Format(Resources.DatabaseNameNotFound, database));
        database = conn;
      }

      lock (_lock)
      {
        ConnectionManager mgr = null;
        if (ApplicationContext.LocalContext.Contains("__db:" + database))
        {
          mgr = (ConnectionManager)(ApplicationContext.LocalContext["__db:" + database]);

        }
        else
        {
          mgr = new ConnectionManager(database);
          ApplicationContext.LocalContext["__db:" + database] = mgr;
        }
        mgr.AddRef();
        return mgr;
      }
    }

    private ConnectionManager(string connectionString)
    {

      _connectionString = connectionString;

      string provider = ConfigurationManager.AppSettings["dbProvider"];
      if (string.IsNullOrEmpty(provider))
          provider = "System.Data.SqlClient";

      DbProviderFactory factory = DbProviderFactories.GetFactory(provider);

      // open connection
      _connection = factory.CreateConnection();
      _connection.ConnectionString = connectionString;
      _connection.Open();

    }

    /// <summary>
    /// Dispose object, dereferencing or
    /// disposing the connection it is
    /// managing.
    /// </summary>
    public IDbConnection Connection
    {
      get
      {
        return _connection;
      }
    }

    #region  Reference counting

    private int mRefCount;

    private void AddRef()
    {
      mRefCount += 1;
    }

    private void DeRef()
    {

      lock (_lock)
      {
        mRefCount -= 1;
        if (mRefCount == 0)
        {
          _connection.Dispose();
          ApplicationContext.LocalContext.Remove("__db:" + _connectionString);
        }
      }

    }

    #endregion

    #region  IDisposable

    /// <summary>
    /// Dispose object, dereferencing or
    /// disposing the connection it is
    /// managing.
    /// </summary>
    public void Dispose()
    {
      DeRef();
    }

    #endregion

  } 
}
