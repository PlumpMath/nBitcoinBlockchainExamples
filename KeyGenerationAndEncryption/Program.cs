﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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

            Console.ReadLine();
        }
    }
}
