using MyHttpServer;

Console.WriteLine("=== MyHttpServer ===");
var app = new Welcome();
await app.Run();
