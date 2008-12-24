#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using Microsoft.Win32;
using System.Globalization;
using System.Web.UI.WebControls;

#endregion

// For a list of all WinNT ADSI objects, methods, properties, required properties and schema see following URLs
//
// http://msdn.microsoft.com/library/en-us/adsi/adsi/adsi_objects_of_winnt.asp?frame=true
// http://msdn.microsoft.com/library/en-us/adsi/adsi/winnt_object_class_hierarchy.asp?frame=true
// http://msdn.microsoft.com/library/en-us/adsi/adsi/winnt_schemaampaposs_mandatory_and_optional_properties.asp?frame=true
//
// For a list of all IIS ADSI objects, methods, properties, etc. see following URL
//
// http://msdn.microsoft.com/library/en-us/iissdk/iis/ref_prog_iaoref.asp?frame=true
namespace NTLM.Account
{
	/// <summary>
	/// all the attribute types supported by Ldap
	/// </summary>
	public enum LdapAttributeTypes
	{ 
		BinaryData,
		BinaryDataWithDN,
		Boolean,
		DNReference,
		Integer,
		LargeInteger,
		LinkedDN,
		SecurityDescriptor,
		SecurityIdentifier,
		String,
		StringWithDN,
		Time
	}

	/// <summary>
	/// provides a number of directory services methods for the WinNT, IIS and Ldap provider
	/// </summary>
	public class DirectoryServicesManager
	{
		private const string Separator = " = ";
		private const string ProviderRegKey = @"Software\Microsoft\ADs\Providers";
		private const string ProviderPath = "://";
		private const string PropertySeparator = ";";
		private const string PathSeparator = "/";
		private const string RootWebFolder = "ROOT";
		private const string LdapCnPrefix = "CN=";
		private const string ClassOIDBase = "1.2.840.113556.1.6.1.2.";
		private const string AttributeOIDBase = "1.2.840.113556.1.6.1.1.";

		public const string UserSchemaClassName = "User";
		public const string NameSchemaName = "Name";
		public const string FullNameSchemaName = "FullName";
		public const string SetPasswordMethodName = "SetPassword";
		public const string GroupSchemaClassName = "Group";
		public const string GroupTypeSchemaName = "groupType";
		public const string DescriptionSchemaName = "Description";
		public const string ServiceSchemaClassName = "Service";
		public const string DisplayNameSchemaClassName = "DisplayName";
		public const string ServiceTypeSchemaClassName = "ServiceType";
		public const string PathSchemaClassName = "Path";
		public const string StartTypeSchemaClassName = "StartType";
		public const string ErrorControlSchemaClassName = "ErrorControl";
		public const string WebSiteSchemaClassName = "IIsWebServer";
		public const string WebFolderSchemaClassName = "IIsWebVirtualDir";
		public const string LogTypeSchemaClassname = "LogType";
		public const string LdapUserSchemaClassName = "user";
		public const string LdapContainerSchemaClassName = "container";
		public const string LdapContainerSchemaClassNames = "container,organization,organizationalUnit";
		public const string LdapDisplayNameSchemaClassName = "displayName";
		public const string LdapGivenNameSchemaClassName = "givenName";
		public const string LdapNameSchemaClassName = "cn";
		public const string LdapClassSchemaClassName = "classSchema";
		public const string LdapAttributeSchemaClassName = "attributeSchema";
		public const string LdapClassIDSchemaClassName = "governsID";
		public const string LdapSubClassSchemaClassName = "subClassOf";
		public const string LdapAttributeDisplayNameSchemaClassName = "lDAPDisplayName";
		public const string LdapIsSingleValuedSchemaClassName = "isSingleValued";
		public const string LdapAttributeSyntaxSchemaClassName = "attributeSyntax";
		public const string LdapAttributeIDSchemaClassName = "attributeID";
		public const string LdapAttributeOMSyntaxSchemaClassName = "oMSyntax";

		public const string WinNTProvider = "WinNT://";
		public const string IISProvider = "IIS://";
		public const string LDAPProvider = "LDAP://";

		/// <summary>
		/// private constructor so you can't create an instance of the object
		/// </summary>
		private DirectoryServicesManager()
		{
		}

		/// <summary>
		/// query all properties of a Adsi object and add it to a collection
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to query</param>
		/// <param name="Collection">the collection to which to add all properties</param>
        //public static void GetDirectoryEntryProperties(string AdsiPath,ListBox.ObjectCollection Collection)
        //{
        //    // connect to the selected ADSI path
        //    DirectoryEntry DirEntry = new DirectoryEntry(AdsiPath);

        //    try
        //    {
        //        // remove any existing values from the property list
        //        Collection.Clear();

        //        // loop through all the properties and get the key for each
        //        foreach (string Key in DirEntry.Properties.PropertyNames)
        //        {
        //            string PropertyValues = String.Empty;

        //            // now loop through all the values in the property; can be a multi-value property
        //            foreach (object Value in DirEntry.Properties[Key])
        //                PropertyValues += Convert.ToString(Value) + PropertySeparator;

        //            // now add the property info to the property list
        //            Collection.Add(Key + Separator + PropertyValues.Substring(0, PropertyValues.Length - 1));
        //        }
        //    }

        //    // catch any exception accessing the directory object
        //    catch (Exception)
        //    { }

        //    // close the directory object
        //    finally
        //    {
        //        DirEntry.Close();
        //    }
        //}

		/// <summary>
		/// queries the Adsi path and adds all the childrens to the tree node passed along; this
		/// method is called recursively to add all childrens we find under the Adsi path started out
		/// </summary>
		/// <param name="AdsiPath">the ADSI path to query</param>
		/// <param name="NodeCollection">the node collection to which to add all the child nodes</param>
        //public static void FillTreeView(string AdsiPath,TreeNodeCollection NodeCollection)
        //{
        //    // connect to the selected ADSI path
        //    DirectoryEntry DirEntry = new DirectoryEntry(AdsiPath);

        //    try
        //    {
        //        // now loop through all the children
        //        foreach (DirectoryEntry ChildEntry in DirEntry.Children)
        //        {
        //            // add the node to the tree view
        //            TreeNode NewNode = NodeCollection.Add(ChildEntry.Path, ChildEntry.Name);

        //            // add any child entries for this entry
        //            FillTreeView(ChildEntry.Path, NewNode.Nodes);
        //        }
        //    }

        //    // catch any exception accessing the directory object
        //    catch (Exception)
        //    { }

        //    // close the directory object
        //    finally
        //    {
        //        DirEntry.Close();
        //    }
        //}

		/// <summary>
		/// searches a Adsi path for all Adsi objects matching the filter criteria; all matching objects
		/// are added to the tree node passed along; calls then FillTreeView to find all childrens of the
		/// found Adsi objects
		/// </summary>
		/// <param name="AdsiPath">the ADSI path to search in</param>
		/// <param name="Filter">the filter to apply</param>
		/// <param name="NodeCollection">the node collection to which to add the matching Adsi objects</param>
        //public static void FillFilteredTreeView(string AdsiPath,string Filter,TreeNodeCollection NodeCollection)
        //{
        //    // connect to the selected ADSI path
        //    DirectoryEntry DirEntry = new DirectoryEntry(AdsiPath);
			
        //    // create a directory searcher which we wrap around the Adsi object we want to search
        //    DirectorySearcher Searcher = new DirectorySearcher(DirEntry);

        //    try
        //    {
        //        // set the filter to apply (is a property=value collection with logical OR and AND)
        //        Searcher.Filter = Filter;

        //        // perform the sarch and get the result collection back
        //        SearchResultCollection ResultCollection = Searcher.FindAll();
				
        //        // now loop through all the objects which are in the search result collection
        //        foreach (SearchResult Result in ResultCollection)
        //        {
        //            // get the found directory entry
        //            DirectoryEntry FoundEntry = Result.GetDirectoryEntry();

        //            // add the node to the tree view
        //            TreeNode NewNode = NodeCollection.Add(FoundEntry.Path, FoundEntry.Name);

        //            // add any child entries for this found Adsi object
        //            FillTreeView(FoundEntry.Path, NewNode.Nodes);

        //            // close the found entry
        //            FoundEntry.Close();
        //        }

        //        // dispose the search result collection
        //        ResultCollection.Dispose();
        //    }

        //    // catch any exception accessing the directory object
        //    catch (Exception)
        //    { }

        //    // close the directory and searcher object
        //    finally
        //    {
        //        Searcher.Dispose();
        //        DirEntry.Close();
        //    }
        //}

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

		/// <summary>
		/// creates a new directory object, sets its properties and invokes methods on it
		/// (this method is called by add user, add group, add service, etc.)
		/// </summary>
		/// <param name="AdsiParentPath">the ADSI parent object to which to add the new object</param>
		/// <param name="ObjectName">the name of the new Adsi object</param>
		/// <param name="ObjectSchemaClassName">the schema class name of the new Adsi object</param>
		/// <param name="Properties">properties to set on the new Adsi object</param>
		/// <param name="MethodsToInvoke">methods to invoke on the new Adsi object</param>
		public static void AddDirectoryObject(string AdsiParentPath,string ObjectName,string ObjectSchemaClassName,object[,] Properties,object[,] MethodsToInvoke)
		{
			// connect to the selected ADSI parent obejct
			DirectoryEntry DirEntry = new DirectoryEntry(AdsiParentPath);

			try
			{
				// creates the new Adsi object
				DirectoryEntry NewAdsiObject = DirEntry.Children.Add(ObjectName,ObjectSchemaClassName);

				// now loop through all the properties and set them
				if (Properties != null)
				{
					for (int Count = 0; Count < Properties.GetLength(0); Count++)
						NewAdsiObject.Properties[Convert.ToString(Properties[Count,0])].Value = Properties[Count,1];
				}

				// now loop through all the methods and invoke them
				if (MethodsToInvoke != null)
				{
					for (int Count = 0; Count < MethodsToInvoke.GetLength(0); Count++)
						NewAdsiObject.Invoke(Convert.ToString(MethodsToInvoke[Count,0]), MethodsToInvoke[Count,1]);
				}

				// commit the changes 
				NewAdsiObject.CommitChanges();
				NewAdsiObject.Close();
			}

			// catch any exception accessing the directory object
			catch (Exception e)
			{
                throw e;
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
		public static void DeleteDirectoryObject(string AdsiObjectPath,string ObjectName,string ObjectSchemaClassName)
		{
			// connect to the selected ADSI object
			DirectoryEntry DirEntry = new DirectoryEntry(AdsiObjectPath);

			try
			{
				DirectoryEntry AdsiObject;

				// find the directory object to delete; for some providers like IIS we need to specify also the
				// class; for others we can search without a class; 
				if (ObjectSchemaClassName != null)
					AdsiObject = DirEntry.Children.Find(ObjectName,ObjectSchemaClassName);
				else
					AdsiObject = DirEntry.Children.Find(ObjectName);

				// if we found the Adsi object then remove it
				if (AdsiObject != null)
					DirEntry.Children.Remove(AdsiObject);
			}

			// catch any exception accessing the directory object
			catch (Exception e)
			{
                throw e;
			}

			// close the directory object
			finally
			{
				DirEntry.Close();
			}
		}

		/// <summary>
		/// removes the Adsi object tree, which includes any child objects
		/// </summary>
		/// <param name="AdsiObjectPath">the path to the directory object</param>
		public static void DeleteDirectoryTree(string AdsiObjectPath)
		{
			// connect to the selected ADSI object
			DirectoryEntry DirEntry = new DirectoryEntry(AdsiObjectPath);

			try
			{
				// delete the whole object tree; which removes also any child object
				DirEntry.DeleteTree();
			}

			// catch any exception accessing the directory object
			catch (Exception e)
			{
                throw e;
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
		public static void EditDirectoryObject(string AdsiObjectPath,string ObjectName,string ObjectSchemaClassName,object[,] Properties,object[,] MethodsToInvoke)
		{
			// connect to the selected ADSI object
			DirectoryEntry DirEntry = new DirectoryEntry(AdsiObjectPath);

			try
			{
				// find the directory object to edit
				DirectoryEntry AdsiObject = DirEntry.Children.Find(ObjectName,ObjectSchemaClassName);

				// if we found the directory object then edit its properties
				if (AdsiObject != null)
				{
					// now loop through all the properties and set them
					if (Properties != null)
					{
						for (int Count = 0; Count < Properties.GetLength(0); Count++)
							AdsiObject.Properties[Convert.ToString(Properties[Count,0])].Value = Properties[Count,1];
					}

					// now loop through all the methods and invoke them
					if (MethodsToInvoke != null)
					{
						for (int Count = 0; Count < MethodsToInvoke.GetLength(0); Count++)
							AdsiObject.Invoke(Convert.ToString(MethodsToInvoke[Count,0]), MethodsToInvoke[Count,1]);
					}

					// commit the changes 
					AdsiObject.CommitChanges();
					AdsiObject.Close();
				}
			}

			// catch any exception accessing the directory object
			catch (Exception e)
			{
                throw e;
			}

			// close the directory object
			finally
			{
				DirEntry.Close();
			}
		}

		/// <summary>
		/// checks if the ADSI object is of the requested type
		/// </summary>
		/// <param name="AdsiPath">the ADSI path of the object</param>
		/// <param name="SchemaClassNames">the schema type we are checking for; can be a comma separated list</param>
		/// <returns></returns>
		public static bool DirectoryObjectIsOfType(string AdsiPath,string SchemaClassNames)
		{
			// connect to the selected ADSI path
			DirectoryEntry DirEntry = new DirectoryEntry(AdsiPath);
			bool IsOfType = false;

			try
			{
				// split the comma separated list of names into an array of names
				string[] SchemaClassName = SchemaClassNames.Split(',');

				// check if the ADSI object is of the requested type
				for (int Count=0; Count < SchemaClassName.Length; Count++)
					IsOfType |= (String.Compare(DirEntry.SchemaClassName,SchemaClassName[Count],false,CultureInfo.InvariantCulture) == 0);
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

		/// <summary>
		/// queries the object and returns the value of the requested property
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to query</param>
		/// <param name="PropertyName">the property we are looking for</param>
		/// <returns>returns the value of the property we are looking for</returns>
		public static string GetObjectProperty(string AdsiPath,string PropertyName)
		{
			// connect to the selected ADSI path
			DirectoryEntry DirEntry = new DirectoryEntry(AdsiPath);
			string PropertyValue = String.Empty;

			try
			{
				// loop through all the properties and get the key for each
				foreach (string Key in DirEntry.Properties.PropertyNames)
				{
					// check if the property is the one we are looking for
					if (String.Compare(Key, PropertyName, false, CultureInfo.InvariantCulture) == 0)
					{
						// now loop through all the values in the property; can be a multi-value property
						foreach (object Value in DirEntry.Properties[Key])
							PropertyValue += Convert.ToString(Value) + PropertySeparator;
					}
				}
			}

			// catch any exception accessing the directory object
			catch (Exception)
			{ }

			// close the directory object
			finally
			{
				DirEntry.Close();
			}

			// return the property value
			if (PropertyValue.Length > 0)
				return PropertyValue.Substring(0, PropertyValue.Length - 1);
			else
				return String.Empty;
		}

		/// <summary>
		/// queries the object and returns the object name; used when the property 'Name' is not
		/// provided by the ADSI provider (e.g. the IIS ADSI provider)
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to query</param>
		/// <returns>returns the value of the obejct name</returns>
		public static string GetObjectName(string AdsiPath)
		{
			// connect to the selected ADSI path
			DirectoryEntry DirEntry = new DirectoryEntry(AdsiPath);
			string ObjectName = String.Empty;

			try
			{
				// get the object name
				ObjectName = DirEntry.Name;
			}

			// catch any exception accessing the directory object
			catch (Exception)
			{ }

			// close the directory object
			finally
			{
				DirEntry.Close();
			}

			// return the object name
			return ObjectName;
		}

		/// <summary>
		/// adds a new windows user
		/// </summary>
		/// <param name="AdsiRootPath">the ADSI root object (can point to the local machine or the domain)</param>
		/// <param name="FullName">the full name of the user</param>
		/// <param name="Username">the username</param>
		/// <param name="Password">the password</param>
		public static void AddWindowsUser(string AdsiRootPath,string FullName,string Username,string Password)
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
		public static void EditWindowsUser(string AdsiPath,string FullName,string Username,string Password)
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
		public static void DeleteWindowsUser(string AdsiPath,string Username)
		{
			DeleteDirectoryObject(AdsiPath, Username,UserSchemaClassName);
		}

		/// <summary>
		/// adds a new windows group
		/// </summary>
		/// <param name="AdsiRootPath">the ADSI root object (can point to the local machine or the domain)</param>
		/// <param name="Groupname">the group name</param>
		/// <param name="Description">the group description</param>
		/// <param name="GroupType">the group type</param>
		public static void AddWindowsGroup(string AdsiRootPath,string Groupname,string Description,int GroupType)
		{
			object[,] Properties = new object[,] { { GroupTypeSchemaName, GroupType }, { DescriptionSchemaName, Description } };

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
		public static void EditWindowsGroup(string AdsiPath,string Groupname,string Description,int GroupType)
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
		public static void DeleteWindowsGroup(string AdsiPath,string Groupname)
		{
			DeleteDirectoryObject(AdsiPath, Groupname,GroupSchemaClassName);
		}

		/// <summary>
		/// adds a new windows service
		/// </summary>
		/// <param name="AdsiRootPath">the ADSI root object (can point to the local machine or the domain)</param>
		/// <param name="Servicename">the service name</param>
		/// <param name="Description">the service description</param>
		/// <param name="Path">the path to the service executable</param>
		public static void AddWindowsService(string AdsiRootPath,string Servicename,string Description,string Path)
		{
			object[,] Properties = new object[,] { { DisplayNameSchemaClassName, Description }, { PathSchemaClassName, Path }, { ServiceTypeSchemaClassName, 16 }, { StartTypeSchemaClassName, 2 }, { ErrorControlSchemaClassName, 1 } };

			// invoke add Adsi object 
			AddDirectoryObject(AdsiRootPath, Servicename, ServiceSchemaClassName, Properties, null);
		}

		/// <summary>
		/// edit the properties of an existing windows service (can not change the service name)
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the service object</param>
		/// <param name="Servicename">the service name</param>
		/// <param name="Description">the service description</param>
		/// <param name="Path">the path to the service executable</param>
		public static void EditWindowsService(string AdsiPath,string Servicename,string Description,string Path)
		{
			object[,] Properties = new object[,] { { DisplayNameSchemaClassName, Description }, { PathSchemaClassName, Path }, { ServiceTypeSchemaClassName, 16 }, { StartTypeSchemaClassName, 2 }, { ErrorControlSchemaClassName, 1 } };

			// invoke edit Adsi object 
			EditDirectoryObject(AdsiPath, Servicename, ServiceSchemaClassName, Properties, null);
		}

		/// <summary>
		/// removes an existing windows service
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the service object</param>
		/// <param name="Servicename">the service name</param>
		public static void DeleteWindowsService(string AdsiPath,string Servicename)
		{
			DeleteDirectoryObject(AdsiPath, Servicename,ServiceSchemaClassName);
		}

		/// <summary>
		/// adds a new web site
		/// </summary>
		/// <param name="AdsiRootPath">the ADSI root object (points to the IIS root)</param>
		/// <param name="Websitename">the web site name</param>
		/// <param name="Path">the root path of the new web site</param>
		/// <param name="LogType">the web site log type</param>
		public static void AddWebSite(string AdsiRootPath,string Websitename,string Path,string LogType)
		{
			object[,] Properties = new object[,] { { LogTypeSchemaClassname, LogType } };

			// invoke edit Adsi object 
			AddDirectoryObject(AdsiRootPath, Websitename, WebSiteSchemaClassName, Properties, null);

			// now add the root folder to the newly created web site
			AddWebfolder(AdsiRootPath + PathSeparator + Websitename, RootWebFolder, Path);
		}

		/// <summary>
		/// edit the properties of an existing web site
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the service object</param>
		/// <param name="Websitename">the web site name</param>
		/// <param name="LogType">the web site log type</param>
		public static void EditWebSite(string AdsiPath, string Websitename, string LogType)
		{
			object[,] Properties = new object[,] { { LogTypeSchemaClassname, LogType } };

			// invoke edit Adsi object 
			EditDirectoryObject(AdsiPath, Websitename, WebSiteSchemaClassName, Properties, null);
		}

		/// <summary>
		/// removes an existing web site
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the service object</param>
		/// <param name="Websitename">the web site name</param>
		public static void DeleteWebSite(string AdsiPath,string Websitename)
		{
			DeleteDirectoryObject(AdsiPath, Websitename, WebSiteSchemaClassName);
		}

		/// <summary>
		/// adds a new web folder
		/// </summary>
		/// <param name="AdsiRootPath">the ADSI root object (points to the IIS root)</param>
		/// <param name="Webfoldername">the web folder name</param>
		/// <param name="Path">the path of the web folder</param>
		public static void AddWebfolder(string AdsiRootPath,string Webfoldername,string Path)
		{
			object[,] Properties = new object[,] { { PathSchemaClassName, Path } };

			// invoke edit Adsi object 
			AddDirectoryObject(AdsiRootPath, Webfoldername, WebFolderSchemaClassName, Properties, null);
		}

		/// <summary>
		/// edit the properties of an existing web folder
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the service object</param>
		/// <param name="Webfoldername">the web folder name</param>
		/// <param name="Path">the path of the web folder</param>
		public static void EditWebfolder(string AdsiPath,string Webfoldername,string Path)
		{
			object[,] Properties = new object[,] { { PathSchemaClassName, Path } };

			// invoke edit Adsi object 
			EditDirectoryObject(AdsiPath, Webfoldername, WebFolderSchemaClassName, Properties, null);
		}

		/// <summary>
		/// removes an existing web folder
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the service object</param>
		/// <param name="Webfoldername">the web folder name</param>
		public static void DeleteWebfolder(string AdsiPath,string Webfoldername)
		{
			DeleteDirectoryObject(AdsiPath, Webfoldername, WebFolderSchemaClassName);
		}

		/// <summary>
		/// adds a new ldap container
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the ldap container object</param>
		/// <param name="Containername">the container name</param>
		/// <param name="DisplayName">the display name</param>
		public static void AddLdapContainer(string AdsiPath,string Containername,string DisplayName)
		{
			object[,] Properties = new object[,] { { DisplayNameSchemaClassName, DisplayName } };

			// if the CN= prefix is missing then add it now
			Containername = Containername.StartsWith(LdapCnPrefix) ? Containername : LdapCnPrefix + Containername;

			// invoke edit Adsi object 
			AddDirectoryObject(AdsiPath, Containername, LdapContainerSchemaClassName, Properties, null);
		}

		/// <summary>
		/// edit the properties of an existing ldap container
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the ldap container object</param>
		/// <param name="Containername">the container name</param>
		/// <param name="DisplayName">the display name</param>
		public static void EditLdapContainer(string AdsiPath,string Containername,string DisplayName)
		{
			object[,] Properties = new object[,] { { DisplayNameSchemaClassName, DisplayName } };

			// if the CN= prefix is missing then add it now
			Containername = Containername.StartsWith(LdapCnPrefix) ? Containername : LdapCnPrefix + Containername;

			// invoke edit Adsi object 
			EditDirectoryObject(AdsiPath, Containername, LdapContainerSchemaClassName, Properties, null);
		}

		/// <summary>
		/// removes an existing ldap container
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the service object</param>
		public static void DeleteLdapContainer(string AdsiPath)
		{
			DeleteDirectoryTree(AdsiPath);
		}

		/// <summary>
		/// adds a new ldap user
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the ldap user object</param>
		/// <param name="Username">the user name</param>
		/// <param name="DisplayName">the display name</param>
		/// <param name="GivenName">the given name</param>
		public static void AddLdapUser(string AdsiPath,string Username,string DisplayName,string GivenName)
		{
			object[,] Properties = new object[,] { { DisplayNameSchemaClassName, DisplayName }, { LdapGivenNameSchemaClassName, GivenName } };

			// if the CN= prefix is missing then add it now
			Username = Username.StartsWith(LdapCnPrefix) ? Username : LdapCnPrefix + Username;

			// invoke edit Adsi object 
			AddDirectoryObject(AdsiPath, Username, LdapUserSchemaClassName, Properties, null);
		}

		/// <summary>
		/// edit the properties of an existing ldap user
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the ldap user object</param>
		/// <param name="Username">the user name</param>
		/// <param name="DisplayName">the display name</param>
		/// <param name="GivenName">the given name</param>
		public static void EditLdapUser(string AdsiPath,string Username,string DisplayName,string GivenName)
		{
			object[,] Properties = new object[,] { { DisplayNameSchemaClassName, DisplayName }, { LdapGivenNameSchemaClassName, GivenName } };

			// if the CN= prefix is missing then add it now
			Username = Username.StartsWith(LdapCnPrefix) ? Username : LdapCnPrefix + Username;

			// invoke edit Adsi object 
			EditDirectoryObject(AdsiPath, Username, LdapUserSchemaClassName, Properties, null);
		}

		/// <summary>
		/// removes an existing ldap user
		/// </summary>
		/// <param name="AdsiPath">the Adsi path to the service object</param>
		public static void DeleteLdapUser(string AdsiPath)
		{
			DeleteDirectoryTree(AdsiPath);
		}

		/// <summary>
		/// creates a new ldap schema class
		/// </summary>
		/// <param name="AdsiSchemaPath">the path to the Ldap schema root</param>
		/// <param name="ClassName">the calss name</param>
		/// <param name="SubClassOf">this class is a sub class of</param>
		/// <param name="ClassOID">the last digits of the class OID - format: 1.2.840.113556.1.6.1.2.ClassOID</param>
		public static void CreateLdapClass(string AdsiSchemaPath,string ClassName,string SubClassOf,string ClassOID)
		{
			object[,] Properties = new object[,] { { LdapSubClassSchemaClassName, SubClassOf }, 
												   { LdapClassIDSchemaClassName, ClassOIDBase + ClassOID } };

			// if the CN= prefix is missing then add it now
			ClassName = ClassName.StartsWith(LdapCnPrefix) ? ClassName : LdapCnPrefix + ClassName;

			// invoke edit Adsi object 
			AddDirectoryObject(AdsiSchemaPath, ClassName, LdapClassSchemaClassName, Properties, null);
		}

		/// <summary>
		/// creates for all the Ldap types the OMSyntax and AttributeSyntax values; these values are defined by
		/// Ldap; http://msdn.microsoft.com/library/default.asp?url=/library/en-us/ad/ad/choosing_a_syntax.asp
		/// </summary>
		/// <param name="AttrType">the attribute type</param>
		/// <param name="OMSyntax">out: the OM Syntax value</param>
		/// <param name="AttributeSyntax">out: the Attribute Syntax value</param>
		private static void GetAttributeTypeInfo(LdapAttributeTypes AttrType,out int OMSyntax, out string AttributeSyntax)
		{
			switch (AttrType)
			{
				// A string that represents an array of bytes. This syntax is used to store binary data
				case LdapAttributeTypes.BinaryData:
					{
						OMSyntax = 4;
						AttributeSyntax = "2.5.5.10";
						break;
					}

				// An octet string that contains a string value and a DN.
				case LdapAttributeTypes.BinaryDataWithDN:
					{
						OMSyntax = 127;
						AttributeSyntax = "2.5.5.14";
						break;
					}

				// Represents a Boolean value.
				case LdapAttributeTypes.Boolean:
					{
						OMSyntax = 1;
						AttributeSyntax = "2.5.5.8";
						break;
					}

				// String that contains a distinguished name (DN).
				case LdapAttributeTypes.DNReference:
					{
						OMSyntax = 127;
						AttributeSyntax = "2.5.5.1";
						break;
					}

				// Represents a 32-bit signed integer value.
				case LdapAttributeTypes.Integer:
					{
						OMSyntax = 2;
						AttributeSyntax = "2.5.5.9";
						break;
					}

				// Represents a 64-bit signed integer value.
				case LdapAttributeTypes.LargeInteger:
					{
						OMSyntax = 65;
						AttributeSyntax = "2.5.5.16";
						break;
					}

				// String that contains a distinguished name (DN).
				case LdapAttributeTypes.LinkedDN:
					{
						OMSyntax = 127;
						AttributeSyntax = "2.5.5.1";
						break;
					}

				// An octet string that contains a Windows NT/Windows 2000 security descriptor.
				case LdapAttributeTypes.SecurityDescriptor:
					{
						OMSyntax = 66;
						AttributeSyntax = "2.5.5.15";
						break;
					}

				// An octet string that contains a security identifier (SID).
				case LdapAttributeTypes.SecurityIdentifier:
					{
						OMSyntax = 4;
						AttributeSyntax = "2.5.5.17";
						break;
					}

				// A case insensitive string that contains characters from the teletex character set.
				case LdapAttributeTypes.String:
					{
						OMSyntax = 20;
						AttributeSyntax = "2.5.5.4";
						break;
					}

				// An octet string that contains a string value and a DN.
				case LdapAttributeTypes.StringWithDN:
					{
						OMSyntax = 127;
						AttributeSyntax = "2.5.5.14";
						break;
					}

				// A time string format defined by ASN.1 standards. For more information, see ISO 8601 and X680.
				case LdapAttributeTypes.Time:
					{
						OMSyntax = 24;
						AttributeSyntax = "2.5.5.11";
						break;
					}

				// unknown data type
				default:
					{
						OMSyntax = Int16.MinValue;
						AttributeSyntax = String.Empty;
						break;
					}
			}
		}

		/// <summary>
		/// create a new ldap attribute
		/// </summary>
		/// <param name="AdsiSchemaPath">the path to the Ldap schema root</param>
		/// <param name="AttributeName">the attribute name</param>
		/// <param name="IsSingleValued">is the attribute single valued or not</param>
		/// <param name="AttributeOID">the last digits of the class OID - format: 1.2.840.113556.1.6.1.1.AttributeOID</param>
		/// <param name="AttrType">the type of the attribute</param>
		public static void CreateLdapAttribute(string AdsiSchemaPath,string AttributeName,bool IsSingleValued,string AttributeOID,LdapAttributeTypes AttrType)
		{
			int OMSyntax = Int16.MinValue;
			string AttributeSyntax = String.Empty;

			// get the OM and attribute syntax from the requested attribute type
			GetAttributeTypeInfo(AttrType,out OMSyntax,out AttributeSyntax);

			// define all the attribute properties
			object[,] Properties = new object[,] { { LdapAttributeOMSyntaxSchemaClassName, OMSyntax }, 
												   { LdapAttributeDisplayNameSchemaClassName, AttributeName }, 
												   { LdapIsSingleValuedSchemaClassName, IsSingleValued }, 
												   { LdapAttributeSyntaxSchemaClassName, AttributeSyntax }, 
												   { LdapAttributeIDSchemaClassName, AttributeOIDBase + AttributeOID } };

			// if the CN= prefix is missing then add it now
			AttributeName = AttributeName.StartsWith(LdapCnPrefix) ? AttributeName : LdapCnPrefix + AttributeName;

			// invoke edit Adsi object 
			AddDirectoryObject(AdsiSchemaPath, AttributeName, LdapAttributeSchemaClassName, Properties, null);
		}
	}
}
