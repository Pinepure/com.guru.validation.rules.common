using System.Collections.Generic;
using System.IO;
using Guru.Validation.Platform.Editor;

namespace Guru.Validation.Rules.Common.Editor
{
    public sealed class MissingMetaFileRule : ValidationRuleBase
    {
        private static readonly ValidationRuleDescriptor CachedDescriptor = new()
        {
            Id = CommonValidationIds.MissingMetaFile,
            Name = "Meta 文件缺失检测",
            Description = "检测 Assets 下变更资源是否缺少对应 .meta 文件。",
            Source = "com.guru.validation.rules.common",
            Category = ValidationRuleCategory.Assets,
            Performance = ValidationPerformance.Fast,
            SupportsAutoFix = false
        };

        public override ValidationRuleDescriptor Descriptor => CachedDescriptor;

        public override IEnumerable<ValidationIssue> Validate(ValidationRuleContext context)
        {
            var candidatePaths = context.TargetPaths.Count > 0
                ? context.TargetPaths
                : ValidationPathUtility.EnumerateClientFiles(context.ProjectRootPath, "*");

            foreach (var clientRelativePath in candidatePaths)
            {
                if (!ValidationPathUtility.IsUnderAssets(clientRelativePath)
                    || ValidationPathUtility.IsMetaFile(clientRelativePath))
                {
                    continue;
                }

                var absolutePath = ValidationPathUtility.ToAbsolutePath(context.ProjectRootPath, clientRelativePath);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                if (File.Exists(absolutePath + ".meta"))
                {
                    continue;
                }

                yield return context.CreateIssue(
                    Descriptor,
                    context.Localize(
                        "guru.common.assets.missing-meta-file.issue.message",
                        "资源文件缺少对应的 .meta 文件。",
                        "The asset is missing its corresponding .meta file."),
                    filePath: clientRelativePath,
                    assetPath: clientRelativePath,
                    suggestedFix: context.Localize(
                        "guru.common.assets.missing-meta-file.issue.fix",
                        "重新导入资源或恢复丢失的 .meta 文件。",
                        "Reimport the asset or restore the missing .meta file."));
            }
        }
    }
}
