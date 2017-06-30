using System.IO;
using System.Threading.Tasks;
using ArxOne.MrAdvice.Advice;

namespace MrAdvice.Aspects.Snapshot
{
    public class SnapshotAsyncAttribute : SnapshotAspect, IMethodAsyncAdvice
    {
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            SnapshotProfile profile = Profile.Value;
            if (profile == null)
            {
                await context.ProceedAsync();
                return;
            }
            if (!context.HasReturnValue)
            {
                return;
            }

            string folderPath = Path.Combine(
                profile.FolderPath, context.TargetType.FullName, context.TargetMethod.Name);

            string filePath = GetFilePath(context.TargetMethod, context.Arguments, folderPath);

            if (profile.Mode == SnapshotMode.Read)
            {
                context.ReturnValue = ReadReturnTaskResult(context.TargetMethod, filePath);
            }
            else if (profile.Mode == SnapshotMode.Write)
            {
                await context.ProceedAsync();

                WriteReturnTaskResult(context.ReturnValue, context.TargetMethod, folderPath, filePath);
            }
        }
    }
}
