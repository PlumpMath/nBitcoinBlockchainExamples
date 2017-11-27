using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using QBitNinja.Client;

namespace SpendYourCoins
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // TODO: replace with real details

            // EXAMPLES 

            // creating key and address example
            //var network = Network.Main;

            //var privateKey = new Key();
            //var bitcoinPrivateKey = privateKey.GetWif(network);
            //var address = bitcoinPrivateKey.GetAddress();

            //Console.WriteLine(bitcoinPrivateKey);
            //Console.WriteLine(address);

            // import priavate key
            var importedBitcoinPrivateKey = new BitcoinSecret("cSZjE4aJNPpBtU6xvJ6J4iBzDgTmzTjbq8w2kqnYvAprBCyTsG4x");
            var network = importedBitcoinPrivateKey.Network;
            var address = importedBitcoinPrivateKey.GetAddress();

            Console.WriteLine(importedBitcoinPrivateKey);
            Console.WriteLine(address);

            // get tx info
            var client = new QBitNinjaClient(network);
            var transactionId = uint256.Parse("e44587cf08b4f03b0e8b4ae7562217796ec47b8c91666681d71329b764add2e3");
            var transactionResponse = client.GetTransaction(transactionId).Result;

            Console.WriteLine(transactionResponse.TransactionId);
            Console.WriteLine(transactionResponse.Block.Confirmations);


            Console.ReadLine();
        }
    }
}
