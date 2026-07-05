# PhotoToneApp

PhotoToneApp 是一个 Windows 本地图片选片工具，面向摄影师和需要快速整理照片的人使用。当前版本是一个可运行的 WPF MVP，支持本地文件夹扫描、缩略图、预览、主色调分析、筛选、评分、入选/淘汰、双图对比和导出。

项目类型：C# WPF Application

目标框架：.NET 8

当前状态：本地选片工具 MVP

## 功能概览

### 图片扫描

- 打开本地图片文件夹。
- 只扫描当前文件夹，不递归扫描子文件夹。
- 支持格式：
  - `.jpg`
  - `.jpeg`
  - `.png`
  - `.bmp`
- 非图片文件会被忽略。
- 损坏图片或无法访问的图片不会导致程序崩溃。

### 缩略图和预览

- 中间区域以网格卡片显示图片缩略图。
- 点击图片后，右侧显示真实大图预览。
- 双击缩略图可打开独立大图查看窗口。
- 图片加载不会长期锁定原图文件，方便用户在资源管理器中继续重命名或删除原图。

### 主色调分析

软件会自动分析每张图片的主色调，并显示在图片卡片和右侧信息区。

支持色调标签：

- 红色
- 橙色
- 黄色
- 绿色
- 青色
- 蓝色
- 紫色
- 粉色
- 棕色
- 黑色
- 白色
- 灰色
- 多色
- 未知

当前主色调分析使用 WPF 内置图像能力，不依赖第三方图片处理库。

### 筛选和搜索

支持多维度组合筛选：

- 状态筛选：
  - 全部
  - 收藏
  - 入选
  - 淘汰
  - 未评分
  - 3星以上
  - 4星以上
  - 5星
- 颜色筛选：
  - 全部颜色
  - 红色、橙色、黄色、绿色、青色、蓝色、紫色、粉色、棕色、黑色、白色、灰色、多色
- 图片形态筛选：
  - 全部形态
  - 横图
  - 竖图
  - 方图
  - 16:9
  - 9:16
- 尺寸质量筛选：
  - 全部尺寸
  - 小图
  - 普通图
  - 高清图
  - 大图
- 文件名搜索：
  - 只搜索文件名
  - 大小写不敏感

左侧筛选按钮有明显选中态，可以看出当前选择了哪些筛选条件。

### 排序

顶部排序下拉框支持：

- 文件名
- 修改时间
- 文件大小
- 宽度
- 高度
- 评分

### 收藏、评分、入选、淘汰

- 收藏 / 取消收藏。
- 0-5 星评分。
- 入选 / 取消入选。
- 淘汰 / 取消淘汰。
- 入选和淘汰互斥：
  - 设置入选时会取消淘汰。
  - 设置淘汰时会取消入选。
- 这些状态会保存到本地 JSON 缓存。

### 重命名和删除

- 支持重命名当前选中图片。
- 重命名时只输入新文件名，不需要输入扩展名。
- 程序自动保留原扩展名。
- 删除图片时会先弹出确认框。
- 删除操作会移动到 Windows 回收站，不会永久删除。

### 单图大图查看

单图查看窗口支持：

- 鼠标滚轮缩放。
- 鼠标左键拖拽移动图片。
- 适应窗口。
- 100% 原始大小。
- 全屏 / 退出全屏。
- 左右方向键切换上一张 / 下一张。
- 设置收藏、评分、入选、淘汰。

### 双图对比

双图对比窗口支持：

- 主窗口主动设置左图和右图。
- 左右并排查看两张图片。
- 左右图片分别切换上一张 / 下一张。
- 在对比窗口内通过下拉框替换左图或右图。
- 防止左右选择同一张图片。
- 同步缩放。
- 同步拖拽。
- 分别设置左图 / 右图的收藏、评分、入选、淘汰。

### 导出图片

支持导出：

- 当前显示图片
- 收藏图片
- 入选图片
- 3星以上图片
- 5星图片

导出规则：

- 导出是复制图片，不是移动图片。
- 原图保留在原文件夹。
- 用户选择目标文件夹。
- 如果目标文件夹已有同名文件，不覆盖，自动追加后缀：
  - `image.jpg`
  - `image_1.jpg`
  - `image_2.jpg`
- 单张图片导出失败不会影响其他图片。
- 导出完成后显示总数、成功数、失败数。

## 技术栈

- C#
- WPF
- .NET 8
- System.Text.Json
- WPF 内置图像 API：
  - BitmapImage
  - BitmapCacheOption.OnLoad
  - FormatConvertedBitmap
  - PngBitmapEncoder

当前没有使用：

- SixLabors.ImageSharp
- SQLite
- 第三方图片处理库
- 完整 MVVM 框架

## 环境要求和新电脑准备

推荐系统：

- Windows 10 或 Windows 11

必须安装：

- Git for Windows
- .NET SDK
- Visual Studio Community，推荐安装 Visual Studio 2022 Community 或 Visual Studio 2026 Community

Visual Studio 安装时请勾选工作负载：

```text
.NET desktop development
```

如果安装器里有单独组件选择，建议确认包含：

- .NET 8 SDK
- .NET 8 Windows Desktop Runtime / Targeting Pack
- MSBuild
- NuGet package manager

官方下载入口：

- .NET 8 SDK: `https://dotnet.microsoft.com/download/dotnet/8.0`
- Visual Studio Community: `https://visualstudio.microsoft.com/`
- Git for Windows: `https://git-scm.com/download/win`

### 已验证环境

本项目目标框架写在 `PhotoToneApp/PhotoToneApp.csproj`：

```xml
<TargetFramework>net8.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
```

也就是说，项目是 `.NET 8 WPF` 应用。

当前已在以下环境观察到可以通过命令行构建和运行：

```text
dotnet --list-sdks
10.0.301 [C:\Program Files\dotnet\sdk]
```

说明：

- `dotnet` CLI 可以使用更高版本 SDK 构建低版本目标框架，例如用 .NET 10 SDK 构建 `net8.0-windows`。
- 但为了 Visual Studio 正确识别、调试和补齐 WPF 目标包，仍建议安装 `.NET 8 SDK`。
- 如果 Visual Studio 提示下载 `.NET 8`，请按提示安装。

检查环境：

```powershell
dotnet --list-sdks
dotnet --info
git --version
```

如果 `dotnet --list-sdks` 只有 `10.0.301`，项目仍可能能运行；但如果 Visual Studio 启动失败、restore 失败或提示缺少目标框架，请安装 `.NET 8 SDK`。

## 推荐启动方式：命令行优先

新电脑第一次运行时，优先使用命令行。这样可以绕开 Visual Studio 对启动项目、解决方案格式、NuGet 还原状态的影响。

完整流程如下。

### 1. 克隆项目

选择一个放代码的目录，例如 `E:\`：

```powershell
E:
git clone https://github.com/PASSIONYOUNGZ/PhotoToneApp.git
cd PhotoToneApp
```

如果之前已经 clone 过，进入项目后拉取最新代码：

```powershell
git pull
```

### 2. 确认当前目录

执行：

```powershell
dir
```

应该能看到：

```text
PhotoToneApp.sln
PhotoToneApp.slnx
README.md
PROJECT_HANDOFF.md
PhotoToneApp\
```

真正的 WPF 项目在子目录：

```text
PhotoToneApp\PhotoToneApp.csproj
```

### 3. 检查 SDK

```powershell
dotnet --list-sdks
```

如果没有 `.NET 8 SDK`，建议安装。即使只有 `.NET 10 SDK`，也可以先继续执行 restore/build 验证。

### 4. 还原 NuGet

从 GitHub clone 下来的项目不会包含 `obj/project.assets.json`，这是正常的。这个文件由 restore 生成，不应该提交到 Git。

在仓库根目录执行：

```powershell
dotnet restore .\PhotoToneApp.sln
```

如果只想还原项目文件，也可以：

```powershell
dotnet restore .\PhotoToneApp\PhotoToneApp.csproj
```

### 5. 编译

推荐先编译解决方案：

```powershell
dotnet build .\PhotoToneApp.sln
```

也可以编译项目：

```powershell
dotnet build .\PhotoToneApp\PhotoToneApp.csproj
```

看到类似输出即表示成功：

```text
PhotoToneApp net8.0-windows 已成功
PhotoToneApp\bin\Debug\net8.0-windows\PhotoToneApp.dll
```

### 6. 运行

命令行运行：

```powershell
dotnet run --project .\PhotoToneApp\PhotoToneApp.csproj
```

运行成功后会打开 PhotoToneApp 主窗口。

然后点击“打开文件夹”，选择一个包含图片的目录，例如：

```text
D:\111
```

支持图片格式：

- `.jpg`
- `.jpeg`
- `.png`
- `.bmp`

## Visual Studio 启动方式

命令行可以运行成功后，再使用 Visual Studio 打开项目进行编辑和调试。

推荐打开：

```text
PhotoToneApp.sln
```

不推荐优先打开：

```text
PhotoToneApp.slnx
```

原因：

- `.slnx` 是较新的解决方案格式。
- 不同 Visual Studio 版本对 `.slnx` 的支持和启动项目识别可能不一致。
- 如果打开 `.slnx` 后提示“无法启动调试，启动项目无法启动”，请关闭后改用 `PhotoToneApp.sln`。

也可以直接打开项目文件：

```text
PhotoToneApp\PhotoToneApp.csproj
```

Visual Studio 打开后请确认：

1. 解决方案资源管理器中能看到 `PhotoToneApp` 项目。
2. 右键 `PhotoToneApp` 项目。
3. 选择“设为启动项目”。
4. 顶部配置选择：
   - `Debug`
   - `Any CPU`
5. 菜单栏选择“生成”。
6. 点击“还原 NuGet 包”。
7. 再点击“生成解决方案”。
8. 最后按 `F5` 或点击绿色运行按钮。

## Visual Studio 是否必须

不是必须。

这个项目可以只用命令行启动：

```powershell
dotnet restore .\PhotoToneApp.sln
dotnet build .\PhotoToneApp.sln
dotnet run --project .\PhotoToneApp\PhotoToneApp.csproj
```

这些命令主要依赖：

- .NET SDK
- MSBuild
- NuGet restore

Visual Studio 的作用是：

- 提供代码编辑器。
- 提供 XAML/WPF 设计和调试体验。
- 安装 `.NET desktop development` 工作负载时会补齐很多 WPF 开发组件。
- 方便断点调试、查看错误、管理项目文件。

所以：

- 运行验证：优先用命令行。
- 日常开发和调试：推荐用 Visual Studio。

## 常见启动问题

### NETSDK1004：找不到 project.assets.json

错误示例：

```text
error NETSDK1004: 找不到资产文件 “...\PhotoToneApp\obj\project.assets.json”。
运行 NuGet 包还原以生成此文件。
```

原因：

- 刚 clone 项目后还没有执行 NuGet restore。
- `obj/project.assets.json` 是本机生成文件，不会提交到 GitHub。

解决：

```powershell
dotnet restore .\PhotoToneApp.sln
dotnet build .\PhotoToneApp.sln
```

或者：

```powershell
dotnet restore .\PhotoToneApp\PhotoToneApp.csproj
dotnet build .\PhotoToneApp\PhotoToneApp.csproj
```

### 打开 slnx 后提示无法启动调试

原因：

- Visual Studio 对 `.slnx` 支持不稳定。
- 启动项目没有正确识别。

解决：

1. 关闭 Visual Studio。
2. 打开 `PhotoToneApp.sln`。
3. 右键 `PhotoToneApp` 项目。
4. 选择“设为启动项目”。
5. 还原 NuGet 包。
6. 重新生成并运行。

### Visual Studio 提示下载 .NET 8

原因：

- 项目目标框架是 `net8.0-windows`。
- 你的电脑可能只有 `.NET 10 SDK`，但 VS 需要 `.NET 8` 相关目标包或运行时来完成调试。

解决：

- 按 Visual Studio 提示安装 `.NET 8`。
- 或手动安装 `.NET 8 SDK`。

### dotnet restore 很慢

第一次 restore 可能需要几十秒，尤其是新电脑第一次访问 NuGet。

如果长时间失败，检查：

- 网络是否能访问 NuGet。
- Visual Studio 的 NuGet 源是否启用 `nuget.org`。
- 公司网络是否需要代理。

### NU1900 警告

如果看到：

```text
warning NU1900: 获取包漏洞数据时出错
```

通常只是 NuGet 漏洞数据联网失败，不代表项目编译失败。只要最后显示“生成成功”，可以先忽略。

## 本地缓存位置

PhotoToneApp 会在本机 LocalAppData 下保存缓存。

缓存目录：

```text
%LocalAppData%\PhotoToneApp
```

JSON 缓存：

```text
%LocalAppData%\PhotoToneApp\cache.json
```

缩略图缓存：

```text
%LocalAppData%\PhotoToneApp\Thumbnails
```

这些缓存不需要提交到 Git，也不会跟随仓库迁移。换电脑后第一次打开图片文件夹时会重新生成。

## 项目结构

```text
PhotoToneApp/
  App.xaml
  App.xaml.cs
  MainWindow.xaml
  MainWindow.xaml.cs
  PhotoToneApp.csproj

  Controls/
    RenameDialog.xaml
    RenameDialog.xaml.cs
    ZoomableImageViewer.xaml
    ZoomableImageViewer.xaml.cs

  Helpers/
    AppPathHelper.cs
    ImageDimensionHelper.cs

  Models/
    CacheData.cs
    ColorAnalyzeResult.cs
    ImageItem.cs

  Services/
    ColorAnalyzeService.cs
    ExportService.cs
    FileOperationService.cs
    ImageLoadService.cs
    ImageScanService.cs
    JsonCacheService.cs
    ThumbnailService.cs

  Windows/
    ImageCompareWindow.xaml
    ImageCompareWindow.xaml.cs
    ImageViewerWindow.xaml
    ImageViewerWindow.xaml.cs
```

## 关键文件说明

- `MainWindow.xaml`：主窗口 UI。
- `MainWindow.xaml.cs`：主窗口主要业务逻辑。
- `Models/ImageItem.cs`：图片数据模型。
- `Services/ImageScanService.cs`：扫描文件夹。
- `Services/ImageLoadService.cs`：加载预览和大图，避免锁定原图。
- `Services/ThumbnailService.cs`：生成缩略图缓存。
- `Services/ColorAnalyzeService.cs`：分析主色调。
- `Services/JsonCacheService.cs`：读写 JSON 缓存。
- `Services/FileOperationService.cs`：重命名、删除到回收站。
- `Services/ExportService.cs`：复制导出图片。
- `Controls/ZoomableImageViewer.*`：可缩放、拖拽的图片查看控件。
- `Windows/ImageViewerWindow.*`：单图查看窗口。
- `Windows/ImageCompareWindow.*`：双图对比窗口。

## 快捷键

主窗口：

| 快捷键 | 功能 |
|---|---|
| `1-5` | 设置当前图片评分 |
| `0` | 清除评分 |
| `P` | 入选 / 取消入选 |
| `X` | 淘汰 / 取消淘汰 |
| `F` | 收藏 / 取消收藏 |
| `Delete` | 删除到回收站 |
| `Enter` | 打开大图查看 |
| `C` | 打开双图对比 |
| `Left / Right` | 切换当前可见图片 |

单图窗口：

| 快捷键 | 功能 |
|---|---|
| `1-5` | 设置评分 |
| `0` | 清除评分 |
| `P` | 入选 / 取消入选 |
| `X` | 淘汰 / 取消淘汰 |
| `F` | 收藏 / 取消收藏 |
| `Left / Right` | 上一张 / 下一张 |
| `F11` | 全屏 / 退出全屏 |
| `Esc` | 退出全屏或关闭窗口 |

双图对比窗口：

| 快捷键 | 功能 |
|---|---|
| `1-5` | 给当前操作侧评分 |
| `0` | 清除当前操作侧评分 |
| `P` | 当前操作侧入选 / 取消入选 |
| `X` | 当前操作侧淘汰 / 取消淘汰 |
| `F` | 当前操作侧收藏 / 取消收藏 |
| `A / D` | 左图上一张 / 下一张 |
| `J / L` | 右图上一张 / 下一张 |
| `S` | 同步视图开关 |
| `Esc` | 关闭对比窗口 |

## 文件不锁定说明

图片加载使用 `FileStream` 加载，并设置：

```csharp
BitmapCacheOption.OnLoad
```

加载完成后释放文件流，并对图片对象调用 `Freeze()`。

这样做的目的是避免 WPF 长期占用原图文件。用户在预览、单图查看或双图对比后，仍应可以在资源管理器里重命名或删除原图。

## 不提交到 Git 的内容

`.gitignore` 已排除：

- `.vs/`
- `.agents/`
- `.codex/`
- `bin/`
- `obj/`
- `*.user`
- 本地导出目录

不要把测试图片、LocalAppData 缓存、缩略图缓存提交到仓库。

## 当前限制

当前版本暂不支持：

- RAW / WebP / TIFF
- AI 图片识别
- 批量重命名
- 批量删除
- 导出压缩包
- 导出报告
- 图片编辑
- 多图对比
- 数据库
- 完整 MVVM 架构

## 后续开发建议

建议先保持当前 MVP 稳定，再逐步增加：

- 扫描进度条和取消扫描。
- 导出进度条。
- 更明显的卡片视觉状态。
- 更好的异常日志。
- 性能优化。
- 更多筛选维度。
- 图片 EXIF 信息读取。

## 项目开发背景和 Codex 上下文

PhotoToneApp 是一个 Windows 本地图片选片 App，目标用户是摄影师或需要快速筛选照片的人。

这个项目是按阶段逐步开发出来的，当前已经完成：

1. 基础 WPF 主界面。
2. 打开文件夹和扫描真实图片。
3. 真实缩略图和右侧大图预览。
4. 不锁定原图的图片加载方式。
5. 缩略图缓存到 LocalAppData。
6. 主色调分析。
7. 颜色筛选和文件名搜索。
8. 收藏 / 取消收藏。
9. 删除到 Windows 回收站。
10. 重命名真实图片。
11. JSON 缓存。
12. 单图大图查看窗口。
13. 缩放、拖拽、全屏。
14. 双图对比窗口。
15. 同步缩放和同步拖拽。
16. 主动选择左图 / 右图进行对比。
17. 对比窗口内替换左图 / 右图。
18. 星级评分。
19. 入选 / 淘汰标记。
20. 按评分、入选、淘汰筛选。
21. 横图 / 竖图 / 方图 / 16:9 / 9:16 筛选。
22. 分辨率质量筛选。
23. 排序。
24. 导出图片。

当前重要技术约束：

- 不使用 `SixLabors.ImageSharp`。
- 不引入第三方图片处理库。
- 不使用 SQLite。
- 不做完整 MVVM 重构。
- 不递归扫描子文件夹。
- 不支持 RAW / WebP / TIFF。
- 图片加载不能锁定原图文件。
- 删除必须进入 Windows 回收站，不能永久删除。
- 导出只能复制，不能移动或覆盖原图。

当前主要数据模型是：

```text
Models/ImageItem.cs
```

当前主要业务入口是：

```text
MainWindow.xaml
MainWindow.xaml.cs
```

当前服务层集中在：

```text
Services/
```

当前可复用控件和独立窗口：

```text
Controls/ZoomableImageViewer.xaml
Windows/ImageViewerWindow.xaml
Windows/ImageCompareWindow.xaml
```

换电脑后，如果继续用 Codex 开发，推荐第一条消息这样写：

```text
我正在继续开发 PhotoToneApp，这是一个 C# WPF .NET 8 本地图片选片 App。

请先阅读 README.md 和 PROJECT_HANDOFF.md。

真正的 WPF 项目在 PhotoToneApp 子目录。

请注意：
1. 当前项目不使用 SixLabors.ImageSharp；
2. 不引入第三方图片处理库；
3. 不重构成完整 MVVM；
4. 图片加载不能锁定原图；
5. 修改后优先运行 dotnet restore .\PhotoToneApp.sln 和 dotnet build .\PhotoToneApp.sln 验证；
6. 如果要运行程序，使用 dotnet run --project .\PhotoToneApp\PhotoToneApp.csproj。

请先理解当前代码结构，不要大幅改动已有功能。
```

如果后续要继续开发新阶段，建议每次明确说明：

- 当前阶段只做什么。
- 哪些功能不能破坏。
- 是否允许修改 XAML。
- 是否允许新增服务类。
- 是否需要保存到 JSON 缓存。
- 是否需要保持文件不锁定。

## 给 Codex 的续接说明

如果在新电脑上继续用 Codex 开发，可以先让 Codex 阅读：

```text
README.md
PROJECT_HANDOFF.md
```

并说明：

```text
这是一个 C# WPF .NET 8 项目。真正的 WPF 项目在 PhotoToneApp 子目录。
请沿用当前代码风格，不要大幅重构，不要改成完整 MVVM。
当前项目不使用 SixLabors.ImageSharp，不引入第三方图片处理库。
每次修改后请运行 dotnet build .\PhotoToneApp\PhotoToneApp.csproj 验证。
```
