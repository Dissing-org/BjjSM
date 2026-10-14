using BjjSM.Client.Models;
using Microsoft.AspNetCore.Components;

namespace BjjSM.Client.Pages;

public partial class TalentsPage : ComponentBase
{
    private List<TalentNode> _talents = [];

    private int _totalPointsInvested => _talents.Sum(t => t.CurrentRank);

    protected override void OnInitialized()
    {
        InitializeTalents();
    }

    private void InitializeTalents()
    {
        _talents =
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
                var minRequiredForThisRow = _talents.Where(t => t.Row == r).Min(t => t.RequiredPointsInTree);
                var pointsBelow = simulated.Where(t => t.Row < r).Sum(t => t.Rank);
                if (pointsBelow < minRequiredForThisRow)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void IncrementTalent(TalentNode talent)
    {
        if (CanIncrement(talent))
        {
            talent.CurrentRank++;
            StateHasChanged();
        }
    }

    private void DecrementTalent(TalentNode talent)
    {
        if (CanDecrement(talent))
        {
            talent.CurrentRank--;
            StateHasChanged();
        }
    }

    private void ResetTalents()
    {
        foreach (var talent in _talents)
        {
            talent.CurrentRank = 0;
        }
        StateHasChanged();
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
