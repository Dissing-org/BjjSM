using BjjSM.Modules.Memberships.Domain;

namespace BjjSM.Client.Models;

public record RankDto(
    BeltLadder Ladder,
    string BeltName,
    AdultBelt? Belt,
    KidsBelt? KidsBelt,
    int Stripes)
{
    public static implicit operator RankDto(Rank r) => new(r.Ladder, r.BeltName, r.Belt, r.KidsBelt, r.Stripes);
}

public record MemberDto(
    Guid Id,
    Guid AcademyId,
    string FirstName,
    string LastName,
    string Email,
    PractitionerRole Roles,
    RankDto CurrentRank,
    IReadOnlyList<GraduationDto> Graduations);

public record GraduationDto(
    Guid Id,
    RankDto Rank,
    DateTime AwardedAt,
    Guid AwardedBy,
    string? Notes);

public record CreateMemberRequest(
    Guid AcademyId,
    string FirstName,
    string LastName,
    string Email,
    PractitionerRole Roles,
    BeltLadder Ladder,
    AdultBelt? AdultBelt,
    KidsBelt? KidsBelt,
    int Stripes);

public record PromoteMemberRequest(
    BeltLadder Ladder,
    AdultBelt? AdultBelt,
    KidsBelt? KidsBelt,
    int Stripes,
    DateTime AwardedAt,
    Guid AwardedBy,
    string? Notes);
