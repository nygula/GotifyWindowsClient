# GotifyWindowsClient - Windows 平台消息推送客户端

![.NET Version](https://img.shields.io/badge/.NET-8.0-%23512bd4) ![Platform](https://img.shields.io/badge/Platform-Windows-%230078D4)

## 项目概述

GotifyWindowsClient 是基于 .NET 8 开发的轻量级 Windows 系统托盘客户端，专为 Gotify 消息推送服务打造。本客户端采用无界面设计，通过 Windows 原生通知系统实现实时消息推送，是服务器监控、自动化脚本通知等场景的理想选择。

---

## 环境要求

✅ **必备组件**: 
- [.NET 8 Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) (x64)

> 📢 注意：程序需在安装 .NET 8 运行时的 Windows 10/11 系统上运行

---

## 核心特性

### 📌 极简部署
- **零界面操作** - 启动后自动最小化至系统托盘
- **原生通知集成** - 调用 Windows 通知中心实现消息展示
- **静默运行** - 无窗口干扰，后台持续监听消息
- **开机运行** - 托盘鼠标右键选择开机自启，防止因为重启导致的程序关闭
- **长连接保持** - WebSocket 长连接保持，断线自动重连（5 秒重试间隔）

### ⚙️ 智能配置
```xml
<!-- GotifyWindowsClient.dll.config -->
<configuration>
  <appSettings>
    <add key="ServerUrl" value="http://your.gotify.server:port" />
    <add key="ClientToken" value="your_client_token_here" />
  </appSettings>
</configuration>
```

- ServerUrl: Gotify 服务端地址（支持 HTTP/HTTPS）
- ClientToken: 从 Gotify 服务端网页新建客户端令牌