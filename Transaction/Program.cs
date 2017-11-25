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
            Console.WriteLine("Begin received coins");
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

            Console.WriteLine("Begin outputs");
            foreach (var output in outputs)
            {
                Money amount = output.Value;
                Console.WriteLine(amount.ToDecimal(MoneyUnit.BTC));

                var paymentScript = output.ScriptPubKey;
                Console.WriteLine(paymentScript);

                var address = paymentScript.GetDestinationAddress(Network.Main);
                Console.WriteLine(address);
            }

            // inputs
            TxInList inputs = nBitcoinTransaction.Inputs;

            Console.WriteLine("Begin inputs");
            foreach (var input in inputs)
            {
                OutPoint previousOutPoint = input.PrevOut;
                Console.WriteLine(previousOutPoint);
                Console.WriteLine(previousOutPoint.Hash); // hash of prev tx
                Console.WriteLine(previousOutPoint.N); // index of out from prev tx, that has been spent in current tx
            }

            // The terms TxOut, Output and out are synonymous.

            // create a txout with 21 bitcoin from the first ScriptPubKey in our current transaction:
            Console.WriteLine("Begin txout");
            Money twentyBtc = new Money(21, MoneyUnit.BTC);
            var scriptPubKey = nBitcoinTransaction.Outputs.FirstOrDefault()?.ScriptPubKey;
            TxOut txOut = new TxOut(twentyBtc, scriptPubKey);

            Console.WriteLine(txOut.Value.ToDecimal(MoneyUnit.BTC));
            Console.WriteLine(txOut.ScriptPubKey);

            // every TxOut is uniq addressed by at the blockchain level by the id of the tx, including the index
            // e.g., OutPoint of TxOut is 

            Console.WriteLine("Begin outpoint");
            OutPoint firstOutPoint = receivedCoins.FirstOrDefault()?.Outpoint;
            Console.WriteLine(firstOutPoint?.Hash);
            Console.WriteLine(firstOutPoint?.N);

            Console.WriteLine("Begin txin");
            // TxIn is composed of the Outpoint of the TxOut being spent and of the ScriptSig (we can see the ScriptSig as the “Proof of Ownership”).
            // with the previous transaction ID, we can review the info associated with that tx
            OutPoint firstPreviousOutPoint = nBitcoinTransaction.Inputs.FirstOrDefault()?.PrevOut;
            if (firstPreviousOutPoint != null)
            {
                var firstPreviousTransaction = client.GetTransaction(firstPreviousOutPoint.Hash).Result.Transaction;
                Console.WriteLine(firstPreviousTransaction.IsCoinBase);
            }

            Console.WriteLine("begin coin base hunt");
            TraceCoinBase(nBitcoinTransaction, client);
            Console.ReadLine();
        }

        public static void TraceCoinBase(NBitcoin.Transaction nBitcoinTransaction, QBitNinjaClient client)
        {
            var first = nBitcoinTransaction.Inputs.FirstOrDefault();
            if (first != null)
            {
                var firstPrevOut = first.PrevOut;
                var transaction = client.GetTransaction(firstPrevOut.Hash).Result.Transaction;
                if (!transaction.IsCoinBase)
                {
                    Console.WriteLine($"{firstPrevOut} not coinbase");
                    TraceCoinBase(transaction, client);
                }
                Console.WriteLine($"{firstPrevOut} is coinbase");
            }

        }
    }
}
