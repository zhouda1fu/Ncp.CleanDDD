using MySqlConnector;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Events;

namespace Ncp.CleanDDD.Web.Infrastructure.Logging
{
    public class MySqlCorrelationSink : ILogEventSink
    {
        private readonly string _connectionString;
        private readonly IFormatProvider? _formatProvider;

        public MySqlCorrelationSink(string connectionString, IFormatProvider? formatProvider = null)
        {
            _connectionString = connectionString;
            _formatProvider = formatProvider;

            // 初始化数据库和表
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Logs (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Timestamp DATETIME(3) NOT NULL,
                Level VARCHAR(50) NOT NULL,
                Message TEXT NOT NULL,
                Exception LONGTEXT,
                CorrelationId VARCHAR(100),
                Properties JSON,
                INDEX idx_timestamp (Timestamp),
                INDEX idx_level (Level),
                INDEX idx_correlation_id (CorrelationId)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ";
            cmd.ExecuteNonQuery();
        }

        public void Emit(LogEvent logEvent)
        {
            var timestamp = logEvent.Timestamp;
            var level = logEvent.Level.ToString();
            var message = logEvent.RenderMessage(_formatProvider);
            var exception = logEvent.Exception?.ToString();
            var correlationId = logEvent.Properties.ContainsKey("CorrelationId")
                ? logEvent.Properties["CorrelationId"].ToString().Trim('"') // 去掉引号
                : null;
            var properties = JsonConvert.SerializeObject(logEvent.Properties);

            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO Logs (Timestamp, Level, Message, Exception, CorrelationId, Properties)
            VALUES (@timestamp, @level, @message, @exception, @correlationId, @properties);
        ";
            cmd.Parameters.AddWithValue("@timestamp", timestamp);
            cmd.Parameters.AddWithValue("@level", level);
            cmd.Parameters.AddWithValue("@message", message);
            cmd.Parameters.AddWithValue("@exception", (object?)exception ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@correlationId", (object?)correlationId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@properties", (object?)properties ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
}
