namespace FtpBindings
{
    public class FtpActionResult
    {
        public FtpActionResult()
        {

        }

        public FtpActionResult(FtpActionResultEnum ftpAction)
        {
            Action = ftpAction;
        }
        public FtpActionResultEnum Action { get; set; } = FtpActionResultEnum.None;
    }

    public enum FtpActionResultEnum
    {
        None = 0,
        DeleteFile = 1,
        MoveFileToOut = 2,
        MoveFileToErr = 3
    }
}
