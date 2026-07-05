# PhotoToneApp 项目交接与迁移说明

## 1. 项目当前状态

项目名称：`PhotoToneApp`

项目类型：C# WPF 桌面应用

目标框架：`.NET 8`，项目文件为 `PhotoToneApp/PhotoToneApp.csproj`

当前外层目录：

```text
C:\Users\PassionYoungz\source\repos\PhotoToneApp
```

真正的 WPF 项目目录：

```text
C:\Users\PassionYoungz\source\repos\PhotoToneApp\PhotoToneApp
```

解决方案文件：

```text
PhotoToneApp.slnx
```

当前没有使用 `SixLabors.ImageSharp`，也没有引入任何第三方图片处理库。图片扫描、加载、缩略图、颜色分析都使用 .NET / WPF 内置能力。

## 2. 已完成功能

### 基础图片管理

- 打开本地图片文件夹。
- 扫描当前文件夹内的图片，不递归扫描子文件夹。
- 支持格式：
  - `.jpg`
  - `.jpeg`
  - `.png`
  - `.bmp`
- 中间区域以卡片网格显示图片。
- 每张卡片显示：
  - 缩略图
  - 文件名
  - 主色调
  - 收藏状态
  - 评分
  - 入选 / 淘汰
  - 图片形态
  - 尺寸质量

### 图片预览

- 点击图片卡片后，右侧显示真实大图预览。
- 右侧信息区显示：
  - 文件名
  - 路径
  - 图片尺寸
  - 文件大小
  - 主色调
  - 收藏状态
  - 评分
  - 入选状态
  - 淘汰状态
  - 图片形态
  - 尺寸质量

### 文件安全

- 图片加载使用 `FileStream + BitmapCacheOption.OnLoad`。
- 缩略图生成和大图预览不会长期锁定原图文件。
- 预览后仍可以在 Windows 资源管理器中重命名或删除原图。

### 缩略图缓存

- 缩略图生成后缓存到：

```text
%LocalAppData%\PhotoToneApp\Thumbnails
```

- 缩略图文件名基于原图路径、文件大小、修改时间生成，避免同名冲突。
- 原图变化后会重新生成缩略图。

### JSON 缓存

- 图片信息缓存到：

```text
%LocalAppData%\PhotoToneApp\cache.json
```

- 缓存内容包括：
  - 文件路径
  - 文件名
  - 扩展名
  - 文件大小
  - 修改时间
  - 宽高
  - 缩略图路径
  - 主色调
  - 主色十六进制
  - 收藏状态
  - 评分
  - 入选 / 淘汰
  - 图片形态
  - 尺寸质量
- 缓存不存在、为空或损坏时，程序不会崩溃，会重新生成。

### 主色调分析

- 自动分析图片主色调。
- 支持标签：
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
- 分析使用 WPF 内置 `BitmapImage`、`FormatConvertedBitmap`、`CopyPixels`。
- 分析前缩小图片，不对原图全尺寸逐像素处理。
- 分析失败不会影响其他图片。

### 筛选和搜索

支持组合筛选：

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

左侧筛选按钮有明显选中态，默认各组为“全部”状态。

### 排序

顶部支持排序：

- 文件名
- 修改时间
- 文件大小
- 宽度
- 高度
- 评分

### 收藏、评分、入选、淘汰

- 支持收藏 / 取消收藏。
- 支持 0-5 星评分。
- 支持入选 / 取消入选。
- 支持淘汰 / 取消淘汰。
- 设置入选时会自动取消淘汰。
- 设置淘汰时会自动取消入选。
- 状态会保存到 JSON 缓存。

### 重命名和删除

- 右侧可以重命名当前图片。
- 重命名时用户只输入新文件名，不输入扩展名。
- 程序自动保留原扩展名。
- 重命名后会更新：
  - FilePath
  - FileName
  - Extension
  - FileSize
  - LastModified
  - ThumbnailPath
  - Width / Height
  - 右侧预览
  - JSON 缓存
- 删除图片时会弹出确认框。
- 删除是移动到 Windows 回收站，不是永久删除。
- 删除后会从界面和缓存中移除。

### 单图大图查看窗口

- 双击缩略图或点击“大图查看”打开。
- 支持滚轮缩放。
- 支持鼠标拖拽。
- 支持适应窗口。
- 支持 100% 原始大小。
- 支持全屏 / 退出全屏。
- 支持左右方向键切换图片。
- 显示文件名、序号、尺寸、主色调、收藏状态、评分、入选、淘汰。
- 可在窗口中修改收藏、评分、入选、淘汰。

### 双图对比窗口

- 主窗口可以主动设置左图和右图。
- 点击“双图对比”后打开双图窗口。
- 双图窗口左右并排显示图片。
- 支持左右两图分别切换上一张 / 下一张。
- 支持在对比窗口中通过下拉框替换左图或右图。
- 不允许左右选择同一张图片。
- 支持同步缩放和同步拖拽。
- 支持适应窗口和 100%。
- 支持分别收藏左图 / 右图。
- 支持分别给左图 / 右图评分、入选、淘汰。
- 支持当前操作侧快捷键。

### 快捷键

主窗口：

- `1-5`：设置当前图片评分
- `0`：清除评分
- `P`：入选 / 取消入选
- `X`：淘汰 / 取消淘汰
- `F`：收藏 / 取消收藏
- `Delete`：删除到回收站
- `Enter`：打开大图查看
- `C`：打开双图对比
- `Left / Right`：切换当前可见图片

搜索框输入时不会误触发这些快捷键。

单图窗口：

- `1-5`：评分
- `0`：清除评分
- `P`：入选 / 取消入选
- `X`：淘汰 / 取消淘汰
- `F`：收藏 / 取消收藏
- `Left / Right`：上一张 / 下一张
- `F11`：全屏 / 退出全屏
- `Esc`：退出全屏或关闭窗口

双图对比窗口：

- `1-5`：给当前操作侧评分
- `0`：清除当前操作侧评分
- `P`：当前操作侧入选 / 取消入选
- `X`：当前操作侧淘汰 / 取消淘汰
- `F`：当前操作侧收藏 / 取消收藏
- `A / D`：左图上一张 / 下一张
- `J / L`：右图上一张 / 下一张
- `S`：同步视图开关
- `Esc`：关闭对比窗口

### 导出图片

顶部支持导出：

- 导出当前显示
- 导出收藏
- 导出入选
- 导出3星以上
- 导出5星

导出规则：

- 用户选择目标文件夹。
- 导出是复制图片，不移动图片。
- 原图保留在原文件夹。
- 如果目标文件夹已有同名文件，不覆盖，自动追加 `_1`、`_2`。
- 单张失败不会影响其他图片。
- 完成后显示总数、成功数量、失败数量和目标文件夹。

## 3. 当前代码结构

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

核心文件说明：

- `MainWindow.xaml`：主界面布局。
- `MainWindow.xaml.cs`：当前主要业务逻辑入口，包括扫描、筛选、搜索、排序、导出、收藏、评分、入选、淘汰、打开窗口等。
- `ImageItem.cs`：图片数据模型。
- `ImageScanService.cs`：扫描文件夹图片。
- `ImageLoadService.cs`：加载右侧预览和大图查看图片，避免锁定原图。
- `ThumbnailService.cs`：生成本地缩略图缓存。
- `ColorAnalyzeService.cs`：分析主色调。
- `JsonCacheService.cs`：读写 `cache.json`。
- `FileOperationService.cs`：重命名和删除到回收站。
- `ExportService.cs`：复制导出图片。
- `ZoomableImageViewer`：可复用缩放拖拽图片查看控件。
- `ImageViewerWindow`：单图大图查看窗口。
- `ImageCompareWindow`：双图对比窗口。

## 4. 本机缓存和不应该上传的内容

不要上传这些内容：

- `.vs/`
- `bin/`
- `obj/`
- `*.csproj.user`
- 本地测试图片，例如 `D:\111`
- `%LocalAppData%\PhotoToneApp\cache.json`
- `%LocalAppData%\PhotoToneApp\Thumbnails`

这些都已经通过 `.gitignore` 排除或本来不在项目目录内。

## 5. 今天是否应该先上传 GitHub

建议：今晚先上传 GitHub，再继续开发。

理由：

- 现在已经完成一个比较完整的阶段：从扫描、预览、分析、筛选、评分、对比到导出都有了。
- 明天要换公司电脑，最重要的是保证当前成果不会丢。
- 当前目录还不是 Git 仓库，风险是代码只存在本机。
- 先上传后，明天可以直接 `git clone`，环境问题也更容易定位。
- 后续继续开发前，可以从这个稳定节点新建分支，例如 `feature/ui-polish` 或 `feature/performance`.

建议今晚做：

1. 确认程序能编译运行。
2. 初始化 Git 仓库。
3. 提交当前代码。
4. 上传 GitHub，建议先设为私有仓库。
5. 明天公司电脑 clone 后再继续。

## 6. 上传 GitHub 步骤

### 6.1 安装 Git

如果本机没有 Git，安装：

```text
Git for Windows
```

安装后在 PowerShell 验证：

```powershell
git --version
```

### 6.2 初始化本地仓库

在外层目录执行：

```powershell
cd C:\Users\PassionYoungz\source\repos\PhotoToneApp
git init
git add .
git commit -m "Initial PhotoToneApp milestone"
```

### 6.3 创建 GitHub 仓库

在 GitHub 创建新仓库：

```text
PhotoToneApp
```

建议设置为 Private。

不要在 GitHub 页面勾选自动创建 README、.gitignore 或 license，避免第一次推送冲突。

### 6.4 推送到 GitHub

把下面的地址替换成你自己的 GitHub 仓库地址：

```powershell
git branch -M main
git remote add origin https://github.com/你的用户名/PhotoToneApp.git
git push -u origin main
```

如果你更习惯 GitHub Desktop，也可以用 GitHub Desktop 打开这个目录，然后 Publish repository。

## 7. 公司电脑需要安装什么

### 必装

- Visual Studio 2022 Community 或更高版本
- Visual Studio 工作负载：
  - `.NET desktop development`
- .NET 8 SDK
- Git for Windows

### 建议安装

- GitHub Desktop，适合图形化管理仓库
- Windows Terminal
- Codex / 你当前使用的 AI 开发工具
- 一个固定测试图片目录，例如：

```text
D:\111
```

如果公司电脑没有 `D:\111`，可以自己建一个测试文件夹，放几张 `.jpg/.jpeg/.png/.bmp` 图片。

## 8. 公司电脑恢复项目步骤

### 8.1 克隆仓库

```powershell
cd C:\Users\你的用户名\source\repos
git clone https://github.com/你的用户名/PhotoToneApp.git
cd PhotoToneApp
```

### 8.2 打开项目

优先打开：

```text
PhotoToneApp.slnx
```

如果公司 Visual Studio 版本不识别 `.slnx`，直接打开：

```text
PhotoToneApp/PhotoToneApp.csproj
```

### 8.3 编译

命令行编译：

```powershell
dotnet build .\PhotoToneApp\PhotoToneApp.csproj
```

Visual Studio 中也可以直接点击 Build。

### 8.4 运行

运行后点击“打开文件夹”，选择测试目录，例如：

```text
D:\111
```

第一次打开会生成缩略图、分析颜色、写入本机缓存。

## 9. Codex 上下文如何交接

Codex 的聊天上下文不会自动跟着 GitHub 仓库走。换电脑后，最稳妥的方式是：

1. 把这个 `PROJECT_HANDOFF.md` 上传到仓库。
2. 在公司电脑 clone 项目。
3. 打开 Codex 时，把下面这段作为第一条上下文发给 Codex。

推荐提示词：

```text
我正在继续开发一个 C# WPF .NET 8 项目 PhotoToneApp。

当前仓库已经包含 PROJECT_HANDOFF.md，请先阅读它，理解项目背景、已完成功能、目录结构、缓存路径、现有限制和后续开发注意事项。

真正的 WPF 项目在子目录 PhotoToneApp。

当前项目不使用 SixLabors.ImageSharp，不引入任何第三方图片处理库。

请先不要大幅重构，不要改成完整 MVVM，优先沿用现有代码风格。

后续每次开发都要保证：
1. 不锁定原图文件；
2. 不破坏打开文件夹、缩略图、预览、筛选、评分、对比、导出；
3. 修改后运行 dotnet build .\PhotoToneApp\PhotoToneApp.csproj 验证。
```

如果要让 Codex 快速理解当前代码，可以再补一句：

```text
请重点阅读 MainWindow.xaml、MainWindow.xaml.cs、Models/ImageItem.cs、Services 目录、Controls/ZoomableImageViewer、Windows/ImageViewerWindow、Windows/ImageCompareWindow。
```

## 10. 明天演示建议流程

建议准备一个测试目录，例如：

```text
D:\111
```

里面放 10-30 张不同类型图片，最好包含：

- 横图
- 竖图
- 方图
- 不同颜色照片
- 不同分辨率照片
- 文件名有明显关键词的照片

演示顺序：

1. 打开软件。
2. 点击“打开文件夹”，选择 `D:\111`。
3. 展示缩略图自动生成。
4. 点击图片，展示右侧真实大图预览和信息。
5. 展示主色调分析。
6. 搜索文件名。
7. 使用颜色筛选。
8. 使用收藏、评分、入选、淘汰。
9. 展示左侧筛选按钮选中态。
10. 使用横图 / 竖图 / 方图筛选。
11. 使用尺寸质量筛选。
12. 使用排序。
13. 双击图片打开单图查看，展示缩放、拖拽、全屏。
14. 设置左图 / 右图，打开双图对比，展示同步缩放拖拽。
15. 重命名一张测试图片。
16. 删除一张测试图片到回收站。
17. 导出当前显示或导出入选图片到一个新文件夹。
18. 关闭软件后重新打开同一文件夹，展示缓存和收藏/评分状态仍然存在。

## 11. 当前限制

当前项目还没有做：

- RAW / WebP / TIFF 支持
- AI 识别
- 批量重命名
- 批量删除
- 导出压缩包
- 导出报告
- 图片编辑
- 多图对比
- 数据库
- 完整 MVVM 架构

这是有意保持的，当前阶段更适合作为本地选片工具 MVP。

## 12. 后续开发建议

优先级建议：

1. 先上传 GitHub，保住当前稳定版本。
2. 明天公司电脑 clone 后，先只做环境验证，不急着开发新功能。
3. 验证完成后再开新分支继续。

建议后续分支：

```powershell
git checkout -b feature/ui-polish
```

后续可做：

- UI 进一步压缩和美化。
- 图片卡片性能优化。
- 扫描和分析进度条。
- 取消扫描按钮。
- 导出时显示进度。
- 收藏/评分/入选/淘汰的更明显视觉标识。
- 按日期、镜头参数等扩展筛选。
- 更稳定的异常日志。

## 13. 常用命令

编译：

```powershell
dotnet build .\PhotoToneApp\PhotoToneApp.csproj
```

运行：

```powershell
dotnet run --project .\PhotoToneApp\PhotoToneApp.csproj
```

查看 Git 状态：

```powershell
git status
```

提交：

```powershell
git add .
git commit -m "Describe changes"
```

推送：

```powershell
git push
```

拉取：

```powershell
git pull
```

