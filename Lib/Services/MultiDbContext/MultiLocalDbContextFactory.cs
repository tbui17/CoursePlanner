using Lib.Models;

namespace Lib.Services.MultiDbContext;

public class MultiLocalDbContextFactory(ILocalDbCtxFactory factory) : MultiDbContextFactory<LocalDbCtx>(factory);