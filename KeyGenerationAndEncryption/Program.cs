﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.Stealth;

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
            // KDF is a hash function that wastes computing resources on purpose.

            var derived = SCrypt.BitcoinComputeDerivedKey("hello", new byte[] {1, 2, 3});
            RandomUtils.AddEntropy(derived);

            // even if attacker knows that your source of entropy contains 5 letters,
            // they will need to run Scrypt to check each possibility

            // standard for encrypting private key with a password using kdf is BIP38

            var privateKey = new Key();
            var bitcoinPrivateKey = privateKey.GetWif(Network.Main);

            Console.WriteLine(bitcoinPrivateKey);

            BitcoinEncryptedSecret encryptedBitcoinPrivateKey = bitcoinPrivateKey.Encrypt("password");
            Console.WriteLine(encryptedBitcoinPrivateKey);

            var decryptedBitcoinPrivateKey = encryptedBitcoinPrivateKey.GetKey("password");
            Console.WriteLine(decryptedBitcoinPrivateKey);

            Key keyFromIncorrectPassword = null;

            Exception error = null;

            try
            {
                keyFromIncorrectPassword = encryptedBitcoinPrivateKey.GetKey("lahsdlahsdlakhslkdash");
            }
            catch (Exception e)
            {
                error = e;
            }

            var result = keyFromIncorrectPassword != null
                ? keyFromIncorrectPassword.ToString()
                : $"{error?.GetType().Name ?? "Error"}: icorrect password";

            Console.WriteLine(result);


            // BIP 38
            // how to delegate Key and Address creation to an untrusted peer

            // create pass phrase code
            BitcoinPassphraseCode passphraseCode = new BitcoinPassphraseCode("my secret", Network.Main, null);
            Console.WriteLine(passphraseCode);

            // then give passPhraseCode to 3rd party key generator
            // third party can generate encrypted keys on your behalf
            // without knowing your password and private key.
            EncryptedKeyResult encryptedKeyResult = passphraseCode.GenerateEncryptedSecret();
            var generatedAddress = encryptedKeyResult.GeneratedAddress;
            var encryptedKey = encryptedKeyResult.EncryptedKey;

            // used by 3rd party to confirm generated key and address correspond to password
            var confirmationCode = encryptedKeyResult.ConfirmationCode;

            // check
            var isValid = confirmationCode.Check("my secret", generatedAddress);
            Console.WriteLine(isValid);

            var bitcoinPrivateKeyFromPassphraseCode = encryptedKey.GetSecret("my secret");
            Console.WriteLine(bitcoinPrivateKeyFromPassphraseCode.GetAddress() == generatedAddress);

            Console.WriteLine(bitcoinPrivateKey);

            // BIP 32
            // hierarchical deterministic wallets
            // prevent outdated backups

            var masterKey = new ExtKey();


            Console.WriteLine($"Master Key: {masterKey.ToString(Network.Main)}");

            for (uint i = 0; i < 5; i++)
            {
                ExtKey nextKey = masterKey.Derive(i);
                Console.WriteLine($"Key {i} : {nextKey.ToString(Network.Main)}");
            }

            // go back from a Key to ExtKey by supplying the Key and the ChainCode to the ExtKey constructor.

            var extKey = new ExtKey();
            byte[] chainCode = extKey.ChainCode;
            Key key = extKey.PrivateKey;

            var newExtKey = new ExtKey(key, chainCode);

            // the base58 type equivalent of ExtKey is BitcoinExtKey


            // "neuter" master key so 3rd party can generate public keys without knowing private key
            ExtPubKey masterPubKey = masterKey.Neuter();

            for (uint i = 0; i < 5; i++)
            {
                ExtPubKey pubKey = masterPubKey.Derive(i);
                Console.WriteLine($"PubKey {i} : {pubKey.ToString(Network.Main)}");
            }

            // get corresponding private key with master key
            masterKey = new ExtKey();
            masterPubKey = masterKey.Neuter();

            // 3rd party generates pubkey1
            ExtPubKey pubKey1 = masterPubKey.Derive(1);

            // get privatekey of pubKey1
            ExtKey key1 = masterKey.Derive(1);

            Console.WriteLine($"Generated address: {pubKey1.PubKey.GetAddress(Network.Main)}");
            Console.WriteLine($"Expected address: {key1.PrivateKey.PubKey.GetAddress(Network.Main)}");

            // deterministic keys
            
            // derive the 1st child of the 1st child

            ExtKey parent = new ExtKey();
            
            // method 1:
            ExtKey child11 = parent.Derive(1).Derive(1);

            // method 2:
            child11 = parent.Derive(new KeyPath("1/1"));

            // why use HD wallets ?
            // easier control, easier to classify keys for multiple accounts

            // but child keys can recover parent key (non-hardened)

            ExtKey ceoExtKey = new ExtKey();
            Console.WriteLine($"CEO: {ceoExtKey.ToString(Network.Main)}");
            ExtKey accountingKey = ceoExtKey.Derive(0, hardened: false);

            ExtPubKey ceoPubKey = ceoExtKey.Neuter();

            // recover ceo key with accounting private key and ceo public key
            ExtKey ceoKeyRecovered = accountingKey.GetParentExtKey(ceoPubKey);
            Console.WriteLine($"CEO recovered: {ceoKeyRecovered.ToString(Network.Main)}");

            // create a hardened key
            var privateCeoExtKey = new ExtKey();
            Console.WriteLine($"Private CEO: {privateCeoExtKey.ToString(Network.Main)}");
            var assitantExtKey = privateCeoExtKey.Derive(1, hardened: true);

            ExtPubKey privateCeoPubKey = privateCeoExtKey.Neuter();

            ExtKey privateCeoKeyRecovered = null;

            try
            {
                privateCeoKeyRecovered = assitantExtKey.GetParentExtKey(privateCeoPubKey);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
            }

            // creating hardened keys via keypath
            var isNonHardened = new KeyPath("1/2/3").IsHardened;
            Console.WriteLine(isNonHardened);

            var isHardened = new KeyPath("1/2/3'").IsHardened;
            Console.WriteLine(isHardened);

            // imagine that the Accounting Department generates 1 parent key for each customer, and a child for each of the customer’s payments.
            // As the CEO, you want to spend the money on one of these addresses.

            var accountingCeoKey = new ExtKey();
            string accounting = "1'"; // hardened with apostrophe
            int customerId = 5;
            int paymentId = 50;
            KeyPath path = new KeyPath($"{accounting}/{customerId}/{paymentId}");

            // path: 1/5/50
            ExtKey paymentKey = accountingCeoKey.Derive(path);

            Console.WriteLine(paymentKey);


            // mnemonic code for HD keys BIP 39
            // used for easy to write keys
            Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
            ExtKey hdRoot = mnemo.DeriveExtKey("my password");
            Console.WriteLine(mnemo);

            // recover hdRoot with mnemonic and password
            mnemo = new Mnemonic(mnemo.ToString(), Wordlist.English);
            ExtKey recoverdHdRoot = mnemo.DeriveExtKey("my password");

            Console.WriteLine(hdRoot.PrivateKey == recoverdHdRoot.PrivateKey);


            // dark wallet
            // Prevent outdated backups
            // Delegate key / address generation to an untrusted peer

            // bonus feature: only share one address (StealthAddress)

            var scanKey = new Key();
            var spendKey = new Key();
            var stealthAddress = new BitcoinStealthAddress(
                scanKey: scanKey.PubKey,
                pubKeys: new[] { spendKey.PubKey},
                signatureCount: 1,
                bitfield: null,
                network: Network.Main
                );

            // payer will take StealthAddress and generate temp key called Ephem Key, and generate a Stealth Pub Key
            // then they package the Ephem PubKey in Stealth Metadata obj embedded in OP_RETURN
            // they will also add the output to the generated bitcoin address (Stealth pub key)

            //The creation of the EphemKey is an implementation detail
            // it can be omitted as NBitcoin will generate one automatically:
            var ephemKey = new Key();
            var transaction = new Transaction();
            stealthAddress.SendTo(transaction, Money.Coins(1.0m), ephemKey);
            Console.WriteLine(transaction);

            // the Scanner knows the StealthAddress
            // and recovers the Stealth PubKey and Bitcoin Address with the Scan Key

            // scanner then checks if if one of the tx corresponds to the address
            // if true, Scanner notifies the Receiver about tx

            // the receiver can get the private key of the address with their spend key

            // note: a StealthAddress can have mulitple spend pubkeys 9multi sig)

           // limit: use of OP_RETURN makes embedding data in tx difficult
           // OP_RETURN limit is 40 bytes

            Console.ReadLine();
        }
    }
}
