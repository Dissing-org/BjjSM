using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class DeckSchedulingOptions
{
    public string LearningStepsMinutes { get; private set; } = "1, 10";
    public int GraduatingIntervalDays { get; private set; } = 1;
    public int EasyIntervalDays { get; private set; } = 4;
    public double StartingEaseFactor { get; private set; } = 2.5;
    public double MinimumEaseFactor { get; private set; } = 1.3;
    public double EasyBonusMultiplier { get; private set; } = 1.3;
    public double HardIntervalMultiplier { get; private set; } = 1.2;
    public double IntervalModifier { get; private set; } = 1.0;
    public double LapseIntervalMultiplier { get; private set; } = 0.0;
    public int MaximumIntervalDays { get; private set; } = 365;
    public int DailyNewCardsLimit { get; private set; } = 20;
    public int DailyReviewLimit { get; private set; } = 100;

    private DeckSchedulingOptions()
    {
    }

    private DeckSchedulingOptions(
        string learningStepsMinutes,
        int graduatingIntervalDays,
        int easyIntervalDays,
        double startingEaseFactor,
        double minimumEaseFactor,
        double easyBonusMultiplier,
        double hardIntervalMultiplier,
        double intervalModifier,
        double lapseIntervalMultiplier,
        int maximumIntervalDays,
        int dailyNewCardsLimit,
        int dailyReviewLimit)
    {
        LearningStepsMinutes = learningStepsMinutes;
        GraduatingIntervalDays = graduatingIntervalDays;
        EasyIntervalDays = easyIntervalDays;
        StartingEaseFactor = startingEaseFactor;
        MinimumEaseFactor = minimumEaseFactor;
        EasyBonusMultiplier = easyBonusMultiplier;
        HardIntervalMultiplier = hardIntervalMultiplier;
        IntervalModifier = intervalModifier;
        LapseIntervalMultiplier = lapseIntervalMultiplier;
        MaximumIntervalDays = maximumIntervalDays;
        DailyNewCardsLimit = dailyNewCardsLimit;
        DailyReviewLimit = dailyReviewLimit;
    }

    public static DeckSchedulingOptions Default() => new();

    public static Result<DeckSchedulingOptions> Create(
        string learningStepsMinutes,
        int graduatingIntervalDays,
        int easyIntervalDays,
        double startingEaseFactor,
        double minimumEaseFactor,
        double easyBonusMultiplier,
        double hardIntervalMultiplier,
        double intervalModifier,
        double lapseIntervalMultiplier,
        int maximumIntervalDays,
        int dailyNewCardsLimit,
        int dailyReviewLimit)
    {
        var steps = ParseSteps(learningStepsMinutes);
        if (steps.Count == 0)
            return Result.Failure<DeckSchedulingOptions>("LearningStepsMinutes must contain at least one positive integer minute value.");

        if (graduatingIntervalDays < 1)
            return Result.Failure<DeckSchedulingOptions>("GraduatingIntervalDays must be at least 1 day.");

        if (easyIntervalDays < 1)
            return Result.Failure<DeckSchedulingOptions>("EasyIntervalDays must be at least 1 day.");

        if (startingEaseFactor < 1.3)
            return Result.Failure<DeckSchedulingOptions>("StartingEaseFactor must be at least 1.3.");

        if (minimumEaseFactor < 1.1 || minimumEaseFactor > startingEaseFactor)
            return Result.Failure<DeckSchedulingOptions>("MinimumEaseFactor must be between 1.1 and StartingEaseFactor.");

        if (easyBonusMultiplier < 1.0)
            return Result.Failure<DeckSchedulingOptions>("EasyBonusMultiplier must be at least 1.0.");

        if (hardIntervalMultiplier < 1.0)
            return Result.Failure<DeckSchedulingOptions>("HardIntervalMultiplier must be at least 1.0.");

        if (intervalModifier < 0.1)
            return Result.Failure<DeckSchedulingOptions>("IntervalModifier must be at least 0.1.");

        if (lapseIntervalMultiplier < 0.0 || lapseIntervalMultiplier > 1.0)
            return Result.Failure<DeckSchedulingOptions>("LapseIntervalMultiplier must be between 0.0 and 1.0.");

        if (maximumIntervalDays < 1)
            return Result.Failure<DeckSchedulingOptions>("MaximumIntervalDays must be at least 1 day.");

        if (dailyNewCardsLimit < 0)
            return Result.Failure<DeckSchedulingOptions>("DailyNewCardsLimit cannot be negative.");

        if (dailyReviewLimit < 0)
            return Result.Failure<DeckSchedulingOptions>("DailyReviewLimit cannot be negative.");

        var normalizedSteps = string.Join(", ", steps);

        return Result.Success(new DeckSchedulingOptions(
            normalizedSteps,
            graduatingIntervalDays,
            easyIntervalDays,
            Math.Round(startingEaseFactor, 2),
            Math.Round(minimumEaseFactor, 2),
            Math.Round(easyBonusMultiplier, 2),
            Math.Round(hardIntervalMultiplier, 2),
            Math.Round(intervalModifier, 2),
            Math.Round(lapseIntervalMultiplier, 2),
            maximumIntervalDays,
            dailyNewCardsLimit,
            dailyReviewLimit));
    }

    public IReadOnlyList<int> GetLearningSteps()
    {
        var parsed = ParseSteps(LearningStepsMinutes);
        return parsed.Count > 0 ? parsed : [1, 10];
    }

    private static List<int> ParseSteps(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        var parts = raw.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var steps = new List<int>();
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var val) || val <= 0)
                return [];
            steps.Add(val);
        }

        return steps;
    }
}
