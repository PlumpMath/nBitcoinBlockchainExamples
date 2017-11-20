using System;
using NBitcoin;

namespace nBitcoinBlockchainExamples
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Private keys are often represented in Base58Check called a Bitcoin Secret
            // (also known as Wallet Import Format or simply WIF), like Bitcoin Addresses.
            var privateKey = new Key();

            /*
             Note:
                it is easy to go from BitcoinSecret to private Key, however
                it is impossible to go from a Bitcoin Address to Public Key
                because the Bitcoin Address contains a hash of the Public Key, not the Public Key itself.
            */

            // pass in enum value: Main/TestNet, for use on respective networks
            BitcoinSecret createdWithGetWif = privateKey.GetWif(Network.Main); // method has same body of .GetBitcoinSecret, returns same result
            BitcoinSecret createdWithGetBitcoinSecret = privateKey.GetBitcoinSecret(Network.Main);

            var wifIsBitcoinSecret = createdWithGetWif == createdWithGetBitcoinSecret;
            Console.WriteLine(wifIsBitcoinSecret); // true

            PubKey publicKey = privateKey.PubKey;
            BitcoinPubKeyAddress bitcoinPubKey = publicKey.GetAddress(Network.Main);

            Console.WriteLine(publicKey);
            Console.WriteLine(bitcoinPubKey);

            BitcoinEncryptedSecret encryptedBitcoinPrivateKey = createdWithGetWif.Encrypt("ilovecrypto");
            Console.WriteLine(encryptedBitcoinPrivateKey);

            BitcoinSecret secret = encryptedBitcoinPrivateKey.GetSecret("ilovecrypto");
            Console.WriteLine(secret);

            Console.WriteLine(secret == createdWithGetWif);

            // keep the console open to read
            Console.ReadLine();
        }
    }
}
