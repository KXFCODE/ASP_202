using System;
namespace ASP_202.Services.Kdf
{
	public interface IKdfService
	{

        /// <summary>
        /// Make Derived Key from password and salt
        /// name
        /// </summary>
        /// <param name="password">Password string</param>
        /// <param name="salt">Salt string</param>
        /// <returns>Derived Key string</returns>

        String GetDerivedKey(String password, String salt);
	}
}

