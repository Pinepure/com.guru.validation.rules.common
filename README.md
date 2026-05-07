# Guru Validation Common Rules

通用规则包，默认提供：

- Addressables 条目误删检测
- `.meta` 文件缺失检测
- `Resources.Load` 用法检测
- `GameObject.Find` 高风险用法检测

规则默认全部关闭，可在 Validation Center 中按 Profile 启用。

## 通过 Git URL 安装

此包依赖 `com.guru.validation.platform`。如果通过 Git URL 安装，需要在 Unity 项目的 `Packages/manifest.json` 中显式同时声明两个包：

```json
{
  "dependencies": {
    "com.guru.validation.platform": "https://github.com/Pinepure/com.guru.validation.platform.git",
    "com.guru.validation.rules.common": "https://github.com/Pinepure/com.guru.validation.rules.common.git"
  }
}
```

不要只安装当前仓库；否则 `Guru.Validation.Platform.Editor` 程序集不会被解析。
