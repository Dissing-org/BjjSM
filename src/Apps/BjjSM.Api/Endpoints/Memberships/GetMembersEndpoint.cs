using BjjSM.Modules.Memberships.Application;
using BjjSM.Modules.Memberships.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Memberships;

public record RankDto(
    BeltLadder Ladder,
    string BeltName,
    AdultBelt? Belt,
    KidsBelt? KidsBelt,
    int Stripes);

public static class RankDtoExtensions
{
    public static RankDto ToDto(this Rank rank) => new(
        rank.Ladder,
        rank.BeltName,
        rank.Belt,
        rank.KidsBelt,
        rank.Stripes);
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

public sealed class GetMembersEndpoint : EndpointWithoutRequest<IReadOnlyList<MemberDto>>
{
    private readonly IPractitionerRepository _repository;

    public GetMembersEndpoint(IPractitionerRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/v1/members");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var academyId = Query<Guid>("academyId", isRequired: false);
        var members = await _repository.GetByAcademyAsync(academyId, ct);

        var dtos = members.Select(m => new MemberDto(
            m.Id,
            m.AcademyId,
            m.FirstName,
            m.LastName,
            m.Email,
            m.Roles,
            m.CurrentRank.ToDto(),
            m.Graduations.Select(g => new GraduationDto(g.Id, g.Rank.ToDto(), g.AwardedAt, g.AwardedBy, g.Notes)).ToList()
        )).ToList();

        await Send.OkAsync(dtos, ct);
    }
}
