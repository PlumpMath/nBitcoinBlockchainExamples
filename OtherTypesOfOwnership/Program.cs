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
            ////
            // P2PKH (pay to public key hash) - private key -> public key -> pubilc key hash -> script pub key
            ////

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

            ////
            // why P2PKH is used:
            ////

            // 1. ECC is vulnerable to a modified Shor's algorithim for solving the discrete logarithim problem on ECs
            // (quantum computers retrieving private keys from public keys)
            // however, not reusing addresses avoids this risk

            // 2.as the hash is smaller (20 bytes), it's easier to embed into small storage units, e.g., QR codes

            ////
            // P2WPKH  (pay to witness public key hash)
            ////

            // the signature contains same info as P2PKH spend
            // but is located in the witness instead of the scriptSig

            // ScriptPubKey is changed
            // from: OP_DUP OP_HASH160 <public-key-hash> OP_EQUALVERIFY OP_CHECKSIG
            // to: 0 <public-key-hash>

            // get ScriptPubKey from a public key
            var key2 = new Key();
            Console.WriteLine(key2.PubKey.WitHash.ScriptPubKey);

            ////
            // multi sig
            ////

            var alice = new Key();
            var bob = new Key();
            var satoshi = new Key();

            var multiScriptPubKey = PayToMultiSigTemplate
                .Instance
                .GenerateScriptPubKey(2, bob.PubKey, alice.PubKey, satoshi.PubKey); // 2 sig required

            Console.WriteLine($"multi sig:\n{multiScriptPubKey}");

            // Transaction.Sign does not work for multi sig

            // multi sig received a coin in a tx:
            var received = new Transaction();
            received.Outputs.Add(new TxOut(Money.Coins(1.0m), multiScriptPubKey));

            // bob and alice agree to pay nico 1.0 btc

            // first get the coin
            var coin = received.Outputs.AsCoins().First();

            // create unsigned tx
            var nicosAddress = new Key().PubKey.GetAddress(Network.Main);
            var builder = new TransactionBuilder();

            Transaction unsigned = builder
                .AddCoins(coin)
                .Send(nicosAddress, Money.Coins(1.0m))
                .BuildTransaction(sign: false);

            // alice signs
            Transaction aliceSigned = builder
                .AddCoins(coin)
                .AddKeys(alice)
                .SignTransaction(unsigned);

            // bob signs
            Transaction bobSigned = builder
                .AddCoins(coin)
                .AddKeys(bob)
                // at this point,
                // SignTransaction(unsigned) has the identical functionality with the SignTransaction(aliceSigned)
                // because unsigned transaction has alread been signed by Alice priv key above
                .SignTransaction(aliceSigned);
            

            // now combine both sigs into 1 tx
            Transaction fullySigned = builder
                .AddCoins(coin)
                // technically, combinesignatures is not needed as bob signed alice's signed tx,
                // combinesignatures is needed if both signers signed the unsigned tx
                .CombineSignatures(aliceSigned, bobSigned);

            Console.WriteLine(fullySigned);

            ////
            // P2SH (pay to script hash)
            ////

            // recently native pay to multi sig and native pay to public key are never used directly
            // instead they are wrapped into Pay To Script Hash


            // P2SH is an easy way to represent a scriptPubKey as a simple BitcoinScriptAddress
            var bob2 = new Key();
            var alice2 = new Key();
            var satoshi2 = new Key();

            var paymentScript = PayToMultiSigTemplate
                .Instance
                .GenerateScriptPubKey(2, bob.PubKey, alice.PubKey, satoshi.PubKey).PaymentScript;

            // the P2SH scriptPubKey represent the hash of the multi-sig script: redeemScript.Hash.ScriptPubKey
            Console.WriteLine(paymentScript);

            // since it's a hash, can easily convert to base58
            // in P2SH payments, the hash of the redeem script is referred to as the scriptPubKey
            Console.WriteLine(paymentScript.Hash.GetAddress(Network.Main));


            // anyone can send payment to such an address and only see the hash of the redeem script
            // with no knowledge of the redeem script, or the fact that it's a multi sig


            // signing the tx is similar to previous methods,
            // only difference being that you also provide the redeem script
            Script redeemScript = 
                PayToMultiSigTemplate
                .Instance
                .GenerateScriptPubKey(2, bob2.PubKey, alice2.PubKey, satoshi2.PubKey);

            var receivedTx = new Transaction();

            // WARNING: payment is sent to redeemScript.Hash and not to redeemScript
            receivedTx.Outputs.Add(new TxOut(Money.Coins(1.0m), redeemScript.Hash));

            // when the owners of the multi sig address wan to spend the coins,
            // the need to create a ScriptCoin
            ScriptCoin scriptCoin = receivedTx.Outputs
                .AsCoins()
                .First()
                .ToScriptCoin(redeemScript);


            // the rest of the code converning tx generation and signing is exactly the same

            Console.ReadLine();
        }
    }
}
