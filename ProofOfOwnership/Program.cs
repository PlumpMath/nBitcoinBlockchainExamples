using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace ProofOfOwnership
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // signing a message


            var bitcoinPrivateKey = new BitcoinSecret("KzgjNRhcJ3HRjxVdFhv14BrYUKrYBzdoxQyR2iJBHG9SNGGgbmtC");
            const string message = "Craig Wright is a fraud";
            string signature = bitcoinPrivateKey.PrivateKey.SignMessage(message);

            Console.WriteLine(signature);

            var genesisBlockAddress = new BitcoinPubKeyAddress("1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa");
            var isCraigWrightSatoshi = genesisBlockAddress.VerifyMessage(message, signature);

            Console.WriteLine($"Is Craig Wright Satoshi? {isCraigWrightSatoshi}!");

            var bookAddress = new BitcoinPubKeyAddress("1KF8kUVHK42XzgcmJF4Lxz4wcL5WDL97PB");
            const string DoriersMessage = "Nicolas Dorier Book Funding Address";
            const string DoriersSignature = "H1jiXPzun3rXi0N9v9R5fAWrfEae9WPmlL5DJBj1eTStSvpKdRR8Io6/uT9tGH/3OnzG6ym5yytuWoA9ahkC3dQ=";

            var isDorierTheBookAddressOwner = bookAddress.VerifyMessage(DoriersMessage, DoriersSignature);

            Console.WriteLine(isDorierTheBookAddressOwner);

            Console.ReadLine();
        }
    }
}
