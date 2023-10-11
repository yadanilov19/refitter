﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using NJsonSchema.CodeGeneration.CSharp;

namespace Refitter.Core;

/// <summary>
/// Provide settings for Refit generator.
/// </summary>
[ExcludeFromCodeCoverage]
public class RefitGeneratorSettings
{
    /// <summary>
    /// Gets or sets the path to the Open API.
    /// </summary>
    [JsonPropertyName("openApiPath")]
    public string OpenApiPath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the namespace for the generated code. (default: GeneratedCode)
    /// </summary>
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = "GeneratedCode";

    /// <summary>
    /// Gets or sets the naming settings.
    /// </summary>
    [JsonPropertyName("naming")]
    public NamingSettings Naming { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether contracts should be generated.
    /// </summary>
    [JsonPropertyName("generateContracts")]
    public bool GenerateContracts { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether XML doc comments should be generated.
    /// </summary>
    [JsonPropertyName("generateXmlDocCodeComments")]
    public bool GenerateXmlDocCodeComments { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to add auto-generated header.
    /// </summary>
    [JsonPropertyName("addAutoGeneratedHeader")]
    public bool AddAutoGeneratedHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to add accept headers [Headers("Accept: application/json")].
    /// </summary>
    [JsonPropertyName("addAcceptHeaders")]
    public bool AddAcceptHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to return <c>IApiResponse</c> objects.
    /// </summary>
    [JsonPropertyName("returnIApiResponse")]
    public bool ReturnIApiResponse { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to generate operation headers.
    /// </summary>
    [JsonPropertyName("generateOperationHeaders")]
    public bool GenerateOperationHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the generated type accessibility. (default: Public)
    /// </summary>
    [JsonPropertyName("typeAccessibility")]
    public TypeAccessibility TypeAccessibility { get; set; } = TypeAccessibility.Public;

    /// <summary>
    /// Enable or disable the use of cancellation tokens.
    /// </summary>
    [JsonPropertyName("useCancellationTokens")]
    public bool UseCancellationTokens { get; set; }

    /// <summary>
    /// Set to <c>true</c> to explicitly format date query string parameters
    /// in ISO 8601 standard date format using delimiters (for example: 2023-06-15)
    /// </summary>
    [JsonPropertyName("useIsoDateFormat")]
    public bool UseIsoDateFormat { get; set; }

    /// <summary>
    /// Add additional namespace to generated types
    /// </summary>
    [JsonPropertyName("additionalNamespaces")]
    public string[] AdditionalNamespaces { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Set to <c>true</c> to Generate a Refit interface for each endpoint
    /// </summary>
    [JsonPropertyName("multipleInterfaces")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MultipleInterfaces MultipleInterfaces { get; set; }

    /// <summary>
    /// Set to <c>true</c> to Generate a Refit interface for each endpoint
    /// </summary>
    [JsonPropertyName("includePathMatches")]
    public string[] IncludePathMatches { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Set to <c>true</c> to Generate a Refit interface for each endpoint
    /// </summary>
    [JsonPropertyName("includeTags")]
    public string[] IncludeTags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Set to <c>true</c> to generate deprecated operations, otherwise <c>false</c>
    /// </summary>
    [JsonPropertyName("generateDeprecatedOperations")]
    public bool GenerateDeprecatedOperations { get; set; } = true;

    /// <summary>
    /// Generate operation names using pattern. 
    /// When using --multiple-interfaces ByEndpoint, this is name of the Execute() method in the interface.
    /// </summary>
    [JsonPropertyName("operationNameTemplate")]
    public string? OperationNameTemplate { get; set; }

    /// <summary>
    /// Set to <c>true</c> to re-order optional parameters to the end of the parameter list
    /// </summary>
    [JsonPropertyName("optionalParameters")]
    public bool OptionalParameters { get; set; }
    
    /// <summary>
    /// Gets or sets the relative path to a folder in which the output files are generated. (default: ./Generated)
    /// </summary>
    [JsonPropertyName("outputFolder")]
    public string OutputFolder { get; set; } = "./Generated";

    /// <summary>
    /// Gets or sets the settings describing how to register generated interface to the .NET Core DI container
    /// </summary>
    [JsonPropertyName("dependencyInjectionSettings")]
    public DependencyInjectionSettings? DependencyInjectionSettings { get; set; }

    /// <summary>
    /// Gets or sets the settings describing how to generate types using NSwag
    /// </summary>
    [JsonPropertyName("codeGeneratorSettings")]
    public CSharpGeneratorSettings? CodeGeneratorSettings { get; set; }
}

/// <summary>
/// Enum representing the different options for generating multiple Refit interfaces.
/// </summary>
public enum MultipleInterfaces
{
    /// <summary>
    /// Do not generate multiple interfaces
    /// </summary>
    [JsonPropertyName("unset")]
    Unset,
    /// <summary>
    /// Generate a Refit interface for each endpoint with a single Execute() method. 
    /// The method name can be customized using the --operation-name-template command line option, 
    /// or the operationNameTemplate property in the settings file.
    /// </summary>
    [JsonPropertyName("byEndpoint")]
    ByEndpoint,
    /// <summary>
    /// Generate a Refit interface for each tag
    /// </summary>
    [JsonPropertyName("byTag")]
    ByTag
}

/// <summary>
/// Configurable settings for naming in the client API
/// </summary>
[ExcludeFromCodeCoverage]
public class NamingSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the OpenApi title should be used. Default is true.
    /// </summary>
    [JsonPropertyName("useOpenApiTitle")]
    public bool UseOpenApiTitle { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the Interface. Default is "ApiClient".
    /// </summary>
    [JsonPropertyName("interfaceName")]
    public string InterfaceName { get; set; } = "ApiClient";
}

/// <summary>
/// Specifies the accessibility of a type.
/// </summary>
public enum TypeAccessibility
{
    /// <summary>
    /// Indicates that the type is accessible by any assembly that references it.
    /// </summary>
    Public,

    /// <summary>
    /// Indicates that the type is only accessible within its own assembly.
    /// </summary>
    Internal
}

/// <summary>
/// Dependency Injection settings describing how the Refit client should be configured.
/// This can be used to configure the HttpClient pipeline with additional handlers
/// </summary>
public class DependencyInjectionSettings
{
    /// <summary>
    /// Base Address for the HttpClient
    /// </summary>
    [JsonPropertyName("baseUrl")]
    public string? BaseUrl { get; set; }

    /// <summary>
    /// A collection of HttpMessageHandlers to be added to the HttpClient pipeline.
    /// This can be for telemetry logging, authorization, etc.
    /// </summary>
    [JsonPropertyName("httpMessageHandlers")]
    public string[] HttpMessageHandlers { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Set this to true to use Polly for transient fault handling.
    /// </summary>
    [JsonPropertyName("usePolly")]
    public bool UsePolly { get; set; }

    /// <summary>
    /// Default max retry count for Polly. Default is 6.
    /// </summary>
    [JsonPropertyName("pollyMaxRetryCount")]
    public int PollyMaxRetryCount { get; set; } = 6;

    /// <summary>
    /// The median delay to target before the first retry in seconds. Default is 1 second
    /// </summary>
    [JsonPropertyName("firstBackoffRetryInSeconds")]
    public double FirstBackoffRetryInSeconds { get; set; } = 1.0;
}
