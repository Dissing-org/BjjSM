namespace BjjSM.Modules.Curriculum.Domain;

public static class FlowchartPresets
{
    public const string ClosedGuardKey = "closed-guard-matrix";
    public const string HalfGuardKey = "half-guard-underhook-system";
    public const string MountEscapesKey = "mount-escapes";

    public static IReadOnlyList<(string Key, string Title, string Description)> GetPresetSummaries() =>
    [
        (ClosedGuardKey, "Closed Guard Matrix", "Classical closed guard game plan covering submissions (Armbar, Triangle, Omoplata) and sweeps to Mount."),
        (HalfGuardKey, "Half Guard Underhook System", "Lucas Leite style underhook half guard matrix leading to Dogfight, Knee Tap, Back Take, and Plan B Sweep."),
        (MountEscapesKey, "Mount Escapes", "Fundamental defensive escape paths from bottom mount: Bridge & Roll / Upa and Elbow-Knee Escape.")
    ];

    public static Flowchart? CreatePreset(string key, Guid academyId)
    {
        return key.ToLowerInvariant() switch
        {
            ClosedGuardKey => CreateClosedGuardMatrix(academyId),
            HalfGuardKey => CreateHalfGuardUnderhookSystem(academyId),
            MountEscapesKey => CreateMountEscapes(academyId),
            _ => null
        };
    }

    public static Flowchart CreateClosedGuardMatrix(Guid academyId)
    {
        var fc = Flowchart.Create(academyId, "Closed Guard Matrix", "Armbar, Triangle, Omoplata, Hip Bump & Flower sweeps").Value;

        var cg = fc.AddNode("Closed Guard", PositionCategory.Bottom, "Control posture and isolate arms", 100, 240).Value;
        var mount = fc.AddNode("Mount", PositionCategory.Top, "Establish heavy hips and high hooks", 460, 80).Value;
        var armbar = fc.AddNode("Armbar", PositionCategory.Submission, "Bite high with leg across spine, thumb pointing up", 460, 220).Value;
        var triangle = fc.AddNode("Triangle Choke", PositionCategory.Submission, "Clear far arm across body, pull shin to cut angle", 780, 160).Value;
        var omoplata = fc.AddNode("Omoplata", PositionCategory.Submission, "Kick legs through armpit, sit up and pin hip", 780, 320).Value;
        var back = fc.AddNode("Back Control", PositionCategory.Top, "Seatbelt grip with dual hooks in", 460, 380).Value;

        fc.AddEdge(cg.Id, mount.Id, "Hip Bump Sweep", TransitionType.Sweep, "Post hand, elevate hips violently", curveOffset: -25);
        fc.AddEdge(cg.Id, mount.Id, "Flower Sweep", TransitionType.Sweep, "Swing outside leg, pendulum momentum", curveOffset: 25);
        fc.AddEdge(cg.Id, armbar.Id, "Armbar Attack", TransitionType.SubmissionAttempt, "Overhook or 2-on-1 grip on wrist");
        fc.AddEdge(cg.Id, triangle.Id, "Triangle Attack", TransitionType.SubmissionAttempt, "Shoot legs over shoulder on posture push");
        fc.AddEdge(cg.Id, omoplata.Id, "Omoplata Entry", TransitionType.SubmissionAttempt, "Overhook angle rotation");
        fc.AddEdge(cg.Id, back.Id, "Arm Drag to Back", TransitionType.Transition, "Clear tricep, climb around lat");

        fc.AddEdge(armbar.Id, triangle.Id, "Triangle Counter", TransitionType.Transition, "When opponent postures out of armbar");
        fc.AddEdge(triangle.Id, armbar.Id, "Armlock Finish", TransitionType.SubmissionAttempt, "Pull arm across inside locked triangle");
        fc.AddEdge(triangle.Id, omoplata.Id, "Omoplata Transition", TransitionType.Transition, "When opponent hides arm behind thigh");

        return fc;
    }

    public static Flowchart CreateHalfGuardUnderhookSystem(Guid academyId)
    {
        var fc = Flowchart.Create(academyId, "Half Guard Underhook System", "Lucas Leite underhook dogfight matrix with sweeps & back take").Value;

        var hg = fc.AddNode("Half Guard", PositionCategory.Bottom, "Stay on hip, active knee shield or deep underhook", 100, 220).Value;
        var dogfight = fc.AddNode("Dogfight", PositionCategory.Neutral, "Strong underhook, drive forward on knees", 420, 220).Value;
        var back = fc.AddNode("Back Control", PositionCategory.Top, "Seatbelt harness and body triangle", 740, 80).Value;
        var side = fc.AddNode("Side Control", PositionCategory.Top, "Crossface and underhook control", 740, 220).Value;
        var roll = fc.AddNode("Kneebar / Plan B", PositionCategory.Submission, "Roll through under opponent's far leg", 740, 360).Value;

        fc.AddEdge(hg.Id, dogfight.Id, "Underhook & Come Up", TransitionType.Transition, "Pummel deep underhook, drive to knees");
        fc.AddEdge(dogfight.Id, back.Id, "Duck Under / Seatbelt", TransitionType.Transition, "Duck head under armpit, climb to back");
        fc.AddEdge(dogfight.Id, side.Id, "Knee Tap Sweep", TransitionType.Sweep, "Reach for far knee, drive weight forward");
        fc.AddEdge(dogfight.Id, roll.Id, "Plan B Sweep", TransitionType.Sweep, "When opponent whizzers heavily, roll under");

        return fc;
    }

    public static Flowchart CreateMountEscapes(Guid academyId)
    {
        var fc = Flowchart.Create(academyId, "Mount Escapes", "Fundamental defensive escape paths from bottom mount").Value;

        var mounted = fc.AddNode("Mounted by Opponent", PositionCategory.Bottom, "Protect neck, keep elbows tight to ribs", 100, 200).Value;
        var guard = fc.AddNode("Closed Guard", PositionCategory.Bottom, "Lock guard and break opponent posture", 460, 100).Value;
        var half = fc.AddNode("Half Guard", PositionCategory.Bottom, "Frame on hip, insert knee shield", 460, 300).Value;
        var top = fc.AddNode("Top Guard", PositionCategory.Top, "Reversed position, inside opponent guard", 760, 200).Value;

        fc.AddEdge(mounted.Id, top.Id, "Bridge & Roll (Upa)", TransitionType.Reversal, "Trap same side arm and foot, bridge 45 degrees");
        fc.AddEdge(mounted.Id, half.Id, "Elbow-Knee Escape", TransitionType.Escape, "Turn on side, slide knee under ankle to half guard");
        fc.AddEdge(half.Id, guard.Id, "Reguard Recovery", TransitionType.Escape, "Shrimp hips out, bring second leg around to full guard");

        return fc;
    }
}
