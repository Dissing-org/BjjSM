namespace BjjSM.Modules.Curriculum.Domain;

public enum CardFieldSide
{
    Front = 1,
    Back = 2
}

public enum CardFieldType
{
    Text = 1,
    MultilineText = 2,
    VideoUrl = 3
}

public enum CardState
{
    New = 0,
    Learning = 1,
    Review = 2,
    Relearning = 3,
    Suspended = 4
}

public enum ReviewRating
{
    Again = 1,
    Hard = 2,
    Good = 3,
    Easy = 4
}

public enum CardMaturityTier
{
    New = 0,
    Learning = 1,
    Young = 2,
    Mature = 3,
    Suspended = 4
}
