using System.Text.Json;
using BjjSM.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BjjSM.Client.Pages;

public partial class TalentsPage : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private const string LocalStorageKey = "bjjsm_talents_v1";

    private List<TalentNode> _talents = [];
    private bool _showModal;
    private TalentFormModel _formModel = new();
    private string? _formError;

    private int _totalPointsInvested => _talents.Sum(t => t.CurrentRank);

    protected override async Task OnInitializedAsync()
    {
        await LoadTalentsAsync();
    }

    private async Task LoadTalentsAsync()
    {
        try
        {
            var json = await JS.InvokeAsync<string?>("localStorage.getItem", LocalStorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var loaded = JsonSerializer.Deserialize<List<TalentNode>>(json);
                if (loaded is { Count: > 0 })
                {
                    _talents = loaded;
                    return;
                }
            }
        }
        catch
        {
            // Fallback to defaults if JS runtime or parsing fails
        }

        _talents = GetDefaultTalents();
    }

    private async Task SaveToLocalStorageAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_talents);
            await JS.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, json);
        }
        catch
        {
            // Ignore storage write issues
        }
    }

    private List<TalentNode> GetDefaultTalents() =>
    [
        // Row 1: 1 talent
        new TalentNode
        {
            Id = "iron-grip",
            Name = "Iron Grip / Kumikata Mastery",
            Icon = "🥋",
            Row = 1,
            Col = 1,
            MaxRank = 5,
            CurrentRank = 0,
            DescriptionFormat = "Øger grebsstyrke og kravekontrol med {0}%.",
            RankValues = [2, 4, 6, 8, 10],
            RequiredPointsInTree = 0
        },
        // Row 2: 2 talents side by side
        new TalentNode
        {
            Id = "explosive-hip-escape",
            Name = "Explosive Hip Escape",
            Icon = "⚡",
            Row = 2,
            Col = 1,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Øger hoftens eksplosivitet og escape-succesrate med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 3
        },
        new TalentNode
        {
            Id = "defensive-framing",
            Name = "Defensive Framing",
            Icon = "🛡️",
            Row = 2,
            Col = 2,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Reducerer modstanderens pres i guard pass med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 3
        },
        // Row 3: 1 talent
        new TalentNode
        {
            Id = "pressure-passing-mastery",
            Name = "Pressure Passing Mastery / The Mat General",
            Icon = "👑",
            Row = 3,
            Col = 1,
            MaxRank = 1,
            CurrentRank = 0,
            DescriptionFormat = "Låser op for nådeløst tryk fra topposition. Modstanderens stamina tappes {0}% hurtigere under dit pres.",
            RankValues = [25],
            RequiredPointsInTree = 5
        }
    ];

    private void OpenCreateModal(int row = 1, int col = 1)
    {
        _formError = null;
        var reqPoints = row switch
        {
            1 => 0,
            2 => 3,
            3 => 5,
            _ => 0
        };

        _formModel = new TalentFormModel
        {
            Row = row,
            Col = row == 2 ? col : 1,
            RequiredPointsInTree = reqPoints,
            Icon = "🥋",
            MaxRank = 3,
            RankValuesRaw = "5, 10, 15",
            DescriptionFormat = "Øger kampeffektivitet med {0}%."
        };
        _showModal = true;
    }

    private void OpenEditModal(TalentNode talent)
    {
        _formError = null;
        _formModel = TalentFormModel.FromTalentNode(talent);
        _showModal = true;
    }

    private void CloseModal()
    {
        _showModal = false;
        _formError = null;
    }

    private void SetFormIcon(string icon)
    {
        _formModel.Icon = icon;
    }

    private void OnRowSelected(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var row))
        {
            OnRowChanged(row);
        }
    }

    private void OnMaxRankSelected(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var maxRank))
        {
            OnMaxRankChanged(maxRank);
        }
    }

    private void OnRowChanged(int newRow)
    {
        _formModel.Row = newRow;
        if (newRow != 2)
        {
            _formModel.Col = 1;
        }

        _formModel.RequiredPointsInTree = newRow switch
        {
            1 => 0,
            2 => 3,
            3 => 5,
            _ => 0
        };
    }

    private void OnMaxRankChanged(int newMaxRank)
    {
        _formModel.MaxRank = Math.Clamp(newMaxRank, 1, 5);
        var defaults = TalentFormModel.ParseRankValues(_formModel.RankValuesRaw, _formModel.MaxRank);
        _formModel.RankValuesRaw = string.Join(", ", defaults);
    }

    private async Task SaveTalentAsync()
    {
        if (string.IsNullOrWhiteSpace(_formModel.Name))
        {
            _formError = "Navn på talent skal udfyldes.";
            return;
        }

        var existingRank = 0;
        if (!_formModel.IsNew)
        {
            var old = _talents.FirstOrDefault(t => t.Id == _formModel.Id);
            if (old is not null)
            {
                existingRank = old.CurrentRank;
                _talents.Remove(old);
            }
        }

        var col = _formModel.Row == 2 ? _formModel.Col : 1;
        var conflict = _talents.FirstOrDefault(t => t.Row == _formModel.Row && t.Col == col);
        if (conflict is not null)
        {
            _talents.Remove(conflict);
        }

        var node = _formModel.ToTalentNode(existingRank);
        _talents.Add(node);

        await SaveToLocalStorageAsync();
        _showModal = false;
    }

    private async Task DeleteTalentAsync()
    {
        if (!_formModel.IsNew)
        {
            _talents.RemoveAll(t => t.Id == _formModel.Id);
            await SaveToLocalStorageAsync();
        }
        _showModal = false;
    }

    private async Task ResetPointsAsync()
    {
        foreach (var talent in _talents)
        {
            talent.CurrentRank = 0;
        }
        await SaveToLocalStorageAsync();
    }

    private async Task ResetToDefaultsAsync()
    {
        _talents = GetDefaultTalents();
        await SaveToLocalStorageAsync();
    }

    private bool CanIncrement(TalentNode talent)
    {
        if (talent.CurrentRank >= talent.MaxRank) return false;

        var pointsBeforeRow = _talents.Where(t => t.Row < talent.Row).Sum(t => t.CurrentRank);
        if (pointsBeforeRow < talent.RequiredPointsInTree) return false;

        if (!string.IsNullOrEmpty(talent.PrerequisiteTalentId))
        {
            var prereq = _talents.FirstOrDefault(t => t.Id == talent.PrerequisiteTalentId);
            if (prereq is null || !prereq.IsMaxed) return false;
        }

        return true;
    }

    private bool CanDecrement(TalentNode talent)
    {
        if (talent.CurrentRank <= 0) return false;

        var simulated = _talents.Select(t => new
        {
            t.Id,
            t.Row,
            t.RequiredPointsInTree,
            t.PrerequisiteTalentId,
            Rank = t.Id == talent.Id ? t.CurrentRank - 1 : t.CurrentRank
        }).ToList();

        foreach (var other in simulated.Where(t => t.Rank > 0))
        {
            if (other.PrerequisiteTalentId == talent.Id)
            {
                var targetPrereq = _talents.First(t => t.Id == talent.Id);
                if (simulated.First(t => t.Id == talent.Id).Rank < targetPrereq.MaxRank)
                {
                    return false;
                }
            }
        }

        var maxRowWithPoints = simulated.Where(t => t.Rank > 0).Select(t => t.Row).DefaultIfEmpty(1).Max();
        for (int r = 2; r <= maxRowWithPoints; r++)
        {
            var rowHasPoints = simulated.Any(t => t.Row == r && t.Rank > 0);
            if (rowHasPoints)
            {
                var rowTalents = _talents.Where(t => t.Row == r).ToList();
                var minRequiredForThisRow = rowTalents.Count > 0 ? rowTalents.Min(t => t.RequiredPointsInTree) : 0;
                var pointsBelow = simulated.Where(t => t.Row < r).Sum(t => t.Rank);
                if (pointsBelow < minRequiredForThisRow)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private async Task IncrementTalent(TalentNode talent)
    {
        if (CanIncrement(talent))
        {
            talent.CurrentRank++;
            await SaveToLocalStorageAsync();
            StateHasChanged();
        }
    }

    private async Task DecrementTalent(TalentNode talent)
    {
        if (CanDecrement(talent))
        {
            talent.CurrentRank--;
            await SaveToLocalStorageAsync();
            StateHasChanged();
        }
    }

    private string GetSlotCss(TalentNode talent)
    {
        if (talent.IsMaxed) return "maxed";
        if (talent.HasPoints) return "has-points";
        if (CanIncrement(talent)) return "unlocked";
        return "locked";
    }

    private string GetRankBadgeCss(TalentNode talent)
    {
        if (talent.IsMaxed) return "maxed";
        if (talent.HasPoints) return "has-points";
        return string.Empty;
    }
}
