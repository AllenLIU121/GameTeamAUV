1.安装Git LFS: https://git-lfs.github.com/  版本号3.2.0
LFS用来自动管理游戏资源等大文件,免费存储上线为2GB

git lfs --version 检查版本号
git lfs install 为Git全局启用LFS

2. Git和Github工作流
  2.1 在每天开始写任何代码前, 先确保你本地的main分支和远程Github上main分支是完全同步的
      git checkout main
      git pull origin main
  2.2 从最新的main分支上创建属于自己的分支, 不要直接使用别人的.
      git checkout -b your-branch-name  # 从main分支创建并切换到新创建的分支
  2.3 保存修改到本地
      git add .
      git commit -m "commit description"
  2.4 提交修改到远程
      git push -u origin "your-branch-name"  # -u参数会在本地分支和远程分支建立链接,第一次push时使用
  2.5 创建Pull Request

3. Unity项目大致结构和使用
   3.1 项目结构
      Assets/
        Art/    <-- 所有美术资源
        Audio/     <-- 所有音乐音效
        Scripts/    <-- 所有代码
        Scenes/    <-- 所有场景
        Materials/     <-- 所有材质
        Prefabs/     <-- 所有预制体
   
  3.2 使用规则
    *不要直接使用别人创建好的内容, 比如Prefab, Scene等, 需要使用时请复制后修改 并命名清楚.
    *自己根据所需, 在结构内定义子文件夹, 以保存自己的进度和内容, 确保文件名称易于理解.
