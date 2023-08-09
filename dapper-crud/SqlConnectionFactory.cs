using Microsoft.Data.SqlClient;

internal class SqlConnectionFactory
{
    private string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString=connectionString;
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}