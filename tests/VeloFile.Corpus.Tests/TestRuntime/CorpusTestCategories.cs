using System.Reflection;

namespace VeloFile.Corpus.Tests;

internal static class CorpusTestCategories
{
    public const string Fast = "Fast";
    public const string Contract = "Contract";
    public const string Smoke = "Smoke";
    public const string CorpusScript = "CorpusScript";
    public const string ReleaseEvidence = "ReleaseEvidence";
    public const string Benchmark = "Benchmark";
    public const string Visual = "Visual";
    public const string ManualEvidence = "ManualEvidence";

    public static readonly IReadOnlySet<string> Accepted = new HashSet<string>(StringComparer.Ordinal)
    {
        Fast,
        Contract,
        Smoke,
        CorpusScript,
        ReleaseEvidence,
        Benchmark,
        Visual,
        ManualEvidence
    };

    public static readonly IReadOnlySet<string> FastExcludedWhenOnlyCategoryPurpose = new HashSet<string>(StringComparer.Ordinal)
    {
        CorpusScript,
        ReleaseEvidence,
        Benchmark,
        Visual,
        ManualEvidence
    };
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class ReleaseEvidenceFastRationaleAttribute(string rationale) : Attribute
{
    public string Rationale { get; } = rationale;
}

internal sealed record CorpusTestCategoryDescriptor(
    string Name,
    IReadOnlyCollection<string> Categories,
    bool HasReleaseEvidenceFastRationale);

internal static class CorpusCategoryInventory
{
    public static IReadOnlyList<string> Validate(IEnumerable<CorpusTestCategoryDescriptor> tests)
    {
        var errors = new List<string>();

        foreach (var test in tests)
        {
            var categories = test.Categories
                .Where(category => !string.IsNullOrWhiteSpace(category))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (categories.Length == 0)
            {
                errors.Add($"missing-category: {test.Name} has no accepted test category metadata.");
                continue;
            }

            foreach (var category in categories.Where(category => !CorpusTestCategories.Accepted.Contains(category)))
            {
                errors.Add($"unknown-category: {test.Name} uses '{category}'.");
            }

            if (categories.Contains(CorpusTestCategories.ReleaseEvidence, StringComparer.Ordinal)
                && categories.Contains(CorpusTestCategories.Fast, StringComparer.Ordinal)
                && !test.HasReleaseEvidenceFastRationale)
            {
                errors.Add($"invalid-category-combination: {test.Name} combines ReleaseEvidence and Fast without rationale.");
            }

            if (categories.Contains(CorpusTestCategories.CorpusScript, StringComparer.Ordinal)
                && !categories.Contains(CorpusTestCategories.Smoke, StringComparer.Ordinal)
                && !categories.Contains(CorpusTestCategories.ReleaseEvidence, StringComparer.Ordinal))
            {
                errors.Add($"invalid-category-combination: {test.Name} marks CorpusScript without Smoke or ReleaseEvidence.");
            }
        }

        return errors;
    }

    public static IReadOnlyList<CorpusTestCategoryDescriptor> FromAssembly(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<TestClassAttribute>() is not null)
            .SelectMany(TestDescriptorsFromType)
            .OrderBy(test => test.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<CorpusTestCategoryDescriptor> TestDescriptorsFromType(Type type)
    {
        var typeCategories = CategoriesFrom(type).ToArray();
        var typeHasRationale = type.GetCustomAttribute<ReleaseEvidenceFastRationaleAttribute>() is not null;

        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.GetCustomAttribute<TestMethodAttribute>() is not null))
        {
            var categories = typeCategories
                .Concat(CategoriesFrom(method))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var hasRationale = typeHasRationale
                || method.GetCustomAttribute<ReleaseEvidenceFastRationaleAttribute>() is not null;

            yield return new CorpusTestCategoryDescriptor(
                $"{type.FullName}.{method.Name}",
                categories,
                hasRationale);
        }
    }

    private static IEnumerable<string> CategoriesFrom(MemberInfo member)
    {
        return member.GetCustomAttributes<TestCategoryAttribute>()
            .SelectMany(attribute => attribute.TestCategories);
    }
}
