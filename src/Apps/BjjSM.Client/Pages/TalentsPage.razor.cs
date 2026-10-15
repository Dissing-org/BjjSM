using System.Text.Json;
using BjjSM.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BjjSM.Client.Pages;

public partial class TalentsPage : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private const string LocalStorageKey = "bjjsm_talents_basics_v2";

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
        // Row 1 (2 Talents, req 0 pts)
        new TalentNode
        {
            Id = "full-guard",
            Name = "Full guard",
            Icon = "🥋",
            Row = 1,
            Col = 1,
            MaxRank = 5,
            CurrentRank = 0,
            DescriptionFormat = "Styrker lukket guard kontrol, hoftekontrol og submission trusler med {0}%.",
            RankValues = [2, 4, 6, 8, 10],
            RequiredPointsInTree = 0
        },
        new TalentNode
        {
            Id = "open-guard",
            Name = "Open guard",
            Icon = "⚡",
            Row = 1,
            Col = 2,
            MaxRank = 5,
            CurrentRank = 0,
            DescriptionFormat = "Øger mobilitet, distance management og guard retention med {0}%.",
            RankValues = [2, 4, 6, 8, 10],
            RequiredPointsInTree = 0
        },

        // Row 2 (3 Talents, req 5 pts)
        new TalentNode
        {
            Id = "passing-under",
            Name = "Passing Under",
            Icon = "🤼",
            Row = 2,
            Col = 1,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Double under og over-under pressure passes øges med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 5
        },
        new TalentNode
        {
            Id = "passer-over",
            Name = "Passer over",
            Icon = "🦅",
            Row = 2,
            Col = 2,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Torreando, x-pass og agility over-passes forbedres med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 5
        },
        new TalentNode
        {
            Id = "passing-inbetween",
            Name = "Passing inbetween",
            Icon = "🗡️",
            Row = 2,
            Col = 3,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Knee slice, knee cut og shin slide passing forbedres med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 5
        },

        // Row 3 (3 Talents, req 10 pts)
        new TalentNode
        {
            Id = "half-guard",
            Name = "Half guard",
            Icon = "🛡️",
            Row = 3,
            Col = 1,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Underhook battles, dogfight og lockdown kontrol øges med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 10
        },
        new TalentNode
        {
            Id = "side-control",
            Name = "Side control",
            Icon = "⛓️",
            Row = 3,
            Col = 2,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Crossface pres, bryst-mod-bryst dominans og pin isolation forbedres med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 10
        },
        new TalentNode
        {
            Id = "north-south",
            Name = "North south",
            Icon = "🔄",
            Row = 3,
            Col = 3,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "North-south rotation, arm isolation og kimura trusler øges med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 10
        },

        // Row 4 (3 Talents, req 15 pts)
        new TalentNode
        {
            Id = "mount",
            Name = "Mount",
            Icon = "👑",
            Row = 4,
            Col = 1,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Tyngdepunkt, low mount base og armbar setups forbedres med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 15
        },
        new TalentNode
        {
            Id = "s-mount",
            Name = "S-mount",
            Icon = "🔥",
            Row = 4,
            Col = 2,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Høj s-mount stramhed og hurtige triangle/armbar transitioner øges med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 15
        },
        new TalentNode
        {
            Id = "reverse-mount",
            Name = "Reverse Mount",
            Icon = "🪢",
            Row = 4,
            Col = 3,
            MaxRank = 3,
            CurrentRank = 0,
            DescriptionFormat = "Twister hook setups og benlås transitionskontrol øges med {0}%.",
            RankValues = [5, 10, 15],
            RequiredPointsInTree = 15
        },

        // Row 5 (1 Talent centered, req 20 pts)
        new TalentNode
        {
            Id = "back",
            Name = "Back",
            Icon = "🐍",
            Row = 5,
            Col = 2,
            MaxRank = 5,
            CurrentRank = 0,
            DescriptionFormat = "Seatbelt grip, hooks kontrol og Rear Naked Choke færdiggørelse styrkes med {0}%.",
            RankValues = [2, 4, 6, 8, 10],
            RequiredPointsInTree = 20
        },

        // Row 6 (Capstone 1 Talent centered, req 25 pts)
        new TalentNode
        {
            Id = "basic-master",
            Name = "Basic Master",
            Icon = "🌟",
            Row = 6,
            Col = 2,
            MaxRank = 1,
            CurrentRank = 0,
            DescriptionFormat = "Total mesterskab af BJJ fundamentals. Reducerer fejl i positioner og giver {0}% øget positionsdominans.",
            RankValues = [25],
            RequiredPointsInTree = 25
        }
    ];

    private void OpenCreateModal(int row = 1, int col = 1)
    {
        _formError = null;
        var targetRow = row;
        var targetCol = col;

        if (_talents.Any(t => t.Row == targetRow && t.Col == targetCol))
        {
            var found = false;
            for (var r = 1; r <= 6; r++)
            {
                var minCol = (r == 5 || r == 6) ? 2 : 1;
                var maxCol = r switch
                {
                    1 => 2,
                    >= 2 and <= 4 => 3,
                    5 or 6 => 2,
                    _ => 3
                };

                for (var c = minCol; c <= maxCol; c++)
                {
                    if (!_talents.Any(t => t.Row == r && t.Col == c))
                    {
                        targetRow = r;
                        targetCol = c;
                        found = true;
                        break;
                    }
                }

                if (found) break;
            }
        }

        var clampedRow = Math.Clamp(targetRow, 1, 6);
        var reqPoints = (clampedRow - 1) * 5;
        var resolvedCol = (clampedRow == 5 || clampedRow == 6) ? 2 : (clampedRow == 1 ? Math.Clamp(targetCol, 1, 2) : Math.Clamp(targetCol, 1, 3));

        _formModel = new TalentFormModel
        {
            Row = clampedRow,
            Col = resolvedCol,
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
        _formModel.Row = Math.Clamp(newRow, 1, 6);
        if (_formModel.Row == 5 || _formModel.Row == 6)
        {
            _formModel.Col = 2;
        }
        else if (_formModel.Row == 1 && _formModel.Col > 2)
        {
            _formModel.Col = 2;
        }

        _formModel.RequiredPointsInTree = (_formModel.Row - 1) * 5;
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
            }
        }

        var node = _formModel.ToTalentNode(existingRank);
        var conflict = _talents.FirstOrDefault(t => t.Row == node.Row && t.Col == node.Col && t.Id != node.Id);
        if (conflict is not null)
        {
            _formError = $"Pladsen (Række {node.Row}, Kolonne {node.Col}) er allerede optaget af '{conflict.Name}'. Vælg en ledig plads.";
            return;
        }

        if (!_formModel.IsNew)
        {
            _talents.RemoveAll(t => t.Id == _formModel.Id);
        }

        _talents.Add(node);

        await SaveToLocalStorageAsync();
        _showModal = false;
    }

    private async Task DeleteTalentAsync()
    {
        if (!_formModel.IsNew)
        {
            var target = _talents.FirstOrDefault(t => t.Id == _formModel.Id);
            if (target is not null && target.CurrentRank > 0)
            {
                _formError = "Nulstil venligst point i dette talent (eller nulstil hele træet) før du sletter det.";
                return;
            }

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
        var confirmed = await JS.InvokeAsync<bool>("confirm", "Er du sikker på, at du vil nulstille til standard BJJ Basics træet? Dette vil fjerne eventuelle brugerdefinerede talenter.");
        if (!confirmed) return;

        _talents = GetDefaultTalents();
        await SaveToLocalStorageAsync();
    }

    private bool CanIncrement(TalentNode talent) =>
        TalentProgressionEngine.CanIncrement(_talents, talent);

    private bool CanDecrement(TalentNode talent) =>
        TalentProgressionEngine.CanDecrement(_talents, talent);

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
