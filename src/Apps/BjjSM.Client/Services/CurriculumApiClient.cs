using System.Net.Http.Json;
using System.Text.Json;
using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public sealed class CurriculumApiClient : ICurriculumApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CurriculumApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<MonthlyPlanDto?> GetPlanAsync(Guid academyId, Guid programId, int year, int month, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/v1/plans?academyId={academyId}&programId={programId}&year={year}&month={month}";
            return await _http.GetFromJsonAsync<MonthlyPlanDto>(url, JsonOptions, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Result<MonthlyPlanDto>> CreatePlanAsync(CreateMonthlyPlanRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/plans", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MonthlyPlanDto>(error);
        }

        var plan = await response.Content.ReadFromJsonAsync<MonthlyPlanDto>(JsonOptions, ct);
        return plan is not null
            ? Result.Success(plan)
            : Result.Failure<MonthlyPlanDto>("Failed to deserialize created plan response.");
    }

    public async Task<Result<MonthlyPlanDto>> AddTechniqueAsync(Guid planId, int weekNumber, AddTechniqueRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"/api/v1/plans/{planId}/weeks/{weekNumber}/techniques", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MonthlyPlanDto>(error);
        }

        var plan = await response.Content.ReadFromJsonAsync<MonthlyPlanDto>(JsonOptions, ct);
        return plan is not null
            ? Result.Success(plan)
            : Result.Failure<MonthlyPlanDto>("Failed to deserialize updated plan response.");
    }

    public async Task<Result<MonthlyPlanDto>> UpdateTechniqueAsync(Guid planId, int weekNumber, Guid techniqueId, UpdateTechniqueRequest request, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"/api/v1/plans/{planId}/weeks/{weekNumber}/techniques/{techniqueId}", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MonthlyPlanDto>(error);
        }

        var plan = await response.Content.ReadFromJsonAsync<MonthlyPlanDto>(JsonOptions, ct);
        return plan is not null
            ? Result.Success(plan)
            : Result.Failure<MonthlyPlanDto>("Failed to deserialize updated plan response.");
    }

    public async Task<Result<MonthlyPlanDto>> DeleteTechniqueAsync(Guid planId, int weekNumber, Guid techniqueId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"/api/v1/plans/{planId}/weeks/{weekNumber}/techniques/{techniqueId}", ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MonthlyPlanDto>(error);
        }

        var plan = await response.Content.ReadFromJsonAsync<MonthlyPlanDto>(JsonOptions, ct);
        return plan is not null
            ? Result.Success(plan)
            : Result.Failure<MonthlyPlanDto>("Failed to deserialize updated plan response.");
    }

    public async Task<Result<bool>> DeletePlanAsync(Guid planId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"/api/v1/plans/{planId}", ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<bool>(error);
        }

        return Result.Success(true);
    }

    public async Task<Result<MonthlyPlanDto>> UpdateWeekThemeAsync(Guid planId, int weekNumber, string theme, CancellationToken ct = default)
    {
        var request = new UpdateWeeklyThemeRequest(theme);
        var response = await _http.PutAsJsonAsync($"/api/v1/plans/{planId}/weeks/{weekNumber}/theme", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MonthlyPlanDto>(error);
        }

        var plan = await response.Content.ReadFromJsonAsync<MonthlyPlanDto>(JsonOptions, ct);
        return plan is not null
            ? Result.Success(plan)
            : Result.Failure<MonthlyPlanDto>("Failed to deserialize updated plan response.");
    }

    public async Task<Result<MonthlyPlanDto>> UpdateObjectiveAsync(Guid planId, string objective, CancellationToken ct = default)
    {
        var request = new UpdateObjectiveRequest(objective);
        var response = await _http.PutAsJsonAsync($"/api/v1/plans/{planId}/objective", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MonthlyPlanDto>(error);
        }

        var plan = await response.Content.ReadFromJsonAsync<MonthlyPlanDto>(JsonOptions, ct);
        return plan is not null
            ? Result.Success(plan)
            : Result.Failure<MonthlyPlanDto>("Failed to deserialize updated plan response.");
    }

    public async Task<byte[]?> DownloadPlanPdfAsync(Guid planId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/api/v1/plans/{planId}/pdf", ct);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return $"Request failed with status {(int)response.StatusCode} {response.ReasonPhrase}";

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("errors", out var errorsProp))
            {
                if (errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var msgs = new List<string>();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                                msgs.Add(item.GetString() ?? "");
                        }
                    }
                    if (msgs.Count > 0)
                        return string.Join("; ", msgs.Where(m => !string.IsNullOrEmpty(m)));
                }
            }
            if (doc.RootElement.TryGetProperty("message", out var msgProp))
                return msgProp.GetString() ?? raw;

            return raw;
        }
        catch
        {
            return $"Request failed with status {(int)response.StatusCode} {response.ReasonPhrase}";
        }
    }
}
