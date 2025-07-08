namespace HouseRules.Configuration
{
    using System.IO;
    using HouseRules.Core;
    using HouseRules.Core.Types;

    internal static class ExampleRulesetExporter
    {
        internal static void ExportRegisteredRulesets(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (var ruleset in HR.Rulebook.Rulesets)
            {
                var newName = $"(Custom) {ruleset.Name}";
                var newDisplayName = $"(Custom) {ruleset.Displayname}";
                var rulesetCopy = Ruleset.NewInstance(newName, newDisplayName, ruleset.Description, ruleset.Longdesc, ruleset.Rules);
                RulesetImporter.WriteToDirectory(rulesetCopy, directory);
            }
        }
    }
}
