using Guru.Validation.Platform.Editor;

namespace Guru.Validation.Rules.Common.Editor
{
    public sealed class CommonRecommendedPreset : ValidationPresetBase
    {
        private static readonly ValidationPresetDescriptor CachedDescriptor = new()
        {
            Id = "guru.common.preset.recommended",
            Name = "Common Recommended",
            Description = "启用通用规则的推荐配置，适合作为项目初始预设。",
            Source = "com.guru.validation.rules.common"
        };

        public override ValidationPresetDescriptor Descriptor => CachedDescriptor;

        public override void Apply(ValidationSettings settings)
        {
            settings.SetRuleSeverity(CommonValidationIds.AddressablesOrphanedEntry, ValidationProfile.Manual, ValidationSeverity.Warning);
            settings.SetRuleSeverity(CommonValidationIds.AddressablesOrphanedEntry, ValidationProfile.PreCommit, ValidationSeverity.Error);
            settings.SetRuleSeverity(CommonValidationIds.AddressablesOrphanedEntry, ValidationProfile.Ci, ValidationSeverity.Error);

            settings.SetRuleSeverity(CommonValidationIds.MissingMetaFile, ValidationProfile.Manual, ValidationSeverity.Warning);
            settings.SetRuleSeverity(CommonValidationIds.MissingMetaFile, ValidationProfile.PreCommit, ValidationSeverity.Error);
            settings.SetRuleSeverity(CommonValidationIds.MissingMetaFile, ValidationProfile.Ci, ValidationSeverity.Error);

            settings.SetRuleSeverity(CommonValidationIds.ResourcesLoadUsage, ValidationProfile.Manual, ValidationSeverity.Warning);
            settings.SetRuleSeverity(CommonValidationIds.ResourcesLoadUsage, ValidationProfile.PreCommit, ValidationSeverity.Warning);
            settings.SetRuleSeverity(CommonValidationIds.ResourcesLoadUsage, ValidationProfile.Ci, ValidationSeverity.Warning);

            settings.SetRuleSeverity(CommonValidationIds.GameObjectFindUsage, ValidationProfile.Manual, ValidationSeverity.Warning);
            settings.SetRuleSeverity(CommonValidationIds.GameObjectFindUsage, ValidationProfile.PreCommit, ValidationSeverity.Warning);
            settings.SetRuleSeverity(CommonValidationIds.GameObjectFindUsage, ValidationProfile.Ci, ValidationSeverity.Warning);
        }
    }
}
