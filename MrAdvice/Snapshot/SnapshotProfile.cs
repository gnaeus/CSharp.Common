using System;

namespace MrAdvice.Aspects
{
    public enum SnapshotMode
    {
        Read,
        Write,
    }

    public class SnapshotProfile
    {
        public string FolderPath { get; set; }
        public SnapshotMode Mode { get; set; }
    }
}
