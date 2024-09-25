using Lib.Interfaces;

namespace Lib.Models;

public record UpdateLog<T>(T Local, T Database) where T : class, IDatabaseEntry;