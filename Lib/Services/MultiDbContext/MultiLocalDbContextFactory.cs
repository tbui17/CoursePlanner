using Lib.Attributes;
using Lib.Models;

namespace Lib.Services.MultiDbContext;

[Inject]
public class MultiLocalDbContextFactory(ILocalDbCtxFactory factory) : MultiDbContextFactory<LocalDbCtx>(factory);