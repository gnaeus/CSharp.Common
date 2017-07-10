using System.IO;
using ArxOne.MrAdvice.Advice;

namespace MrAdvice.Aspects
{
    public class SnapshotAttribute : SnapshotAspect, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            SnapshotProfile profile = Profile.Value;
            if (profile == null)
            {
                context.Proceed();
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
                context.ReturnValue = ReadReturnValue(context.TargetMethod, filePath);
            }
            else if (profile.Mode == SnapshotMode.Write)
            {
                context.Proceed();

                WriteReturnValue(context.ReturnValue, context.TargetMethod, folderPath, filePath);
            }
        }
    }
}
