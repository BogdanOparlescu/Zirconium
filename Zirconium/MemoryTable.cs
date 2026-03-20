using System.Data.SQLite;
using System.Text;

namespace Zirconium;

public class MemoryTable : IDisposable
{
    private SQLiteConnection connection;
    public string name { get; }
    public string[] headers { get; }

    public MemoryTable(string name, string[] headers)
    {
        this.name = name;
        this.headers = headers;

        connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");
        connection.Open();

        string columns = string.Join(", ", headers.Select(h => $"\"{h}\" TEXT"));
        string sql = $"CREATE TABLE {name} ({columns})";
        using(var cmd = new SQLiteCommand(sql, connection))
            cmd.ExecuteNonQuery();
    }

    public void Insert(string[] values)
    {
        if (values.Length != headers.Length)
            throw new ArgumentException("Number of values must match the number of headers.");

        string paramNames = string.Join(", ", headers.Select((h,i) => $"@p{i}"));
        string sql = $"INSERT INTO \"{name}\" VALUES ({paramNames})";

        using (var cmd = new SQLiteCommand(sql, connection))
        {
            for (int i = 0; i < values.Length; ++i)
                cmd.Parameters.AddWithValue($"@p{i}", values[i]);
            cmd.ExecuteNonQuery();
        }
    }

    public MemoryTable Query(string query)
    {
        using (var cmd = new SQLiteCommand(query, connection))
        using (var reader = cmd.ExecuteReader())
        {
            var newHeaders = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                newHeaders[i] = reader.GetName(i);

            var resultTable = new MemoryTable(this.name + "_result", newHeaders);

            while (reader.Read())
            {
                string[] rowValues = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                    rowValues[i] = reader[i].ToString()!;
                resultTable.Insert(rowValues);
            }

            return resultTable;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Join(',', headers));
        List<string[]> contents = Contents();
        foreach (string[] row in contents)
            sb.Append(string.Join(',', row)).Append('\n');
        return sb.ToString();
    }

    public List<string[]> Contents()
    {
        var contents = new List<string[]>();
        string sql = $"SELECT * FROM \"{name}\"";

        using (var cmd = new SQLiteCommand(sql, connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var rowValues = new string[headers.Length];
                for (int i = 0; i < reader.FieldCount; i++)
                    rowValues[i] = reader[i]?.ToString() ?? "";
                contents.Add(rowValues);
            }
        }
        return contents;
    }

    public void Dispose()
    {
        connection.Close();
        connection.Dispose();
    }
}