using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace EDMApiWebRole
{
 public class RSAClass
 {
     private static string _publicKey = AuthorizeToken.GetToken("public");
     private static string _privateKey = AuthorizeToken.GetToken("private");
     //private static string _privateKey = "<RSAKeyValue><Modulus>WP+tZymPWWS9vh4EJGYbermlC2X2gqsTHFVF2St6H1BtseAy0p2awdSfEQXMHWKK5sAo6hTfiddPbGLmpBK4rQ==</Modulus><Exponent>AQAB</Exponent><P>q+JMlnuY0Q/G4FRzxp2yfFI50tWYPjJOqCtWa1YnMg0=</P><Q>hI16iFa4w2g/p+PG4hV7EJ/bbRvjMEF2yMcJDbsqGSE=</Q><DP>Og+1g1e45VYI/hpJCZyXgDteYQPZ65iezVvmU1fE4bk=</DP><DQ>FEILGAso8bRdBiupmaPuyvujbWl1r0pR/R1uJMsWBAE=</DQ><InverseQ>oD4GalTKvBrW7g0uSAg4K0HyN1vxfDdUhkx0+n2A1ro=</InverseQ><D>OmTHaYCjRYwL0snu/dLhBMz5tVjZPTLx/w0UH0Gfhxt+nffGX8uB9Lr70pK2UeEjH31kDnje8hQXXQcSxc+UAQ==</D></RSAKeyValue>";
     //private static string _publicKey = "<RSAKeyValue><Modulus>WP+tZymPWWS9vh4EJGYbermlC2X2gqsTHFVF2St6H1BtseAy0p2awdSfEQXMHWKK5sAo6hTfiddPbGLmpBK4rQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
  private static UnicodeEncoding _encoder = new UnicodeEncoding();


  public static string Decrypt(string data) {

   var rsa = new RSACryptoServiceProvider();
   var dataArray = data.Split(new char[] { ',' });
   byte[] dataByte = new byte[dataArray.Length];
   for (int i = 0; i < dataArray.Length; i++)
   {
    dataByte[i] = Convert.ToByte(dataArray[i]);
   }

   rsa.FromXmlString(_privateKey);
   var decryptedByte = rsa.Decrypt(dataByte, false);
   return _encoder.GetString(decryptedByte);

  }

  public static string  Encrypt(string data) {

   var rsa = new RSACryptoServiceProvider();
   rsa.FromXmlString(_publicKey);
   var dataToEncrypt = _encoder.GetBytes(data);
   var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false).ToArray();
   var length = encryptedByteArray.Count();
   var item = 0;
   var sb = new StringBuilder();
   foreach (var x in encryptedByteArray)
   {
    item++;
    sb.Append(x);

    if (item < length)
     sb.Append(",");
   }

   return sb.ToString();
  
  }



 }
}