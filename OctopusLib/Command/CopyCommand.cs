using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using AutomationHelper;

namespace OctopusLib
{
    [Serializable]
    public class CopyCommand : Command
    {
        public enum Direction { LocalToRemote, RemoteToLocal }

        #region Private members
        private Direction _copydirection;
        private string _copysourcefiles;
        private string _copysourcedir;
        private string _copytargetdir;
        private bool _isforce;
        #endregion

        #region Properties
        public Direction CopyDirection
        {
            get
            {
                return _copydirection;
            }
            set
            {
                if (value != _copydirection)
                {
                    _copydirection = value;
                    OnPropertyChanged("CopyDirection");
                }
            }
        }

        public string CopySourceFiles
        {
            get
            {
                return _copysourcefiles;
            }
            set
            {
                if (value != _copysourcefiles)
                {
                    _copysourcefiles = value;
                    OnPropertyChanged("CopySourceFiles");
                }
            }
        }

        public string CopySourceDir
        {
            get
            {
                return _copysourcedir;
            }
            set
            {
                if (value != _copysourcedir)
                {
                    _copysourcedir = value;
                    OnPropertyChanged("CopySourceDir");
                }
            }
        }

        public string CopyTargetDir
        {
            get
            {
                return _copytargetdir;
            }
            set
            {
                if (value != _copytargetdir)
                {
                    _copytargetdir = value;
                    OnPropertyChanged("CopyTargetDir");
                }
            }
        }

        public bool IsForce
        {
            get
            {
                return _isforce;
            }
            set
            {
                if (value != _isforce)
                {
                    _isforce = value;
                    OnPropertyChanged("IsForce");
                }
            }
        }
        #endregion

        #region Public methods
        public override bool Execute(Machine machine)
        {
            bool result = false; 
            int countMax = 0;

            int retryCount = 0;
            while (!result && retryCount <= RetryTimes)
            {
                try
                {
                    result = CopyFile(machine);
                    if (result)
                        break;
                }
                catch (Exception ex)
                {
                    Message = ex.Message;
                    countMax++;
                    System.Threading.Thread.Sleep(2000);
                }
                retryCount++;
                System.Threading.Thread.Sleep(RetryIntervalSeconds * 3000);
            }

            return result;
                
        }
        #endregion

        #region Private methods

        private bool CopyFile(Machine machine)
        {
            bool result = false;
            NetworkCredential cred = new NetworkCredential(
                OctopusLib.Command.Normalize(machine.Username, ParameterCollection),
                OctopusLib.Command.Normalize(machine.Password, ParameterCollection),
                OctopusLib.Command.Normalize(machine.Domain, ParameterCollection));

            string normalizedSourceFiles = OctopusLib.Command.Normalize(CopySourceFiles, ParameterCollection, machine);
            string normalizedSourceDirectory = OctopusLib.Command.Normalize(CopySourceDir, ParameterCollection, machine);
            string normalizedTargetDirectory = OctopusLib.Command.Normalize(CopyTargetDir, ParameterCollection, machine);

            string networkShare = string.Format(@"\\{0}\C$", machine.IP);
            // if copy from local to Remote
            if (CopyDirection == Direction.LocalToRemote)
            {
                // if source directory is not empty, try copy all files under the source directory to remote
                if (string.IsNullOrEmpty(normalizedSourceFiles) && !string.IsNullOrEmpty(normalizedSourceDirectory))
                {
                    string targetDirectory = Regex.Replace(string.Format(@"\\{0}\{1}", machine.IP, normalizedTargetDirectory), ":", "$");

                    try
                    {
                        if (!System.IO.Directory.Exists(targetDirectory))
                            System.IO.Directory.CreateDirectory(targetDirectory);

                        FileHelper.CopyFolder(normalizedSourceDirectory, targetDirectory, IsForce);
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        using (new NetworkConnection(networkShare, cred))
                        {
                            try
                            {
                                if (!System.IO.Directory.Exists(targetDirectory))
                                    System.IO.Directory.CreateDirectory(targetDirectory);

                                FileHelper.CopyFolder(normalizedSourceDirectory, targetDirectory, IsForce);
                                result = true;
                            }
                            catch(Exception e)
                            {
                                Message = e.Message;
                            }
                        }
                    }
                }
                // if source files is not empty, try copy all files to remote
                else if (string.IsNullOrEmpty(normalizedSourceDirectory) && !string.IsNullOrEmpty(normalizedSourceFiles))
                {
                    foreach (string sourcefile in normalizedSourceFiles.Split(new char[] { ';' }))
                    {
                        string targetDirectory = Regex.Replace(string.Format(@"\\{0}\{1}", machine.IP, normalizedTargetDirectory), ":", "$");
                        string targetPath = System.IO.Path.Combine(Regex.Replace(string.Format(@"\\{0}\{1}", machine.IP, normalizedTargetDirectory), ":", "$"), System.IO.Path.GetFileName(sourcefile));

                        try
                        {
                            if (!System.IO.Directory.Exists(targetDirectory))
                                System.IO.Directory.CreateDirectory(targetDirectory);

                            System.IO.File.Copy(sourcefile, targetPath, IsForce);
                            result = true;
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            using (new NetworkConnection(networkShare, cred))
                            {
                                try
                                {
                                    if (!System.IO.Directory.Exists(targetDirectory))
                                        System.IO.Directory.CreateDirectory(targetDirectory);

                                    System.IO.File.Copy(sourcefile, targetPath, IsForce);
                                    result = true;
                                }
                                catch (Exception e)
                                {
                                    Message = e.Message;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Message = "Source files or source directory should have at least one.";
                }
            }
            else // if copy from remote to local
            {
                // if source directory is not empty, try copy all files under the source directory to local
                if (string.IsNullOrEmpty(normalizedSourceFiles) && !string.IsNullOrEmpty(normalizedSourceDirectory))
                {
                    string sourcePath = Regex.Replace(string.Format(@"\\{0}\{1}", machine.IP, normalizedSourceDirectory), ":", "$");                    

                    try
                    {
                        if (!System.IO.Directory.Exists(normalizedTargetDirectory))
                            System.IO.Directory.CreateDirectory(normalizedTargetDirectory);

                        FileHelper.CopyFolder(sourcePath, normalizedTargetDirectory, IsForce);
                        result = true;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                        using (new NetworkConnection(networkShare, cred))
                        {
                            try
                            {
                                if (!System.IO.Directory.Exists(normalizedTargetDirectory))
                                    System.IO.Directory.CreateDirectory(normalizedTargetDirectory);

                                FileHelper.CopyFolder(sourcePath, normalizedTargetDirectory, IsForce);
                                result = true;
                            }
                            catch(Exception e)
                            {
                                Message = e.Message;
                            }
                            
                        }
                    }                    
                }
                // if source files is not empty, try copy all files to local
                else if (string.IsNullOrEmpty(normalizedSourceDirectory) && !string.IsNullOrEmpty(normalizedSourceFiles))
                {
                    foreach (string sourcefile in normalizedSourceFiles.Split(new char[] { ';' }))
                    {
                        string sourcePath = Regex.Replace(string.Format(@"\\{0}\{1}", machine.IP, sourcefile), ":", "$");
                        string targetPath = System.IO.Path.Combine(normalizedTargetDirectory, System.IO.Path.GetFileName(sourcefile));

                        try
                        {
                            if (!System.IO.Directory.Exists(normalizedTargetDirectory))
                                System.IO.Directory.CreateDirectory(normalizedTargetDirectory);

                            System.IO.File.Copy(sourcePath, targetPath, IsForce);
                            result = true;
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            using (new NetworkConnection(networkShare, cred))
                            {
                                try
                                {
                                    if (!System.IO.Directory.Exists(normalizedTargetDirectory))
                                        System.IO.Directory.CreateDirectory(normalizedTargetDirectory);

                                    System.IO.File.Copy(sourcePath, targetPath, IsForce);
                                    result = true;
                                }
                                catch (Exception e)
                                {
                                    Message = e.Message;
                                }
                                
                            }
                        }
                    }
                }
            }

            return result;
        }
        #endregion
    }
}
