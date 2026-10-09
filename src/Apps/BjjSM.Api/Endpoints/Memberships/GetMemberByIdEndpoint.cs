using BjjSM.Modules.Memberships.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Memberships;

public sealed class GetMemberByIdEndpoint : EndpointWithoutRequest<MemberDto>
{
    private readonly IPractitionerRepository _repository;

    public GetMemberByIdEndpoint(IPractitionerRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/v1/members/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var member = await _repository.GetByIdAsync(id, ct);

        if (member is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var dto = new MemberDto(
            member.Id,
            member.AcademyId,
            member.FirstName,
            member.LastName,
            member.Email,
            member.Roles,
            member.CurrentRank.ToDto(),
            member.Graduations.Select(g => new GraduationDto(g.Id, g.Rank.ToDto(), g.AwardedAt, g.AwardedBy, g.Notes)).ToList()
        );

        await Send.OkAsync(dto, ct);
    }
}
