namespace SmartJob.API.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "wwwroot/uploads";
    public string AvatarFolder { get; set; } = "avatars";
    public string ResumeFolder { get; set; } = "resumes";
}
