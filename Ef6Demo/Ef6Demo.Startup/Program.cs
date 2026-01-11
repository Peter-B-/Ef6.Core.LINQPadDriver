using Ef6Demo.Startup;

Console.WriteLine("Code first:");
await CodeFirstExample.Run();

Console.WriteLine("Model first:");
await ModelFirstExample.Run();
