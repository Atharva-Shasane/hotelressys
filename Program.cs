using hotelressys.Models;
using hotelressys.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

bool IsAdmin(HttpContext ctx) => ctx.Request.Headers["X-Role"] == "Admin";

app.MapPost("/api/login", (LoginRequest req) =>
{
    if (req.Username == "admin" && req.Password == "admin123")
        return Results.Ok(new { Role = "Admin" });
        
    return Results.Unauthorized();
});

app.MapGet("/api/rooms", async (AppDbContext db) => 
    await db.Rooms.ToListAsync());

app.MapPost("/api/reservations", async (Reservation newReservation, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(newReservation.GuestName)) return Results.BadRequest("Guest name is required.");
    if (string.IsNullOrWhiteSpace(newReservation.PhoneNumber)) return Results.BadRequest("Phone number is required.");
    if (string.IsNullOrWhiteSpace(newReservation.EmailId)) return Results.BadRequest("Email ID is required.");
    if (newReservation.CheckOutDate <= newReservation.CheckInDate) return Results.BadRequest("Check-out must be after Check-in.");

    var room = await db.Rooms.FindAsync(newReservation.RoomId);
    if (room == null || !room.IsAvailable) return Results.BadRequest("Room is unavailable.");

    db.Reservations.Add(newReservation);
    room.IsAvailable = false; 
    await db.SaveChangesAsync();

    return Results.Created($"/api/reservations/{newReservation.Id}", newReservation);
});

app.MapDelete("/api/reservations/{id}", async (int id, AppDbContext db) =>
{
    var reservation = await db.Reservations.FindAsync(id);
    if (reservation == null) return Results.NotFound("Reservation not found.");

    var room = await db.Rooms.FindAsync(reservation.RoomId);
    if (room != null) room.IsAvailable = true; 

    db.Reservations.Remove(reservation);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/api/reservations", async (AppDbContext db, HttpContext ctx) =>
{
    if (!IsAdmin(ctx)) return Results.Unauthorized();
    
    var reservations = await db.Reservations
        .Join(db.Rooms, res => res.RoomId, r => r.Id, 
             (res, r) => new { res.Id, res.GuestName, res.PhoneNumber, res.EmailId, r.RoomNumber, res.CheckInDate, res.CheckOutDate })
        .ToListAsync();
        
    return Results.Ok(reservations);
});

app.MapPost("/api/rooms", async (Room newRoom, AppDbContext db, HttpContext ctx) =>
{
    if (!IsAdmin(ctx)) return Results.Unauthorized();
    if (string.IsNullOrWhiteSpace(newRoom.RoomNumber)) return Results.BadRequest("Room number required.");
        
    db.Rooms.Add(newRoom);
    await db.SaveChangesAsync();
    return Results.Ok(newRoom);
});

app.MapDelete("/api/rooms/{id}", async (int id, AppDbContext db, HttpContext ctx) =>
{
    if (!IsAdmin(ctx)) return Results.Unauthorized();
    var room = await db.Rooms.FindAsync(id);
    if (room == null) return Results.NotFound();

    db.Rooms.Remove(room);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}