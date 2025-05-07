using BAL;
using CustomerOrderManagement.Helper; // INCLUDE THE NAMESPACE FOR MYSQLSERVICE AND OTHERS

var builder = WebApplication.CreateBuilder(args);

// REGISTER SERVICES NEEDED IN DI CONTAINER
// ADD MYSQLSERVICE WITH A CONNECTION STRING FROM APPSETTINGS.JSON
builder.Services.AddTransient<MySqlService>(sp =>
{
  var connectionString = builder.Configuration.GetConnectionString("MySqlConnection");
  var connectionStringPadm = builder.Configuration.GetConnectionString("connectionStringPadm");
  return new MySqlService(connectionString, connectionStringPadm);
});

// Register all BAL services
builder.Services.AddTransient<LoginBAL>();
builder.Services.AddTransient<DashboardBAL>();
builder.Services.AddTransient<CountryMasterBAL>();
builder.Services.AddTransient<StateMasterBAL>();
builder.Services.AddTransient<CityMasterBAL>();
builder.Services.AddTransient<ProductGroupMasterBAL>();
builder.Services.AddTransient<PurityMasterBAL>();
builder.Services.AddTransient<StoneColorMasterBAL>();
builder.Services.AddTransient<FilesManagementBAL>();
builder.Services.AddTransient<ItemMasterBAL>();
builder.Services.AddTransient<CategoryMasterBAL>();
builder.Services.AddTransient<VendorMasterBAL>();
builder.Services.AddTransient<SubVendorMasterBAL>();
builder.Services.AddTransient<OrdersMasterBAL>();
builder.Services.AddTransient<BranchMasterBAL>();
builder.Services.AddTransient<TenantMasterBAL>();
builder.Services.AddTransient<UserMappingBAL>();
builder.Services.AddTransient<CustomerMasterBAL>();

// REGISTER IHTTPCONTEXTACCESSOR AND GLOBALSESSIONBAL FOR SESSION MANAGEMENT
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<GlobalSessionBAL>();

// ENABLE SESSION MANAGEMENT
builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromHours(4); //(ANIKET 22 - 02 - 2025)
  //options.IdleTimeout = TimeSpan.FromMinutes(30); // SET SESSION TIMEOUT DURATION
  options.Cookie.HttpOnly = true; // SECURE THE SESSION COOKIE
  options.Cookie.IsEssential = true; // REQUIRED FOR EU GDPR COMPLIANCE
});

// ADD CONTROLLERS AND VIEWS SUPPORT (MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// USE MIDDLEWARE FOR HTTPS REDIRECTION, STATIC FILES, ROUTING, AND AUTHORIZATION
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ENABLE SESSION MIDDLEWARE
app.UseSession();

//// THIS MIDDLEWARE FUNCTIONS ARE USED FOR CHECK SESSION EXPRED AGAINST EVERY REQUEST(ANIKET 22-02-2025)
//app.Use(async (context, next) =>
//{
//  var userId = context.Session.GetString("UserId");
//  var roleId = context.Session.GetString("RoleId");
//  var mobileNo = context.Session.GetString("MobileNo");
//  var tenantId = context.Session.GetString("TenantId");

//  // CHECK IF ROLEID IS 5 AND ALLOW TENANTID TO BE NULL
//  bool isSessionInvalid = string.IsNullOrEmpty(userId) ||
//                          string.IsNullOrEmpty(roleId) ||
//                          string.IsNullOrEmpty(mobileNo) ||
//                          (roleId != "5" && string.IsNullOrEmpty(tenantId));

//  if (isSessionInvalid && !context.Request.Path.StartsWithSegments("/Login"))
//  {
//    context.Response.Redirect("/Login/StartPage");
//    return;
//  }

//  await next();
//});


// CONFIGURE MVC ROUTE PATTERN
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=StartPage}/{id?}");

app.Run();
