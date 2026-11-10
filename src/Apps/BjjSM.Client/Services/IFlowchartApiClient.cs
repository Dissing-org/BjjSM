using BjjSM.Client.Models;
using BjjSM.SharedKernel;

namespace BjjSM.Client.Services;

public interface IFlowchartApiClient
{
    Task<IReadOnlyList<FlowchartPresetSummaryDto>> GetPresetsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FlowchartSummaryDto>> GetFlowchartsAsync(Guid? academyId = null, CancellationToken ct = default);
    Task<Result<FlowchartDetailDto>> GetFlowchartByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<FlowchartDetailDto>> CreateFlowchartAsync(CreateFlowchartRequest request, CancellationToken ct = default);
    Task<Result<FlowchartDetailDto>> UpdateFlowchartAsync(Guid id, UpdateFlowchartRequest request, CancellationToken ct = default);
    Task<Result> DeleteFlowchartAsync(Guid id, CancellationToken ct = default);
}
