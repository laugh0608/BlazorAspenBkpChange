using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using ServiceStack;
using Microsoft.Extensions.FileProviders;

namespace BlazorAspenBkpChange.Components.Pages
{
    public partial class AspenChange
    {
        private readonly List<string> _uploadedFiles = new List<string>();
        private string _message = string.Empty;
        private string _firstLine = string.Empty;
        private string _formattedContent = string.Empty;
        private string _aspenVersion = string.Empty;
        private string _systemVersion = string.Empty;
        private string _username = string.Empty;
        private string _computerName = string.Empty;
        private string _creatTime = string.Empty;
        private string _mainDatabase = string.Empty;
        private string _aspenVersionDescription = string.Empty; // 新增字段，用于描述 Aspen 版本
        private List<string> _availableVersions = new(); // 新增字段，用于存储下拉列表选项
        private string _selectedVersion = string.Empty; // 新增字段，用于存储用户选择的版本
        private string _uploadedFilePath = string.Empty; // 新增字段，用于存储上传文件的路径
        private string _modifiedFilePath = string.Empty; // 新增字段，用于存储修改后的文件路径
        private string _apiFilePath = string.Empty; // 新增字段，用于存储API下载文件的URL

        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB  

        private async Task HandleFileSelected(InputFileChangeEventArgs e)
        {
            // 清空之前的状态
            _message = string.Empty;
            _firstLine = string.Empty;
            _formattedContent = string.Empty;
            _aspenVersion = string.Empty;
            _systemVersion = string.Empty;
            _username = string.Empty;
            _computerName = string.Empty;
            _creatTime = string.Empty;
            _mainDatabase = string.Empty;
            _aspenVersionDescription = string.Empty;
            _availableVersions.Clear();
            _selectedVersion = string.Empty;
            _uploadedFiles.Clear();
            _uploadedFilePath = string.Empty;
            _modifiedFilePath = string.Empty;

            // 获取上传的文件  
            foreach (var file in e.GetMultipleFiles())
            {
                try
                {
                    if (file.Size > MaxFileSize)
                    {
                        _message = $"文件 {file.Name} 超过了 10MB 的大小限制。";
                        continue;
                    }

                    // 生成时间戳文件名  
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var extension = Path.GetExtension(file.Name);
                    var newFileName = $"uploaded_{timestamp}{extension}";

                    // 指定保存路径（这里是 wwwroot/bkpUploads 文件夹）  
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/bkpUploads", newFileName);
                    _uploadedFilePath = path; // 保存上传文件路径

                    // 确保目录存在  
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/bkpUploads"));

                    // 保存文件  
                    await using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.OpenReadStream(MaxFileSize).CopyToAsync(stream);
                    }

                    // 读取文件的第一行内容  
                    using (var reader = new StreamReader(path, System.Text.Encoding.UTF8)) // 使用纯文本模式的 UTF-8
                    {
                        var line = await reader.ReadLineAsync();
                        _firstLine = line ?? string.Empty;

                        // 使用正则表达式匹配 DATETIME 后的时间
                        var datetimeMatch = System.Text.RegularExpressions.Regex.Match(_firstLine, @"DATETIME\s+""(.*?)""");
                        if (datetimeMatch.Success)
                        {
                            var datetimeString = datetimeMatch.Groups[1].Value;

                            // 定义时间格式
                            var format = "ddd MMM dd HH:mm:ss yyyy"; // 对应测试时间 "Sun Apr 20 21:38:14 2025"
                            var culture = System.Globalization.CultureInfo.InvariantCulture;

                            try
                            {
                                // 使用 ParseExact 解析时间
                                var parsedDateTime = DateTime.ParseExact(datetimeString, format, culture);
                                _creatTime = parsedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            catch (FormatException)
                            {
                                _creatTime = "无法解析的时间格式";
                            }
                        }
                        else
                        {
                            _creatTime = "未找到创建时间";
                        }
                    }

                    // 输出第一行内容和创建时间到控制台  
                    Console.WriteLine($"文件 {file.Name} 的第一行内容: {_firstLine}");
                    Console.WriteLine($"文件 {file.Name} 的创建时间: {_creatTime}");

                    // 读取文件内容并匹配正则表达式  
                    string fileContent;
                    using (var reader = new StreamReader(path, new System.Text.UTF8Encoding(false))) // 使用 UTF-8 无 BOM
                    {
                        fileContent = await reader.ReadToEndAsync();
                    }

                    // 使用正则表达式匹配 DSET RUN-STATUS VERS @VERS，支持字段名和内容跨行
                    var match = System.Text.RegularExpressions.Regex.Match(
                        fileContent,
                        @"DSET\s+RUN-STATUS\s+VERS\s+@VERS\s*\((.*?)\)",
                        System.Text.RegularExpressions.RegexOptions.Singleline // 启用单行模式
                    );
                    if (match.Success)
                    {
                        var matchedString = match.Groups[1].Value;
                        var parts = matchedString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        // 提取各部分内容  
                        _aspenVersion = parts.Length > 0 ? parts[0].Trim('"') : "未知";
                        _systemVersion = parts.Length > 1 ? parts[1].Trim('"') : "未知";
                        _username = parts.Length > 3 ? parts[3].Trim('"') : "未知";
                        _computerName = parts.Length > 4 ? parts[4].Trim('"') : "未知";

                        // 根据 Aspen 版本设置描述和下拉列表选项
                        UpdateAspenVersionDetails();

                        // 设置格式化内容
                        _formattedContent = "匹配成功！";

                        // 输出到控制台  
                        Console.WriteLine($"文件版本号: {_aspenVersion}");
                        Console.WriteLine($"系统版本: {_systemVersion}");
                        Console.WriteLine($"创建人: {_username}");
                        Console.WriteLine($"计算机名: {_computerName}");
                        Console.WriteLine($"Aspen版本描述: {_aspenVersionDescription}");

                        // 强制更新 UI
                        StateHasChanged();
                    }
                    else
                    {
                        _formattedContent = "未找到匹配内容，请检查文件完整性。";
                    }

                    // 使用正则表达式匹配 DATABANKS FILE-SYM-NAM，支持字段名和内容跨行
                    var databaseMatch = System.Text.RegularExpressions.Regex.Match(
                        fileContent,
                        @"DATABANKS\s+FILE-SYM-NAM\s*=\s*\((.*?)\)",
                        System.Text.RegularExpressions.RegexOptions.Singleline // 启用单行模式
                    );
                    if (databaseMatch.Success)
                    {
                        _mainDatabase = databaseMatch.Groups[1].Value.Trim('"');

                        // 输出到控制台  
                        Console.WriteLine($"使用的主数据库: {_mainDatabase}");
                    }
                    else
                    {
                        _mainDatabase = "未找到主数据库信息";
                    }

                    // 将新文件名添加到列表中  
                    _uploadedFiles.Add(newFileName);
                }
                catch (Exception ex)
                {
                    _message = $"文件 {file.Name} 上传失败: {ex.Message}";
                }
            }

            if (!string.IsNullOrEmpty(_message))
            {
                return;
            }

            _message = "文件上传成功！";
        }

        private async Task SaveModifiedFile()
        {
            _apiFilePath = string.Empty;
            if (string.IsNullOrEmpty(_uploadedFilePath) || string.IsNullOrEmpty(_selectedVersion))
            {
                _message = "请先上传文件并选择目标版本。";
                return;
            }

            try
            {
                // 确定目标版本对应的 _aspenVersion 和 APV 值
                string? targetAspenVersion = _selectedVersion switch
                {
                    "V12" => "38.0",
                    "V11" => "37.0",
                    "V9" => "36.0",
                    _ => null
                };

                string? targetApvField = _selectedVersion switch
                {
                    "V12" => "APV120",
                    "V11" => "APV110",
                    "V9" => "APV90",
                    _ => null
                };

                if (string.IsNullOrEmpty(targetAspenVersion) || string.IsNullOrEmpty(targetApvField))
                {
                    _message = "无效的目标版本选择。";
                    return;
                }

                // 读取上传文件内容
                string fileContent;
                using (var reader = new StreamReader(_uploadedFilePath, new System.Text.UTF8Encoding(false)))
                {
                    fileContent = await reader.ReadToEndAsync();
                }

                // 匹配 _aspenVersion 的值
                var versionMatch = System.Text.RegularExpressions.Regex.Match(
                    fileContent,
                    @"DSET\s+RUN-STATUS\s+VERS\s+@VERS\s*\(\s*""([^""]+)""",
                    System.Text.RegularExpressions.RegexOptions.Singleline
                );

                if (!versionMatch.Success)
                {
                    _message = "未找到 _aspenVersion 值，无法进行修改。";
                    return;
                }

                string currentAspenVersion = versionMatch.Groups[1].Value; // 提取当前 _aspenVersion 值
                Console.WriteLine($"匹配到的 _aspenVersion 值: {currentAspenVersion}");

                // 匹配 APV 字段
                string? currentApvField = _aspenVersion switch
                {
                    "40.0" => "APV140",
                    "38.0" => "APV120",
                    "37.0" => "APV110",
                    "36.0" => "APV90",
                    _ => null
                };

                if (string.IsNullOrEmpty(currentApvField))
                {
                    _message = "未找到对应的 APV 字段，无法进行修改。";
                    return;
                }

                // 修改 _aspenVersion 和 APV 字段
                int matchCount = 0;
                fileContent = System.Text.RegularExpressions.Regex.Replace(
                    fileContent,
                    $@"""{currentAspenVersion}""", // 匹配全文中当前 _aspenVersion 的值
                    match =>
                    {
                        matchCount++;
                        return $"\"{targetAspenVersion}\""; // 替换为目标版本值
                    }
                );

                fileContent = System.Text.RegularExpressions.Regex.Replace(
                    fileContent,
                    $@"\b{currentApvField}\b", // 匹配全文中当前 APV 字段，考虑跨行
                    match =>
                    {
                        matchCount++;
                        return targetApvField; // 替换为目标 APV 字段
                    },
                    System.Text.RegularExpressions.RegexOptions.Singleline
                );

                // 确保下载路径的目录存在
                var changedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/bkpChanged");
                Directory.CreateDirectory(changedDirectory);

                // 生成新的文件名
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var newFileName = $"changed_{timestamp}.bkp";
                var newFilePath = Path.Combine(changedDirectory, newFileName);

                // 确保目录存在
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/bkpChanged"));

                // 保存修改后的文件，使用 UTF-8 无 BOM
                await using (var writer = new StreamWriter(newFilePath, false, new System.Text.UTF8Encoding(false)))
                {
                    await writer.WriteAsync(fileContent);
                }

                // 输出匹配和修改次数到页面和控制台
                _message = $"文件已成功保存为: {newFileName}，匹配并修改了 {matchCount} 次。";
                Console.WriteLine($"修改后的文件已保存到: {newFilePath}");
                Console.WriteLine($"匹配并修改了 {matchCount} 次。");

                // 保存修改后的文件路径
                _modifiedFilePath = $"/bkpChanged/{newFileName}";
                //_modifiedFilePath = newFilePath;

                Console.WriteLine($"文件保存路径: {newFilePath}");
                Console.WriteLine($"文件是否存在: {File.Exists(newFilePath)}");
                Console.WriteLine($"文件的相对路径: {_modifiedFilePath}");
                //Console.WriteLine($"物理路径: {Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "bkpChanged", "changed_20250506_144657.bkp")}");

                // 拼接 API 下载文件的URL
                _apiFilePath = $"/download/{newFileName}";
                // 输出检查一下
                Console.WriteLine($"API 下载文件的URL: {_apiFilePath}");

                // 强制更新 UI
                StateHasChanged();
                // 强制刷新静态文件中间件
                //var fileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "bkpChanged"));
                //using var changeToken = fileProvider.Watch("**/*");
            }
            catch (Exception ex)
            {
                _message = $"保存修改后的文件时出错: {ex.Message}";
            }
        }

        // 下载文件的 JavaScript 函数
        private async Task DownloadFileFromURL()
        {
            if (string.IsNullOrEmpty(_modifiedFilePath))
            {
                _message = "没有可供下载的文件。";
                return;
            }

            // 提取文件名
            var fileName = Path.GetFileName(_modifiedFilePath);
            //var fileURL = _modifiedFilePath;
            var fileURL = _apiFilePath; // 更改为使用 API 下载文件

            // 调用 JavaScript 函数触发下载
            await JS.InvokeVoidAsync("triggerFileDownload", fileName, fileURL);
        }

        private void UpdateAspenVersionDetails()
        {
            switch (_aspenVersion)
            {
                case "40.0":
                    _aspenVersionDescription = "上传的文件Aspen版本为V14";
                    _availableVersions = new List<string> { "V12", "V11", "V9" };
                    break;
                case "38.0":
                    _aspenVersionDescription = "上传的文件Aspen版本为V12";
                    _availableVersions = new List<string> { "V11", "V9" };
                    break;
                case "37.0":
                    _aspenVersionDescription = "上传的文件Aspen版本为V11";
                    _availableVersions = new List<string> { "V9" };
                    break;
                case "36.0":
                    _aspenVersionDescription = "上传的文件Aspen版本为V9，无法转换更低版本";
                    _availableVersions = new List<string>();
                    break;
                default:
                    _aspenVersionDescription = "未知的Aspen版本";
                    _availableVersions = new List<string>();
                    break;
            }
        }
    }
}
