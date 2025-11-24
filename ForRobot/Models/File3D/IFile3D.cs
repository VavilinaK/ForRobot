using System;


namespace ForRobot.Models.File3D
{
    public interface IFile3D : IDisposable
    {
        bool IsSaved { get; }
        bool IsLoadedFile { get; }

        string Path { get; set; }
        string Name { get; }
        string NameWithoutExtension { get; }


    }
}
