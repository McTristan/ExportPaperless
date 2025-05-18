using ExportPaperless.Domain.Services;
using Quartz;

namespace ExportPaperless.Jobs;

public class DeleteOldFilesJob(IStorageService storageService) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        storageService.CleanUpFiles();
        return Task.CompletedTask;
    }
}