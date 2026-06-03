# Project Structure: CityLink-SCP

**Generated:** 2026-06-03 05:27:21

---

## File Structure

```
CityLink-SCP/
└── CityLink-SCP/
    ├── Common/
    │   ├── DbActionResult.cs
    │   ├── QueryExtensions.cs
    │   ├── QueryParameterClasses.cs
    │   ├── QueryParameters.cs
    │   ├── RestrictedQueryModelBinder.cs
    │   └── RestrictedQueryModelBinderProvider.cs
    ├── Controllers/
    │   ├── AdminAuthController.cs
    │   ├── AdminController.cs
    │   ├── HomeController.cs
    │   └── NavigationController.cs
    ├── Database/
    │   ├── CityLinksContext.cs
    │   └── DbInitialiser.cs
    ├── DbModels/
    │   ├── ApplicationStaff.cs
    │   ├── ApplicationUser.cs
    │   ├── Event.cs
    │   ├── EventRegistration.cs
    │   ├── Feedback.cs
    │   ├── Service.cs
    │   ├── ServiceBooking.cs
    │   └── XmlConfig.cs
    ├── Extensions/
    │   └── DbModelExtensions.cs
    ├── Models/
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
    │   ├── DatabaseService.cs
    │   └── XmlConfigService.cs
    ├── ViewModels/
    │   └── ViewModels.cs
    └── Program.cs
```

---

## Code Structure

### Namespace: `<global namespace>`

#### Class: `EventQueryParams`

**Modifiers:** public
**Inherits:** QueryParameters
**File:** `Common\QueryParameterClasses.cs`

**Properties:**
- `public int? EventId { get; set; }`
- `public string? SearchTerm { get; set; }`
- `public string? Location { get; set; }`
- `public decimal? MinCost { get; set; }`
- `public decimal? MaxCost { get; set; }`
- `public bool? TicketsAvaliable { get; set; }`
- `public DateTime? From { get; set; }`
- `public DateTime? To { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public string? StaffId { get; set; }`

---

#### Class: `EventRegistrationQueryParams`

**Modifiers:** public
**Inherits:** QueryParameters
**File:** `Common\QueryParameterClasses.cs`

**Properties:**
- `public int? MinAttendees { get; set; }`
- `public int? MaxAttendees { get; set; }`
- `public double? MinCost { get; set; }`
- `public double? MaxCost { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public string? UserId { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public int? EventId { get; set; }`

---

#### Class: `FeedBackQueryParams`

**Modifiers:** public
**Inherits:** QueryParameters
**File:** `Common\QueryParameterClasses.cs`

**Properties:**
- `public int? FeedBackId { get; set; }`
- `public string? SearchTerm { get; set; }`
- `public FeedbackStatus? Status { get; set; }`
- `public DateTime? CreatedAfter { get; set; }`
- `public DateTime? CreatedBefore { get; set; }`
- `public DateTime? ResolvedAfter { get; set; }`
- `public DateTime? ResolvedBefore { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public string? UserId { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public string? StaffId { get; set; }`

---

#### Class: `ServiceBookingQueryParams`

**Modifiers:** public
**Inherits:** QueryParameters
**File:** `Common\QueryParameterClasses.cs`

**Properties:**
- `public double? MinCost { get; set; }`
- `public double? MaxCost { get; set; }`
- `public DateTime? From { get; set; }`
- `public DateTime? To { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public string? UserId { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public int? ServiceId { get; set; }`

---

#### Class: `ServiceQueryParams`

**Modifiers:** public
**Inherits:** QueryParameters
**File:** `Common\QueryParameterClasses.cs`

**Properties:**
- `public int? ServiceId { get; set; }`
- `public string? SearchTerm { get; set; }`
- `public TimeOnly? From { get; set; }`
- `public TimeOnly? To { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public string? StaffId { get; set; }`

---

#### Class: `UserQueryParams`

**Modifiers:** public
**Inherits:** QueryParameters
**File:** `Common\QueryParameterClasses.cs`

**Properties:**
- `public string? UserId { get; set; }`
- `public string? FirstName { get; set; }`
- `public string? LastName { get; set; }`
- `public string? Email { get; set; }`
- `public string? PhoneNumber { get; set; }`
- `public string? Address { get; set; }`

---

#### Class: `XmlConfigQueryParams`

**Modifiers:** public
**Inherits:** QueryParameters
**File:** `Common\QueryParameterClasses.cs`

**Properties:**
- `public int? XmlConfigId { get; set; }`
- `public string? Label { get; set; }`
- `public string? Type { get; set; }`
- `public string? Version { get; set; }`
- `public bool? IsActive { get; set; }`
- `public DateTime? UploadedAfter { get; set; }`
- `public DateTime? UploadedBefore { get; set; }`
- `[CityLink_SCP.Common.UserRoleAttribute, CityLink_SCP.Common.QueryControllerAttribute] public string? StaffId { get; set; }`

---

### Namespace: `CityLink_SCP`

#### Class: `Program`

**Modifiers:** public
**File:** `Program.cs`

**Methods:**
- `public static async Task Main(string[] args)`

---

### Namespace: `CityLink_SCP.Common`

#### Class: `DbActionResult`

**Modifiers:** public
**File:** `Common\DbActionResult.cs`

**Properties:**
- `public bool Success { get; set; }`
- `public string Message { get; set; }`

---

#### Class: `DbActionResult<T>`

**Modifiers:** public
**File:** `Common\DbActionResult.cs`

**Properties:**
- `public bool Success { get; set; }`
- `public string Message { get; set; }`
- `public T? Data { get; set; }`

---

#### Class: `QueryControllerAttribute`

**Modifiers:** public
**Inherits:** Attribute
**Attributes:** AttributeUsage
**File:** `Common\RestrictedQueryModelBinder.cs`

**Properties:**
- `public string ControllerName { get; }`

---

#### Class: `QueryExtensions`

**Modifiers:** public static
**File:** `Common\QueryExtensions.cs`

**Methods:**
- `public static IQueryable<Event> ApplyQuery(IQueryable<Event> source, EventQueryParams query)`
- `public static IQueryable<EventRegistration> ApplyQuery(IQueryable<EventRegistration> source, EventRegistrationQueryParams query)`
- `public static IQueryable<Service> ApplyQuery(IQueryable<Service> source, ServiceQueryParams query)`
- `public static IQueryable<ServiceBooking> ApplyQuery(IQueryable<ServiceBooking> source, ServiceBookingQueryParams query)`
- `public static IQueryable<Feedback> ApplyQuery(IQueryable<Feedback> source, FeedBackQueryParams query)`
- `public static IQueryable<XmlConfig> ApplyQuery(IQueryable<XmlConfig> source, XmlConfigQueryParams query)`
- `public static IQueryable<ApplicationUser> ApplyQuery(IQueryable<ApplicationUser> source, UserQueryParams query)`

---

#### Class: `QueryParameters`

**Modifiers:** public
**File:** `Common\QueryParameters.cs`

**Fields:**
- `private static const int _maxSize`
- `private int _size`
- `private string _sortOrder`

**Properties:**
- `public int Size { get; set; }`
- `public int Page { get; set; }`
- `public string SortBy { get; set; }`
- `public string SortOrder { get; set; }`

---

#### Class: `RestrictedQueryModelBinder`

**Modifiers:** public
**Inherits:** IModelBinder
**File:** `Common\RestrictedQueryModelBinder.cs`

**Methods:**
- `public Task BindModelAsync(ModelBindingContext bindingContext)`

---

#### Class: `RestrictedQueryModelBinderProvider`

**Modifiers:** public
**Inherits:** IModelBinderProvider
**File:** `Common\RestrictedQueryModelBinderProvider.cs`

**Methods:**
- `public IModelBinder? GetBinder(ModelBinderProviderContext context)`

---

#### Class: `UserRoleAttribute`

**Modifiers:** public
**Inherits:** Attribute
**Attributes:** AttributeUsage
**File:** `Common\RestrictedQueryModelBinder.cs`

**Properties:**
- `public string Role { get; }`

---

### Namespace: `CityLink_SCP.Controllers`

#### Class: `AdminAuthController`

**Modifiers:** public
**Inherits:** Controller
**Attributes:** Route
**File:** `Controllers\AdminAuthController.cs`

**Fields:**
- `private readonly SignInManager<ApplicationUser> _signInManager`
- `private readonly UserManager<ApplicationUser> _userManager`
- `private readonly ILogger<AdminAuthController> _logger`

**Methods:**
- `[HttpGet, AllowAnonymous] public IActionResult Login(string? returnUrl = null)`
- `[HttpPost, AllowAnonymous, ValidateAntiForgeryToken] public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)`
- `[HttpPost, Authorize, ValidateAntiForgeryToken] public async Task<IActionResult> Logout()`
- `private IActionResult RedirectToLocal(string? returnUrl)`

---

#### Class: `AdminController`

**Modifiers:** public
**Inherits:** Controller
**Attributes:** Authorize
**File:** `Controllers\AdminController.cs`

**Fields:**
- `private readonly XmlConfigService _xmlService`
- `private readonly DatabaseService _dbService`
- `private readonly UserManager<ApplicationUser> _userManager`
- `private readonly ILogger<AdminController> _logger`

**Methods:**
- `public IActionResult Index()`
- `[HttpGet] public IActionResult LoadXml(int id)`
- `[HttpGet] public IActionResult LoadTemplate(string typeName)`
- `[HttpGet] public async Task<IActionResult> GetXmlPreview(int id)`
- `[HttpPost] public async Task<IActionResult> UploadXmlConfig([FromForm] XmlConfigDto xmlConfig)`
- `[HttpPost] public async Task<IActionResult> ActivateVersion(int recordId)`
- `[HttpGet] public IActionResult GetConfigHistory(string typeName)`

---

#### Class: `HomeController`

**Modifiers:** public
**Inherits:** Controller
**File:** `Controllers\HomeController.cs`

**Fields:**
- `private readonly ILogger<HomeController> _logger`
- `private readonly DatabaseService _dbService`
- `private readonly XmlConfigService _xmlService`
- `private readonly UserManager<ApplicationUser> _userManager`

**Methods:**
- `public IActionResult Index()`
- `public IActionResult WhatsOn()`
- `public IActionResult Services()`
- `public IActionResult News()`
- `public IActionResult Events()`
- `public IActionResult BookEvent()`
- `public IActionResult BookService()`
- `public IActionResult ContactUs()`
- `public IActionResult Signin()`
- `public IActionResult Login()`
- `public async Task<IActionResult> Profile()`
- `[ResponseCache] public IActionResult Error()`
- `private IndexViewModel GetIndexViewModel()`
- `private EventsViewModel GetEventsDefault()`
- `private ServicesViewModel GetServicesDefault()`
- `private FAQViewModel GetFAQsDefault()`

---

#### Class: `NavigationController`

**Modifiers:** public
**File:** `Controllers\NavigationController.cs`

---

### Namespace: `CityLink_SCP.Database`

#### Class: `CityLinksContext`

**Modifiers:** public
**Inherits:** IdentityDbContext<ApplicationUser>
**File:** `Database\CityLinksContext.cs`

**Properties:**
- `public DbSet<ApplicationUser> AppUsers { get; }`
- `public DbSet<ApplicationStaff> AppStaff { get; }`
- `public DbSet<XmlConfig> XML_Configurations { get; }`
- `public DbSet<EventRegistration> EventRegistrations { get; }`
- `public DbSet<ServiceBooking> ServiceBookings { get; }`
- `public DbSet<Event> Events { get; }`
- `public DbSet<Service> Services { get; }`
- `public DbSet<Feedback> Feedbacks { get; }`
- `public string DbPath { get; private set; }`

**Methods:**
- `protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)`
- `protected override void OnModelCreating(ModelBuilder modelBuilder)`

---

#### Class: `DbInitialiser`

**Modifiers:** public static
**File:** `Database\DbInitialiser.cs`

**Methods:**
- `public static async Task InitialiseAsync(CityLinksContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, DatabaseService dbService, XmlConfigService xmlService)`
- `public static string GetFriendlyName(Type type)`

---

### Namespace: `CityLink_SCP.DbModels`

#### Class: `ApplicationStaff`

**Modifiers:** public
**Inherits:** ApplicationUser
**File:** `DbModels\ApplicationStaff.cs`

**Properties:**
- `public string JobTitle { get; set; }`
- `public ICollection<XmlConfig> XML_Configurations { get; set; }`
- `public ICollection<Service> Services { get; set; }`
- `public ICollection<Event> Events { get; set; }`
- `public ICollection<Feedback> Feedbacks { get; set; }`

---

#### Class: `ApplicationUser`

**Modifiers:** public
**Inherits:** IdentityUser
**File:** `DbModels\ApplicationUser.cs`

**Properties:**
- `public string FirstName { get; set; }`
- `public string LastName { get; set; }`
- `public string Address { get; set; }`
- `public ICollection<EventRegistration> EventRegistrations { get; set; }`
- `public ICollection<ServiceBooking> ServiceBookings { get; set; }`
- `public ICollection<Feedback> Feedbacks { get; set; }`

---

#### Class: `Event`

**Modifiers:** public
**File:** `DbModels\Event.cs`

**Properties:**
- `public int Id { get; set; }`
- `public string Title { get; set; }`
- `public string Description { get; set; }`
- `public string Location { get; set; }`
- `public double Cost { get; set; }`
- `public int Max_Capcity { get; set; }`
- `public DateTime Start_Date_Time { get; set; }`
- `public DateTime End_Date_Time { get; set; }`
- `public ICollection<EventRegistration> EventRegistrations { get; set; }`
- `public string StaffId { get; set; }`
- `public ApplicationStaff Staff { get; set; }`

---

#### Class: `EventRegistration`

**Modifiers:** public
**File:** `DbModels\EventRegistration.cs`

**Properties:**
- `public int NumberOfAttendees { get; set; }`
- `public double TotalCost { get; set; }`
- `public string UserId { get; set; }`
- `public ApplicationUser User { get; set; }`
- `public int EventId { get; set; }`
- `public Event Event { get; set; }`

---

#### Class: `Feedback`

**Modifiers:** public
**File:** `DbModels\Feedback.cs`

**Properties:**
- `public int Id { get; set; }`
- `public string Message { get; set; }`
- `public FeedbackStatus Status { get; set; }`
- `public string? Resolution_Message { get; set; }`
- `public DateTime CreatedAt { get; set; }`
- `public DateTime? ResolvedAt { get; set; }`
- `public string StaffId { get; set; }`
- `public ApplicationStaff Staff { get; set; }`
- `public string UserId { get; set; }`
- `public ApplicationUser User { get; set; }`

---

#### Class: `Service`

**Modifiers:** public
**File:** `DbModels\Service.cs`

**Properties:**
- `public int Id { get; set; }`
- `public string Title { get; set; }`
- `public string Description { get; set; }`
- `public string Location { get; set; }`
- `public double Cost { get; set; }`
- `public TimeOnly Available_Start_Time { get; set; }`
- `public TimeOnly Available_End_Time { get; set; }`
- `public ICollection<ServiceBooking> ServiceBookings { get; set; }`
- `public string StaffId { get; set; }`
- `public ApplicationStaff Staff { get; set; }`

---

#### Class: `ServiceBooking`

**Modifiers:** public
**File:** `DbModels\ServiceBooking.cs`

**Properties:**
- `public double TotalCost { get; set; }`
- `public DateTime Start_Time { get; set; }`
- `public DateTime End_Time { get; set; }`
- `public string UserId { get; set; }`
- `public ApplicationUser User { get; set; }`
- `public int ServiceId { get; set; }`
- `public Service Service { get; set; }`

---

#### Class: `XmlConfig`

**Modifiers:** public
**File:** `DbModels\XmlConfig.cs`

**Properties:**
- `public int Id { get; set; }`
- `public string XmlContent { get; set; }`
- `public string Type { get; set; }`
- `public string Version { get; set; }`
- `public bool IsActive { get; set; }`
- `public DateTime UploadedAt { get; set; }`
- `public string Label { get; set; }`
- `public string StaffId { get; set; }`
- `public ApplicationStaff Staff { get; set; }`

---

### Namespace: `CityLink_SCP.Extensions`

#### Class: `DbModelExtensions`

**Modifiers:** public static
**File:** `Extensions\DbModelExtensions.cs`

**Methods:**
- `public static ServicesViewModel ToCardViewModel(List<Service> services)`
- `public static EventsViewModel ToCardViewModel(List<Event> events)`
- `public static XmlConfigDto ToViewModel(XmlConfig config)`

---

### Namespace: `CityLink_SCP.Models`

#### Class: `BookServiceViewModel`

**Modifiers:** public
**File:** `Models\BookServiceViewModel.cs`

**Fields:**
- `private DateOnly _serviceDate`
- `private TimeOnly _serviceTime`

**Properties:**
- `public Service Service { get; set; }`
- `public ServiceBooking ServiceBooking { get; set; }`
- `public ApplicationUser User { get; set; }`
- `public DateOnly ServiceDate { get; set; }`
- `public TimeOnly ServiceTime { get; set; }`

---

#### Class: `CardViewModel`

**Modifiers:** public
**File:** `Models\CardViewModel.cs`

**Properties:**
- `public string Title { get; set; }`
- `public string Description { get; set; }`
- `public string ButtonLabel { get; set; }`

---

#### Class: `ErrorViewModel`

**Modifiers:** public
**File:** `Models\ErrorViewModel.cs`

**Properties:**
- `public string? RequestId { get; set; }`
- `public bool ShowRequestId { get; }`

---

#### Class: `EventsViewModel`

**Modifiers:** public
**Implements:** IXmlViewModel
**File:** `Models\EventsViewModel.cs`

**Properties:**
- `public string PartialName { get; }`
- `public List<EventViewModel> Events { get; set; }`

---

#### Class: `EventViewModel`

**Modifiers:** public
**File:** `Models\EventsViewModel.cs`

**Properties:**
- `public string Title { get; set; }`
- `public string Description { get; set; }`
- `public string ButtonLabel { get; set; }`

---

#### Class: `FAQ`

**Modifiers:** public
**File:** `Models\FAQViewModel.cs`

**Properties:**
- `public string Question { get; set; }`
- `public string Answer { get; set; }`

---

#### Class: `FAQViewModel`

**Modifiers:** public
**Implements:** IXmlViewModel
**File:** `Models\FAQViewModel.cs`

**Properties:**
- `public string PartialName { get; }`
- `public List<FAQ> FAQs { get; set; }`

---

#### Class: `FooterModel`

**Modifiers:** public
**Implements:** IXmlViewModel
**Attributes:** XmlRoot
**File:** `Models\FooterModel.cs`

**Properties:**
- `public string PartialName { get; }`
- `public string Email { get; set; }`
- `public string Phone { get; set; }`
- `public string Address { get; set; }`
- `public string Disclaimer { get; set; }`
- `public List<TitleLink> QuickLinks { get; set; }`
- `public List<TitleLink> SocialMedia { get; set; }`

---

#### Interface: `IXmlViewModel`

**Modifiers:** public
**File:** `Models\IXmlViewModel.cs`

**Properties:**
- `public abstract string PartialName { get; }`

---

#### Class: `ServicesViewModel`

**Modifiers:** public
**Implements:** IXmlViewModel
**File:** `Models\ServicesViewModel.cs`

**Properties:**
- `public string PartialName { get; }`
- `public List<ServiceViewModel> Services { get; set; }`

---

#### Class: `ServiceViewModel`

**Modifiers:** public
**File:** `Models\ServicesViewModel.cs`

**Properties:**
- `public string Title { get; set; }`
- `public string Description { get; set; }`
- `public string ButtonLabel { get; set; }`

---

#### Class: `TitleLink`

**Modifiers:** public
**File:** `Models\FooterModel.cs`

**Properties:**
- `public string Title { get; set; }`
- `public string Url { get; set; }`

---

#### Class: `XmlConfigDto`

**Modifiers:** public
**File:** `Models\XmlConfigDto.cs`

**Properties:**
- `public int Id { get; set; }`
- `public string XmlContent { get; set; }`
- `public string Type { get; set; }`
- `public string Version { get; set; }`
- `public bool IsActive { get; set; }`
- `public DateTime UploadedAt { get; set; }`
- `public string Label { get; set; }`

---

### Namespace: `CityLink_SCP.PageModels`

#### Class: `AdminIndexViewModel`

**Modifiers:** public
**File:** `PageModels\AdminIndexViewModel.cs`

**Properties:**
- `public List<XmlConfigDto> XmlConfigs { get; set; }`
- `public List<Event> Events { get; set; }`
- `public List<Service> Services { get; set; }`
- `public List<Feedback> Feedbacks { get; set; }`
- `public List<ApplicationUser> Users { get; set; }`
- `public List<ApplicationStaff> StaffMembers { get; set; }`
- `public List<ServiceBooking> ServiceBookings { get; set; }`
- `public List<EventRegistration> EventRegistrations { get; set; }`
- `public List<string> AvailableTypes { get; set; }`

---

#### Class: `AdminLoginViewModel`

**Modifiers:** public
**File:** `PageModels\AdminDtos.cs`

**Properties:**
- `[Required, EmailAddress] public string Email { get; set; }`
- `[Required, DataType] public string Password { get; set; }`
- `[Display] public bool RememberMe { get; set; }`

---

#### Class: `EventDto`

**Modifiers:** public
**File:** `PageModels\AdminDtos.cs`

**Properties:**
- `public int Id { get; set; }`
- `public string Title { get; set; }`
- `public string Description { get; set; }`
- `public string Location { get; set; }`
- `public double Cost { get; set; }`
- `public int Max_Capcity { get; set; }`
- `public DateTime Start_Date_Time { get; set; }`
- `public DateTime End_Date_Time { get; set; }`
- `public string StaffId { get; set; }`

---

#### Class: `FeedbackDto`

**Modifiers:** public
**File:** `PageModels\AdminDtos.cs`

**Properties:**
- `public int Id { get; set; }`
- `public string? Resolution_Message { get; set; }`
- `public int Status { get; set; }`
- `public string StaffId { get; set; }`

---

#### Class: `IndexViewModel`

**Modifiers:** public
**Implements:** IXmlViewModel
**File:** `PageModels\IndexViewModel.cs`

**Properties:**
- `public string PartialName { get; }`
- `public EventsViewModel Events { get; set; }`
- `public ServicesViewModel Services { get; set; }`
- `public FAQViewModel FAQs { get; set; }`
- `public Enquiry Enquiry { get; set; }`

---

#### Class: `ProfileViewModel`

**Modifiers:** public
**File:** `PageModels\ProfileViewModel.cs`

**Properties:**
- `public string FirstName { get; set; }`
- `public string LastName { get; set; }`
- `public string Email { get; set; }`
- `public string PhoneNumber { get; set; }`
- `public string Address { get; set; }`
- `public bool IsStaff { get; set; }`
- `public string? JobTitle { get; set; }`
- `public List<EventRegistration> EventRegistrations { get; set; }`
- `public List<ServiceBooking> ServiceBookings { get; set; }`
- `public string FullName { get; }`
- `public bool IsGuest { get; }`

---

#### Class: `ServiceDto`

**Modifiers:** public
**File:** `PageModels\AdminDtos.cs`

**Properties:**
- `public int Id { get; set; }`
- `public string Title { get; set; }`
- `public string Description { get; set; }`
- `public string Location { get; set; }`
- `public double Cost { get; set; }`
- `public TimeOnly Available_Start_Time { get; set; }`
- `public TimeOnly Available_End_Time { get; set; }`
- `public string StaffId { get; set; }`

---

#### Class: `StaffDto`

**Modifiers:** public
**File:** `PageModels\AdminDtos.cs`

**Properties:**
- `public string Id { get; set; }`
- `public string First_Name { get; set; }`
- `public string Last_Name { get; set; }`
- `public string Email { get; set; }`
- `public string Phone_Number { get; set; }`
- `public string Address { get; set; }`
- `public string JobTitle { get; set; }`
- `public string? Password { get; set; }`

---

#### Class: `UserDto`

**Modifiers:** public
**File:** `PageModels\AdminDtos.cs`

**Properties:**
- `public string Id { get; set; }`
- `public string First_Name { get; set; }`
- `public string Last_Name { get; set; }`
- `public string Email { get; set; }`
- `public string Phone_Number { get; set; }`
- `public string Address { get; set; }`
- `public string? Password { get; set; }`

---

### Namespace: `CityLink_SCP.Services`

#### Class: `DatabaseService`

**Modifiers:** public
**File:** `Services\DatabaseService.cs`

**Fields:**
- `public readonly CityLinksContext _context`

**Methods:**
- `public DbActionResult<ServiceBooking> AddServiceBooking(string userId, int serviceId, DateTime start, DateTime end)`
- `public DbActionResult IsServiceBookingTimeAvailable(int serviceId, DateTime start, DateTime end)`
- `public DbActionResult<EventRegistration> AddEventRegistration(string userId, int eventId, int numOfAttendees)`

---

#### Class: `XmlConfigService`

**Modifiers:** public
**File:** `Services\XmlConfigService.cs`

**Fields:**
- `private readonly CityLinksContext _db`

**Methods:**
- `public T? GetActive<T>()`
- `public object? GetActive(Type type)`
- `public object? GetXmlViewModel(int id)`
- `public string? GetXmlContentById(int id)`
- `public string ToXml<T>(T viewModel)`
- `public T? ToViewModel<T>(string xml)`
- `public XmlConfig FromViewModel<T>(T viewModel)`
- `public async Task SaveNewVersionAsync(ApplicationStaff staff, XmlConfigDto xmlConfig)`
- `public async Task<bool> ActivateVersionAsync(int recordId)`
- `public (bool valid, string? error) Validate(string typeName, string xmlContent)`
- `public string GenerateTemplate(string typeName)`
- `public static string GetFriendlyName(Type type)`
- `public static Type? GetViewModelType(string typeName)`
- `public static List<string> GetAvailableTypes()`

---

### Namespace: `CityLink_SCP.ViewModels`

#### Class: `BookEvent`

**Modifiers:** public
**Implements:** IXmlViewModel
**File:** `ViewModels\ViewModels.cs`

**Properties:**
- `public string PartialName { get; }`
- `[XmlIgnore] public ApplicationUser? User { get; set; }`
- `[XmlIgnore] public Event? Event { get; set; }`
- `[XmlIgnore] public EventRegistration? EventRegistration { get; set; }`
- `public string PageHeading { get; set; }`

---

#### Class: `BookService`

**Modifiers:** public
**Implements:** IXmlViewModel
**File:** `ViewModels\ViewModels.cs`

**Fields:**
- `private DateOnly _serviceDate`
- `private TimeOnly _serviceTime`

**Properties:**
- `public string PartialName { get; }`
- `[XmlIgnore] public ApplicationUser? User { get; set; }`
- `[XmlIgnore] public Service Service { get; set; }`
- `[XmlIgnore] public ServiceBooking ServiceBooking { get; set; }`
- `public DateOnly ServiceDate { get; set; }`
- `public TimeOnly ServiceTime { get; set; }`
- `public string PageHeading { get; set; }`
- `public string FormHeading { get; set; }`

---

#### Class: `ContactUs`

**Modifiers:** public
**Implements:** IXmlViewModel
**File:** `ViewModels\ViewModels.cs`

**Properties:**
- `public string PartialName { get; }`
- `[XmlIgnore] public ApplicationUser? User { get; set; }`
- `public string PageHeading { get; set; }`
- `public string PageSubHeading { get; set; }`
- `public string FormTitle { get; set; }`

---

#### Class: `Enquiry`

**Modifiers:** public
**Implements:** IXmlViewModel
**File:** `ViewModels\ViewModels.cs`

**Properties:**
- `public string PartialName { get; }`
- `[XmlIgnore] public ApplicationUser? User { get; set; }`
- `public string Heading { get; set; }`
- `public string SubHeading { get; set; }`

---

