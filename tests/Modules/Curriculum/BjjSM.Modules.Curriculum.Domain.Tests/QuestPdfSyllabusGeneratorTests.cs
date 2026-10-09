using BjjSM.Modules.Curriculum.Domain;
using BjjSM.Modules.Curriculum.Infrastructure.Pdf;
using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class QuestPdfSyllabusGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnNonEmptyPdfByteArray_WhenGivenValidPublishedPlan()
    {
        // Arrange
        var academyId = Guid.NewGuid();
        var programId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();

        var plan = MonthlyPlan.Create(
            academyId,
            programId,
            year: 2026,
            month: 10,
            objective: "Closed Guard Mastery & Back Takes",
            createdByInstructorId: instructorId,
            weekCount: 4).Value;

        for (int i = 1; i <= 4; i++)
        {
            plan.UpdateWeekTheme(i, $"Week {i} Theme: Sweeps & Submissions");
            plan.AddTechnique(i, new TechniqueDrill(
                Guid.NewGuid(),
                $"Technique {i}A",
                TechniqueCategory.Technique,
                "Focus on hip angle and collar grip",
                "https://example.com/video",
                SortOrder: 1));
        }

        var generator = new QuestPdfSyllabusGenerator();

        // Act
        var pdfBytes = generator.Generate(
            plan,
            academyName: "Alliance Jiu-Jitsu Academy",
            programName: "Adults Fundamentals",
            instructorName: "Professor Marcelo Silva");

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);

        // PDF file header signature check (%PDF-1.)
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes.Take(5).ToArray());
        Assert.Equal("%PDF-", header);
    }
}
