namespace BjjSM.Modules.Curriculum.Domain;

public sealed record ScheduleTransition(
    CardState NextState,
    int NextStepIndex,
    double NextIntervalDays,
    double NextEaseFactor,
    int NextRepetitions,
    int NextLapses,
    DateTime NextDueAt,
    string HumanReadableInterval);

public sealed record IntervalPreview(
    string Again,
    string Hard,
    string Good,
    string Easy);

public static class Sm2Scheduler
{
    public static IntervalPreview PreviewIntervals(
        Flashcard card,
        DeckSchedulingOptions options,
        DateTime nowUtc)
    {
        var again = CalculateNextSchedule(card, options, ReviewRating.Again, nowUtc).HumanReadableInterval;
        var hard = CalculateNextSchedule(card, options, ReviewRating.Hard, nowUtc).HumanReadableInterval;
        var good = CalculateNextSchedule(card, options, ReviewRating.Good, nowUtc).HumanReadableInterval;
        var easy = CalculateNextSchedule(card, options, ReviewRating.Easy, nowUtc).HumanReadableInterval;

        return new IntervalPreview(again, hard, good, easy);
    }

    public static ScheduleTransition CalculateNextSchedule(
        Flashcard card,
        DeckSchedulingOptions options,
        ReviewRating rating,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(card);
        ArgumentNullException.ThrowIfNull(options);

        var steps = options.GetLearningSteps();
        var ease = card.EaseFactor <= 0 ? options.StartingEaseFactor : card.EaseFactor;

        return card.State switch
        {
            CardState.New or CardState.Learning => CalculateLearningSchedule(card, options, steps, ease, rating, nowUtc),
            CardState.Relearning => CalculateRelearningSchedule(card, options, steps, ease, rating, nowUtc),
            CardState.Review => CalculateReviewSchedule(card, options, steps, ease, rating, nowUtc),
            CardState.Suspended => CalculateLearningSchedule(card, options, steps, ease, rating, nowUtc),
            _ => CalculateLearningSchedule(card, options, steps, ease, rating, nowUtc)
        };
    }

    private static ScheduleTransition CalculateLearningSchedule(
        Flashcard card,
        DeckSchedulingOptions options,
        IReadOnlyList<int> steps,
        double ease,
        ReviewRating rating,
        DateTime nowUtc)
    {
        var currentStep = Math.Clamp(card.LearningStepIndex, 0, steps.Count - 1);

        switch (rating)
        {
            case ReviewRating.Again:
            {
                var firstMinutes = steps[0];
                return new ScheduleTransition(
                    CardState.Learning,
                    0,
                    0,
                    ease,
                    card.Repetitions,
                    card.Lapses,
                    nowUtc.AddMinutes(firstMinutes),
                    FormatMinutes(firstMinutes));
            }
            case ReviewRating.Hard:
            {
                int hardMinutes;
                if (currentStep == 0 && steps.Count > 1)
                {
                    hardMinutes = Math.Max(1, (int)Math.Round((steps[0] + steps[1]) / 2.0));
                }
                else
                {
                    hardMinutes = steps[currentStep];
                }

                return new ScheduleTransition(
                    CardState.Learning,
                    currentStep,
                    0,
                    ease,
                    card.Repetitions,
                    card.Lapses,
                    nowUtc.AddMinutes(hardMinutes),
                    FormatMinutes(hardMinutes));
            }
            case ReviewRating.Good:
            {
                var nextStep = currentStep + 1;
                if (nextStep < steps.Count)
                {
                    var nextMinutes = steps[nextStep];
                    return new ScheduleTransition(
                        CardState.Learning,
                        nextStep,
                        0,
                        ease,
                        card.Repetitions,
                        card.Lapses,
                        nowUtc.AddMinutes(nextMinutes),
                        FormatMinutes(nextMinutes));
                }

                var gradDays = ClampDays(options.GraduatingIntervalDays * options.IntervalModifier, options.MaximumIntervalDays);
                return new ScheduleTransition(
                    CardState.Review,
                    0,
                    gradDays,
                    ease,
                    card.Repetitions + 1,
                    card.Lapses,
                    nowUtc.AddDays(gradDays),
                    FormatDays(gradDays));
            }
            case ReviewRating.Easy:
            default:
            {
                var easyDays = ClampDays(options.EasyIntervalDays * options.IntervalModifier, options.MaximumIntervalDays);
                var nextEase = Math.Round(Math.Max(options.MinimumEaseFactor, ease + 0.15), 2);
                return new ScheduleTransition(
                    CardState.Review,
                    0,
                    easyDays,
                    nextEase,
                    card.Repetitions + 1,
                    card.Lapses,
                    nowUtc.AddDays(easyDays),
                    FormatDays(easyDays));
            }
        }
    }

    private static ScheduleTransition CalculateRelearningSchedule(
        Flashcard card,
        DeckSchedulingOptions options,
        IReadOnlyList<int> steps,
        double ease,
        ReviewRating rating,
        DateTime nowUtc)
    {
        var currentStep = Math.Clamp(card.LearningStepIndex, 0, steps.Count - 1);
        var preservedInterval = Math.Max(1.0, card.IntervalDays);

        switch (rating)
        {
            case ReviewRating.Again:
            {
                var firstMinutes = steps[0];
                return new ScheduleTransition(
                    CardState.Relearning,
                    0,
                    preservedInterval,
                    ease,
                    card.Repetitions,
                    card.Lapses,
                    nowUtc.AddMinutes(firstMinutes),
                    FormatMinutes(firstMinutes));
            }
            case ReviewRating.Hard:
            {
                var hardMinutes = Math.Max(1, (int)Math.Round(steps[currentStep] * 1.2));
                return new ScheduleTransition(
                    CardState.Relearning,
                    currentStep,
                    preservedInterval,
                    ease,
                    card.Repetitions,
                    card.Lapses,
                    nowUtc.AddMinutes(hardMinutes),
                    FormatMinutes(hardMinutes));
            }
            case ReviewRating.Good:
            {
                var nextStep = currentStep + 1;
                if (nextStep < steps.Count)
                {
                    var nextMinutes = steps[nextStep];
                    return new ScheduleTransition(
                        CardState.Relearning,
                        nextStep,
                        preservedInterval,
                        ease,
                        card.Repetitions,
                        card.Lapses,
                        nowUtc.AddMinutes(nextMinutes),
                        FormatMinutes(nextMinutes));
                }

                var regradDays = ClampDays(preservedInterval, options.MaximumIntervalDays);
                return new ScheduleTransition(
                    CardState.Review,
                    0,
                    regradDays,
                    ease,
                    card.Repetitions + 1,
                    card.Lapses,
                    nowUtc.AddDays(regradDays),
                    FormatDays(regradDays));
            }
            case ReviewRating.Easy:
            default:
            {
                var easyDays = ClampDays(Math.Max(preservedInterval + 1, preservedInterval * options.EasyBonusMultiplier), options.MaximumIntervalDays);
                return new ScheduleTransition(
                    CardState.Review,
                    0,
                    easyDays,
                    ease,
                    card.Repetitions + 1,
                    card.Lapses,
                    nowUtc.AddDays(easyDays),
                    FormatDays(easyDays));
            }
        }
    }

    private static ScheduleTransition CalculateReviewSchedule(
        Flashcard card,
        DeckSchedulingOptions options,
        IReadOnlyList<int> steps,
        double ease,
        ReviewRating rating,
        DateTime nowUtc)
    {
        var currentInterval = Math.Max(1.0, card.IntervalDays);

        switch (rating)
        {
            case ReviewRating.Again:
            {
                var nextEase = Math.Round(Math.Max(options.MinimumEaseFactor, ease - 0.20), 2);
                var lapsedInterval = options.LapseIntervalMultiplier <= 0.0
                    ? 1.0
                    : Math.Max(1.0, Math.Round(currentInterval * options.LapseIntervalMultiplier, 1));
                var firstMinutes = steps[0];

                return new ScheduleTransition(
                    CardState.Relearning,
                    0,
                    lapsedInterval,
                    nextEase,
                    card.Repetitions,
                    card.Lapses + 1,
                    nowUtc.AddMinutes(firstMinutes),
                    FormatMinutes(firstMinutes));
            }
            case ReviewRating.Hard:
            {
                var nextEase = Math.Round(Math.Max(options.MinimumEaseFactor, ease - 0.15), 2);
                var rawHard = Math.Max(currentInterval + 1.0, currentInterval * options.HardIntervalMultiplier * options.IntervalModifier);
                var hardDays = ClampDays(rawHard, options.MaximumIntervalDays);

                return new ScheduleTransition(
                    CardState.Review,
                    0,
                    hardDays,
                    nextEase,
                    card.Repetitions + 1,
                    card.Lapses,
                    nowUtc.AddDays(hardDays),
                    FormatDays(hardDays));
            }
            case ReviewRating.Good:
            {
                var rawGood = Math.Max(currentInterval + 1.0, currentInterval * ease * options.IntervalModifier);
                var goodDays = ClampDays(rawGood, options.MaximumIntervalDays);

                return new ScheduleTransition(
                    CardState.Review,
                    0,
                    goodDays,
                    ease,
                    card.Repetitions + 1,
                    card.Lapses,
                    nowUtc.AddDays(goodDays),
                    FormatDays(goodDays));
            }
            case ReviewRating.Easy:
            default:
            {
                var nextEase = Math.Round(Math.Max(options.MinimumEaseFactor, ease + 0.15), 2);
                var rawEasy = Math.Max(currentInterval + 2.0, currentInterval * ease * options.EasyBonusMultiplier * options.IntervalModifier);
                var easyDays = ClampDays(rawEasy, options.MaximumIntervalDays);

                return new ScheduleTransition(
                    CardState.Review,
                    0,
                    easyDays,
                    nextEase,
                    card.Repetitions + 1,
                    card.Lapses,
                    nowUtc.AddDays(easyDays),
                    FormatDays(easyDays));
            }
        }
    }

    private static double ClampDays(double days, int maxDays)
    {
        var clamped = Math.Clamp(days, 1.0, Math.Max(1, maxDays));
        return Math.Round(clamped, 1);
    }

    private static string FormatMinutes(int minutes)
    {
        if (minutes < 60)
            return $"{minutes}m";

        var hours = Math.Round(minutes / 60.0, 1);
        return $"{hours:0.#}h";
    }

    private static string FormatDays(double days)
    {
        if (days < 30)
            return $"{Math.Round(days):0}d";

        if (days < 365)
            return $"{days / 30.0:0.#}mo";

        return $"{days / 365.0:0.#}y";
    }
}
