using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace JAStudio.Core.SysUtils.Data.Sqlite;

/// <summary>
/// A SQL statement prepared once, to be run many times with different values, such as one adding a row, run for each of thousands of rows<br/>
/// in one transaction. SQLite then parses the statement once, not once for every row.
/// </summary>
sealed class PreparedSqliteStatement : IDisposable
{
   readonly SqliteCommand _command;

   internal PreparedSqliteStatement(SqliteConnection connection, string statement, IReadOnlyList<string> parameterNames)
   {
      _command = connection.CreateCommand();
      _command.CommandText = statement;
      foreach(var name in parameterNames) _command.Parameters.Add(new SqliteParameter { ParameterName = name });
      _command.Prepare();
   }

   /// <summary>Runs the statement with <paramref name="values"/>, given in the order of its parameters' names; a null is SQL's null.</summary>
   public void RunWith(params object?[] values)
   {
      for(var index = 0; index < values.Length; index++) _command.Parameters[index].Value = values[index] ?? DBNull.Value;
      _command.ExecuteNonQuery();
   }

   public void Dispose() => _command.Dispose();
}
