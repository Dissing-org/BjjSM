using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BjjSM.Modules.Curriculum.Infrastructure.Pdf;

public sealed class QuestPdfSyllabusGenerator : ISyllabusPdfGenerator
{
    private static readonly Color BrandBlue = Color.FromHex("#20409A");
    private static readonly Color BrandYellow = Color.FromHex("#FFEE00");
    private static readonly Color InsigniaBlack = Color.FromHex("#050505");
    private static readonly Color CrispWhite = Color.FromHex("#FFFFFF");
    private static readonly Color SurfaceLight = Color.FromHex("#F8FAFC");
    private static readonly Color BorderGray = Color.FromHex("#E2E8F0");
    private static readonly Color TextMuted = Color.FromHex("#64748B");

    static QuestPdfSyllabusGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(
        MonthlyPlan plan,
        string academyName,
        string programName,
        string instructorName)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(15, Unit.Point);
                page.PageColor(CrispWhite);
                page.DefaultTextStyle(x => x.FontFamily("Lato").FontColor(InsigniaBlack));

                page.Header().Column(col =>
                {
                    col.Item().Background(BrandBlue).PaddingVertical(10).PaddingHorizontal(16).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(academyName.ToUpperInvariant())
                                .FontSize(16).Bold().FontColor(CrispWhite);
                            c.Item().Text($"PROGRAM: {programName.ToUpperInvariant()}")
                                .FontSize(11).Bold().FontColor(BrandYellow);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            var monthName = new DateTime(plan.Year, plan.Month, 1).ToString("MMMM yyyy").ToUpperInvariant();
                            c.Item().Text(monthName).FontSize(16).Bold().FontColor(CrispWhite);
                            c.Item().Text($"COACH: {instructorName}").FontSize(10).FontColor(CrispWhite);
                        });
                    });

                    col.Item().Height(4).Background(BrandYellow);

                    col.Item().Background(SurfaceLight).Padding(8).Row(r =>
                    {
                        r.AutoItem().Text("MONTHLY OBJECTIVE: ").Bold().FontSize(10).FontColor(BrandBlue);
                        r.RelativeItem().Text(plan.MonthlyObjective).FontSize(10).FontColor(InsigniaBlack);
                    });
                });

                page.Content().PaddingTop(10).Row(row =>
                {
                    var weeks = plan.Weeks.OrderBy(w => w.WeekNumber).ToList();
                    foreach (var week in weeks)
                    {
                        row.RelativeItem().PaddingHorizontal(4).Border(1).BorderColor(BorderGray).Background(CrispWhite).Column(col =>
                        {
                            col.Item().Background(BrandBlue).Padding(6).Row(hr =>
                            {
                                hr.AutoItem().Background(BrandYellow).PaddingHorizontal(4).PaddingVertical(1)
                                    .Text($"W{week.WeekNumber}").Bold().FontSize(9).FontColor(InsigniaBlack);

                                hr.RelativeItem().PaddingLeft(6).Text(string.IsNullOrWhiteSpace(week.Theme) ? "TBD" : week.Theme)
                                    .Bold().FontSize(9).FontColor(CrispWhite);
                            });

                            col.Item().Padding(6).Column(tc =>
                            {
                                if (week.Techniques.Count == 0)
                                {
                                    tc.Item().PaddingTop(8).Text("No techniques scheduled").Italic().FontSize(8).FontColor(TextMuted);
                                }
                                else
                                {
                                    foreach (var technique in week.Techniques.OrderBy(t => t.SortOrder))
                                    {
                                        tc.Item().PaddingBottom(6).BorderBottom(0.5f).BorderColor(BorderGray).Column(tItem =>
                                        {
                                            tItem.Item().Row(tr =>
                                            {
                                                tr.AutoItem().Background(SurfaceLight).PaddingHorizontal(3).PaddingVertical(1)
                                                    .Text(technique.Category.ToString().ToUpperInvariant())
                                                    .FontSize(7).Bold().FontColor(BrandBlue);

                                                tr.RelativeItem().PaddingLeft(4).Text(technique.Title)
                                                    .Bold().FontSize(9).FontColor(InsigniaBlack);
                                            });

                                            if (!string.IsNullOrWhiteSpace(technique.CoachingNotes))
                                            {
                                                tItem.Item().PaddingTop(2).Text(technique.CoachingNotes)
                                                    .FontSize(7.5f).FontColor(TextMuted);
                                            }

                                            if (!string.IsNullOrWhiteSpace(technique.ReferenceUrl))
                                            {
                                                tItem.Item().PaddingTop(2).Hyperlink(technique.ReferenceUrl)
                                                    .Text("[Video] Watch Drill")
                                                    .FontSize(7.5f).Bold().FontColor(BrandBlue);
                                            }
                                        });
                                    }
                                }
                            });
                        });
                    }
                });

                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("BjjSM Academy Management - Curriculum Syllabus").FontSize(8).FontColor(TextMuted);
                    r.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
