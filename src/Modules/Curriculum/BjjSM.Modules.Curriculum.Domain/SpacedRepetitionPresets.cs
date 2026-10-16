namespace BjjSM.Modules.Curriculum.Domain;

public static class SpacedRepetitionPresets
{
    public static readonly Guid StandardTemplateId = Guid.Parse("70000000-0000-0000-0000-000000000001");
    public static readonly Guid PositionalCounterTemplateId = Guid.Parse("70000000-0000-0000-0000-000000000002");
    public static readonly Guid SubmissionMechanicsTemplateId = Guid.Parse("70000000-0000-0000-0000-000000000003");

    public static IReadOnlyList<CardTemplate> CreateDefaultTemplates(Guid academyId)
    {
        var standard = CardTemplate.Create(
            StandardTemplateId,
            academyId,
            "Standard Flashcard",
            "General-purpose BJJ concept, rule, or terminology prompt and answer.",
            [
                new CardFieldDefinition(Guid.Parse("71000000-0000-0000-0000-000000000001"), "Prompt / Question", CardFieldSide.Front, CardFieldType.Text, true, 1),
                new CardFieldDefinition(Guid.Parse("71000000-0000-0000-0000-000000000002"), "Answer", CardFieldSide.Back, CardFieldType.MultilineText, true, 2),
                new CardFieldDefinition(Guid.Parse("71000000-0000-0000-0000-000000000003"), "Coaching Notes", CardFieldSide.Back, CardFieldType.MultilineText, false, 3)
            ]).Value;

        var positional = CardTemplate.Create(
            PositionalCounterTemplateId,
            academyId,
            "BJJ Positional Reaction & Counter",
            "Structured cue-reaction card for positional recognition, grip fighting, and immediate counter-offense.",
            [
                new CardFieldDefinition(Guid.Parse("72000000-0000-0000-0000-000000000001"), "Position & Alignment", CardFieldSide.Front, CardFieldType.Text, true, 1),
                new CardFieldDefinition(Guid.Parse("72000000-0000-0000-0000-000000000002"), "Opponent Reaction / Trigger", CardFieldSide.Front, CardFieldType.MultilineText, true, 2),
                new CardFieldDefinition(Guid.Parse("72000000-0000-0000-0000-000000000003"), "Primary Counter Technique", CardFieldSide.Back, CardFieldType.Text, true, 3),
                new CardFieldDefinition(Guid.Parse("72000000-0000-0000-0000-000000000004"), "Key Grips & Wedges", CardFieldSide.Back, CardFieldType.MultilineText, true, 4),
                new CardFieldDefinition(Guid.Parse("72000000-0000-0000-0000-000000000005"), "Reference Video URL", CardFieldSide.Back, CardFieldType.VideoUrl, false, 5)
            ]).Value;

        var submission = CardTemplate.Create(
            SubmissionMechanicsTemplateId,
            academyId,
            "Submission Mechanics Checklist",
            "High-percentage finishing details, control wedges, and secondary follow-ups when opponent defends.",
            [
                new CardFieldDefinition(Guid.Parse("73000000-0000-0000-0000-000000000001"), "Submission & Entry", CardFieldSide.Front, CardFieldType.Text, true, 1),
                new CardFieldDefinition(Guid.Parse("73000000-0000-0000-0000-000000000002"), "Defensive Obstacle", CardFieldSide.Front, CardFieldType.Text, false, 2),
                new CardFieldDefinition(Guid.Parse("73000000-0000-0000-0000-000000000003"), "Breaking Mechanics & Control Wedges", CardFieldSide.Back, CardFieldType.MultilineText, true, 3),
                new CardFieldDefinition(Guid.Parse("73000000-0000-0000-0000-000000000004"), "Follow-Up Transition", CardFieldSide.Back, CardFieldType.MultilineText, false, 4)
            ]).Value;

        return [standard, positional, submission];
    }

    public static IReadOnlyList<StudyDeck> CreateStarterDecks(Guid academyId, IReadOnlyList<CardTemplate> templates)
    {
        var now = DateTime.UtcNow;
        var positionalTemplate = templates.FirstOrDefault(t => t.Id == PositionalCounterTemplateId) ?? templates.First();
        var submissionTemplate = templates.FirstOrDefault(t => t.Id == SubmissionMechanicsTemplateId) ?? templates.Last();

        var guardDeck = StudyDeck.Create(
            Guid.Parse("80000000-0000-0000-0000-000000000001"),
            academyId,
            positionalTemplate.Id,
            "Guard Retention & Open Guard Counters",
            "Decision trees for knee shield, De La Riva, and late-stage guard retention against headquarters and toreando passing.",
            "Guard Retention",
            DeckSchedulingOptions.Default()).Value;

        AddPositionalCard(
            guardDeck,
            positionalTemplate,
            "De La Riva Guard (Bottom)",
            "Top passer stuffs the non-DLR leg and drives knee inward into Headquarters position.",
            "Technical Unbalance to Single Leg X / X-Guard",
            "Switch collar grip to far sleeve or pant cuff, pummel outside foot onto hip, extend DLR hook overhead to float passer's weight.",
            "https://youtube.com",
            ["guard", "dlr", "headquarters"],
            now);

        AddPositionalCard(
            guardDeck,
            positionalTemplate,
            "Knee Shield Half Guard (Z-Guard)",
            "Passer weaves arm through the shield and sprawls hips back to smash the legs flat.",
            "High Knee Shield Frame & Shoulder Crunch Sweep",
            "Reinforce top knee frame with elbow-to-knee connection, pummel underhook or overhook-pinch on weaving arm, hip escape to re-center angle.",
            null,
            ["half-guard", "knee-shield", "retention"],
            now);

        AddPositionalCard(
            guardDeck,
            positionalTemplate,
            "Late-Stage Open Guard (Supine)",
            "Opponent secures double pant grips and initiates a fast Toreando bullfighter pass to the north-south angle.",
            "High-Leg Over & Self-Frame Granby Recovery",
            "Anchor bottom elbow inside hip line, C-grip own shin or pant leg to prevent chest-to-chest contact, lasso high leg over passer's far shoulder.",
            null,
            ["retention", "toreando", "granby"],
            now);

        AddPositionalCard(
            guardDeck,
            positionalTemplate,
            "Closed Guard Offense",
            "Opponent postures up with stiff frames on the hips and steps one knee up into combat base.",
            "Kuzushi Hip Bump to Kimberley / Guillotine / Triangle",
            "Swim inside wrist control, post on hand as opponent leans back, explode hips into their chest or snap down into high-elbow guillotine.",
            null,
            ["closed-guard", "dilemma", "sweeps"],
            now);

        var subDeck = StudyDeck.Create(
            Guid.Parse("80000000-0000-0000-0000-000000000002"),
            academyId,
            submissionTemplate.Id,
            "High-Percentage Submissions & Finishing Details",
            "Mechanical checklists for Rear Naked Choke, Juji-Gatame Armbar, Triangle Strangle, and Inside Heel Hook.",
            "Submissions",
            DeckSchedulingOptions.Default()).Value;

        AddSubmissionCard(
            subDeck,
            submissionTemplate,
            "Rear NakedStrangle (Mata Leão) from Back Control",
            "Defender tucks chin tightly and two-on-ones the choking forearm.",
            "Use straight-jacket system to trap secondary arm with bottom leg; blade choking wrist under mandible line, lock bicep grip, expand chest and draw choking elbow back in a rotational arc.",
            "Switch to Mandible Strangle or transition to dorsal Kimora / Armbar from back mount.",
            ["back-attack", "rnc", "strangle"],
            now);

        AddSubmissionCard(
            subDeck,
            submissionTemplate,
            "Juji-Gatame (Armbar) from Top / Mount",
            "Defender locks a Rear-Naked-Style figure-four grip to prevent arm extension.",
            "Pin knees tightly together with heels flared down; thread Kimberley grip through defender's elbow pit, hug trapped wrist to your sternum, and break towards the pinky finger before extending hips.",
            "Mount Triangle or Back Take as defender rolls onto their side.",
            ["armbar", "mount", "grip-break"],
            now);

        AddSubmissionCard(
            subDeck,
            submissionTemplate,
            "Front Triangle Choke from Guard",
            "Opponent postures tall and hides the far shoulder outside the strangle.",
            "Control own shin before unlocking ankles, post free foot on hip, cut a 90-degree perpendicular angle until you see opponent's ear, lock figure-four over the Achilles tendon and flare knees.",
            "Transition directly to Juji-Gatame or Omoplata sweep.",
            ["triangle", "guard", "strangle"],
            now);

        AddSubmissionCard(
            subDeck,
            submissionTemplate,
            "Cross Ashi (Saddle) Inside Heel Hook",
            "Opponent attempts to boot the foot and hide the heel against the mat.",
            "Secure knee-line clamp with triangulated legs, use wrist bone beneath calcaneus (watch-strap grip), curl hamstring to expose the heel, and bridge hips through the knee ligament line.",
            "Aoki Lock or transition to Cloverleaf / backside 50-50.",
            ["leg-locks", "saddle", "heel-hook"],
            now);

        return [guardDeck, subDeck];
    }

    private static void AddPositionalCard(
        StudyDeck deck,
        CardTemplate template,
        string position,
        string reaction,
        string counter,
        string mechanics,
        string? videoUrl,
        string[] tags,
        DateTime now)
    {
        var fields = template.Fields.OrderBy(f => f.SortOrder).ToList();
        var values = new List<CardFieldValue>
        {
            new(Guid.NewGuid(), fields[0].Id, fields[0].Name, fields[0].Side, fields[0].FieldType, position, 1),
            new(Guid.NewGuid(), fields[1].Id, fields[1].Name, fields[1].Side, fields[1].FieldType, reaction, 2),
            new(Guid.NewGuid(), fields[2].Id, fields[2].Name, fields[2].Side, fields[2].FieldType, counter, 3),
            new(Guid.NewGuid(), fields[3].Id, fields[3].Name, fields[3].Side, fields[3].FieldType, mechanics, 4),
            new(Guid.NewGuid(), fields[4].Id, fields[4].Name, fields[4].Side, fields[4].FieldType, videoUrl ?? string.Empty, 5)
        };
        deck.AddCard(Guid.NewGuid(), template.Id, tags, values, now);
    }

    private static void AddSubmissionCard(
        StudyDeck deck,
        CardTemplate template,
        string entry,
        string obstacle,
        string mechanics,
        string followUp,
        string[] tags,
        DateTime now)
    {
        var fields = template.Fields.OrderBy(f => f.SortOrder).ToList();
        var values = new List<CardFieldValue>
        {
            new(Guid.NewGuid(), fields[0].Id, fields[0].Name, fields[0].Side, fields[0].FieldType, entry, 1),
            new(Guid.NewGuid(), fields[1].Id, fields[1].Name, fields[1].Side, fields[1].FieldType, obstacle, 2),
            new(Guid.NewGuid(), fields[2].Id, fields[2].Name, fields[2].Side, fields[2].FieldType, mechanics, 3),
            new(Guid.NewGuid(), fields[3].Id, fields[3].Name, fields[3].Side, fields[3].FieldType, followUp, 4)
        };
        deck.AddCard(Guid.NewGuid(), template.Id, tags, values, now);
    }
}
