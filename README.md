# Duckov Custom Model Tools

Unity Editor 工具集，用于为 Duckov 游戏构建 Mod DLL 和 AssetBundle。

## 功能

- **Mod DLL 生成工具**: 自动生成 Mod DLL 文件，包含必要的配置和元数据
- **AssetBundle 打包工具**: 将模型预制件打包为 AssetBundle
- **游戏路径设置**: 自动查找或手动设置游戏安装路径

## 安装

### 通过 Git URL 安装

1. 在 Unity 中打开 Package Manager
2. 点击左上角的 `+` 按钮
3. 选择 "Add package from git URL"
4. 输入仓库的 Git URL：`https://github.com/BAKAOLC/duckov-custom-model-tools.git`

### 通过本地路径安装

1. 将整个文件夹复制到项目的 `Packages` 目录下
2. Unity 会自动识别并加载 package

## 使用方法

### 设置游戏路径

1. 在 Unity 菜单栏选择 `Duckov Custom Model > 游戏路径设置`
2. 点击"自动查找"按钮（Windows 平台）或"浏览"按钮手动选择游戏安装目录

### 生成 Mod DLL

1. 在 Unity 菜单栏选择 `Duckov Custom Model > 生成 Mod DLL`
2. 填写 DLL 名称（将用作命名空间）
3. 填写 Mod 显示名称和描述（可选）
4. 选择预览图（可选）
5. 勾选"自动复制到游戏文件夹"（需要先设置游戏路径）
6. 点击"生成 Mod DLL"按钮

### 打包 AssetBundle

1. 在 Unity 菜单栏选择 `Duckov Custom Model > AssetBundle 打包工具`
2. 输入 Bundle 名称
3. 选择目标平台
4. 添加模型预制件
5. 点击"导出模型 Bundle"按钮

## 系统要求

- Unity 2020.3 或更高版本
- Windows 平台（Steam 路径自动查找功能仅限 Windows）

## 许可证

本项目采用 [MIT License](LICENSE) 许可证。

Copyright (c) 2025 OLC

