using System;
using Microsoft.Win32;
using System.DirectoryServices;
using System.Globalization;
namespace NTLMManage
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] providers = GetListOfDirectoryProviders();
            foreach (string s in providers)
            {
                if (s.StartsWith("WinNT"))
                {
                    Console.WriteLine(s);
                    ListItem(s, 0);
                    break;
                }
            }
            Console.Read();
        }


        /*
         * SetPassword' requires admin rights to execute - which is not something you probably want to do. 
         * 'ChangePassword' does not and can be used by the end user themselves.  
         * It takes the old password and new password as arguments (do a search in this forum for 'ChangePassword' to see examples).
         * This would be the preferred way of executing this and it would also verify their identity for you without a database 
         * lookup (or at least verify that the user knows their old password).
         */

        public void CreateUserAccount(string login, string password, string fullName, bool isAdmin)
        {
            try
            {
                DirectoryEntry dirEntry = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                DirectoryEntries entries = dirEntry.Children;
                DirectoryEntry newUser = entries.Add(login, "user");
                newUser.Properties["FullName"].Add(fullName);
                newUser.Invoke("SetPassword", password);
                newUser.CommitChanges();

                // Remove the if condition along with the else to create user account in "user" group.
                DirectoryEntry grp;
                if (isAdmin)
                {
                    grp = dirEntry.Children.Find("Administrators", "group");
                    if (grp != null) { grp.Invoke("Add", new object[] { newUser.Path.ToString() }); }
                }
                else
                {
                    grp = dirEntry.Children.Find("Guests", "group");
                    if (grp != null) { grp.Invoke("Add", new object[] { newUser.Path.ToString() }); }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void ListItem(string AdsiPath, int indentCount)
        {
            // connect to the selected ADSI path
            DirectoryEntry DirEntry = new DirectoryEntry(AdsiPath);

            if (indentCount == 0)
            {
                // loop through all the properties and get the key for each
                foreach (string Key in DirEntry.Properties.PropertyNames)
                {
                    string PropertyValues = String.Empty;

                    // now loop through all the values in the property; can be a multi-value property
                    foreach (object Value in DirEntry.Properties[Key])
                        PropertyValues += Convert.ToString(Value) + ";";

                    // now add the property info to the property list
                    Console.WriteLine(Key + "=" + PropertyValues.Substring(0, PropertyValues.Length - 1));
                }
            }

            try
            {
                // now loop through all the children
                foreach (DirectoryEntry ChildEntry in DirEntry.Children)
                {
                    //Console.ForegroundColor = ConsoleColor.White; 
                    //Console.WriteLine(new string('*', 50));

                    //Console.WriteLine(
                    //    new String('\t', indentCount) +
                    //    ChildEntry.Name
                    //    + "\t" + ChildEntry.Path + "\t"
                    //    + ChildEntry.Properties["PrimaryGroupID"].Value); //513

                    //groupType = 4


                    if (DirectoryObjectIsOfType(ChildEntry.Path, UserSchemaClassName))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("找到用户：" + ChildEntry.Name);
                    }

                    //if (ChildEntry.Properties["PrimaryGroupID"] != null
                    //    && ChildEntry.Properties["PrimaryGroupID"].Value != null
                    //    && ChildEntry.Properties["PrimaryGroupID"].Value.ToString() == "513")
                    //{
                    //    Console.ForegroundColor = ConsoleColor.White; 
                    //    Console.WriteLine("找到用户：" + ChildEntry.Name);
                    //}


                    if (DirectoryObjectIsOfType(ChildEntry.Path, GroupSchemaClassName))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("找到用户组：" + ChildEntry.Name);
                    }

                    //if (ChildEntry.Properties["groupType"] != null
                    //   && ChildEntry.Properties["groupType"].Value != null 
                    //   && ChildEntry.Properties["groupType"].Value.ToString() == "4")
                    //{
                    //    Console.ForegroundColor = ConsoleColor.White;
                    //    Console.WriteLine("找到用户组：" + ChildEntry.Name);
                    //}

                    // add any child entries for this entry
                    ListItem(ChildEntry.Path, indentCount + 2);
                }
            }

            // catch any exception accessing the directory object
            catch (Exception exp)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exp.Message);
                Console.WriteLine(exp.StackTrace);
            }

            // close the directory object
            finally
            {
                DirEntry.Close();
            }
        }


        private const string ProviderRegKey = @"Software\Microsoft\ADs\Providers";
        private const string ProviderPath = "://";
		
        /// <summary>
        /// returns a list of directory service providers available of the local machine
        /// </summary>
        /// <returns>returns a string array with all the providers</returns>
        public static string[] GetListOfDirectoryProviders()
        {
            // get the HKLM registry key
            RegistryKey RegKey = Registry.LocalMachine;

            // open the sub-key which contains all the providers
            RegistryKey ProviderKey = RegKey.OpenSubKey(ProviderRegKey);

            // get the list of the sub-keys
            string[] SubKeys = ProviderKey.GetSubKeyNames();

            // create the string array which will hold the provider list
            string[] ListOfProviders = new string[SubKeys.Length];

            // now add all providers to the array; all providers are pointed to the local machine
            for (int Count = 0; Count < SubKeys.Length; Count++)
                ListOfProviders[Count] = SubKeys[Count] + ProviderPath + Environment.MachineName.ToLower();

            // return the list of providers
            return ListOfProviders;
        }


        public const string FullNameSchemaName = "FullName";
        public const string SetPasswordMethodName = "SetPassword";
        public const string UserSchemaClassName = "User";

        #region windows用户
        /// <summary>
        /// adds a new windows user
        /// </summary>
        /// <param name="AdsiRootPath">the ADSI root object (can point to the local machine or the domain)</param>
        /// <param name="FullName">the full name of the user</param>
        /// <param name="Username">the username</param>
        /// <param name="Password">the password</param>
        public static void AddWindowsUser(string AdsiRootPath, string FullName, string Username, string Password)
        {
            object[,] Properties = new object[,] { { FullNameSchemaName, FullName } };
            object[,] MethodsToInvoke = new object[,] { { SetPasswordMethodName, Password } };

            // invoke add Adsi object 
            AddDirectoryObject(AdsiRootPath, Username, UserSchemaClassName, Properties, MethodsToInvoke);
        }

        /// <summary>
        /// edit the properties of an existing windows user (can not change the user name)
        /// </summary>
        /// <param name="AdsiPath">the Adsi path to the user object</param>
        /// <param name="FullName">the full name of the user</param>
        /// <param name="Username">the existing username</param>
        /// <param name="Password">the password</param>
        public static void EditWindowsUser(string AdsiPath, string FullName, string Username, string Password)
        {
            object[,] Properties = new object[,] { { FullNameSchemaName, FullName } };
            object[,] MethodsToInvoke = new object[,] { { SetPasswordMethodName, Password } };

            // invoke edit Adsi object 
            EditDirectoryObject(AdsiPath, Username, UserSchemaClassName, Properties, MethodsToInvoke);
        }

        /// <summary>
        /// removes an existing windows user
        /// </summary>
        /// <param name="AdsiPath">the Adsi path to the user object</param>
        /// <param name="Username">the username</param>
        public static void DeleteWindowsUser(string AdsiPath, string Username)
        {
            DeleteDirectoryObject(AdsiPath, Username, UserSchemaClassName);
        } 
        #endregion


        #region 目录增删改
        /// <summary>
        /// creates a new directory object, sets its properties and invokes methods on it
        /// (this method is called by add user, add group, add service, etc.)
        /// </summary>
        /// <param name="AdsiParentPath">the ADSI parent object to which to add the new object</param>
        /// <param name="ObjectName">the name of the new Adsi object</param>
        /// <param name="ObjectSchemaClassName">the schema class name of the new Adsi object</param>
        /// <param name="Properties">properties to set on the new Adsi object</param>
        /// <param name="MethodsToInvoke">methods to invoke on the new Adsi object</param>
        public static void AddDirectoryObject(string AdsiParentPath, string ObjectName, string ObjectSchemaClassName, object[,] Properties, object[,] MethodsToInvoke)
        {
            // connect to the selected ADSI parent obejct
            DirectoryEntry DirEntry = new DirectoryEntry(AdsiParentPath);

            try
            {
                // creates the new Adsi object
                DirectoryEntry NewAdsiObject = DirEntry.Children.Add(ObjectName, ObjectSchemaClassName);

                // now loop through all the properties and set them
                if (Properties != null)
                {
                    for (int Count = 0; Count < Properties.GetLength(0); Count++)
                        NewAdsiObject.Properties[Convert.ToString(Properties[Count, 0])].Value = Properties[Count, 1];
                }

                // now loop through all the methods and invoke them
                if (MethodsToInvoke != null)
                {
                    for (int Count = 0; Count < MethodsToInvoke.GetLength(0); Count++)
                        NewAdsiObject.Invoke(Convert.ToString(MethodsToInvoke[Count, 0]), MethodsToInvoke[Count, 1]);
                }

                // commit the changes 
                NewAdsiObject.CommitChanges();
                NewAdsiObject.Close();
            }

            // catch any exception accessing the directory object
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // close the directory object
            finally
            {
                DirEntry.Close();
            }
        }

        /// <summary>
        /// removes an existing Adsi object; called by delete user, delete group, delete service, etc.
        /// </summary>
        /// <param name="AdsiObjectPath">the path to the directory object</param>
        /// <param name="ObjectName">the name of the directory object</param>
        /// <param name="ObjectSchemaClassName">the schema class name of the new Adsi object</param>
        public static void DeleteDirectoryObject(string AdsiObjectPath, string ObjectName, string ObjectSchemaClassName)
        {
            // connect to the selected ADSI object
            DirectoryEntry DirEntry = new DirectoryEntry(AdsiObjectPath);

            try
            {
                DirectoryEntry AdsiObject;

                // find the directory object to delete; for some providers like IIS we need to specify also the
                // class; for others we can search without a class; 
                if (ObjectSchemaClassName != null)
                    AdsiObject = DirEntry.Children.Find(ObjectName, ObjectSchemaClassName);
                else
                    AdsiObject = DirEntry.Children.Find(ObjectName);

                // if we found the Adsi object then remove it
                if (AdsiObject != null)
                    DirEntry.Children.Remove(AdsiObject);
            }

            // catch any exception accessing the directory object
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // close the directory object
            finally
            {
                DirEntry.Close();
            }
        }

        /// <summary>
        /// edit an exiting directory object, sets its properties and invokes methods on it
        /// (this method is called by add user, add group, add service, etc.)
        /// </summary>
        /// <param name="AdsiObjectPath">the path to the directory object</param>
        /// <param name="ObjectName">the name of the new Adsi object</param>
        /// <param name="ObjectSchemaClassName">the schema class name of the new Adsi object</param>
        /// <param name="Properties">properties to set on the new Adsi object</param>
        /// <param name="MethodsToInvoke">methods to invoke on the new Adsi object</param>
        public static void EditDirectoryObject(string AdsiObjectPath, string ObjectName, string ObjectSchemaClassName, object[,] Properties, object[,] MethodsToInvoke)
        {
            // connect to the selected ADSI object
            DirectoryEntry DirEntry = new DirectoryEntry(AdsiObjectPath);

            try
            {
                // find the directory object to edit
                DirectoryEntry AdsiObject = DirEntry.Children.Find(ObjectName, ObjectSchemaClassName);

                // if we found the directory object then edit its properties
                if (AdsiObject != null)
                {
                    // now loop through all the properties and set them
                    if (Properties != null)
                    {
                        for (int Count = 0; Count < Properties.GetLength(0); Count++)
                            AdsiObject.Properties[Convert.ToString(Properties[Count, 0])].Value = Properties[Count, 1];
                    }

                    // now loop through all the methods and invoke them
                    if (MethodsToInvoke != null)
                    {
                        for (int Count = 0; Count < MethodsToInvoke.GetLength(0); Count++)
                            AdsiObject.Invoke(Convert.ToString(MethodsToInvoke[Count, 0]), MethodsToInvoke[Count, 1]);
                    }

                    // commit the changes 
                    AdsiObject.CommitChanges();
                    AdsiObject.Close();
                }
            }

            // catch any exception accessing the directory object
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // close the directory object
            finally
            {
                DirEntry.Close();
            }
        } 
        #endregion

        public const string GroupSchemaClassName = "Group";
        public const string GroupTypeSchemaName = "groupType";
        public const string DescriptionSchemaName = "Description";

        #region 用户组
        /// <summary>
        /// adds a new windows group
        /// </summary>
        /// <param name="AdsiRootPath">the ADSI root object (can point to the local machine or the domain)</param>
        /// <param name="Groupname">the group name</param>
        /// <param name="Description">the group description</param>
        /// <param name="GroupType">the group type</param>
        public static void AddWindowsGroup(string AdsiRootPath, string Groupname, string Description, int GroupType)
        {
            object[,] Properties = new object[,] { { GroupTypeSchemaName, GroupType },
            { DescriptionSchemaName, Description } };

            // invoke add Adsi object 
            AddDirectoryObject(AdsiRootPath, Groupname, GroupSchemaClassName, Properties, null);
        }

        /// <summary>
        /// edit the properties of an existing windows group (can not change the group name)
        /// </summary>
        /// <param name="AdsiPath">the Adsi path to the group object</param>
        /// <param name="Groupname">the group name</param>
        /// <param name="Description">the group description</param>
        /// <param name="GroupType">the group type</param>
        public static void EditWindowsGroup(string AdsiPath, string Groupname, string Description, int GroupType)
        {
            object[,] Properties = new object[,] { { GroupTypeSchemaName, GroupType }, { DescriptionSchemaName, Description } };

            // invoke edit Adsi object 
            EditDirectoryObject(AdsiPath, Groupname, GroupSchemaClassName, Properties, null);
        }

        /// <summary>
        /// removes an existing windows group
        /// </summary>
        /// <param name="AdsiPath">the Adsi path to the group object</param>
        /// <param name="Groupname">the groupname</param>
        public static void DeleteWindowsGroup(string AdsiPath, string Groupname)
        {
            DeleteDirectoryObject(AdsiPath, Groupname, GroupSchemaClassName);
        } 
        #endregion


        /// <summary>
        /// checks if the ADSI object is of the requested type
        /// </summary>
        /// <param name="AdsiPath">the ADSI path of the object</param>
        /// <param name="SchemaClassNames">the schema type we are checking for; can be a comma separated list</param>
        /// <returns></returns>
        public static bool DirectoryObjectIsOfType(string AdsiPath, string SchemaClassNames)
        {
            // connect to the selected ADSI path
            DirectoryEntry DirEntry = new DirectoryEntry(AdsiPath);
            bool IsOfType = false;

            try
            {
                // split the comma separated list of names into an array of names
                string[] SchemaClassName = SchemaClassNames.Split(',');

                // check if the ADSI object is of the requested type
                for (int Count = 0; Count < SchemaClassName.Length; Count++)
                    IsOfType |= (String.Compare(DirEntry.SchemaClassName, SchemaClassName[Count], false, CultureInfo.InvariantCulture) == 0);
            }

            // catch any exception accessing the directory object
            catch (Exception)
            { }

            // close the directory object
            finally
            {
                DirEntry.Close();
            }

            // return if is of the sametype or not
            return IsOfType;
        }

    }
}
