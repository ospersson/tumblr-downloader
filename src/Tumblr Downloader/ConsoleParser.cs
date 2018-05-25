namespace TDownConsole
{
    public class ConsoleParser
    {
        public void Parse(
            string[] args2, 
            out string baseDomainUrl, 
            out string baseDiskPath, 
            out bool downloadRaw, 
            out bool doWriteJson)
        {
            baseDomainUrl = string.Empty;
            baseDiskPath = string.Empty;
            downloadRaw = false;
            doWriteJson = false;

            if (args2.Length == 1)
            {
                baseDomainUrl = args2[1];
            }
            if (args2.Length == 2)
            {
                baseDomainUrl = args2[1];
                baseDiskPath = args2[2];
            }
            if (args2.Length == 3)
            {
                baseDomainUrl = args2[1];
                baseDiskPath = args2[2];
                if (args2[2] == "-r")
                    downloadRaw = true;
            }
            if (args2.Length == 4)
            {
                baseDomainUrl = args2[1];
                baseDiskPath = args2[2];
                if (args2[3] == "-r")
                    downloadRaw = true;
                if (args2[3] == "-l")
                    doWriteJson = true;
            }
        }
    }
}
