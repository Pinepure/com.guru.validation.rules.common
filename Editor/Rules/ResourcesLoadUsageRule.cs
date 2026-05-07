using System.Collections.Generic;
using System.IO;
using Guru.Validation.Platform.Editor;

namespace Guru.Validation.Rules.Common.Editor
{
    public sealed class ResourcesLoadUsageRule : ValidationRuleBase
    {
        private static readonly ValidationRuleDescriptor CachedDescriptor = new()
        {
            Id = CommonValidationIds.ResourcesLoadUsage,
            Name = "Resources.Load 用法检测",
            Description = "检测 C# 文件中对 Resources.Load 的直接调用，推荐改用 Addressables。",
            Source = "com.guru.validation.rules.common",
            Category = ValidationRuleCategory.Code,
            Performance = ValidationPerformance.Fast,
            SupportsAutoFix = false
        };

        public override ValidationRuleDescriptor Descriptor => CachedDescriptor;

        public override IEnumerable<ValidationIssue> Validate(ValidationRuleContext context)
        {
            var candidatePaths = context.TargetPaths.Count > 0
                ? context.TargetPaths
                : ValidationPathUtility.EnumerateClientFiles(context.ProjectRootPath, "*.cs");

            foreach (var clientRelativePath in candidatePaths)
            {
                if (!ValidationPathUtility.IsCodeFile(clientRelativePath))
                {
                    continue;
                }

                var absolutePath = ValidationPathUtility.ToAbsolutePath(context.ProjectRootPath, clientRelativePath);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                var lines = File.ReadAllLines(absolutePath);
                for (var index = 0; index < lines.Length; index++)
                {
                    var line = lines[index].Trim();
                    if (line.StartsWith("//") || line.IndexOf("Resources.Load", System.StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    yield return context.CreateIssue(
                        Descriptor,
                        context.Localize(
                            "guru.common.code.resources-load-usage.issue.message",
                            "检测到 Resources.Load 直接调用。",
                            "Detected direct Resources.Load usage."),
                        filePath: clientRelativePath,
                        line: index + 1,
                        suggestedFix: context.Localize(
                            "guru.common.code.resources-load-usage.issue.fix",
                            "优先改用 Addressables 或项目内统一资源加载层。",
                            "Prefer Addressables or the project's unified asset loading layer."));
                }
            }
        }
    }
}
