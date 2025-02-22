using System.Text;

using NSwag;
using NSwag.CodeGeneration.CSharp.Models;

namespace Refitter.Core;

internal class RefitInterfaceGenerator : IRefitInterfaceGenerator
{
    protected const string Separator = "    ";

    protected readonly RefitGeneratorSettings settings;
    protected readonly OpenApiDocument document;
    protected readonly CustomCSharpClientGenerator generator;

    internal RefitInterfaceGenerator(
        RefitGeneratorSettings settings,
        OpenApiDocument document,
        CustomCSharpClientGenerator generator)
    {
        this.settings = settings;
        this.document = document;
        this.generator = generator;
        generator.BaseSettings.OperationNameGenerator = new OperationNameGenerator(document, settings);
    }

    public virtual RefitGeneratedCode GenerateCode()
    {
        return new RefitGeneratedCode(
            $$"""
              {{GenerateInterfaceDeclaration(out var interfaceName)}}
              {{Separator}}{
              {{GenerateInterfaceBody()}}
              {{Separator}}}
              """,
            interfaceName);
    }

    private string GenerateInterfaceBody()
    {
        var code = new StringBuilder();
        foreach (var kv in document.Paths)
        {
            foreach (var operations in kv.Value)
            {
                var operation = operations.Value;

                if (!settings.GenerateDeprecatedOperations && operation.IsDeprecated)
                {
                    continue;
                }

                var returnType = GetTypeName(operation);
                var verb = operations.Key.CapitalizeFirstCharacter();
                var name = GenerateOperationName(kv.Key, verb, operation);

                var operationModel = generator.CreateOperationModel(operation);
                var parameters = ParameterExtractor.GetParameters(operationModel, operation, settings);
                var parametersString = string.Join(", ", parameters);

                GenerateMethodXmlDocComments(operation, code);
                GenerateObsoleteAttribute(operation, code);
                GenerateForMultipartFormData(operationModel, code);
                GenerateAcceptHeaders(operations, operation, code);

                code.AppendLine($"{Separator}{Separator}[{verb}(\"{kv.Key}\")]")
                    .AppendLine($"{Separator}{Separator}{returnType} {name}({parametersString});")
                    .AppendLine();
            }
        }

        return code.ToString();
    }

    protected string GetTypeName(OpenApiOperation operation)
    {
        var returnTypeParameter = 
            (new[] { "200", "201", "203", "206" })
                .Where(operation.Responses.ContainsKey)
                .Select(code => GetTypeName(code, operation))
                .FirstOrDefault();

        return GetReturnType(returnTypeParameter);
    }

    private string GetTypeName(string code, OpenApiOperation operation)
    {
        var schema = operation.Responses[code].ActualResponse.Schema;
        var typeName = generator.GetTypeName(schema, true, null);

        if (!string.IsNullOrWhiteSpace(settings.CodeGeneratorSettings?.ArrayType) &&
            schema?.Type == NJsonSchema.JsonObjectType.Array)
        {
            typeName = typeName
                .Replace("ICollection", settings.CodeGeneratorSettings!.ArrayType)
                .Replace("IEnumerable", settings.CodeGeneratorSettings!.ArrayType);
        }

        return typeName;
    }

    protected string GenerateOperationName(
        string path,
        string verb,
        OpenApiOperation operation,
        bool capitalizeFirstCharacter = false)
    {
        const string operationNamePlaceholder = "{operationName}";

        var operationName = generator
            .BaseSettings
            .OperationNameGenerator
            .GetOperationName(document, path, verb, operation);

        if (capitalizeFirstCharacter)
            operationName = operationName.CapitalizeFirstCharacter();

        if (settings.OperationNameTemplate?.Contains(operationNamePlaceholder) ?? false)
        {
            operationName = settings.OperationNameTemplate!
                .Replace(operationNamePlaceholder, operationName);
        }

        return operationName;
    }

    protected static void GenerateForMultipartFormData(CSharpOperationModel operationModel, StringBuilder code)
    {
        if (operationModel.Consumes.Contains("multipart/form-data"))
        {
            code.AppendLine($"{Separator}{Separator}[Multipart]");
        }
    }

    protected void GenerateAcceptHeaders(
        KeyValuePair<string, OpenApiOperation> operations,
        OpenApiOperation operation,
        StringBuilder code)
    {
        if (settings.AddAcceptHeaders && document.SchemaType is >= NJsonSchema.SchemaType.OpenApi3)
        {
            //Generate header "Accept"
            var contentTypes = operations.Value.Responses.Select(pair => operation.Responses[pair.Key].Content.Keys);

            //remove duplicates
            var uniqueContentTypes = contentTypes
                .GroupBy(x => x)
                .SelectMany(y => y.First())
                .Distinct()
                .ToList();

            if (uniqueContentTypes.Any())
            {
                code.AppendLine($"{Separator}{Separator}[Headers(\"Accept: {string.Join(", ", uniqueContentTypes)}\")]");
            }
        }
    }

    protected string GetReturnType(string? returnTypeParameter)
    {
        return returnTypeParameter is null or "void"
            ? GetDefaultReturnType()
            : GetConfiguredReturnType(returnTypeParameter);
    }

    private string GetDefaultReturnType()
    {
        return settings.ReturnIApiResponse
            ? "Task<IApiResponse>"
            : "Task";
    }

    private string GetConfiguredReturnType(string returnTypeParameter)
    {
        return settings.ReturnIApiResponse
            ? $"Task<IApiResponse<{WellKnownNamesspaces.TrimImportedNamespaces(returnTypeParameter)}>>"
            : $"Task<{WellKnownNamesspaces.TrimImportedNamespaces(returnTypeParameter)}>";
    }

    protected void GenerateMethodXmlDocComments(OpenApiOperation operation, StringBuilder code)
    {
        if (!settings.GenerateXmlDocCodeComments)
            return;

        if (!string.IsNullOrWhiteSpace(operation.Description))
        {
            code.AppendLine($"{Separator}{Separator}/// <summary>");

            foreach (var line in operation.Description.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                code.AppendLine($"{Separator}{Separator}/// {line.Trim()}");

            code.AppendLine($"{Separator}{Separator}/// </summary>");
        }
    }

    protected void GenerateObsoleteAttribute(OpenApiOperation operation, StringBuilder code)
    {
        if (operation.IsDeprecated)
        {
            code.AppendLine($"{Separator}{Separator}[System.Obsolete]");
        }
    }

    private string GenerateInterfaceDeclaration(out string interfaceName)
    {
        var title = settings.Naming.UseOpenApiTitle
            ? IdentifierUtils.Sanitize(document.Info?.Title ?? "ApiClient")
            : settings.Naming.InterfaceName;

        interfaceName = $"I{title.CapitalizeFirstCharacter()}";
        var modifier = settings.TypeAccessibility.ToString().ToLowerInvariant();
        return $"""
                {Separator}{GetGeneratedCodeAttribute()}
                {Separator}{modifier} partial interface I{title.CapitalizeFirstCharacter()}
                """;
    }

    protected void GenerateInterfaceXmlDocComments(OpenApiOperation operation, StringBuilder code)
    {
        if (!settings.GenerateXmlDocCodeComments ||
            string.IsNullOrWhiteSpace(operation.Summary))
            return;

        code.AppendLine(
            $"""
             {Separator}/// <summary>
             {Separator}/// {operation.Summary}
             {Separator}/// </summary>
             """);
    }

    protected string GetGeneratedCodeAttribute() =>
        $"""
         [System.CodeDom.Compiler.GeneratedCode("Refitter", "{GetType().Assembly.GetName().Version}")]
         """;
}