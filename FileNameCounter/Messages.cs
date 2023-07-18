// Ignore Spelling: nof

namespace FileNameCounter
{
    public static class Messages
    {
        public static class Arguments
        {
            public const string ContainsColon = "The file name must not contain any colons.";
            public const string FileDoesNotExist = "File does not exits";
            public const string Help = "There should be one argument giving the file path.";
            public const string NoAccess = "You don't have access to that file.";
            public const string NoMainPart = "The file name only has an extension, there is nothing to search for.";
            public const string PathTooLong = "The path is too long";
            public const string Unexpected = "There was an unexpected fault related to the argument, please contact support.";
        }
        public static class Processing
        {
            public const string UnexpectedNoFile = "Although the file was firstly found, it has been removed during processing.";
            public const string UnexpectedNoAccess = "Although access was firstly granted, it has been revoked during processing.";
            public const string ProblemReading = "There was an unexpected problem reading the file.";
        }
        public static class Result
        {
            public static string Successful(long nofInstances, string searchString, string fileName) => $"There were {nofInstances} instances of {searchString} in file {fileName}";
        }

        public const string UnknownError = "There was an unexpected error.";
    }
}
