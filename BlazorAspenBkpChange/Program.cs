using BlazorAspenBkpChange.Components;

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

// ӳ�侲̬�ʲ�·���ս��Լ����MapStaticAssets��
// ӳ�侲̬�ļ�����ͼ�񡢽ű�����ʽ�������ɹ���������Ϊ�ս��
app.MapStaticAssets();
// MapRazorComponents ���� App ����ж�������ӳ�䵽������ .NET ���򼯲����ֿ�·�ɵ����
// ��Ϊ AddInteractiveServerRenderMode Ӧ�����ý���ʽ�������˳��֣�����ʽ SSR��֧��
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ͨ������ Run (WebApplication) �ϵ� app ����Ӧ��
app.Run();
