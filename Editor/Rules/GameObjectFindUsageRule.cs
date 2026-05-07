using System.Collections.Generic;
using System.IO;
using Guru.Validation.Platform.Editor;

namespace Guru.Validation.Rules.Common.Editor
{
    public sealed class GameObjectFindUsageRule : ValidationRuleBase
    {
        private static readonly ValidationRuleDescriptor CachedDescriptor = new()
        {
            Id = CommonValidationIds.GameObjectFindUsage,
            Name = "GameObject.Find 高风险用法检测",
            Description = "检测 GameObject.Find / FindWithTag / FindObjectOfType 等高风险查找模式。",
            Source = "com.guru.validation.rules.common",
            Category = ValidationRuleCategory.Code,
            Performance = ValidationPerformance.Fast,
            SupportsAutoFix = false
        };

        private static readonly string[] Patterns =
        {
            "GameObject.Find(",
            "GameObject.FindWithTag(",
            "FindObjectOfType<",
            "FindFirstObjectByType<"
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
                    if (line.StartsWith("//"))
                    {
                        continue;
                    }

                    foreach (var pattern in Patterns)
                    {
                        if (line.IndexOf(pattern, System.StringComparison.Ordinal) < 0)
                        {
                            continue;
                        }

                        yield return context.CreateIssue(
                            Descriptor,
                            context.Localize(
                                "guru.common.code.gameobject-find-usage.issue.message",
                                "检测到高风险对象查找调用：{0}",
                                "Detected a high-risk object lookup call: {0}",
                                pattern),
                            filePath: clientRelativePath,
                            line: index + 1,
                            suggestedFix: context.Localize(
                                "guru.common.code.gameobject-find-usage.issue.fix",
                                "优先使用缓存、序列化引用或依赖注入，避免运行时全局查找。",
                                "Prefer cached references, serialized references, or dependency injection instead of runtime global lookups."));
                        break;
                    }
                }
            }
        }
    }
}
