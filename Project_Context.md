# CityLink Initiatives — Project Context

## What this project is

CityLink Initiatives is an ASP.NET Core MVC web application built for a fictional Western Australian local government. It is a community portal that lets residents browse local events and services, submit feedback, and book services or register for events. A separate staff portal allows authenticated council employees to manage all site content.

This is a TAFE assignment project (AWD AT2, 2026).

---

## Technology stack

- **Framework:** ASP.NET Core MVC (.NET 8/9)
- **Language:** C#
- **Database:** SQLite via Entity Framework Core
- **Authentication:** ASP.NET Core Identity (cookie-based)
- **Frontend:** Razor views, jQuery (AJAX), vanilla CSS (no Tailwind, no Bootstrap)
- **Content system:** XML-serialised ViewModels stored in the database, deserialised at runtime to drive page content

---

## Project structure

```
CityLink-SCP/
├── Common/
│   ├── QueryParameters.cs              — base paginated query class
│   ├── QueryParameterClasses.cs        — one query class per DB model
│   ├── QueryExtensions.cs              — ApplyQuery() extension methods per model
│   ├── RestrictedQueryModelBinder.cs   — custom binder + [QueryController] / [UserRole] attributes
│   ├── RestrictedQueryModelBinderProvider.cs
│   └── DbActionResult.cs              — generic success/failure wrapper
├── Controllers/
│   ├── HomeController.cs               — public-facing pages
│   ├── AdminController.cs              — staff dashboard (XML config + partial view returns)
│   └── AdminAuthController.cs          — staff login/logout
│   └── NavigationController.cs
├── Database/
│   ├── CityLinksContext.cs             — EF Core DbContext (inherits IdentityDbContext)
│   └── DbInitialiser.cs               — seeds DB on first run via UserManager
├── DbModels/
│   ├── ApplicationUser.cs              — extends IdentityUser 
│   ├── ApplicationStaff.cs             — extends ApplicationUser 
│   ├── Event.cs
│   ├── EventRegistration.cs            — composite PK (UserId, EventId)
│   ├── Service.cs
│   ├── ServiceBooking.cs               — composite PK (UserId, ServiceId)
│   ├── Feedback.cs
│   └── XmlConfig.cs
├── Extensions/
│   └── DbModelExtensions.cs            — ToCardViewModel(), ToViewModel() helpers
├── Models/                             — XML ViewModels (FooterModel, FAQViewModel, etc.)
│   ├── BookServiceViewModel.cs
│   ├── CardViewModel.cs
│   ├── ErrorViewModel.cs
│   ├── EventsViewModel.cs
│   ├── FAQViewModel.cs
│   ├── FooterModel.cs
│   ├── IXmlViewModel.cs
│   ├── ServicesViewModel.cs
│   └── XmlConfigDto.cs
├── PageModels/
│   ├── AdminDtos.cs
│   ├── AdminIndexViewModel.cs
│   ├── IndexViewModel.cs
│   └── ProfileViewModel.cs
├── Services/
│   ├── DatabaseService.cs              — booking/registration helpers
│   └── XmlConfigService.cs            — XML serialise/deserialise/validate
├── ViewModels/
│   └── ViewModels.cs
├── Views/
│   ├── Admin/
│   │   ├── Index.cshtml               — staff dashboard shell
│   │   ├── Login.cshtml               — staff login page (standalone, no layout)
│   │   ├── _AdminNav.cshtml           — top bar with user name + sign out
│   │   ├── _XmlEditorPanel.cshtml     — slide-in XML editor drawer
│   │   ├── _PreviewPanel.cshtml       — centred XML preview overlay
│   │   ├── _XmlConfigsSection.cshtml
│   │   ├── _EventsSection.cshtml
│   │   ├── _ServicesSection.cshtml
│   │   ├── _FeedbackSection.cshtml
│   │   ├── _UsersSection.cshtml
│   │   ├── _StaffSection.cshtml
│   │   ├── _EventRegistrationsSection.cshtml
│   │   └── _ServiceBookingsSection.cshtml
│   └── Home/
│       └── Profile.cshtml             — user/staff profile page
├── wwwroot/
│   ├── css/
│   │   ├── index.css                  — site-wide variables and styles
│   │   └── admin.css + admin-additions.css
│   └── js/
│       └── admin.js                   — XML config AJAX + shared panel helpers
└── Program.cs
```

---

## Database models

### ApplicationUser (inherits IdentityUser)
The base user type. Identity manages auth. Custom fields: `First_Name`, `Last_Name`, `Address`, `Phone_Number` (proxies to `IdentityUser.PhoneNumber`). Primary key is a **string GUID** (Identity default).

### ApplicationStaff (inherits ApplicationUser)
Staff member type. Adds `JobTitle` (their department role, e.g. "IT Admin"). Stored in the same table as `ApplicationUser` via **TPH (Table-Per-Hierarchy)** with a `UserType` discriminator column. Assigned the Identity role `"Staff"` which gates the admin area.

### Event
`Id (int)`, `Title`, `Description`, `Location`, `Cost (double)`, `Max_Capcity (int)` *(typo in original, preserved)*, `Start_Date_Time`, `End_Date_Time`, `StaffId (string FK → ApplicationStaff)`.

### Service
`Id (int)`, `Title`, `Description`, `Location`, `Cost (double)`, `Available_Start_Time (TimeOnly)`, `Available_End_Time (TimeOnly)`, `StaffId (string FK → ApplicationStaff)`.

### EventRegistration
Composite PK `(UserId string, EventId int)`. Fields: `NumberOfAttendees`, `TotalCost`. One registration per user per event.

### ServiceBooking
Composite PK `(UserId string, ServiceId int)`. Fields: `Start_Time`, `End_Time`, `TotalCost`. One booking slot per user per service (by design).

### Feedback
`Id`, `Message`, `Status (enum: Pending/InProgress/Resolved/Closed)`, `Resolution_Message`, `CreatedAt`, `ResolvedAt`, `UserId (string FK → ApplicationUser)`, `StaffId (string FK → ApplicationStaff)`.

### XmlConfig
Stores serialised XML for page content. Fields: `Id`, `Type` (ViewModel type name), `XmlContent`, `Version`, `IsActive`, `UploadedAt`, `Label`, `StaffId (string FK → ApplicationStaff)`. Multiple versions per type; only one `IsActive` at a time.

---

## Authentication and authorisation

- **ASP.NET Core Identity** with a single cookie scheme.
- Login path: `/Admin/Login` — staff only.
- `AdminController` is protected with `[Authorize(Roles = "Staff")]`.
- Public pages (HomeController) are open — no user-facing auth yet.
- `Profile` page at `/Home/Profile` is accessible to anyone; shows a guest prompt if not signed in, otherwise shows the signed-in user's details and their bookings/registrations.
- Seed credentials (first run): `admin@citylink.wa.gov.au` / `Admin1234!`

---

## The XML content system

Page content (hero text, FAQ questions, footer links, event/service cards) is driven by XML stored in `XML_Configurations`. The flow is:

1. A ViewModel class (e.g. `FooterModel`, `FAQViewModel`) is decorated with `[XmlRoot]` and registered in `XmlConfigService.GetViewModelType()`.
2. Staff upload XML via the admin editor. `XmlConfigService.Validate()` deserialises it as a check.
3. On save, the previous active version for that type is deactivated; the new one is saved as `IsActive = true`.
4. At page render, `XmlConfigService.GetActive<T>()` fetches and deserialises the active record.
5. If no active record exists, controllers fall back to static XML files in `XML/`.

---

## Query system

Every DB model has a corresponding query parameter class in `QueryParameterClasses.cs`. All inherit from `QueryParameters` (base class in `Common/`).

### Base class: `QueryParameters`
```csharp
public int    Size      { get; set; } = 10;   // capped at 100
public int    Page      { get; set; } = 1;
public string SortBy    { get; set; } = "Id";
public string SortOrder { get; set; } = "asc"; // only "asc" or "desc" accepted
```

### Query parameter classes

| Class | Key filter properties |
|---|---|
| `EventQueryParams` | `EventId`, `SearchTerm` (title/desc), `Location`, `MinCost`, `MaxCost`, `TicketsAvaliable`, `From`, `To`, `StaffId`* |
| `ServiceQueryParams` | `ServiceId`, `SearchTerm` (title/desc/location), `From`, `To` (TimeOnly), `StaffId`* |
| `EventRegistrationQueryParams` | `MinAttendees`, `MaxAttendees`, `MinCost`, `MaxCost`, `UserId`*, `EventId`* |
| `ServiceBookingQueryParams` | `MinCost`, `MaxCost`, `From`, `To`, `UserId`*, `ServiceId`* |
| `FeedBackQueryParams` | `FeedBackId`, `SearchTerm` (message/resolution), `Status` (enum), `CreatedAfter/Before`, `ResolvedAfter/Before`, `UserId`*, `StaffId`* |
| `XmlConfigQueryParams` | `XmlConfigId`, `Label`, `Type`, `Version`, `IsActive`, `UploadedAfter/Before`, `StaffId`* |
| `UserQueryParams` | `UserId`, `FirstName`, `LastName`, `Email`, `PhoneNumber`, `Address` |

*\* Restricted properties — see below.*

### Restricted properties: `[QueryController]` and `[UserRole]`

Properties marked with these custom attributes are **silently skipped by the model binder** unless the conditions are met:

```csharp
[UserRole("Admin")]        // only binds if the request user is in the "Admin" role
[QueryController("Admin")] // only binds if the action is on the AdminController
public string? StaffId { get; set; }
```

This is enforced by `RestrictedQueryModelBinder`, registered via `RestrictedQueryModelBinderProvider`. It activates for any type that inherits `QueryParameters`. If a condition fails, the property is left `null` — the filter is simply not applied. No error is thrown.

### `QueryExtensions` — `ApplyQuery()`

Each model has an `IQueryable<T>.ApplyQuery(TQuery query)` extension method in `QueryExtensions.cs`. These apply all non-null filters, then sorting, then `Skip`/`Take` for pagination. They are called directly on `_dbService._context.SomeSet` inside controller endpoints.

---

## Admin dashboard

### Structure
Single-page dashboard at `/Admin/Index`. Built entirely from Razor partial views. The dashboard shell (`Index.cshtml`) renders a fixed set of section partials on page load, each receiving data from `AdminIndexViewModel`:

```
_AdminNav                    — sign-out bar (top of page)
_XmlEditorPanel              — slide-in XML editor drawer (rendered once)
_XmlConfigsSection
_EventsSection
_ServicesSection
_FeedbackSection
_EventRegistrationsSection
_ServiceBookingsSection
_UsersSection
_StaffSection
```

### Data flow pattern

The intended pattern for all CRUD and query operations is **partial view replacement** — the server renders HTML and the client swaps it in:

```
user interaction
  → fetch/AJAX to a controller endpoint (with query params)
  → controller runs ApplyQuery(), returns PartialView(...)
  → client receives HTML
  → target element.innerHTML = responseHTML -> !parseHtml so events are binded properly!
```

This keeps client-side logic minimal. No JSON parsing, no client-side template rendering. The JS only needs to know which element to replace and which endpoint to call.

### XML config section — currently implemented AJAX

The XML config section is the only part currently wired with full AJAX. It uses jQuery and the existing slide-in drawer pattern:

| Action | Endpoint | Method | Returns |
|---|---|---|---|
| Load XML for editing | `GET /Admin/LoadXml?id=` | GET | Raw XML string |
| Load empty template | `GET /Admin/LoadTemplate?typeName=` | GET | Raw XML string |
| Preview rendered output | `GET /Admin/GetXmlPreview?id=` | GET | Partial view HTML |
| Save new version | `POST /Admin/UploadXmlConfig` | POST | OK / BadRequest |
| Activate a version | `POST /Admin/ActivateVersion?recordId=` | POST | OK / NotFound |
| Get version history | `GET /Admin/GetConfigHistory?typeName=` | GET | JSON array |

All other sections (events, services, feedback, users, staff, bookings, registrations) have their controller endpoint regions stubbed (`#region Events EndPoints`, etc.) and are pending implementation using the query + partial view pattern above.

---

## CSS design system

All styles use CSS custom properties defined in `index.css`. Key variables:

```css
--green: #1B3A2F        /* primary brand colour */
--cream: #F5F2EC        /* background tint */
--mist:  #E8E4DC        /* borders */
--font-heading: 'Playfair Display', serif
--font-body:    'DM Sans', sans-serif
--radius-lg, --radius-sm, --radius-pill
--shadow, --shadow-hover
--ease: 0.25s ease
```

`admin.css` extends `index.css` using the same variables. `admin-additions.css` appends CRUD panel, form grid, and nav bar styles.

---