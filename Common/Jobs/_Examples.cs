using System.Threading.Tasks;
using Common.Jobs;

partial class _Examples
{
    class JobsService
    {
        readonly AsyncJobsManager _asyncJobsManager;

        private async Task FirstJob()
        {
            await Task.Delay(200);
        }

        private async Task SecondJob()
        {
            await Task.Delay(300);
        }

        public async Task RunJobs()
        {
            for (int i = 0; i < 5; i++)
            {
                _asyncJobsManager.ExecuteAsync(FirstJob);
                _asyncJobsManager.ExecuteAsync(SecondJob);
            }

            await Task.Delay(500);

            // FirstJob will be executed three times (600ms)
            // SecondJob will be executed two times (600ms)

            await _asyncJobsManager.StopAsync();
        }
    }
}
