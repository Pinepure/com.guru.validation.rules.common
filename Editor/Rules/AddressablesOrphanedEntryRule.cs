using System;
using System.Collections.Generic;
using System.IO;
using Guru.Validation.Platform.Editor;
using UnityEditor;

namespace Guru.Validation.Rules.Common.Editor
{
    public sealed class AddressablesOrphanedEntryRule : ValidationRuleBase
    {
        private static readonly ValidationRuleDescriptor CachedDescriptor = new()
        {
            Id = CommonValidationIds.AddressablesOrphanedEntry,
            Name = "Addressables 条目误删检测",
            Description = "检测 Addressable Group 中的 GUID 是否被移除，但资源文件本体仍然存在且未迁移到其他 Group。",
            Source = "com.guru.validation.rules.common",
            Category = ValidationRuleCategory.Addressables,
            Performance = ValidationPerformance.Fast,
            SupportsAutoFix = false
        };

        public override ValidationRuleDescriptor Descriptor => CachedDescriptor;

        public override IEnumerable<ValidationIssue> Validate(ValidationRuleContext context)
        {
            if (context.Mode != ValidationExecutionMode.StagedChanges
                && context.Mode != ValidationExecutionMode.ChangedFiles)
            {
                yield break;
            }

            var groupPaths = context.TargetPaths;
            foreach (var clientRelativePath in groupPaths)
            {
                if (!clientRelativePath.StartsWith("Assets/AddressableAssetsData/AssetGroups/", StringComparison.Ordinal)
                    || !clientRelativePath.EndsWith(".asset", StringComparison.Ordinal))
                {
                    continue;
                }

                var oldContent = GitUtility.GetHeadFileText(context.ProjectRootPath, context.RepoRootPath, clientRelativePath);
                if (string.IsNullOrEmpty(oldContent))
                {
                    continue;
                }

                var newContent = GetCurrentContent(context, clientRelativePath);
                if (string.IsNullOrEmpty(newContent))
                {
                    continue;
                }

                var oldEntries = AddressableGroupEntryParser.ParseEntries(oldContent);
                var newEntries = AddressableGroupEntryParser.ParseEntries(newContent);
                foreach (var pair in oldEntries)
                {
                    if (newEntries.ContainsKey(pair.Key))
                    {
                        continue;
                    }

                    var assetPath = AssetDatabase.GUIDToAssetPath(pair.Key);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        continue;
                    }

                    var assetAbsolutePath = ValidationPathUtility.ToAbsolutePath(context.ProjectRootPath, assetPath);
                    if (!File.Exists(assetAbsolutePath))
                    {
                        continue;
                    }

                    if (IsStillRegisteredInCurrentGroups(context.ProjectRootPath, pair.Key))
                    {
                        continue;
                    }

                    yield return context.CreateIssue(
                        Descriptor,
                        context.Localize(
                            "guru.common.addressables.orphaned-entry.issue.message",
                            "Addressables 条目已从 Group 移除，但资源仍存在：{0}",
                            "An Addressables entry was removed from the group while the asset still exists: {0}",
                            pair.Value),
                        filePath: clientRelativePath,
                        assetPath: assetPath,
                        details: context.Localize(
                            "guru.common.addressables.orphaned-entry.issue.details",
                            "GUID={0}",
                            "GUID={0}",
                            pair.Key),
                        suggestedFix: context.Localize(
                            "guru.common.addressables.orphaned-entry.issue.fix",
                            "确认这是有意删除；如果不是，请在 Addressables Groups 中恢复或迁移该条目。",
                            "Confirm this removal is intentional. Otherwise restore or move the entry in Addressables Groups."));
                }
            }
        }

        private static string GetCurrentContent(ValidationRuleContext context, string clientRelativePath)
        {
            if (context.Mode == ValidationExecutionMode.StagedChanges)
            {
                var stagedContent = GitUtility.GetStagedFileText(context.ProjectRootPath, context.RepoRootPath, clientRelativePath);
                if (!string.IsNullOrEmpty(stagedContent))
                {
                    return stagedContent;
                }
            }

            var absolutePath = ValidationPathUtility.ToAbsolutePath(context.ProjectRootPath, clientRelativePath);
            return File.Exists(absolutePath) ? File.ReadAllText(absolutePath) : string.Empty;
        }

        private static bool IsStillRegisteredInCurrentGroups(string projectRootPath, string guid)
        {
            var groupsDirectory = Path.Combine(projectRootPath, "Assets", "AddressableAssetsData", "AssetGroups");
            if (!Directory.Exists(groupsDirectory))
            {
                return false;
            }

            foreach (var path in Directory.GetFiles(groupsDirectory, "*.asset", SearchOption.TopDirectoryOnly))
            {
                var content = File.ReadAllText(path);
                if (content.IndexOf(guid, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
