using Microsoft.Extensions.Configuration;
using MySqlConnector;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Application.Queries;

/// <summary>
/// 日志查询服务
/// 提供日志数据的查询功能，包括分页查询和按关联ID查询
/// </summary>
public class LogQuery
{
    private readonly string _connectionString;

    public LogQuery(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MySql")!;
    }

    /// <summary>
    /// 获取日志列表（分页）
    /// </summary>
    /// <param name="pageIndex">页码（从0开始）</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="level">日志级别筛选</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="keyword">关键词搜索（在消息中搜索）</param>
    /// <param name="countTotal">是否计算总数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>分页的日志数据</returns>
    public async Task<PagedData<LogItemDto>> GetLogsAsync(
        int pageIndex,
        int pageSize,
        string? level = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        string? keyword = null,
        bool countTotal = true,
        CancellationToken cancellationToken = default)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // 构建WHERE条件
        var whereConditions = new List<string>();
        var parameters = new List<MySqlParameter>();

        if (!string.IsNullOrEmpty(level))
        {
            whereConditions.Add("Level = @level");
            parameters.Add(new MySqlParameter("@level", level));
        }

        if (startTime.HasValue)
        {
            whereConditions.Add("Timestamp >= @startTime");
            parameters.Add(new MySqlParameter("@startTime", startTime.Value));
        }

        if (endTime.HasValue)
        {
            whereConditions.Add("Timestamp <= @endTime");
            parameters.Add(new MySqlParameter("@endTime", endTime.Value));
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            whereConditions.Add("Message LIKE @keyword");
            parameters.Add(new MySqlParameter("@keyword", $"%{keyword}%"));
        }

        var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        // 计算总数
        int total = 0;
        if (countTotal)
        {
            var countCmd = connection.CreateCommand();
            countCmd.CommandText = $"SELECT COUNT(*) FROM Logs {whereClause}";
            countCmd.Parameters.AddRange(parameters.ToArray());
            total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
        }

        // 查询数据
        var offset = pageIndex * pageSize;
        var dataCmd = connection.CreateCommand();
        dataCmd.CommandText = $@"
            SELECT Id, Timestamp, Level, Message, Exception, CorrelationId, Properties
            FROM Logs 
            {whereClause}
            ORDER BY Timestamp DESC
            LIMIT @pageSize OFFSET @offset";

        dataCmd.Parameters.AddRange(parameters.ToArray());
        dataCmd.Parameters.Add(new MySqlParameter("@pageSize", pageSize));
        dataCmd.Parameters.Add(new MySqlParameter("@offset", offset));

        var logs = new List<LogItemDto>();
        using var reader = await dataCmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            logs.Add(new LogItemDto(
                Id: reader.GetInt64("Id"),
                Timestamp: reader.GetDateTime("Timestamp"),
                Level: reader.GetString("Level"),
                Message: reader.GetString("Message"),
                Exception: reader.IsDBNull(4) ? null : reader.GetString("Exception"),
                CorrelationId: reader.IsDBNull(5) ? null : reader.GetString("CorrelationId"),
                Properties: reader.IsDBNull(6) ? null : reader.GetString("Properties")
            ));
        }

        return new PagedData<LogItemDto>(logs, total, pageIndex, pageSize);
    }

    /// <summary>
    /// 根据关联ID获取日志列表
    /// </summary>
    /// <param name="correlationId">关联ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>日志列表</returns>
    public async Task<IEnumerable<LogItemDto>> GetLogsByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT Id, Timestamp, Level, Message, Exception, CorrelationId, Properties
            FROM Logs 
            WHERE CorrelationId = @correlationId
            ORDER BY Timestamp ASC";

        cmd.Parameters.Add(new MySqlParameter("@correlationId", correlationId));

        var logs = new List<LogItemDto>();
        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            logs.Add(new LogItemDto(
                Id: reader.GetInt64("Id"),
                Timestamp: reader.GetDateTime("Timestamp"),
                Level: reader.GetString("Level"),
                Message: reader.GetString("Message"),
                Exception: reader.IsDBNull(4) ? null : reader.GetString("Exception"),
                CorrelationId: reader.IsDBNull(5) ? null : reader.GetString("CorrelationId"),
                Properties: reader.IsDBNull(6) ? null : reader.GetString("Properties")
            ));
        }

        return logs;
    }
}

/// <summary>
/// 日志项数据传输对象
/// </summary>
/// <param name="Id">日志ID</param>
/// <param name="Timestamp">时间戳</param>
/// <param name="Level">日志级别</param>
/// <param name="Message">日志消息</param>
/// <param name="Exception">异常信息</param>
/// <param name="CorrelationId">关联ID</param>
/// <param name="Properties">属性信息（JSON格式）</param>
public record LogItemDto(
    long Id,
    DateTime Timestamp,
    string Level,
    string Message,
    string? Exception,
    string? CorrelationId,
    string? Properties
);
