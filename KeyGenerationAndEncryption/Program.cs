using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Crypto;

namespace KeyGenerationAndEncryption
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // add entropy before creating key
            RandomUtils.AddEntropy("hello");
            RandomUtils.AddEntropy(new byte[] {1, 2, 3});

            // key created with added entropy
            var nsaProofKey = new Key();

            // What NBitcoin does when you call AddEntropy(data) is:
            // additionalEntropy = SHA(SHA(data) ^ additionalEntropy)

            // Then when you generate a new number:
            // result = SHA(PRNG() ^ additionalEntropy)

            // Key Derivation Function is a way to have a stronger key, even if your entropy is low
            // KDF is a hash function that waste computing resources on purpose.

            var derived = SCrypt.BitcoinComputeDerivedKey("hello", new byte[] {1, 2, 3});
            RandomUtils.AddEntropy(derived);

            // even if attacker knows that your source of entropy contains 5 letters,
            // they will need to run Scrypt to check each possibility

        }
    }
}
