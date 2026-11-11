using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace JAStudio.Core.SysUtils.Data.Sqlite;

/// <summary>Running SQL on an open <see cref="SqliteConnection"/> in a single call: a statement, a query's rows or its first column, or a single value.</summary>
static class SqliteConnectionExtensions
{
   extension(SqliteConnection @this)
   {
      /// <summary>Runs <paramref name="statements"/>, one or several separated by semicolons, with the values of <paramref name="parameters"/>.</summary>
      internal void Execute(string statements, params (string Name, object Value)[] parameters)
      {
         using var command = CommandWith(@this, statements, parameters);
         command.ExecuteNonQuery();
      }

      /// <summary>The single value <paramref name="select"/> gives, such as a count or an id, as a <see cref="long"/>.</summary>
      internal long ScalarAsLong(string select, params (string Name, object Value)[] parameters)
      {
         using var command = CommandWith(@this, select, parameters);
         return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture);
      }

      /// <summary>Every row <paramref name="select"/> gives, with the values of <paramref name="parameters"/>, each read by <paramref name="row"/> while the reader is at it.</summary>
      internal List<T> Rows<T>(string select, Func<SqliteDataReader, T> row, params (string Name, object Value)[] parameters) =>
         [.. @this.RowsReadAsEnumerated(select, row, parameters)];

      /// <summary>
      /// Every row <paramref name="select"/> gives, with the values of <paramref name="parameters"/>, each read by <paramref name="row"/> as it's<br/>
      /// enumerated, so that a query of millions of rows never holds them all.
      /// </summary>
      internal IEnumerable<T> RowsReadAsEnumerated<T>(string select, Func<SqliteDataReader, T> row, params (string Name, object Value)[] parameters)
      {
         using var command = CommandWith(@this, select, parameters);
         using var reader = command.ExecuteReader();
         while(reader.Read()) yield return row(reader);
      }

      /// <summary>The value in the first column of every row <paramref name="select"/> gives.</summary>
      internal List<T> Column<T>(string select) => @this.Rows(select, reader => reader.GetFieldValue<T>(0));

      /// <summary><paramref name="statement"/>, prepared to be run many times with different values for <paramref name="parameterNames"/>.</summary>
      internal PreparedSqliteStatement Prepared(string statement, params string[] parameterNames) => new(@this, statement, parameterNames);
   }

   static SqliteCommand CommandWith(SqliteConnection connection, string sql, (string Name, object Value)[] parameters)
   {
      var command = connection.CreateCommand();
      command.CommandText = sql;
      foreach(var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
      return command;
   }
}
