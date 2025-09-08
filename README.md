## 1. 环境配置：Git LFS

本项目使用 Git LFS (Large File Storage) 管理美术、音频等大文件。**此为必须步骤**。

* **第一步：安装 Git LFS**
    * 前往 [Git LFS 官网](https://git-lfs.github.com/) 下载并安装最新版本。
    * GitHub 每月为每个仓库提供 **1GB 的免费存储空间** 和 **1GB 的免费月度流量**。

* **第二步：为 Git 启用 LFS**
    * 打开命令行工具 (Git Bash, PowerShell, Terminal)，只需运行一次以下命令：
        ```bash
        # 检查 LFS 版本以确认安装成功
        git lfs --version

        # 为你的 Git 账户全局启用 LFS 功能
        git lfs install
        ```

## 2. Git 与 GitHub 工作流
* **不要使用别人的分支**
* **不要在main分支上修改**
#### 2.1 开始新任务：同步并创建分支
在开始编码前，确保你的工作是基于最新版本。

```bash
# 1. 切换到主分支并拉取最新代码
git checkout main
git pull origin main

# 2. 根据你的任务创建一个新分支
git checkout -b your-branch-name
```

#### 2.2 保存进度：本地提交
完成一个小功能点后，就进行一次本地提交。

```bash
# 1. 添加更改到暂存区
git add .

# 2. 提交并撰写说明
git commit -m "your commit description"
```

#### 2.3 分享进度：推送到远程
将你的本地分支推送到 GitHub 以备份和分享。

```bash
# -u 参数会在首次推送时建立链接
git push -u origin feature/your-task-name
```

#### 2.4 完成任务后：创建 Pull Request
当分支上的功能开发完毕，前往 GitHub 仓库页面，为你的分支创建一个 Pull Request，请求合并到 `main` 分支.

## 3. Unity 项目结构与使用规范

#### 3.1 项目结构

```
Assets/
   ├── Art/             <-- 美术资源 (模型, 贴图, 精灵)
   ├── Audio/           <-- 音频资源 (音乐, 音效)
   ├── Prefabs/         <-- 预制体
   ├── Scenes/          <-- 场景
   |     |一一 TestScenes/       <-- 个人测试场景
   ├── Scripts/         <-- C# 脚本
   └── Materials/        <-- 材质相关
```

#### 3.2 使用规则 (非常重要！)

* **预制体**
    * 游戏中的所有可复用对象（角色、道具、UI窗口等）**需要**做成 Prefab。
    * 当你需要一个“特殊版本”的 Prefab 时，请在原始 Prefab 上右键 -> `Create` -> `Prefab Variant`。
    * 在 Variant 上进行修改，当原始 Prefab 更新时（例如增加新动画），所有 Variant 都会自动继承这些更新。

* **在个人测试场景中开发**
    * 在Scenes/TestScenes/下创建你自己的测试场景, 它已经被gitignore掉 不会被上传到github上。
    * 在测试场景中完成开发和调试，将最终成果应用到相应的 Prefab 或主场景中。
