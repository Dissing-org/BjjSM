using BjjSM.Client.Models;
using BjjSM.Client.Services;
using Microsoft.AspNetCore.Components;

namespace BjjSM.Client.Pages;

public partial class AnnualWheelPage : ComponentBase
{
    [Inject]
    private IAnnualWheelApiClient WheelApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private readonly Guid _academyId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly Guid _programId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private int _selectedYear = DateTime.UtcNow.Year;
    private int _selectedMonth = 1;
    private AnnualWheelDto? _wheel;
    private AnnualWheelSlotDto? _currentSlot => _wheel?.Slots.FirstOrDefault(s => s.Month == _selectedMonth);

    private bool _isLoading = true;
    private string? _errorMessage;

    private bool _showSwapModal;
    private int _targetSwapMonth = 2;

    private bool _showThemeModal;
    private Guid _targetThemeId;
    private IReadOnlyList<CurriculumThemeDto> _availableThemes = [];

    private bool _isSubmittingModal;
    private string? _modalError;
    private bool _isInstantiating;

    protected override async Task OnInitializedAsync()
    {
        await LoadWheelAsync();
        _availableThemes = await WheelApi.GetThemesAsync();
    }

    private async Task LoadWheelAsync()
    {
        _isLoading = true;
        _errorMessage = null;
        try
        {
            var res = await WheelApi.GetWheelAsync(_academyId, _programId, _selectedYear);
            if (res.IsSuccess)
            {
                _wheel = res.Value;
            }
            else
            {
                _errorMessage = res.Error;
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SelectMonth(int month)
    {
        _selectedMonth = month;
    }

    private void OpenSwapModal()
    {
        _targetSwapMonth = _selectedMonth == 12 ? 1 : _selectedMonth + 1;
        _modalError = null;
        _showSwapModal = true;
    }

    private async Task ExecuteSwapAsync()
    {
        if (_wheel is null) return;
        _isSubmittingModal = true;
        _modalError = null;
        try
        {
            var result = await WheelApi.SwapMonthsAsync(_wheel.Id, _selectedMonth, _targetSwapMonth);
            if (result.IsFailure)
            {
                _modalError = result.Error;
                return;
            }

            _wheel = result.Value;
            _showSwapModal = false;
        }
        finally
        {
            _isSubmittingModal = false;
        }
    }

    private void OpenThemeModal()
    {
        if (_currentSlot is not null)
            _targetThemeId = _currentSlot.ThemeId;

        _modalError = null;
        _showThemeModal = true;
    }

    private async Task ExecuteAssignThemeAsync()
    {
        if (_wheel is null || _targetThemeId == Guid.Empty) return;
        _isSubmittingModal = true;
        _modalError = null;
        try
        {
            var result = await WheelApi.AssignThemeAsync(_wheel.Id, _selectedMonth, _targetThemeId, null);
            if (result.IsFailure)
            {
                _modalError = result.Error;
                return;
            }

            _wheel = result.Value;
            _showThemeModal = false;
        }
        finally
        {
            _isSubmittingModal = false;
        }
    }

    private async Task InstantiateCurrentMonthAsync()
    {
        if (_wheel is null || _currentSlot is null) return;
        _isInstantiating = true;
        try
        {
            var result = await WheelApi.InstantiateMonthAsync(_wheel.Id, _selectedMonth, Guid.NewGuid());
            if (result.IsSuccess)
            {
                await LoadWheelAsync();
            }
            else
            {
                _errorMessage = result.Error;
            }
        }
        finally
        {
            _isInstantiating = false;
        }
    }

    private void NavigateToPlanner()
    {
        Navigation.NavigateTo($"/curriculum?year={_selectedYear}&month={_selectedMonth}");
    }

    private static string GetMonthName(int month)
    {
        return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
    }
}
