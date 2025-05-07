## BlazorAspenBkpChange

AspNetBlazor 写的一个 Aspen 的 bkp 文件版本转换器，支持 V9-14 的向下兼容转换。

学习 C# 的一个小项目，使用了 Blazor 的 WebAssembly 技术。

由于没有使用 ASP.NET Core 的 MVC 模式，所以代码结构比较简单。

项目依赖于 .NET 9.0 SDK。

> 学习代码示例，代码比较垃圾，不喜勿喷（），真正意义上自己写的第一个小小小小项目。

## 注意

因为各个版本的 PURE 数据库不一致，目前来说无法做到完全匹配，所以打开之后大概率会出现丢失数据库的警告；

关闭警告之后在物性设置中将 **企业数据库中的所有数据库** 选中，但还是可能会出现文件可以打开但无法运算的问题。

这是因为 Aspen 的保存规则会将流股中的特征组分数据库一并保存，目前程序无法做到准确识别每一个并替换；

如果全部替换则可能会导致更多的报错数据库丢失。

## 使用方法

1. 下载 Release 压缩包解压；
2. 双击 `BlazorAspenBkpChange.exe` 文件；
3. 等待服务启动之后打开浏览器；
4. 访问 `http://localhost:5000` 即可使用。

> 注：这是第一次打包和发布，可能有未知的 bug，欢迎提交 issues 进行反馈。

## 开发

> dotNET 9.0 SDK 及以上版本

1. `git clone https://github.com/laugh0608/BlazorAspenBkpChange.git` ；
2. `cd BlazorAspenBkpChange` ；
3. `dotnet run` ；
4. 访问 `http://localhost:5167` 或 `https://localhost:7167` 。
