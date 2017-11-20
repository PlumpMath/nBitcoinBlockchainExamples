using System;
using NBitcoin;

namespace ScriptPubKey
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // as the Blockchain is concerned, there is no such thing as a Bitcoin Address.
            // Internally, the Bitcoin protocol identifies the recipient of Bitcoin by a ScriptPubKey.
            // e.g., OP_DUP OP_HASH160 14836dbe7f38c5ac3d49e8d790af808a4ee9edcf OP_EQUALVERIFY OP_CHECKSIG

            // ScriptPubKey explains the conditions that must be met to claim ownership of bitcoin

            // generate the scriptpubkey from the bitcoin address
            var publicKeyHash = new KeyId("14836dbe7f38c5ac3d49e8d790af808a4ee9edcf");

            var testNetAddress = publicKeyHash.GetAddress(Network.TestNet);
            var mainNetAddress = publicKeyHash.GetAddress(Network.Main);

            // scriptpubkeys will contain hash of public key
            Console.WriteLine(mainNetAddress.ScriptPubKey);
            Console.WriteLine(testNetAddress.ScriptPubKey);


            // Bitcoin Addresses are composed of a version byte which identifies the network where to use the address and the hash of a public key.
            // So we can go backwards and generate a bitcoin address from the ScriptPubKey and the network identifier.
            var paymentScript = publicKeyHash.ScriptPubKey;
            var sameMainNetAddress = paymentScript.GetDestinationAddress(Network.Main);

            Console.WriteLine(mainNetAddress == sameMainNetAddress);

            // also possible to retrieve the hash from the ScriptPubKey and generate a Bitcoin Address from it
            var samePublicKeyHash = paymentScript.GetDestination() as KeyId;
            Console.WriteLine(publicKeyHash == samePublicKeyHash);

            var sameMainNetAddress2 = new BitcoinPubKeyAddress(samePublicKeyHash, Network.Main);
            Console.WriteLine(mainNetAddress == sameMainNetAddress2);

            // Note: A ScriptPubKey does not necessarily contain the hashed public key(s) permitted to spend the bitcoin.

            Console.ReadLine();
        }
    }
}
