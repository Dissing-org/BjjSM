using System.Net.Http.Json;
using System.Text.Json;
using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public sealed class FlowchartApiClient : IFlowchartApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public FlowchartApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<FlowchartPresetSummaryDto>> GetPresetsAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<IReadOnlyList<FlowchartPresetSummaryDto>>("/api/flowcharts/presets", JsonOptions, ct);
            return res ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<FlowchartSummaryDto>> GetFlowchartsAsync(Guid? academyId = null, CancellationToken ct = default)
    {
        try
        {
            var url = academyId.HasValue ? $"/api/flowcharts?academyId={academyId.Value}" : "/api/flowcharts";
            var res = await _http.GetFromJsonAsync<IReadOnlyList<FlowchartSummaryDto>>(url, JsonOptions, ct);
            return res ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<Result<FlowchartDetailDto>> GetFlowchartByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync($"/api/flowcharts/{id}", ct);
            if (!response.IsSuccessStatusCode)
                return Result.Failure<FlowchartDetailDto>($"Flowchart not found (Status: {response.StatusCode})");

            var dto = await response.Content.ReadFromJsonAsync<FlowchartDetailDto>(JsonOptions, ct);
            return dto is not null
                ? Result.Success(dto)
                : Result.Failure<FlowchartDetailDto>("Failed to deserialize flowchart.");
        }
        catch (Exception ex)
        {
            return Result.Failure<FlowchartDetailDto>(ex.Message);
        }
    }

    public async Task<Result<FlowchartDetailDto>> CreateFlowchartAsync(CreateFlowchartRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/flowcharts", request, JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
                return Result.Failure<FlowchartDetailDto>(await ExtractErrorAsync(response));

            var dto = await response.Content.ReadFromJsonAsync<FlowchartDetailDto>(JsonOptions, ct);
            return dto is not null
                ? Result.Success(dto)
                : Result.Failure<FlowchartDetailDto>("Failed to deserialize created flowchart.");
        }
        catch (Exception ex)
        {
            return Result.Failure<FlowchartDetailDto>(ex.Message);
        }
    }

    public async Task<Result<FlowchartDetailDto>> UpdateFlowchartAsync(Guid id, UpdateFlowchartRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"/api/flowcharts/{id}", request, JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
                return Result.Failure<FlowchartDetailDto>(await ExtractErrorAsync(response));

            var dto = await response.Content.ReadFromJsonAsync<FlowchartDetailDto>(JsonOptions, ct);
            return dto is not null
                ? Result.Success(dto)
                : Result.Failure<FlowchartDetailDto>("Failed to deserialize updated flowchart.");
        }
        catch (Exception ex)
        {
            return Result.Failure<FlowchartDetailDto>(ex.Message);
        }
    }

    public async Task<Result> DeleteFlowchartAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.DeleteAsync($"/api/flowcharts/{id}", ct);
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(await ExtractErrorAsync(response));
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return $"Request failed with status {(int)response.StatusCode}";

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("error", out var err))
                return err.GetString() ?? raw;
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString() ?? raw;
            return raw;
        }
        catch
        {
            return $"Request failed with status {(int)response.StatusCode}";
        }
    }
}
