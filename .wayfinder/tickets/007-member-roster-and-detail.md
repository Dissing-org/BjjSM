# Ticket 007: Member Roster Page, Creation Form & Detail View

**Labels**: `wayfinder:prototype`, `blazor-ui`  
**Parent**: [Wayfinder Frontend Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP_FRONTEND.md)  
**Status**: Open (Claimable)

## Question

How should the Member Roster (`/members`), New Student Creation Modal, and Member Detail view (`/members/{id}`) be designed to deliver an authentic, high-contrast BJJ academy management experience?

### Tasks:
1. Update `NavMenu.razor` and `MainLayout.razor` with Japanese martial arts brand theme navigation.
2. Build `MembersPage.razor`:
   - Member search bar & filter by belt rank.
   - Table of members with `<BjjBeltBadge />`, role tags, and action buttons.
3. Build `CreateMemberModal.razor`:
   - First name, last name, email, role checkboxes, initial belt ladder & stripe dropdown.
4. Build `MemberDetailPage.razor`:
   - Active rank header, graduation timeline, and interactive promotion modal.
