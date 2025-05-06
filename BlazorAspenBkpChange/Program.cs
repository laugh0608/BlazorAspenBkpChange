using BlazorAspenBkpChange.Components;
using Microsoft.Extensions.FileProviders;

// WebApplicationBuilder 使用预配置默认值创建应用
var builder = WebApplication.CreateBuilder(args);

// Razor 组件服务通过调用 AddRazorComponents添加到应用
// 使组件能够在 Razor 服务器上呈现和执行代码
// 并用 AddInteractiveServerComponents 添加支持呈现 Interactive Server 组件的服务
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 生成 WebApplication（由以下代码中的 app 变量保存）
var app = builder.Build();

// 配置 HTTP 请求管道
if (!app.Environment.IsDevelopment())
{
    // 异常处理程序中间件 (UseExceptionHandler) 在开发应用运行时处理错误并显示开发人员异常页
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HTTP 严格传输安全协议 (HSTS) 中间件 (UseHsts) 处理 HSTS
    // 详见 https://aka.ms/aspnetcore-hsts 
    app.UseHsts();
}

// HTTPS 重定向中间件 (UseHttpsRedirection)
// 通过将 HTTP 请求重定向到 HTTPS 来强制实施 HTTPS 协议（如果 HTTPS 端口可用）
app.UseHttpsRedirection();

// 防伪中间件 (UseAntiforgery) 对表单处理强制实施防伪保护
app.UseAntiforgery();

// 配置静态文件中间件
app.UseStaticFiles(); // 默认处理 wwwroot 下的静态文件

// 显式添加对 wwwroot/bkpChanged 的支持
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "bkpChanged")),
    RequestPath = "/bkpChanged", // 映射到 /bkpChanged 路径
    ServeUnknownFileTypes = true, // 允许服务未知文件类型
    DefaultContentType = "application/octet-stream", // 设置默认 MIME 类型
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
    }
});

// 映射静态资产路由终结点约定（MapStaticAssets）
// 映射静态文件，如图像、脚本和样式表，在生成过程中生成为终结点
app.MapStaticAssets();
// MapRazorComponents 将根 App 组件中定义的组件映射到给定的 .NET 程序集并呈现可路由的组件
// 并为 AddInteractiveServerRenderMode 应用配置交互式服务器端呈现（交互式 SSR）支持
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 静态文件中间件无法满足需求，可以通过创建一个 API 端点来提供文件下载
app.MapGet("/download/{fileName}", async (string fileName) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "bkpChanged", fileName);
    if (!File.Exists(filePath))
    {
        return Results.NotFound("文件不存在");
    }

    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    return Results.File(fileStream, "application/octet-stream", fileName);
});

// 通过调用 Run (WebApplication) 上的 app 运行应用
app.Run();
