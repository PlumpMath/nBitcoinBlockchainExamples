using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;

namespace Transaction
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // create client
            var client = new QBitNinjaClient(Network.Main);

            // parse tx id to NBitcoin.uint256 for client
            var transactionId = uint256.Parse("f13dc48fb035bbf0a6e989a26b3ecb57b84f85e0836e777d6edf60d87a4a2d94");

            // query tx
            GetTransactionResponse transactionResponse = client.GetTransaction(transactionId).Result;

            // get NBitcoin.Transaction type
            NBitcoin.Transaction nBitcoinTransaction = transactionResponse.Transaction;

            var fromTransactionClass = nBitcoinTransaction.GetHash();

            var fromGetTransactionResponseClass = transactionResponse.TransactionId;

            Console.WriteLine(fromTransactionClass == transactionId && fromGetTransactionResponseClass == transactionId);

            Console.ReadLine();
        }
    }
}
