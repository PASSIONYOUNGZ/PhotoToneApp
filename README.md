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

## 环境要求

推荐环境：

- Windows 10 或 Windows 11
- Visual Studio 2022 Community 或更高版本
- Visual Studio 工作负载：
  - `.NET desktop development`
- .NET 8 SDK
- Git for Windows

检查 .NET SDK：

```powershell
dotnet --list-sdks
```

建议能看到 `8.0.xxx`。如果只看到 `10.0.xxx`，也可能可以打开项目，但本项目目标框架是 `.NET 8`，建议额外安装 `.NET 8 SDK`，避免 WPF 目标包或调试环境不完整。

检查 Git：

```powershell
git --version
```

## 获取项目

克隆仓库：

```powershell
git clone https://github.com/PASSIONYOUNGZ/PhotoToneApp.git
cd PhotoToneApp
```

## 打开项目

推荐优先打开传统解决方案文件：

```text
PhotoToneApp.sln
```

也可以直接打开项目文件：

```text
PhotoToneApp/PhotoToneApp.csproj
```

仓库里也保留了 `PhotoToneApp.slnx`，但不同 Visual Studio 版本对 `.slnx` 的支持不完全一致。如果打开 `.slnx` 后提示“无法启动调试，启动项目无法启动”，请改为打开 `PhotoToneApp.sln`。

Visual Studio 中打开后，如果运行按钮不可用或提示没有启动项目：

1. 在解决方案资源管理器中右键 `PhotoToneApp` 项目。
2. 选择“设为启动项目”。
3. 顶部配置选择 `Debug` 和 `Any CPU`。
4. 再点击运行。

## 第一次启动前必须还原 NuGet

从 GitHub clone 下来的项目不会包含 `obj/project.assets.json`，这是正常的。这个文件是 NuGet restore 生成的，不应该提交到 Git。

如果看到类似错误：

```text
error NETSDK1004: 找不到资产文件 “...\PhotoToneApp\obj\project.assets.json”。
运行 NuGet 包还原以生成此文件。
```

说明还没有执行 NuGet 还原。

请在仓库根目录执行：

```powershell
cd E:\PhotoToneApp
dotnet restore .\PhotoToneApp\PhotoToneApp.csproj
dotnet build .\PhotoToneApp\PhotoToneApp.csproj
```

如果你 clone 到的路径不是 `E:\PhotoToneApp`，请改成自己的实际路径。

也可以在 Visual Studio 中执行：

1. 菜单栏选择“生成”。
2. 点击“还原 NuGet 包”。
3. 或右键解决方案，选择“还原 NuGet 包”。
4. 还原完成后再生成和运行。

如果 restore 失败，请检查：

- 公司网络是否能访问 NuGet。
- Visual Studio 是否安装了 `.NET desktop development` 工作负载。
- 是否安装了 `.NET 8 SDK`。
- Visual Studio 的 NuGet 源是否启用了 `nuget.org`。

## 编译

```powershell
dotnet build .\PhotoToneApp\PhotoToneApp.csproj
```

## 运行

方式一：Visual Studio 中直接运行。

方式二：命令行运行：

```powershell
dotnet run --project .\PhotoToneApp\PhotoToneApp.csproj
```

运行后点击“打开文件夹”，选择一个包含 `.jpg/.jpeg/.png/.bmp` 图片的文件夹。

测试时可以创建：

```text
D:\111
```

然后放入一些测试图片。

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
