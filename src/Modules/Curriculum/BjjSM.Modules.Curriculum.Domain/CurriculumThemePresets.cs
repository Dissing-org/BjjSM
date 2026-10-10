namespace BjjSM.Modules.Curriculum.Domain;

public static class CurriculumThemePresets
{
    public static IReadOnlyList<CurriculumTheme> CreatePresets() => BuildPresets();

    public static IReadOnlyList<CurriculumTheme> All => CreatePresets();

    private static IReadOnlyList<CurriculumTheme> BuildPresets()
    {
        return new List<CurriculumTheme>
        {
            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                "Closed Guard: Sweeps & Breaking Posture",
                "Guard",
                "Master posture breakdown, collar & sleeve control, and high-percentage sweeps.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000001"), "Overhook & Cross-Collar Posture Control", TechniqueCategory.Warmup, "Break partner down using collar feed and high knees", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000002"), "Scissor Sweep & Pendulum Sweep Progression", TechniqueCategory.Technique, "Load partner weight onto shins before kicking across", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000003"), "Flower Sweep Counter to Posture Defense", TechniqueCategory.Technique, "Underhook far leg, pendulum outer leg forcefully", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000004"), "Closed Guard Sweep or Pass Gauntlet", TechniqueCategory.SituationalDrill, "Bottom player must sweep within 90s, top passes", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                "Open Guard Passing: Pressure & Agility (Knee Cut & Torreando)",
                "Passing",
                "Execute dynamic passing angles and crushing knee cut pressure against open guards.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000005"), "Torreando Bullfighter Footwork & Grip Breaks", TechniqueCategory.Technique, "Push pants down, throw legs aside, establish side control", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000006"), "Classic Knee Cut Pass with Underhook & Head Control", TechniqueCategory.Technique, "Slide knee 45 degrees across thigh, pin far shoulder", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000007"), "Knee Cut to Long Step Transition vs Reverse De La Riva", TechniqueCategory.Technique, "Disengage hook, backstep trailing leg, secure collar frame", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000008"), "Passing vs Retention 2-Minute Pressure Rounds", TechniqueCategory.SituationalDrill, "Top tries to establish chest-to-chest pin; bottom retains guard", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000003"),
                "Side Control & North-South: Escapes & Defensive Frames",
                "Escapes",
                "Neutralize crossfaces, establish forearm frames, and safely recover guard.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000009"), "Forearm Framing & Hip Bump Reguarding", TechniqueCategory.Technique, "Keep bottom frame across throat, bridge high, insert near knee", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000010"), "Ghost Escape vs High Crossface", TechniqueCategory.Technique, "Spin out under armpit towards North-South, re-enter turtle or guard", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000011"), "North-South Inversion & Guard Spin Recovery", TechniqueCategory.Technique, "Double frame hips, invert over shoulders, catch butterfly hooks", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000012"), "Pin Survival Sparring (Reset on Tap or Reguard)", TechniqueCategory.SituationalDrill, "Bottom must recover closed or half guard; top scores sub or mount", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000004"),
                "Mount Mastery: Control, High Mount & Submissions",
                "Submissions",
                "Cement mount stabilization, climb to high mount, and execute submission dilemmas.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000013"), "Low Mount Grapevine & Posture Riding", TechniqueCategory.Technique, "Spread hands wide, drive hips down, counter bridge and roll", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000014"), "S-Mount Climb & Armbar Finishing Mechanics", TechniqueCategory.Technique, "Isolate near elbow, slide knee behind head, pinch knees tight", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000015"), "Mounted Cross Collar Choke into Ezekiel Transition", TechniqueCategory.Technique, "Deep first grip behind neck, four-finger cuff sleeve choke", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000016"), "King of Mount Positional Sparring", TechniqueCategory.SituationalDrill, "Top player attempts submission; bottom executes kipping or elbow escape", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000005"),
                "Back Attacks: Seatbelt Control, Turtle Breaks & Rear Naked Chokes",
                "Back Control",
                "Dominate the back position with harness grips, break down turtle, and finish the RNC.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000017"), "Harness / Seatbelt Grips & Body Triangle Alignment", TechniqueCategory.Technique, "Head on choking side shoulder, hide fingers, maintain chest-to-back", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000018"), "Spiral Ride & Clock Choke Break from Turtle", TechniqueCategory.Technique, "Pry far elbow, feed lapel under chin, walk perimeter", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000019"), "Rear Naked Choke (RNC) Hand-Fighting & Rotational Finish", TechniqueCategory.Technique, "Trap defending wrist with hook, knife blade under chin, squeeze lats", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000020"), "Back Escapes vs Submissions Pressure Test", TechniqueCategory.SituationalDrill, "Bottom turns shoulders to mat; top fights to finish choke or armbar", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000006"),
                "Half Guard & Dogfight: Underhooks & Deep Half",
                "Guard",
                "Win underhook battles, rise to dogfight position, and sweep via deep half guard.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000021"), "Knee Shield Framing & Underhook Pummeling", TechniqueCategory.Technique, "Keep collar tie frame, drop knee shield, shoot deep underhook", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000022"), "Dogfight Position: Whizzer Counter & Limp Arm to Back", TechniqueCategory.Technique, "Build to hand, tackle far knee, or slip under whizzer for seatbelt", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000023"), "Deep Half Guard Waiter Sweep & Back Entry", TechniqueCategory.Technique, "Duck under hips, hook far ankle with waiter grip, tip partner backwards", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000024"), "Underhook Battle King of the Mat", TechniqueCategory.SituationalDrill, "Start locked in half guard; fight for underhook and first sweep/pass", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000007"),
                "Guard Retention & Framing: Active Inversion & Recovery",
                "Defense",
                "Build impenetrable guard retention mechanics using granby rolls and collar-bicep frames.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000025"), "Bicep Frame & Hip Shrimping Defensive Matrix", TechniqueCategory.Warmup, "Maintain frame distance, shrimp hips away, insert recovery hook", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000026"), "Granby Roll & Inverted Reguard against North-South", TechniqueCategory.Technique, "Tuck chin, roll across upper shoulders, thread legs back to butterfly", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000027"), "Shin-to-Shin Frame against Heavy Pressure Passing", TechniqueCategory.Technique, "Block hip line with shin, elevate center of gravity, reset guard", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000028"), "Retention Gauntlet: 3-Minute Pass-Proof Challenge", TechniqueCategory.SituationalDrill, "Passer rotates fresh partners every minute; guard player must survive", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000008"),
                "Standing Wrestling & Judo: Clinch, Takedowns & Grip Fighting",
                "Takedowns",
                "Control standing exchanges, win inside collar ties, and execute high-percentage takedowns.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000029"), "Inside Bicep Ties & 2-on-1 Russian Tie Grip Breaks", TechniqueCategory.Technique, "Strip wrists, clamp two hands on tricep, control posture", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000030"), "Single Leg Takedown: Sweep Single & Run the Pipe", TechniqueCategory.Technique, "Level change, drive shoulder into thigh, circle back to dump partner", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000031"), "O-Uchi Gari Inner Leg Reap to Knee Tap Combo", TechniqueCategory.Technique, "Snap head down, hook inner ankle, drive diagonally across hips", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000032"), "Standing Takedown / Guard Pull Live Rounds", TechniqueCategory.SituationalDrill, "First player to score takedown or complete sweep wins the point", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000009"),
                "Butterfly & Dynamic Open Guards: Sweeps & Leg Entries",
                "Guard",
                "Master butterfly hooks, elevation mechanics, arm drags, and dynamic sweeps.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000033"), "Overhook Butterfly Hook Sweep Mechanics", TechniqueCategory.Technique, "Pinch overhook tight, dive head to mat, elevate partner with hook", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000034"), "Arm Drag to Back Take from Butterfly Guard", TechniqueCategory.Technique, "Cross-grip wrist, drag tricep across chest, climb onto opponent back", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000035"), "Butterfly Elevation into Single Leg X Entry", TechniqueCategory.Technique, "Lift with both butterfly hooks, slide under into Single Leg X (SLX)", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000036"), "Butterfly Sparring: Sweep or Flatten Round", TechniqueCategory.SituationalDrill, "Bottom scores sweep or back take; top scores complete pass", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000010"),
                "Knee on Belly & Pin Transitions: Riding & Submissions",
                "Control",
                "Ride heavy knee on belly pressure, react to hip pushes, and spin into near-side armlocks.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000037"), "Knee on Belly Alignment: Collar and Belt Controls", TechniqueCategory.Technique, "Drive far knee across solar plexus, post out kickstand leg", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000038"), "Windshield Wiper Transition to Opposite Side", TechniqueCategory.Technique, "Switch knees over belly as bottom player pushes knee away", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000039"), "Near-Side Spinning Armbar & Baseball Bat Choke", TechniqueCategory.Technique, "Spin on chest when bottom turns in; lock tight armbar or grip lapels", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000040"), "Pin Escapes vs Pin Maintenance Sparring", TechniqueCategory.SituationalDrill, "Top must maintain Knee on Belly or transition to mount; bottom reguards", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000011"),
                "Leg Entanglements: Straight Footlocks & Ashi Garami",
                "Leglocks",
                "Understand Ashi Garami control, straight ankle lock breaking mechanics, and defense.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000041"), "Ashi Garami Positional Control & Hip Clamping", TechniqueCategory.Technique, "Foot on hip, outer foot tucked behind far thigh, pinch knees", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000042"), "Straight Ankle Lock (Achilles) Breaking Mechanics", TechniqueCategory.Technique, "High wrist grip on Achilles, look over shoulder, arch hips forward", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000043"), "The Boot Defense & Standing Ashi Garami Escape", TechniqueCategory.Technique, "Flex foot ('put the boot on'), push foot off hip, step out of entanglement", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000044"), "Ashi Garami Position Battle (Tap or Clear Knee Line)", TechniqueCategory.SituationalDrill, "Start inside Ashi Garami; attacker hunts straight lock, defender escapes", null, 4, 1)
                }).Value,

            CurriculumTheme.Create(
                Guid.Parse("10000000-0000-0000-0000-000000000012"),
                "Curriculum Review: Situational Sparring Gauntlet & Belt Prep",
                "Integration",
                "Comprehensive yearly review testing technical fluency across all 11 prior positions.",
                new[]
                {
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000045"), "Yearly Flow Drill: Closed Guard -> Pass -> Mount -> Back", TechniqueCategory.Warmup, "Continuous flowing transitions without resistance across 4 core positions", null, 1, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000046"), "Submission Defense & Late Stage Escape Review", TechniqueCategory.Technique, "Defend deep armbar, hitchhiker escape, triangle posture recovery", null, 2, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000047"), "Iron Man Gauntlet: 5-Minute Technical Endurance Rounds", TechniqueCategory.SituationalDrill, "High-tempo rounds testing technical clarity under fatigue", null, 3, 1),
                    new ThemeDrillTemplate(Guid.Parse("20000000-0000-0000-0000-000000000048"), "Belt Promotion Simulation & Full Match Scenarios", TechniqueCategory.SituationalDrill, "IBJJF rules match simulation with points and advantages", null, 4, 1)
                }).Value
        };
    }
}
