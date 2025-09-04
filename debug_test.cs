using Microwave.Infrastructure.Services;
using Microwave.Infrastructure.Repositories;

var repo = new JsonCustomProgramRepository("/tmp/test.json");
var service = new ProgramDisplayService(repo);

Console.WriteLine($"Is '.' unique? {await service.IsCharacterUniqueAsync('.')}");

var usedChars = await service.GetUsedCharactersAsync();
Console.WriteLine($"Used characters: {string.Join(", ", usedChars)}");
Console.WriteLine($"Contains '.'? {usedChars.Contains('.')}");
