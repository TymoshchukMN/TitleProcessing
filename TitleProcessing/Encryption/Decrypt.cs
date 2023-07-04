using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TitleProcessing.Encription
{
    public static class Decrypt
    {
        public static string DecryptCipherTextToPlainText(string cipherText)
        {
            const string KeyPath = "N:\\Personal\\TymoshchukMN\\" +
                "TitleProcessingConfigs\\SecurityKey.txt";

            byte[] toEncryptArray = Convert.FromBase64String(cipherText);
            MD5CryptoServiceProvider objMD5CryptoService
                = new MD5CryptoServiceProvider();

            string securityKey = File.ReadAllText(KeyPath);

            // Gettting the bytes from the Security Key and Passing it to compute the Corresponding Hash Value.
            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(
                Encoding.UTF8.GetBytes(securityKey));
            objMD5CryptoService.Clear();

            var objTripleDESCryptoService
                = new TripleDESCryptoServiceProvider();

            // Assigning the Security key to the TripleDES Service Provider.
            objTripleDESCryptoService.Key = securityKeyArray;

            // Mode of the Crypto service is Electronic Code Book.
            objTripleDESCryptoService.Mode = CipherMode.ECB;

            // Padding Mode is PKCS7 if there is any extra byte is added.
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

            var objCrytpoTransform
                = objTripleDESCryptoService.CreateDecryptor();

            // Transform the bytes array to resultArray
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(
                toEncryptArray, 0, toEncryptArray.Length);

            objTripleDESCryptoService.Clear();

            // Convert and return the decrypted data/byte into string format.
            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
