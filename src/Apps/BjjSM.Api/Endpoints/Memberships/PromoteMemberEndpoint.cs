using BjjSM.Modules.Memberships.Application;
using BjjSM.Modules.Memberships.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Memberships;

public record PromoteMemberRequest(
    BeltLadder Ladder,
    AdultBelt? AdultBelt,
    KidsBelt? KidsBelt,
    int Stripes,
    DateTime AwardedAt,
    Guid AwardedBy,
    string? Notes);

public sealed class PromoteMemberEndpoint : Endpoint<PromoteMemberRequest, MemberDto>
{
    private readonly IPractitionerRepository _repository;

    public PromoteMemberEndpoint(IPractitionerRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/v1/members/{id:guid}/promote");
        AllowAnonymous();
    }

    public override async Task HandleAsync(PromoteMemberRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var member = await _repository.GetByIdAsync(id, ct);

        if (member is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var rankResult = req.Ladder == BeltLadder.Adult
            ? Rank.CreateAdult(req.AdultBelt ?? AdultBelt.White, req.Stripes)
            : Rank.CreateKids(req.KidsBelt ?? KidsBelt.White, req.Stripes);

        if (rankResult.IsFailure)
        {
            AddError(rankResult.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var promoteResult = member.Promote(rankResult.Value, req.AwardedAt, req.AwardedBy, req.Notes);
        if (promoteResult.IsFailure)
        {
            AddError(promoteResult.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await _repository.SaveChangesAsync(ct);

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
