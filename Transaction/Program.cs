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

            List<ICoin> receivedCoins = transactionResponse.ReceivedCoins;

            // received coins
            foreach (var coin in receivedCoins)
            {
                Money amount = coin.Amount as Money;
                Console.WriteLine(amount?.ToDecimal(MoneyUnit.BTC));

                var paymentScript = coin.TxOut.ScriptPubKey;
                Console.WriteLine(paymentScript);

                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine(address);

            }

            // outputs
            TxOutList outputs = nBitcoinTransaction.Outputs;

            foreach (var output in outputs)
            {
                Money amount = output.Value;
                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));

                var paymentScript = output.ScriptPubKey;
                Console.WriteLine(paymentScript);

                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.Write(address);
            }

            // inputs
            TxInList inputs = nBitcoinTransaction.Inputs;

            foreach (var input in inputs)
            {
                OutPoint previousOutPoint = input.PrevOut;
                Console.WriteLine(previousOutPoint);
                Console.WriteLine(previousOutPoint.Hash); // hash of prev tx
                Console.WriteLine(previousOutPoint.N); // idx of out from prev tx, that has been spent in current tx
            }

            Console.ReadLine();
        }
    }
}
