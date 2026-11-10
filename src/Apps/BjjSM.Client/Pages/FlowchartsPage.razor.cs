using BjjSM.Client.Models;
using BjjSM.Client.Services;
using Microsoft.AspNetCore.Components;

namespace BjjSM.Client.Pages;

public partial class FlowchartsPage : ComponentBase
{
    [Inject]
    public IFlowchartApiClient FlowchartApiClient { get; set; } = default!;

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    protected bool _isLoading = true;
    protected string? _errorMessage;
    protected IReadOnlyList<FlowchartSummaryDto> _flowcharts = [];
    protected IReadOnlyList<FlowchartPresetSummaryDto> _presets = [];

    protected bool _showCreateModal;
    protected bool _isCreating;
    protected string? _createModalError;
    protected string _newTitle = string.Empty;
    protected string _newDescription = string.Empty;
    protected string _selectedPresetKey = string.Empty;

    private static readonly Guid DefaultAcademyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    protected override async Task OnInitializedAsync()
    {
        await LoadFlowchartsAsync();
    }

    protected async Task LoadFlowchartsAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            var presetsTask = FlowchartApiClient.GetPresetsAsync();
            var flowchartsTask = FlowchartApiClient.GetFlowchartsAsync(DefaultAcademyId);

            await Task.WhenAll(presetsTask, flowchartsTask);

            _presets = await presetsTask;
            _flowcharts = await flowchartsTask;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load flowcharts: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected void OpenCreateModal()
    {
        _selectedPresetKey = string.Empty;
        _newTitle = string.Empty;
        _newDescription = string.Empty;
        _createModalError = null;
        _showCreateModal = true;
    }

    protected void CloseCreateModal()
    {
        _showCreateModal = false;
    }

    protected async Task CreateFlowchartAsync()
    {
        _createModalError = null;
        var title = string.IsNullOrWhiteSpace(_newTitle)
            ? (!string.IsNullOrWhiteSpace(_selectedPresetKey)
                ? _presets.FirstOrDefault(p => p.Key == _selectedPresetKey)?.Title ?? "New Game Plan"
                : "New Game Plan")
            : _newTitle.Trim();

        _isCreating = true;
        try
        {
            var req = new CreateFlowchartRequest(
                DefaultAcademyId,
                title,
                string.IsNullOrWhiteSpace(_newDescription) ? null : _newDescription.Trim(),
                string.IsNullOrWhiteSpace(_selectedPresetKey) ? null : _selectedPresetKey);

            var result = await FlowchartApiClient.CreateFlowchartAsync(req);
            if (result.IsSuccess)
            {
                _showCreateModal = false;
                NavigationManager.NavigateTo($"flowcharts/{result.Value.Id}");
            }
            else
            {
                _createModalError = result.Error;
            }
        }
        catch (Exception ex)
        {
            _createModalError = ex.Message;
        }
        finally
        {
            _isCreating = false;
        }
    }

    protected async Task DeleteFlowchartAsync(Guid id)
    {
        var result = await FlowchartApiClient.DeleteFlowchartAsync(id);
        if (result.IsSuccess)
        {
            await LoadFlowchartsAsync();
        }
        else
        {
            _errorMessage = $"Could not delete flowchart: {result.Error}";
        }
    }
}
