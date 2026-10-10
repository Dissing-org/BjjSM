using System.Net.Http.Json;
using System.Text.Json;
using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public sealed class AnnualWheelApiClient : IAnnualWheelApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AnnualWheelApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<Result<AnnualWheelDto>> GetWheelAsync(Guid academyId, Guid programId, int year, CancellationToken ct = default)
    {
        var url = $"/api/curriculum/wheel?year={year}&academyId={academyId}&programId={programId}";
        var response = await _http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<AnnualWheelDto>(error);
        }

        var wheel = await response.Content.ReadFromJsonAsync<AnnualWheelDto>(JsonOptions, ct);
        return wheel is not null
            ? Result.Success(wheel)
            : Result.Failure<AnnualWheelDto>("Failed to deserialize wheel.");
    }

    public async Task<Result<AnnualWheelDto>> SwapMonthsAsync(Guid wheelId, int monthA, int monthB, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"/api/curriculum/wheel/{wheelId}/swap", new SwapMonthsRequest(monthA, monthB), JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<AnnualWheelDto>(error);
        }

        var wheel = await response.Content.ReadFromJsonAsync<AnnualWheelDto>(JsonOptions, ct);
        return wheel is not null
            ? Result.Success(wheel)
            : Result.Failure<AnnualWheelDto>("Failed to deserialize swapped wheel.");
    }

    public async Task<Result<AnnualWheelDto>> AssignThemeAsync(Guid wheelId, int month, Guid themeId, string? customObjective, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync($"/api/curriculum/wheel/{wheelId}/months/{month}/theme", new AssignThemeRequest(themeId, customObjective), JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<AnnualWheelDto>(error);
        }

        var wheel = await response.Content.ReadFromJsonAsync<AnnualWheelDto>(JsonOptions, ct);
        return wheel is not null
            ? Result.Success(wheel)
            : Result.Failure<AnnualWheelDto>("Failed to deserialize updated wheel.");
    }

    public async Task<Result<MonthlyPlanDto>> InstantiateMonthAsync(Guid wheelId, int month, Guid instructorId, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"/api/curriculum/wheel/{wheelId}/months/{month}/instantiate", new InstantiateMonthRequest(instructorId), JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MonthlyPlanDto>(error);
        }

        var plan = await response.Content.ReadFromJsonAsync<MonthlyPlanDto>(JsonOptions, ct);
        return plan is not null
            ? Result.Success(plan)
            : Result.Failure<MonthlyPlanDto>("Failed to deserialize created monthly plan.");
    }

    public async Task<IReadOnlyList<CurriculumThemeDto>> GetThemesAsync(CancellationToken ct = default)
    {
        try
        {
            var themes = await _http.GetFromJsonAsync<IReadOnlyList<CurriculumThemeDto>>("/api/curriculum/themes", JsonOptions, ct);
            return themes ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return $"Request failed with status {(int)response.StatusCode}";

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
                            {
                                var val = item.GetString();
                                if (!string.IsNullOrWhiteSpace(val)) msgs.Add(val);
                            }
                        }
                    }
                    if (msgs.Count > 0)
                        return string.Join("; ", msgs);
                }
            }

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
