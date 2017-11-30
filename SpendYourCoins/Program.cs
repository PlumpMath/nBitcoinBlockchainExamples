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

            // FROM: coin to spend
            var receivedCoins = transactionResponse.ReceivedCoins;

            var coinToSpend = receivedCoins.FirstOrDefault(rc => rc.TxOut.ScriptPubKey == importedBitcoinPrivateKey.ScriptPubKey);

            if (coinToSpend == null)
            {
                Console.WriteLine("txOut doesn't contain ScriptPubKey");
                return;
            }

            var outPointToSpend = coinToSpend.Outpoint;

            Console.WriteLine("we want to spend{0}. outpoint: ", outPointToSpend.N +1);

            // transaction to send
            var transaction = new Transaction
            {
                Inputs =
                {
                    new TxIn
                    {
                        PrevOut = outPointToSpend
                    }
                }
            };

            // TO: address to send to
            // change this to testnet address when actually sending
            var hallOfTheMakerAddress = BitcoinAddress.Create("mzp4No5cmCXjZUpf112B1XWsvWBfws5bbB");

            // AMOUNT: how much btc to send
            var hallOfTheMakersTxOut = new TxOut
            {
                Value = new Money((decimal) 0.5, MoneyUnit.BTC),
                ScriptPubKey = hallOfTheMakerAddress.ScriptPubKey
            };

            // change back
            var changeBackTxOut = new TxOut
            {
                Value = new Money((decimal) 0.4999, MoneyUnit.BTC),
                ScriptPubKey = importedBitcoinPrivateKey.ScriptPubKey
            };

            transaction.Outputs.Add(hallOfTheMakersTxOut);
            transaction.Outputs.Add(changeBackTxOut);


            Console.ReadLine();
        }
    }
}
