using System.Net.Http.Json;
using System.Text.Json;
using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public sealed class MemberApiClient : IMemberApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MemberApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(Guid? academyId = null, CancellationToken ct = default)
    {
        var url = academyId.HasValue ? $"/api/v1/members?academyId={academyId.Value}" : "/api/v1/members";
        var result = await _http.GetFromJsonAsync<IReadOnlyList<MemberDto>>(url, JsonOptions, ct);
        return result ?? [];
    }

    public async Task<MemberDto?> GetMemberByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<MemberDto>($"/api/v1/members/{id}", JsonOptions, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Result<MemberDto>> CreateMemberAsync(CreateMemberRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/members", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MemberDto>(error);
        }

        var member = await response.Content.ReadFromJsonAsync<MemberDto>(JsonOptions, ct);
        return member is not null 
            ? Result.Success(member) 
            : Result.Failure<MemberDto>("Failed to deserialize created member response.");
    }

    public async Task<Result<MemberDto>> PromoteMemberAsync(Guid id, PromoteMemberRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"/api/v1/members/{id}/promote", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ExtractErrorMessageAsync(response);
            return Result.Failure<MemberDto>(error);
        }

        var member = await response.Content.ReadFromJsonAsync<MemberDto>(JsonOptions, ct);
        return member is not null 
            ? Result.Success(member) 
            : Result.Failure<MemberDto>("Failed to deserialize promoted member response.");
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
