using BjjSM.Modules.Memberships.Application;
using BjjSM.Modules.Memberships.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Memberships;

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

public sealed class CreateMemberEndpoint : Endpoint<CreateMemberRequest, MemberDto>
{
    private readonly IPractitionerRepository _repository;

    public CreateMemberEndpoint(IPractitionerRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/v1/members");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateMemberRequest req, CancellationToken ct)
    {
        var rankResult = req.Ladder == BeltLadder.Adult
            ? Rank.CreateAdult(req.AdultBelt ?? AdultBelt.White, req.Stripes)
            : Rank.CreateKids(req.KidsBelt ?? KidsBelt.White, req.Stripes);

        if (rankResult.IsFailure)
        {
            AddError(rankResult.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var memberResult = Practitioner.Create(
            req.AcademyId == Guid.Empty ? Guid.NewGuid() : req.AcademyId,
            req.FirstName,
            req.LastName,
            req.Email,
            req.Roles,
            rankResult.Value);

        if (memberResult.IsFailure)
        {
            AddError(memberResult.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var member = memberResult.Value;
        await _repository.AddAsync(member, ct);
        await _repository.SaveChangesAsync(ct);

        var response = new MemberDto(
            member.Id,
            member.AcademyId,
            member.FirstName,
            member.LastName,
            member.Email,
            member.Roles,
            member.CurrentRank.ToDto(),
            []);

        await Send.CreatedAtAsync<GetMemberByIdEndpoint>(new { id = member.Id }, response, cancellation: ct);
    }
}
