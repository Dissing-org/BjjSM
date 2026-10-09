namespace BjjSM.ArchitectureTests;

public class DomainBoundaryTests
{
    [Fact]
    public void MembershipsDomain_ShouldNotReference_EntityFrameworkCore()
    {
        var domainAssembly = typeof(BjjSM.Modules.Memberships.Domain.Practitioner).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        Assert.DoesNotContain(referencedAssemblies, a => a.Name?.Contains("EntityFrameworkCore") == true);
    }

    [Fact]
    public void CurriculumDomain_ShouldNotReference_EntityFrameworkCore()
    {
        var domainAssembly = typeof(BjjSM.Modules.Curriculum.Domain.MonthlyPlan).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        Assert.DoesNotContain(referencedAssemblies, a => a.Name?.Contains("EntityFrameworkCore") == true);
    }
}
