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
            var transaction = GetTxResponse().GetAwaiter();
            var result = transaction.GetResult();


            Console.WriteLine(transaction);

            // get NBitcoin.Transaction type
            var nBitcoinTransaction = result.Transaction;

            Console.WriteLine(nBitcoinTransaction);


        }

        private static async Task<GetTransactionResponse> GetTxResponse()
        {
            // create client
            var client = new QBitNinjaClient(Network.Main);

            // parse tx id to NBitcoin.uint256 for client
            var transactionId = uint256.Parse("f13dc48fb035bbf0a6e989a26b3ecb57b84f85e0836e777d6edf60d87a4a2d94");

            // query tx
            var transactionResponse = await client.GetTransaction(transactionId);

            return transactionResponse;
        }
    }
}
