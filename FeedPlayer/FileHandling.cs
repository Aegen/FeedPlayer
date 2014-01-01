using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.IO;

namespace FeedPlayer
{
	class FileHandling
	{
		private static readonly byte[] Entropy = new byte[]{ 5, 43, 18, 97, 19 };


		public static void StoreCredentials(string username, string password)
		{
			byte[] byteRepresentation = Encoding.Unicode.GetBytes(username + "," + password);
			Encoding.

			byte[] encryptedRepresentation = ProtectedData.Protect(byteRepresentation, Entropy, DataProtectionScope.CurrentUser);

			File.WriteAllBytes(@"\CredentialsLocker.config", encryptedRepresentation);
		}

		public static string[] RetrieveCredentials()
		{

			byte[] protectedBytes = File.ReadAllBytes(@"\CredentialsLocker.config");

			byte[] restoredBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);

			string stringRepresentation = Encoding.Unicode.GetString(restoredBytes);

			string[] splitRepresentation = stringRepresentation.Split(',');

			if (splitRepresentation.Length != 2)
				return null;

			return splitRepresentation;
		}
	}
}
