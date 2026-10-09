using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public interface IMemberApiClient
{
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(Guid? academyId = null, CancellationToken ct = default);
    Task<MemberDto?> GetMemberByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<MemberDto>> CreateMemberAsync(CreateMemberRequest request, CancellationToken ct = default);
    Task<Result<MemberDto>> PromoteMemberAsync(Guid id, PromoteMemberRequest request, CancellationToken ct = default);
}
