using Lib.Attributes;
using Microsoft.Extensions.Logging;

namespace Lib.Services;

public interface IAssessmentService
{
}

[Inject]
public class AssessmentService(ILocalDbCtxFactory factory, ILogger<AssessmentService> logger) : IAssessmentService
{


    public async Task Merge(){}


}