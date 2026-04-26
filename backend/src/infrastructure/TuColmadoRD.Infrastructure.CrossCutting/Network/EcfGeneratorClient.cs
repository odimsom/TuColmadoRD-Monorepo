using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TuColmadoRD.Core.Application.Interfaces.Services;

namespace TuColmadoRD.Infrastructure.CrossCutting.Network;

public class EcfGeneratorClient : IEcfGeneratorClient
{
    private readonly HttpClient _httpClient;

    public EcfGeneratorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateXmlAsync(object payload)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/ecf/generate", payload);
        
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error calling python ECF generator: {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<EcfGeneratorResponse>();
        if (result == null || string.IsNullOrWhiteSpace(result.Xml))
        {
            throw new InvalidOperationException("Python ECF generator returned an empty XML.");
        }

        return result.Xml;
    }
}

public class EcfGeneratorResponse
{
    public string Xml { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
}
