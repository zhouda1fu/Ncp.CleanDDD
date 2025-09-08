using FastEndpoints;
using NetCorePal.Extensions.Dto;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using System.Globalization;
using Ncp.CleanDDD.Web.AppPermissions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Web.Utils;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

/// <summary>
/// 批量导入用户端点
/// </summary>
[Tags("Users")]
public class BatchImportUsersEndpoint : Endpoint<BatchImportUsersRequest, ResponseData<BatchImportUsersResponse>>
{
    // 常量定义
    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
    private const int ExcelColumnCount = 8;
    private const string DefaultPassword = "123456";
    private const string DateFormat = "yyyy-MM-dd";

    private static readonly string[] AllowedFileExtensions = [".xlsx", ".xls"];
    private static readonly string[] ValidGenders = ["男", "女"];

    // Excel列索引定义
    private static class ExcelColumns
    {
        public const int IdType = 0;      // A列：id类型
        public const int IdNo = 1;        // B列：id
        public const int Name = 2;        // C列：用户名
        public const int RealName = 3;    // D列：真实姓名
        public const int Email = 4;       // E列：邮箱
        public const int Phone = 5;       // F列：手机号
        public const int Gender = 6;      // G列：性别
        public const int BirthDate = 7;   // H列：出生日期
    }

    private readonly IMediator _mediator;
    private readonly RoleQuery _roleQuery;
    private readonly UserQuery _userQuery;

    public BatchImportUsersEndpoint(IMediator mediator, RoleQuery roleQuery, UserQuery userQuery)
    {
        _mediator = mediator;
        _roleQuery = roleQuery;
        _userQuery = userQuery;
    }

    public override void Configure()
    {
        Post("/api/users/batch-import");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserCreate);
    }

    public override async Task HandleAsync(BatchImportUsersRequest request, CancellationToken ct)
    {
        if (request.File == null || request.File.Length == 0)
        {
            throw new KnownException("请选择要导入的文件");
        }

        // 验证文件格式
        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!AllowedFileExtensions.Contains(fileExtension))
        {
            throw new KnownException("请上传Excel文件（.xlsx 或 .xls）");
        }

        // 验证文件大小
        if (request.File.Length > MaxFileSize)
        {
            throw new KnownException("文件大小不能超过 10MB");
        }

        try
        {
            var result = await ProcessExcelFileAsync(request, ct);
            await Send.OkAsync(result.AsResponseData(), cancellation: ct);
        }
        catch (KnownException ex)
        {
            throw new KnownException($"导入失败：{ex.Message}");
        }
    }

    private async Task<BatchImportUsersResponse> ProcessExcelFileAsync(BatchImportUsersRequest request, CancellationToken ct)
    {
        var file = request.File;
        var successCount = 0;
        var failedRows = new List<BatchImportFailedRow>();
        var totalCount = 0;

        // 预处理：收集所有用户数据进行批量验证
        var allUserData = new List<UserImportData>();
        var rowIndexMap = new Dictionary<int, int>(); // Excel行号 -> 列表索引

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, ct);
        stream.Position = 0;

        using var workbook = CreateWorkbook(stream, file.FileName);
        var worksheet = workbook.GetSheetAt(0);

        if (worksheet == null)
        {
            throw new KnownException("Excel文件中没有找到工作表");
        }

        var rowCount = worksheet.LastRowNum + 1;
        if (rowCount <= 1)
        {
            throw new KnownException("Excel文件中没有数据行");
        }

        // 第一遍：读取所有数据
        for (int row = 2; row < rowCount; row++) // 从第1行开始（跳过标题行）
        {
            try
            {
                var userData = ReadUserDataFromRow(worksheet, row);
                if (userData == null) continue; // 空行跳过

                allUserData.Add(userData);
                rowIndexMap[allUserData.Count - 1] = row;
                totalCount++;
            }
            catch (Exception ex)
            {
                var userData = ReadUserDataFromRowSafe(worksheet, row);
                failedRows.Add(new BatchImportFailedRow(row + 1, [$"读取行数据失败: {ex.Message}"], userData));
            }
        }

        // 批量验证所有用户数据
        if (allUserData.Count > 0)
        {
            var validationResults = await ValidateAllUsersAsync(allUserData, ct);

            // 处理验证结果并创建用户
            for (int i = 0; i < allUserData.Count; i++)
            {
                var userData = allUserData[i];
                var excelRow = rowIndexMap[i] + 1; // Excel行号从1开始

                if (validationResults[i].Count > 0)
                {
                    // 验证失败
                    failedRows.Add(new BatchImportFailedRow(excelRow, validationResults[i], userData));
                }
                else
                {
                    // 验证通过，创建用户
                    try
                    {
                        await CreateUserAsync(userData, request.OrganizationUnitId, request.OrganizationUnitName, request.RoleIds, ct);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failedRows.Add(new BatchImportFailedRow(excelRow, [$"创建用户失败: {ex.Message}"], userData));
                    }
                }
            }
        }

        return new BatchImportUsersResponse(
            successCount,
            failedRows.Count,
            totalCount,
            failedRows
        );
    }

    private static IWorkbook CreateWorkbook(Stream stream, string fileName)
    {
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        if (fileExtension == ".xlsx")
        {
            return new XSSFWorkbook(stream);
        }
        else if (fileExtension == ".xls")
        {
            return new HSSFWorkbook(stream);
        }
        else
        {
            throw new KnownException("不支持的文件格式");
        }
    }

    private async Task<List<List<string>>> ValidateAllUsersAsync(List<UserImportData> allUserData, CancellationToken ct)
    {
        // 使用并行验证来提高性能，但限制并发数量避免数据库压力过大
        var validationTasks = allUserData.Select(async (userData, index) =>
        {
            var errors = new List<string>();

            // 基本字段验证
            if (string.IsNullOrWhiteSpace(userData.IdType))
                errors.Add("id类型不能为空");

            if (string.IsNullOrWhiteSpace(userData.IdNo))
                errors.Add("id不能为空");

            if (string.IsNullOrWhiteSpace(userData.Name))
                errors.Add("用户名不能为空");

            if (string.IsNullOrWhiteSpace(userData.RealName))
                errors.Add("真实姓名不能为空");

            if (string.IsNullOrWhiteSpace(userData.Gender))
            {
                errors.Add("性别不能为空");
            }
            else if (!ValidGenders.Contains(userData.Gender))
            {
                errors.Add("性别只能是'男'或'女'");
            }

            if (string.IsNullOrWhiteSpace(userData.BirthDate))
            {
                errors.Add("出生日期不能为空");
            }
            else if (!TryParseBirthDate(userData.BirthDate, out _))
            {
                errors.Add("出生日期格式不正确，请使用YYYY-MM-DD格式");
            }

            // 数据库重复性检查 - 只在基本验证都通过时进行
            if (!errors.Any())
            {
                try
                {
                    // 并行检查用户名、邮箱和ID是否已存在
                    var userExistsTask = _userQuery.DoesUserExist(userData.Name, ct);
                   // var idNoExistsTask = _userQuery.DoesIdNoExist(userData.IdNo, ct);

                    await Task.WhenAll(userExistsTask);

                    if (await userExistsTask)
                        errors.Add($"用户名'{userData.Name}'已存在");

                    //if (await idNoExistsTask)
                    //    errors.Add($"id'{userData.IdNo}'已存在");

                    // 邮箱检查（可选）
                    if (!string.IsNullOrWhiteSpace(userData.Email))
                    {
                        if (await _userQuery.DoesEmailExist(userData.Email, ct))
                            errors.Add($"邮箱'{userData.Email}'已存在");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"数据库验证失败: {ex.Message}");
                }
            }

            return errors;
        });

        // 限制并发数量为3，避免数据库压力过大
        var semaphore = new SemaphoreSlim(3);
        var results = new List<List<string>>();

        foreach (var task in validationTasks)
        {
            await semaphore.WaitAsync(ct);
            try
            {
                results.Add(await task);
            }
            finally
            {
                semaphore.Release();
            }
        }

        return results;
    }

    private UserImportData? ReadUserDataFromRow(ISheet worksheet, int row)
    {
        // 检查是否为空行
        if (IsEmptyRow(worksheet, row))
            return null;

        return new UserImportData
        {
            IdType = GetCellValue(worksheet, row, ExcelColumns.IdType),
            IdNo = GetCellValue(worksheet, row, ExcelColumns.IdNo),
            Name = GetCellValue(worksheet, row, ExcelColumns.Name),
            RealName = GetCellValue(worksheet, row, ExcelColumns.RealName),
            Email = GetCellValue(worksheet, row, ExcelColumns.Email),
            Phone = GetCellValue(worksheet, row, ExcelColumns.Phone),
            Gender = GetCellValue(worksheet, row, ExcelColumns.Gender),
            BirthDate = GetCellValue(worksheet, row, ExcelColumns.BirthDate)
        };
    }

    private object ReadUserDataFromRowSafe(ISheet worksheet, int row)
    {
        try
        {
            var result = ReadUserDataFromRow(worksheet, row);
            if (result == null)
            {
                return new { Row = row + 1, Error = "空行" };
            }
            else
            {
                return result;
            }
        }
        catch
        {
            return new { Row = row + 1, Error = "读取行数据失败" };
        }
    }

    private bool IsEmptyRow(ISheet worksheet, int row)
    {
        var dataRow = worksheet.GetRow(row);
        if (dataRow == null) return true;

        for (int col = 0; col < ExcelColumnCount; col++)
        {
            if (!string.IsNullOrWhiteSpace(GetCellValue(worksheet, row, col)))
                return false;
        }
        return true;
    }

    private string GetCellValue(ISheet worksheet, int row, int col)
    {
        var dataRow = worksheet.GetRow(row);
        if (dataRow == null) return string.Empty;

        var cell = dataRow.GetCell(col);
        if (cell == null) return string.Empty;

        switch (cell.CellType)
        {
            case CellType.String:
                return cell.StringCellValue?.Trim() ?? string.Empty;
            case CellType.Numeric:
                if (DateUtil.IsCellDateFormatted(cell))
                {
                    if (cell.DateCellValue != null)
                    {
                        return cell.DateCellValue.Value.ToString(DateFormat);
                    }
                }
                return cell.NumericCellValue.ToString();
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            case CellType.Formula:
                return cell.StringCellValue?.Trim() ?? string.Empty;
            default:
                return string.Empty;
        }
    }

    private bool TryParseBirthDate(string birthDateStr, out DateTimeOffset birthDate)
    {
        birthDate = default;

        if (DateTimeOffset.TryParseExact(birthDateStr, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            birthDate = date;
            return true;
        }

        if (DateTimeOffset.TryParse(birthDateStr, out date))
        {
            birthDate = date;
            return true;
        }

        return false;
    }

    private async Task CreateUserAsync(UserImportData userData, OrganizationUnitId organizationUnitId, string organizationUnitName, List<RoleId> roleIds, CancellationToken ct)
    {
        // 解析出生日期
        if (!TryParseBirthDate(userData.BirthDate, out var birthDate))
        {
            throw new KnownException("出生日期格式错误");
        }

        // 生成默认密码
        var passwordHash = PasswordHasher.HashPassword(DefaultPassword);

        // 处理角色分配
        var rolesToBeAssigned = await _roleQuery.GetAdminRolesForAssignmentAsync(roleIds, ct);

        var cmd = new CreateUserCommand(
            userData.Name,
            userData.Email,
            passwordHash,
            userData.Phone,
            userData.RealName,
            1, // 默认启用状态
            userData.Gender,
            birthDate,
            organizationUnitId,
            organizationUnitName,
            rolesToBeAssigned
        );

        await _mediator.Send(cmd, ct);
    }

    /// <summary>
    /// 用户导入数据模型
    /// </summary>
    private class UserImportData
    {
        public string IdType { get; set; } = string.Empty;
        public string IdNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
    }
}

public record BatchImportUsersRequest(
    IFormFile File,
    OrganizationUnitId OrganizationUnitId,
    string OrganizationUnitName,
    List<RoleId> RoleIds
);

public record BatchImportUsersResponse(
    int SuccessCount,
    int FailCount,
    int TotalCount,
    IEnumerable<BatchImportFailedRow> FailedRows
);

public record BatchImportFailedRow(
    int Row,
    IEnumerable<string> Errors,
    object Data
);

///// <summary>
///// 批量导入用户端点摘要
///// </summary>
//public class BatchImportUsersSummary : Summary<BatchImportUsersEndpoint>
//{
//    public BatchImportUsersSummary()
//    {
//        Summary = "批量导入用户";
//        Description = "通过Excel文件批量导入用户数据";
//        RequestExamples[0] = new BatchImportUsersRequest(
//            null!, // 文件对象
//            OrganizationUnitId.New(),
//            "组织单位名称",
//            [RoleId.New()]
//        );
//        ResponseExamples[0] = new BatchImportUsersResponse(
//            95,
//            5,
//            100,
//            [
//                new BatchImportFailedRow(3, ["用户名不能为空", "邮箱格式不正确"], new { Name = "", Email = "invalid" }),
//                new BatchImportFailedRow(7, ["该用户名已存在"], new { Name = "existing_user", Email = "test@example.com" })
//            ]
//        );
//    }
//}