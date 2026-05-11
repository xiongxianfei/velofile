using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

return UiContractsCli.Run(args);

internal static partial class UiContractsCli
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    public static int Run(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            WriteUsage();
            return args.Length == 0 ? 1 : 0;
        }

        if (!string.Equals(args[0], "validate-tokens", StringComparison.Ordinal))
        {
            Console.Error.WriteLine($"Unknown command '{args[0]}'.");
            WriteUsage();
            return 1;
        }

        try
        {
            var options = ParseOptions(args.Skip(1).ToArray());
            var failures = new List<string>();
            var tokenContract = TokenContract.Load(options.ContractPath, failures);

            if (tokenContract is not null)
            {
                var resources = XamlResourceSet.Load(options.XamlRoot, failures);
                ValidateTokens(tokenContract, resources, failures);

                if (options.ScopesPath is not null)
                {
                    ValidateScopes(options.ScopesPath, options.ScopeRoot ?? options.XamlRoot, failures);
                }
            }

            if (failures.Count > 0)
            {
                foreach (var failure in failures)
                {
                    Console.Error.WriteLine(failure);
                }

                return 1;
            }

            Console.WriteLine("UI contract validation passed.");
            return 0;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static Options ParseOptions(string[] args)
    {
        string? contract = null;
        string? xamlRoot = null;
        string? scopes = null;
        string? scopeRoot = null;

        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];
            string ReadValue()
            {
                if (index + 1 >= args.Length)
                {
                    throw new InvalidOperationException($"Missing value for {option}.");
                }

                index++;
                return args[index];
            }

            switch (option)
            {
                case "--contract":
                    contract = ReadValue();
                    break;
                case "--xaml-root":
                    xamlRoot = ReadValue();
                    break;
                case "--scopes":
                    scopes = ReadValue();
                    break;
                case "--scope-root":
                    scopeRoot = ReadValue();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown option '{option}'.");
            }
        }

        if (string.IsNullOrWhiteSpace(contract))
        {
            throw new InvalidOperationException("--contract is required.");
        }

        if (string.IsNullOrWhiteSpace(xamlRoot))
        {
            throw new InvalidOperationException("--xaml-root is required.");
        }

        return new Options(
            Path.GetFullPath(contract),
            Path.GetFullPath(xamlRoot),
            string.IsNullOrWhiteSpace(scopes) ? null : Path.GetFullPath(scopes),
            string.IsNullOrWhiteSpace(scopeRoot) ? null : Path.GetFullPath(scopeRoot));
    }

    private static void WriteUsage()
    {
        Console.Error.WriteLine("Usage: VeloFile.UiContracts validate-tokens --contract <tokens.v1.json> --xaml-root <resource-root> [--scopes <ui-contract-scopes.json> --scope-root <root>]");
    }

    private static void ValidateTokens(TokenContract contract, XamlResourceSet resources, List<string> failures)
    {
        foreach (var token in contract.Tokens.Where(token => token.RequiredInFirstSlice))
        {
            foreach (var key in token.XamlKeys)
            {
                if (!resources.TryGet(key, out _))
                {
                    failures.Add($"{token.Id}: missing XAML key '{key}'.");
                }
            }
        }

        foreach (var token in contract.Tokens.Where(token => token.RequiredInFirstSlice))
        {
            if (token.XamlKeys.Any(key => !resources.TryGet(key, out _)))
            {
                continue;
            }

            switch (token.Type)
            {
                case "ColorAndBrush":
                    ValidateColorAndBrush(token, resources, failures);
                    break;
                case "BrushReference":
                    ValidateBrushReference(token, contract, resources, failures);
                    break;
                case "Double":
                    ValidateTypedValues(token, resources, "Double", ValidateDoubleValue, failures);
                    break;
                case "FontFamily":
                    ValidateTypedValues(token, resources, "FontFamily", ValidateStringValue, failures);
                    break;
                case "CornerRadius":
                    ValidateTypedValues(token, resources, "CornerRadius", ValidateStringValue, failures);
                    break;
                case "String":
                    ValidateTypedValues(token, resources, "String", ValidateStringValue, failures);
                    break;
                default:
                    failures.Add($"{token.Id}: unsupported token type '{token.Type}'.");
                    break;
            }
        }
    }

    private static void ValidateColorAndBrush(Token token, XamlResourceSet resources, List<string> failures)
    {
        if (token.XamlKeys.Count < 2)
        {
            failures.Add($"{token.Id}: ColorAndBrush tokens require color and brush XAML keys.");
            return;
        }

        var color = resources.Get(token.XamlKeys[0]);
        if (!string.Equals(color.TypeName, "Color", StringComparison.Ordinal))
        {
            failures.Add($"{token.Id}: key '{color.Key}' expected type Color, observed {color.TypeName} at {color.Path}.");
            return;
        }

        if (!string.Equals(color.Value, token.StringValue, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"{token.Id}: key '{color.Key}' expected value {token.StringValue}, observed {color.Value} at {color.Path}.");
        }

        var brush = resources.Get(token.XamlKeys[1]);
        if (!string.Equals(brush.TypeName, "SolidColorBrush", StringComparison.Ordinal))
        {
            failures.Add($"{token.Id}: key '{brush.Key}' expected type SolidColorBrush, observed {brush.TypeName} at {brush.Path}.");
            return;
        }

        if (!IsStaticResourceReference(brush.ColorAttribute, color.Key)
            && !string.Equals(brush.ColorAttribute, token.StringValue, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"{token.Id}: brush '{brush.Key}' expected Color '{{StaticResource {color.Key}}}' or {token.StringValue}, observed {brush.ColorAttribute ?? "<missing>"} at {brush.Path}.");
        }
    }

    private static void ValidateBrushReference(Token token, TokenContract contract, XamlResourceSet resources, List<string> failures)
    {
        var reference = contract.Tokens.SingleOrDefault(candidate => string.Equals(candidate.Id, token.StringValue, StringComparison.Ordinal));
        if (reference is null)
        {
            failures.Add($"{token.Id}: referenced token '{token.StringValue}' does not exist.");
            return;
        }

        var expectedColorKey = reference.XamlKeys.FirstOrDefault();
        if (expectedColorKey is null)
        {
            failures.Add($"{token.Id}: referenced token '{reference.Id}' has no XAML color key.");
            return;
        }

        var brush = resources.Get(token.XamlKeys[0]);
        if (!string.Equals(brush.TypeName, "SolidColorBrush", StringComparison.Ordinal))
        {
            failures.Add($"{token.Id}: key '{brush.Key}' expected type SolidColorBrush, observed {brush.TypeName} at {brush.Path}.");
            return;
        }

        if (!IsStaticResourceReference(brush.ColorAttribute, expectedColorKey))
        {
            failures.Add($"{token.Id}: brush '{brush.Key}' expected Color '{{StaticResource {expectedColorKey}}}', observed {brush.ColorAttribute ?? "<missing>"} at {brush.Path}.");
        }
    }

    private static void ValidateTypedValues(Token token, XamlResourceSet resources, string expectedType, Func<Token, XamlResource, string?> validator, List<string> failures)
    {
        foreach (var key in token.XamlKeys)
        {
            var resource = resources.Get(key);
            if (!string.Equals(resource.TypeName, expectedType, StringComparison.Ordinal)
                && !(expectedType == "Double" && resource.TypeName is "Int32" or "Single"))
            {
                failures.Add($"{token.Id}: key '{key}' expected type {expectedType}, observed {resource.TypeName} at {resource.Path}.");
                continue;
            }

            var valueFailure = validator(token, resource);
            if (valueFailure is not null)
            {
                failures.Add(valueFailure);
            }
        }
    }

    private static string? ValidateDoubleValue(Token token, XamlResource resource)
    {
        if (!double.TryParse(resource.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var observed)
            || !double.TryParse(token.StringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var expected)
            || Math.Abs(observed - expected) > 0.0001)
        {
            return $"{token.Id}: key '{resource.Key}' expected value {token.StringValue}, observed {resource.Value} at {resource.Path}.";
        }

        return null;
    }

    private static string? ValidateStringValue(Token token, XamlResource resource)
    {
        return string.Equals(resource.Value, token.StringValue, StringComparison.Ordinal)
            ? null
            : $"{token.Id}: key '{resource.Key}' expected value {token.StringValue}, observed {resource.Value} at {resource.Path}.";
    }

    private static void ValidateScopes(string scopesPath, string scopeRoot, List<string> failures)
    {
        if (!File.Exists(scopesPath))
        {
            failures.Add($"Scope file not found: {scopesPath}");
            return;
        }

        var scopesJson = JsonNode.Parse(File.ReadAllText(scopesPath))!.AsObject();
        foreach (var scopeNode in scopesJson["scopes"]!.AsArray().Select(scope => scope!.AsObject()))
        {
            var id = RequireString(scopeNode, "id", scopesPath, failures);
            var files = ReadStringArray(scopeNode, "files", scopesPath, failures);
            var requiredReferences = ReadStringArray(scopeNode, "requiredResourceReferences", scopesPath, failures);
            var forbiddenRules = ReadStringArray(scopeNode, "forbiddenLiteralRules", scopesPath, failures);
            var scopedTexts = new List<(string Path, string Text)>();

            foreach (var relativePath in files)
            {
                var path = Path.GetFullPath(Path.Combine(scopeRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
                if (!File.Exists(path))
                {
                    failures.Add($"{id}: scope file not found: {path}");
                    continue;
                }

                var text = File.ReadAllText(path);
                scopedTexts.Add((path, ExtractScopeText(text, id)));
            }

            var combinedScopeText = string.Join(Environment.NewLine, scopedTexts.Select(item => item.Text));
            foreach (var requiredReference in requiredReferences)
            {
                if (!combinedScopeText.Contains(requiredReference, StringComparison.Ordinal))
                {
                    failures.Add($"{id}: required resource reference '{requiredReference}' missing from governed scope files.");
                }
            }

            foreach (var scopedText in scopedTexts)
            {
                foreach (var rule in forbiddenRules)
                {
                    foreach (var failure in FindForbiddenLiterals(rule, scopedText.Text, scopedText.Path, id))
                    {
                        failures.Add(failure);
                    }
                }
            }
        }
    }

    private static string ExtractScopeText(string text, string scopeId)
    {
        var start = $"<!-- ui-contract-scope:{scopeId}:start -->";
        var end = $"<!-- ui-contract-scope:{scopeId}:end -->";
        var segments = new List<string>();
        var searchIndex = 0;

        while (searchIndex < text.Length)
        {
            var startIndex = text.IndexOf(start, searchIndex, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                break;
            }

            startIndex += start.Length;
            var endIndex = text.IndexOf(end, startIndex, StringComparison.Ordinal);
            if (endIndex < 0)
            {
                break;
            }

            segments.Add(text[startIndex..endIndex]);
            searchIndex = endIndex + end.Length;
        }

        return segments.Count == 0 ? text : string.Join(Environment.NewLine, segments);
    }

    private static IEnumerable<string> FindForbiddenLiterals(string rule, string text, string path, string scopeId)
    {
        return rule switch
        {
            "inline-color" => HexColorRegex().Matches(text).Select(match => $"{scopeId}: {rule} literal '{match.Value}' in {path}."),
            "inline-row-height" => RowHeightRegex().Matches(text).Select(match => $"{scopeId}: {rule} literal '{match.Value}' in {path}."),
            "inline-row-padding" => PaddingRegex().Matches(text).Select(match => $"{scopeId}: {rule} literal '{match.Value}' in {path}."),
            "inline-selection-brush" => SelectionBrushRegex().Matches(text).Select(match => $"{scopeId}: {rule} literal '{match.Value}' in {path}."),
            "inline-focus-thickness" => FocusThicknessRegex().Matches(text).Select(match => $"{scopeId}: {rule} literal '{match.Value}' in {path}."),
            _ => []
        };
    }

    private static string RequireString(JsonObject node, string propertyName, string path, List<string> failures)
    {
        var value = (string?)node[propertyName];
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add($"{path}: missing required string '{propertyName}'.");
            return string.Empty;
        }

        return value;
    }

    private static string[] ReadStringArray(JsonObject node, string propertyName, string path, List<string> failures)
    {
        var array = node[propertyName]?.AsArray();
        if (array is null)
        {
            failures.Add($"{path}: missing required array '{propertyName}'.");
            return [];
        }

        return array.Select(value => (string?)value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();
    }

    private static bool IsStaticResourceReference(string? value, string expectedKey)
    {
        return string.Equals(value?.Trim(), "{StaticResource " + expectedKey + "}", StringComparison.Ordinal);
    }

    [GeneratedRegex("#[0-9A-Fa-f]{6,8}", RegexOptions.CultureInvariant)]
    private static partial Regex HexColorRegex();

    [GeneratedRegex("\\b(?:Height|MinHeight)=\"\\d+(?:\\.\\d+)?\"", RegexOptions.CultureInvariant)]
    private static partial Regex RowHeightRegex();

    [GeneratedRegex("\\bPadding=\"\\d+(?:\\.\\d+)?(?:,\\d+(?:\\.\\d+)?){0,3}\"", RegexOptions.CultureInvariant)]
    private static partial Regex PaddingRegex();

    [GeneratedRegex("\\b(?:Background|SelectionHighlightColor)=\"#[0-9A-Fa-f]{6,8}\"", RegexOptions.CultureInvariant)]
    private static partial Regex SelectionBrushRegex();

    [GeneratedRegex("\\b(?:BorderThickness|FocusVisualPrimaryThickness|FocusVisualSecondaryThickness)=\"\\d+(?:\\.\\d+)?\"", RegexOptions.CultureInvariant)]
    private static partial Regex FocusThicknessRegex();

    private sealed record Options(string ContractPath, string XamlRoot, string? ScopesPath, string? ScopeRoot);

    private sealed record Token(string Id, IReadOnlyList<string> XamlKeys, string Type, string StringValue, bool RequiredInFirstSlice);

    private sealed class TokenContract
    {
        private TokenContract(IReadOnlyList<Token> tokens)
        {
            Tokens = tokens;
        }

        public IReadOnlyList<Token> Tokens { get; }

        public static TokenContract? Load(string path, List<string> failures)
        {
            if (!File.Exists(path))
            {
                failures.Add($"Token contract not found: {path}");
                return null;
            }

            var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            var tokens = new List<Token>();
            foreach (var node in root["tokens"]!.AsArray().Select(token => token!.AsObject()))
            {
                var id = RequireString(node, "id", path, failures);
                var xamlKeys = ReadStringArray(node, "xamlKeys", path, failures);
                var type = RequireString(node, "type", path, failures);
                var required = (bool?)node["requiredInFirstSlice"] ?? false;
                var valueNode = node["value"];
                if (valueNode is null)
                {
                    failures.Add($"{path}: token {id} missing value.");
                    continue;
                }

                tokens.Add(new Token(id, xamlKeys, type, ValueToInvariantString(valueNode), required));
            }

            return new TokenContract(tokens);
        }

        private static string ValueToInvariantString(JsonNode value)
        {
            return value.GetValueKind() switch
            {
                JsonValueKind.String => value.GetValue<string>(),
                JsonValueKind.Number => value.ToJsonString(),
                _ => value.ToJsonString()
            };
        }
    }

    private sealed class XamlResourceSet
    {
        private readonly Dictionary<string, XamlResource> resources;

        private XamlResourceSet(Dictionary<string, XamlResource> resources)
        {
            this.resources = resources;
        }

        public static XamlResourceSet Load(string root, List<string> failures)
        {
            if (!Directory.Exists(root))
            {
                failures.Add($"XAML root not found: {root}");
                return new XamlResourceSet(new Dictionary<string, XamlResource>(StringComparer.Ordinal));
            }

            var resources = new Dictionary<string, XamlResource>(StringComparer.Ordinal);
            foreach (var path in Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
            {
                XDocument document;
                try
                {
                    document = XDocument.Load(path, LoadOptions.SetLineInfo);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException)
                {
                    failures.Add($"{path}: failed to parse XAML: {ex.Message}");
                    continue;
                }

                foreach (var element in document.Descendants())
                {
                    var key = (string?)element.Attribute(XamlNamespace + "Key");
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    var resource = new XamlResource(
                        key,
                        element.Name.LocalName,
                        element.Value.Trim(),
                        (string?)element.Attribute("Color"),
                        path);

                    if (resources.TryGetValue(key, out var existing))
                    {
                        failures.Add($"duplicate XAML key '{key}' in {path}; first defined in {existing.Path}.");
                        continue;
                    }

                    resources.Add(key, resource);
                }
            }

            return new XamlResourceSet(resources);
        }

        public bool TryGet(string key, out XamlResource? resource)
        {
            return resources.TryGetValue(key, out resource);
        }

        public XamlResource Get(string key)
        {
            return resources[key];
        }
    }

    private sealed record XamlResource(string Key, string TypeName, string Value, string? ColorAttribute, string Path);
}
