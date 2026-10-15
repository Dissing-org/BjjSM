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
    private CurriculumThemeDto? _currentTheme => _availableThemes.FirstOrDefault(t => t.Id == _currentSlot?.ThemeId);

    private bool _isLoading = true;
    private string? _errorMessage;
    private string? _successMessage;

    // Drag & Drop tracking (Native HTML5 in Blazor)
    private Guid? _draggedThemeId;
    private int? _draggedMonth;
    private int? _hoveredDropMonth;

    // Click-to-place and Click-to-swap fallback state
    private Guid? _selectedThemeToPlace;
    private int? _selectedMonthToSwap;

    private IReadOnlyList<CurriculumThemeDto> _availableThemes = [];
    private string _selectedCategory = "Alle";
    private bool _isInstantiating;

    private static readonly string[] DefaultCategoryFilter = ["Alle"];

    private IEnumerable<string> AvailableCategories =>
        DefaultCategoryFilter.Concat(_availableThemes.Select(t => t.Category).Distinct().OrderBy(c => c));

    private IEnumerable<CurriculumThemeDto> FilteredThemes =>
        _selectedCategory == "Alle"
            ? _availableThemes
            : _availableThemes.Where(t => string.Equals(t.Category, _selectedCategory, StringComparison.OrdinalIgnoreCase));

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

    private void OnDragStartTheme(Guid themeId)
    {
        _draggedThemeId = themeId;
        _draggedMonth = null;
        _selectedThemeToPlace = null;
        _selectedMonthToSwap = null;
        _errorMessage = null;
        _successMessage = null;
    }

    private void OnDragStartMonth(int month)
    {
        _draggedMonth = month;
        _draggedThemeId = null;
        _selectedThemeToPlace = null;
        _selectedMonthToSwap = null;
        _errorMessage = null;
        _successMessage = null;
    }

    private void OnDragOverMonth(int month)
    {
        if (_hoveredDropMonth != month)
        {
            _hoveredDropMonth = month;
        }
    }

    private void OnDragLeaveMonth(int month)
    {
        if (_hoveredDropMonth == month)
        {
            _hoveredDropMonth = null;
        }
    }

    private void OnDragEnd()
    {
        _draggedThemeId = null;
        _draggedMonth = null;
        _hoveredDropMonth = null;
    }

    private async Task OnDropOnMonthAsync(int targetMonth)
    {
        _hoveredDropMonth = null;
        _errorMessage = null;
        _successMessage = null;

        if (_wheel is null) return;

        var themeToAssign = _draggedThemeId;
        var monthToSwap = _draggedMonth;

        _draggedThemeId = null;
        _draggedMonth = null;

        if (themeToAssign.HasValue)
        {
            var theme = _availableThemes.FirstOrDefault(t => t.Id == themeToAssign.Value);
            var result = await WheelApi.AssignThemeAsync(_wheel.Id, targetMonth, themeToAssign.Value, null);
            if (result.IsSuccess)
            {
                _wheel = result.Value;
                _selectedMonth = targetMonth;
                _successMessage = $"Temaet '{(theme?.Name ?? "Valgt tema")}' blev tildelt til {GetMonthName(targetMonth)}!";
            }
            else
            {
                _errorMessage = result.Error;
            }
        }
        else if (monthToSwap.HasValue && monthToSwap.Value != targetMonth)
        {
            var result = await WheelApi.SwapMonthsAsync(_wheel.Id, monthToSwap.Value, targetMonth);
            if (result.IsSuccess)
            {
                _wheel = result.Value;
                _selectedMonth = targetMonth;
                _successMessage = $"{GetMonthName(monthToSwap.Value)} og {GetMonthName(targetMonth)} blev byttet om!";
            }
            else
            {
                _errorMessage = result.Error;
            }
        }
    }

    private void OnClickTheme(Guid themeId)
    {
        _errorMessage = null;
        _selectedMonthToSwap = null;
        if (_selectedThemeToPlace == themeId)
        {
            _selectedThemeToPlace = null;
            _successMessage = null;
        }
        else
        {
            _selectedThemeToPlace = themeId;
            var theme = _availableThemes.FirstOrDefault(t => t.Id == themeId);
            _successMessage = $"Tema '{theme?.Name}' valgt. Klik på den måned, du vil placere det i.";
        }
    }

    private async Task OnClickMonthSlotAsync(int month)
    {
        _errorMessage = null;

        if (_selectedThemeToPlace.HasValue)
        {
            if (_wheel is not null)
            {
                var themeId = _selectedThemeToPlace.Value;
                var theme = _availableThemes.FirstOrDefault(t => t.Id == themeId);
                _selectedThemeToPlace = null;
                var result = await WheelApi.AssignThemeAsync(_wheel.Id, month, themeId, null);
                if (result.IsSuccess)
                {
                    _wheel = result.Value;
                    _selectedMonth = month;
                    _successMessage = $"Temaet '{(theme?.Name ?? "Valgt tema")}' blev tildelt til {GetMonthName(month)}!";
                }
                else
                {
                    _errorMessage = result.Error;
                }
            }
            return;
        }

        if (_selectedMonthToSwap.HasValue)
        {
            var sourceMonth = _selectedMonthToSwap.Value;
            if (sourceMonth != month && _wheel is not null)
            {
                _selectedMonthToSwap = null;
                var result = await WheelApi.SwapMonthsAsync(_wheel.Id, sourceMonth, month);
                if (result.IsSuccess)
                {
                    _wheel = result.Value;
                    _selectedMonth = month;
                    _successMessage = $"{GetMonthName(sourceMonth)} og {GetMonthName(month)} blev byttet om!";
                }
                else
                {
                    _errorMessage = result.Error;
                }
            }
            else
            {
                _selectedMonthToSwap = null;
            }
            return;
        }

        _selectedMonth = month;
    }

    private void ToggleSwapMode()
    {
        _errorMessage = null;
        _selectedThemeToPlace = null;
        if (_selectedMonthToSwap.HasValue)
        {
            _selectedMonthToSwap = null;
        }
        else
        {
            _selectedMonthToSwap = _selectedMonth;
        }
    }

    private async Task InstantiateCurrentMonthAsync()
    {
        if (_wheel is null || _currentSlot is null) return;
        _isInstantiating = true;
        _errorMessage = null;
        _successMessage = null;
        try
        {
            var result = await WheelApi.InstantiateMonthAsync(_wheel.Id, _selectedMonth, Guid.NewGuid());
            if (result.IsSuccess)
            {
                await LoadWheelAsync();
                _successMessage = $"{GetMonthName(_selectedMonth)} er nu oprettet i Curriculum Planner!";
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

    private List<int> GetAssignedMonthsForTheme(Guid themeId)
    {
        if (_wheel is null) return [];
        return _wheel.Slots.Where(s => s.ThemeId == themeId).Select(s => s.Month).OrderBy(m => m).ToList();
    }

    private string GetThemeCategory(Guid themeId)
    {
        return _availableThemes.FirstOrDefault(t => t.Id == themeId)?.Category ?? "BJJ";
    }

    private static string GetMonthName(int month)
    {
        return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
    }
}
