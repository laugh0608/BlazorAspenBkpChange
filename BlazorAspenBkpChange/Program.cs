using BlazorAspenBkpChange.Components;

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

// 映射静态资产路由终结点约定（MapStaticAssets）
// 映射静态文件，如图像、脚本和样式表，在生成过程中生成为终结点
app.MapStaticAssets();
// MapRazorComponents 将根 App 组件中定义的组件映射到给定的 .NET 程序集并呈现可路由的组件
// 并为 AddInteractiveServerRenderMode 应用配置交互式服务器端呈现（交互式 SSR）支持
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 通过调用 Run (WebApplication) 上的 app 运行应用
app.Run();
