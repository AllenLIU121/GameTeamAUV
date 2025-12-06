{ pkgs, ... }: {
  
  # 使用 unstable 频道通常能获得更新的 .NET SDK
  channel = "unstable"; 

  packages = [
    # 1. 核心运行时
    pkgs.dotnet-sdk_8     # 最新的 .NET SDK，用于驱动现代 C# 插件功能
    pkgs.mono             # 必须。Unity 依赖 Mono 运行时来处理某些构建和各类库
    pkgs.msbuild          # 构建工具，辅助 OmniSharp 解析项目结构

    # 2. 辅助工具
    pkgs.git              # 版本控制
    pkgs.git-lfs          # 大文件存储 (Unity 必备)
  ];

  env = {
    # 禁用 .NET 遥测，提升启动速度，保持纯净
    DOTNET_CLI_TELEMETRY_OPTOUT = "1";
    # 指定 OmniSharp 使用全局 Mono (解决找不到 MSBuild 的常见报错)
    OMNISHARP_USE_GLOBAL_MONO = "1";
  };

  idx = {
    # Unity 专属插件清单 (从 Open VSX 获取)
    extensions = [
      # --- 核心 C# 支持 ---
      "ms-dotnettools.csharp"          # 基础 C# 支持 (OmniSharp)
      
      # --- Unity 增强 ---
      "viz.vscode-unity-debug"         # Unity 调试器 (虽然无法本地Attach，但保留配置方便查看)
      "Kleber-WF.unity-code-snippets"  # 高效编码：输入 "mono" 自动生成 MonoBehaviour 模版
      "jmbbe.unity-tools"              # 快速浏览文档等小工具
      
      # --- 代码质量与协作 ---
      "eamodio.gitlens"                # 极其强大的 Git 历史查看工具
      "usernamehw.errorlens"           # 将报错直接显示在代码行尾，不用去翻控制台
    ];

    workspace = {
      onCreate = {
        # 初始化 Git LFS (防止拉取项目时大资源损坏)
        setup-lfs = "git lfs install";
      };
      onStart = {
        # 每次启动时检查并还原 .NET 依赖
        dotnet-restore = "dotnet restore"; 
      };
    };
  };
}