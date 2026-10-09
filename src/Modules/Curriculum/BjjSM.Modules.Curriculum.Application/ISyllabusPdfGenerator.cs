using BjjSM.Modules.Curriculum.Domain;

namespace BjjSM.Modules.Curriculum.Application;

public interface ISyllabusPdfGenerator
{
    byte[] Generate(
        MonthlyPlan plan,
        string academyName,
        string programName,
        string instructorName);
}
