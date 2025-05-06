using BlazorAspenBkpChange.Components;
using Microsoft.Extensions.FileProviders;

// WebApplicationBuilder ʹ��Ԥ����Ĭ��ֵ����Ӧ��
var builder = WebApplication.CreateBuilder(args);

// Razor �������ͨ������ AddRazorComponents��ӵ�Ӧ��
// ʹ����ܹ��� Razor �������ϳ��ֺ�ִ�д���
// ���� AddInteractiveServerComponents ���֧�ֳ��� Interactive Server ����ķ���
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ���� WebApplication�������´����е� app �������棩
var app = builder.Build();

// ���� HTTP ����ܵ�
if (!app.Environment.IsDevelopment())
{
    // �쳣��������м�� (UseExceptionHandler) �ڿ���Ӧ������ʱ���������ʾ������Ա�쳣ҳ
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HTTP �ϸ��䰲ȫЭ�� (HSTS) �м�� (UseHsts) ���� HSTS
    // ��� https://aka.ms/aspnetcore-hsts 
    app.UseHsts();
}

// HTTPS �ض����м�� (UseHttpsRedirection)
// ͨ���� HTTP �����ض��� HTTPS ��ǿ��ʵʩ HTTPS Э�飨��� HTTPS �˿ڿ��ã�
app.UseHttpsRedirection();

// ��α�м�� (UseAntiforgery) �Ա�����ǿ��ʵʩ��α����
app.UseAntiforgery();

// ���þ�̬�ļ��м��
app.UseStaticFiles(); // Ĭ�ϴ��� wwwroot �µľ�̬�ļ�

// ��ʽ��Ӷ� wwwroot/bkpChanged ��֧��
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "bkpChanged")),
    RequestPath = "/bkpChanged", // ӳ�䵽 /bkpChanged ·��
    ServeUnknownFileTypes = true, // �������δ֪�ļ�����
    DefaultContentType = "application/octet-stream", // ����Ĭ�� MIME ����
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache");
    }
});

// ӳ�侲̬�ʲ�·���ս��Լ����MapStaticAssets��
// ӳ�侲̬�ļ�����ͼ�񡢽ű�����ʽ�������ɹ���������Ϊ�ս��
app.MapStaticAssets();
// MapRazorComponents ���� App ����ж�������ӳ�䵽������ .NET ���򼯲����ֿ�·�ɵ����
// ��Ϊ AddInteractiveServerRenderMode Ӧ�����ý���ʽ�������˳��֣�����ʽ SSR��֧��
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ��̬�ļ��м���޷��������󣬿���ͨ������һ�� API �˵����ṩ�ļ�����
app.MapGet("/download/{fileName}", async (string fileName) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "bkpChanged", fileName);
    if (!File.Exists(filePath))
    {
        return Results.NotFound("�ļ�������");
    }

    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    return Results.File(fileStream, "application/octet-stream", fileName);
});

// ͨ������ Run (WebApplication) �ϵ� app ����Ӧ��
app.Run();
