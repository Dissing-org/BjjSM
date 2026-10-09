using BjjSM.SharedKernel;

namespace BjjSM.Modules.Memberships.Domain;

public sealed record Rank
{
    public BeltLadder Ladder { get; }
    public string BeltName { get; }
    public AdultBelt? Belt { get; }
    public KidsBelt? KidsBelt { get; }
    public int Stripes { get; }

    // Required by EF Core
    private Rank()
    {
        BeltName = string.Empty;
    }

    private Rank(BeltLadder ladder, AdultBelt? adultBelt, KidsBelt? kidsBelt, int stripes)
    {
        Ladder = ladder;
        Belt = adultBelt;
        KidsBelt = kidsBelt;
        BeltName = ladder == BeltLadder.Adult
            ? adultBelt?.ToString() ?? string.Empty
            : kidsBelt?.ToString() ?? string.Empty;
        Stripes = stripes;
    }

    public static Result<Rank> CreateAdult(AdultBelt belt, int stripes)
    {
        if (stripes < 0)
        {
            return Result.Failure<Rank>("Stripes must be between 0 and 4 for standard adult belts.");
        }

        switch (belt)
        {
            case AdultBelt.White:
            case AdultBelt.Blue:
            case AdultBelt.Purple:
            case AdultBelt.Brown:
                if (stripes > 4)
                {
                    return Result.Failure<Rank>("Stripes must be between 0 and 4 for standard adult belts.");
                }
                break;

            case AdultBelt.Black:
                if (stripes > 6)
                {
                    return Result.Failure<Rank>("Black belt degrees must be between 0 and 6.");
                }
                break;

            case AdultBelt.Coral:
                if (stripes < 7 || stripes > 8)
                {
                    return Result.Failure<Rank>("Coral belt degrees must be 7 or 8.");
                }
                break;

            case AdultBelt.Red:
                if (stripes < 9 || stripes > 10)
                {
                    return Result.Failure<Rank>("Red belt degrees must be 9 or 10.");
                }
                break;
        }

        return Result.Success(new Rank(BeltLadder.Adult, belt, null, stripes));
    }

    public static Result<Rank> CreateKids(KidsBelt belt, int stripes)
    {
        if (stripes < 0 || stripes > 4)
        {
            return Result.Failure<Rank>("Stripes must be between 0 and 4 for kids belts.");
        }

        return Result.Success(new Rank(BeltLadder.Kids, null, belt, stripes));
    }
}
