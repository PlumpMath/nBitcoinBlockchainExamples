using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace OtherTypesOfOwnership
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // P2PKH (pay to public key hash) - private key -> public key -> pubilc key hash -> script pub key

            var publicKeyHash = new Key().PubKey.Hash;
            var bitcoinAddress = publicKeyHash.GetAddress(Network.Main);

            // a bitcoin address is a hash of a public key
            Console.WriteLine(publicKeyHash);
            Console.WriteLine(bitcoinAddress);

            // technically, there is no such thing as a "bitcoin address"
            // the blockchain identifies a receiver with a ScriptPubKey
            // the ScriptPubKey could be generated from the adddress

            var scriptPubKey = bitcoinAddress.ScriptPubKey;
            Console.WriteLine(scriptPubKey);

            // and vice versa
            var sameBitconAddress = scriptPubKey.GetDestinationAddress(Network.Main);
            Console.WriteLine(bitcoinAddress == sameBitconAddress);


            // P2PK (pay to public key) - private key -> public key -> script pub key
            Block genesisBlock = Network.Main.GetGenesis();
            var firstTransactionEver = genesisBlock.Transactions.First();
            var firstOutPutEver = firstTransactionEver.Outputs.First();

            var firstScriptPubKey = firstOutPutEver.ScriptPubKey;
            BitcoinAddress firstBitcoinAddress = firstScriptPubKey.GetDestinationAddress(Network.Main);

            Console.WriteLine(firstBitcoinAddress == null);
            Console.WriteLine(firstTransactionEver.ToString());

            var key = new Key();
            Console.WriteLine($"Pay to public key: {key.PubKey.ScriptPubKey}");
            Console.WriteLine($"Pay to public key hash: {key.PubKey.Hash.ScriptPubKey}");


            // why P2PKH is used:

            // 1. ECC is vulnerable to a modified Shor's algorithim for solving the discrete logarithim problem on ECs
            // (quantum computers retrieving private keys from public keys)
            // however, not reusing addresses avoids this risk

            // 2.as the hash is smaller (20 bytes), it's easier to embed into small storage units, e.g., QR codes


            // P2WPKH  (pay to witness public key hash)

            // the signature contains same info as P2PKH spend
            // but is located in the witness instead of the scriptSig

            // ScriptPubKey is changed
            // from: OP_DUP OP_HASH160 <public-key-hash> OP_EQUALVERIFY OP_CHECKSIG
            // to: 0 <public-key-hash>

            // get ScriptPubKey from a public key
            var key2 = new Key();
            Console.WriteLine(key2.PubKey.WitHash.ScriptPubKey);



            Console.ReadLine();
        }
    }
}
