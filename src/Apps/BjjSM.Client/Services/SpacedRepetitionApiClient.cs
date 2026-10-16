using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public sealed class SpacedRepetitionApiClient : ISpacedRepetitionApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public SpacedRepetitionApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<Result<SpacedRepetitionOverviewDto>> GetOverviewAsync(Guid academyId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/api/spaced-repetition/overview?academyId={academyId}", ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure<SpacedRepetitionOverviewDto>(await ExtractErrorMessageAsync(response));

        var dto = await response.Content.ReadFromJsonAsync<SpacedRepetitionOverviewDto>(JsonOptions, ct);
        return dto is not null
            ? Result.Success(dto)
            : Result.Failure<SpacedRepetitionOverviewDto>("Failed to deserialize Spaced Repetition overview.");
    }

    public async Task<Result<CardTemplateDto>> UpsertTemplateAsync(UpsertCardTemplateRequestDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/api/spaced-repetition/templates", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure<CardTemplateDto>(await ExtractErrorMessageAsync(response));

        var dto = await response.Content.ReadFromJsonAsync<CardTemplateDto>(JsonOptions, ct);
        return dto is not null
            ? Result.Success(dto)
            : Result.Failure<CardTemplateDto>("Failed to deserialize template response.");
    }

    public async Task<Result> DeleteTemplateAsync(Guid templateId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"/api/spaced-repetition/templates/{templateId}", ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure(await ExtractErrorMessageAsync(response));

        return Result.Success();
    }

    public async Task<Result<StudyDeckDto>> UpsertDeckAsync(UpsertStudyDeckRequestDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("/api/spaced-repetition/decks", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure<StudyDeckDto>(await ExtractErrorMessageAsync(response));

        var dto = await response.Content.ReadFromJsonAsync<StudyDeckDto>(JsonOptions, ct);
        return dto is not null
            ? Result.Success(dto)
            : Result.Failure<StudyDeckDto>("Failed to deserialize deck response.");
    }

    public async Task<Result> DeleteDeckAsync(Guid deckId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"/api/spaced-repetition/decks/{deckId}", ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure(await ExtractErrorMessageAsync(response));

        return Result.Success();
    }

    public async Task<Result<StudyDeckDto>> UpsertCardAsync(Guid deckId, UpsertFlashcardRequestDto request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync($"/api/spaced-repetition/decks/{deckId}/cards", request, JsonOptions, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure<StudyDeckDto>(await ExtractErrorMessageAsync(response));

        var dto = await response.Content.ReadFromJsonAsync<StudyDeckDto>(JsonOptions, ct);
        return dto is not null
            ? Result.Success(dto)
            : Result.Failure<StudyDeckDto>("Failed to deserialize updated deck.");
    }

    public async Task<Result<StudyDeckDto>> DeleteCardAsync(Guid deckId, Guid cardId, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"/api/spaced-repetition/decks/{deckId}/cards/{cardId}", ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure<StudyDeckDto>(await ExtractErrorMessageAsync(response));

        var dto = await response.Content.ReadFromJsonAsync<StudyDeckDto>(JsonOptions, ct);
        return dto is not null
            ? Result.Success(dto)
            : Result.Failure<StudyDeckDto>("Failed to deserialize updated deck.");
    }

    public async Task<Result<FlashcardDto>> SubmitReviewAsync(Guid deckId, Guid cardId, int rating, bool reschedule, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"/api/spaced-repetition/decks/{deckId}/cards/{cardId}/review",
            new SubmitCardReviewRequestDto(rating, reschedule),
            JsonOptions,
            ct);

        if (!response.IsSuccessStatusCode)
            return Result.Failure<FlashcardDto>(await ExtractErrorMessageAsync(response));

        var dto = await response.Content.ReadFromJsonAsync<FlashcardDto>(JsonOptions, ct);
        return dto is not null
            ? Result.Success(dto)
            : Result.Failure<FlashcardDto>("Failed to deserialize reviewed card.");
    }

    public async Task<Result<FlashcardDto>> ExecuteLifecycleActionAsync(Guid deckId, Guid cardId, string action, int? intervalDays = null, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"/api/spaced-repetition/decks/{deckId}/cards/{cardId}/lifecycle",
            new CardLifecycleRequestDto(action, intervalDays),
            JsonOptions,
            ct);

        if (!response.IsSuccessStatusCode)
            return Result.Failure<FlashcardDto>(await ExtractErrorMessageAsync(response));

        var dto = await response.Content.ReadFromJsonAsync<FlashcardDto>(JsonOptions, ct);
        return dto is not null
            ? Result.Success(dto)
            : Result.Failure<FlashcardDto>("Failed to deserialize card lifecycle update.");
    }

    public async Task<Result<string>> ExportDeckJsonAsync(Guid deckId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"/api/spaced-repetition/decks/{deckId}/export", ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure<string>(await ExtractErrorMessageAsync(response));

        var package = await response.Content.ReadFromJsonAsync<DeckExportPackageDto>(JsonOptions, ct);
        if (package is null)
            return Result.Failure<string>("Failed to read exported deck package.");

        var formatted = JsonSerializer.Serialize(package, JsonOptions);
        return Result.Success(formatted);
    }

    public async Task<Result<StudyDeckDto>> ImportDeckJsonAsync(string rawJson, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
            return Result.Failure<StudyDeckDto>("JSON payload cannot be empty.");

        using var content = new StringContent(rawJson, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("/api/spaced-repetition/decks/import", content, ct);
        if (!response.IsSuccessStatusCode)
            return Result.Failure<StudyDeckDto>(await ExtractErrorMessageAsync(response));

        var dto = await response.Content.ReadFromJsonAsync<StudyDeckDto>(JsonOptions, ct);
        return dto is not null
            ? Result.Success(dto)
            : Result.Failure<StudyDeckDto>("Failed to deserialize imported deck.");
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return $"Request failed with status {(int)response.StatusCode}";

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
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
