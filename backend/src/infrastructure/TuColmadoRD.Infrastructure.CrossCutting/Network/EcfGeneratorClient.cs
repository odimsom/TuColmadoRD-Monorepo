using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuColmadoRD.Core.Application.Interfaces.Services;

namespace TuColmadoRD.Infrastructure.CrossCutting.Network;

public class EcfGeneratorClient : IEcfGeneratorClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EcfGeneratorClient> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EcfGeneratorClient(HttpClient httpClient, ILogger<EcfGeneratorClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GenerateXmlAsync(object payload)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/ecf/generate", payload);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error calling python ECF generator: {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<EcfGeneratorResponse>(_jsonOptions);
        if (result == null || string.IsNullOrWhiteSpace(result.Xml))
            throw new InvalidOperationException("Python ECF generator returned an empty XML.");

        foreach (var warning in result.Warnings)
            _logger.LogWarning("ECF XSD warning: {Warning}", warning);

        return result.Xml;
    }
}

public class EcfGeneratorResponse
{
    [JsonPropertyName("xml")]
    public string Xml { get; set; } = string.Empty;

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}
