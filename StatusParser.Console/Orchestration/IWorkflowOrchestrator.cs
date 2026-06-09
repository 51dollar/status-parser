using StatusParser.Console.Models;

namespace StatusParser.Console.Orchestration;

public interface IWorkflowOrchestrator
{
    ProcessResult Run();
}
