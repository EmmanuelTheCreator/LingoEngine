namespace LingoEngine.Xtras.BuddyApi
{
    /// <summary>
    /// Interface representing the BuddyAPI functions.
    /// </summary>
    public interface IBuddyAPI
    {
        /// <summary>
        /// Retrieves the location of a special Windows directory.
        /// </summary>
        /// <param name="folder">Name of the folder to retrieve (e.g., "system", "desktop").</param>
        /// <returns>Full path to the specified folder.</returns>
        string baSysFolder(string folder);

        /// <summary>
        /// Finds the application associated with a specific file extension.
        /// </summary>
        /// <param name="extension">File extension (e.g., ".txt").</param>
        /// <returns>Full path to the associated application, or empty string if not found.</returns>
        string baFindApp(string extension);

        /// <summary>
        /// Checks whether a specific file exists.
        /// </summary>
        /// <param name="fileName">Full path to the file.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        bool baFileExists(string fileName);

        /// <summary>
        /// Checks whether a specific folder exists.
        /// </summary>
        /// <param name="folderName">Full path to the folder.</param>
        /// <returns>True if the folder exists; otherwise, false.</returns>
        bool baFolderExists(string folderName);

        /// <summary>
        /// Creates a new folder, including any necessary intermediate directories.
        /// </summary>
        /// <param name="folderName">Full path to the folder to create.</param>
        /// <returns>True if the folder was created successfully or already exists; otherwise, false.</returns>
        bool baCreateFolder(string folderName);

        /// <summary>
        /// Deletes an empty folder.
        /// </summary>
        /// <param name="folderName">Full path to the folder to delete.</param>
        /// <returns>True if the folder was deleted successfully or doesn't exist; otherwise, false.</returns>
        bool baDeleteFolder(string folderName);

        /// <summary>
        /// Deletes a specified file.
        /// </summary>
        /// <param name="fileName">Full path to the file to delete.</param>
        /// <returns>True if the file was deleted successfully or doesn't exist; otherwise, false.</returns>
        bool baDeleteFile(string fileName);

        /// <summary>
        /// Deletes multiple files matching a wildcard pattern within a specified directory.
        /// </summary>
        /// <param name="dirName">Directory to delete files from.</param>
        /// <param name="fileSpec">Wildcard pattern to match files (e.g., "*.tmp").</param>
        /// <returns>True if all matching files were deleted successfully or if the directory doesn't exist; otherwise, false.</returns>
        bool baDeleteXFiles(string dirName, string fileSpec);

        /// <summary>
        /// Retrieves the size of a specified file in bytes.
        /// </summary>
        /// <param name="fileName">Full path to the file.</param>
        /// <returns>Size of the file in bytes, or -1 if the file doesn't exist.</returns>
        long baFileSize(string fileName);

        /// <summary>
        /// Retrieves the attributes of a specified file.
        /// </summary>
        /// <param name="fileName">Full path to the file.</param>
        /// <returns>String containing the attributes set on the file (e.g., "ReadOnly, Hidden").</returns>
        string baFileAttributes(string fileName);

        /// <summary>
        /// Sets the attributes of a specified file.
        /// </summary>
        /// <param name="fileName">Full path to the file.</param>
        /// <param name="attributes">Attributes to set (e.g., "ReadOnly, Hidden").</param>
        /// <returns>True if the attributes were set successfully; otherwise, false.</returns>
        bool baSetFileAttributes(string fileName, string attributes);

        /// <summary>
        /// Copies a file from a source to a destination with specified overwrite behavior.
        /// </summary>
        /// <param name="sourceFile">Full path to the source file.</param>
        /// <param name="destFile">Full path to the destination file.</param>
        /// <param name="overwrite">Overwrite behavior: "Always", "IfNewer", or "IfNotExist".</param>
        /// <returns>
        /// 0 if the file was copied successfully; otherwise, an error code:
        /// 1 - Invalid source file name
        /// 2 - Invalid destination file name
        /// 3 - Error reading the source file
        /// 4 - Error writing the destination file
        /// 5 - Couldn't create directory for destination file
        /// 6 - Destination file exists
        /// 7 - Destination file is newer than source file
        /// </returns>
        int baCopyFile(string sourceFile, string destFile, string overwrite);

        /// <summary>
        /// Copies multiple files matching a wildcard pattern from one directory to another with specified overwrite behavior.
        /// </summary>
        /// <param name="sourceDir">Source directory.</param>
        /// <param name="destDir">Destination directory.</param>
        /// <param name="fileSpec">Wildcard pattern to match files (e.g., "*.txt").</param>
        /// <param name="overwrite">Overwrite behavior: "Always", "IfNewer", or "IfNotExist".</param>
        /// <returns>
        /// 0 if all files were copied successfully; otherwise, an error code:
        /// 1 - Invalid source directory name
        /// 2 - Invalid destination directory name
        /// 3 - Error reading a source file
        /// 4 - Error writing a destination file
        /// 5 - Couldn't create directory for destination files
        /// 6 - Destination file exists
        /// 7 - Destination file is newer than source file
        /// 8 - No files matched the specified wildcard
        /// </returns>
        int baCopyXFiles(string sourceDir, string destDir, string fileSpec, string overwrite);

        /// <summary>
        /// Opens an internet document using the default browser.
        /// </summary>
        /// <param name="url">URL of the document to open.</param>
        /// <param name="state">Window state: "Normal", "Hidden", "Maximised", or "Minimised".</param>
        /// <returns>True if the document was opened successfully; otherwise, false.</returns>
        bool baOpenURL(string url, string state);

        /// <summary>
        /// Prints a document using the program associated with its file type.
        /// </summary>
        /// <param name="fileName">Full path to the file to print.</param>
        /// <returns>
        /// 0 if successful; otherwise, an error code less than 32 indicating the type of error (e.g., file not found, out of memory).
        /// </returns>
        int baPrintFile(string fileName);

        /// <summary>
        /// Retrieves the main application window handle.
        /// </summary>
        /// <returns>Handle to the main application window.</returns>
        IntPtr baWinHandle();

        /// <summary>
        /// Displays information about BuddyAPI, including version and registration details.
        /// </summary>
        void baAbout();

        /// <summary>
        /// Registers BuddyAPI with the provided user name and registration number.
        /// </summary>
        /// <param name="userName">User name received upon registration.</param>
        /// <param name="regNumber">Registration number received upon registration.</param>
        /// <returns>Number of functions licensed for use.</returns>
        int baRegister(string userName, int regNumber);

        /// <summary>
        /// Saves the BuddyAPI registration information.
        /// </summary>
        /// <param name="userName">User name received upon registration.</param>
        /// <param name="regNumber">Registration number received upon registration.</param>
        /// <returns>True if the registration information was saved successfully; otherwise, false.</returns>
        bool baSaveRegistration(string userName, int regNumber);
    }

}
