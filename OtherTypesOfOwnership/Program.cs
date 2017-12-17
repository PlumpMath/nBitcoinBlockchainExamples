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
            // P2PK[H] (pay to public key [hash])

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
        }
    }
}
