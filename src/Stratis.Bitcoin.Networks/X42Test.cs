using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class X42Test : X42Main
    {
        public X42Test()
        {
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0x42;
            messageStart[1] = 0x66;
            messageStart[2] = 0x52;
            messageStart[3] = 0x04;
            uint magic = BitConverter.ToUInt32(messageStart, 0); //0x4526642

            this.Name = "x42Test";
            this.NetworkType = NetworkType.Testnet;
            this.Magic = magic;
            this.DefaultPort = 62342;
            this.DefaultRPCPort = 62343;
            this.DefaultAPIPort = 42221;
            this.DefaultSignalRPort = 42223;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = Money.Zero;
            this.FallbackFee = Money.Zero;
            this.MinRelayTxFee = Money.Zero;
            this.RootFolderName = x42RootFolderName;
            this.DefaultConfigFilename = x42DefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "Tx42";
            this.DefaultBanTimeSeconds = 16000; // 500 (MaxReorg) * 64 (TargetSpacing) / 2 = 4 hours, 26 minutes and 40 seconds

            var powLimit = new Target(new uint256("0000ffff00000000000000000000000000000000000000000000000000000000"));

            var consensusFactory = new PosConsensusFactory();

            // Create the testnet genesis block.
            this.GenesisTime = 1588635707;
            this.GenesisNonce = 2433759;
            this.GenesisBits = powLimit;
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            Block genesisBlock = CreateX42GenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward);

            this.Genesis = genesisBlock;

            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5
            );

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new StratisBIP9Deployments()
            {
                [StratisBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters("ColdStaking", 2,
                    new DateTime(2018, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2019, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    BIP9DeploymentsParameters.DefaultMainnetThreshold)
            };

            this.Consensus = new X42Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 424242,
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8"),
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: null,
                maxMoney: Money.Coins(42 * 1000000),
                coinbaseMaturity: 10,
                premineHeight: 2,
                premineReward: Money.Coins(10.5m * 1000000),
                proofOfWorkReward: Money.Zero,
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                powTargetSpacing: TimeSpan.FromSeconds(60),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: powLimit,
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 523,
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(20),
                subsidyLimit: 100000,
                proofOfStakeRewardAfterSubsidyLimit: Money.Coins(5),
                lastProofOfStakeRewardHeight: 1000000
            );

            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (65) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (196) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (65 + 128) };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x11bd504102b42b24680d7b4f9b9e9521adc1b690253494d108193cdfcdd2ef0b"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }, // Genisis
            };

            var encoder = new Bech32Encoder("bc");
            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.DNSSeeds = new List<DNSSeedData>
            {
                new DNSSeedData("testnet1.x42seed.host", "testnet1.x42seed.host"),
            };

            string[] seedNodes = { "63.32.82.169" };
            this.SeedNodes = ConvertToNetworkAddresses(seedNodes, this.DefaultPort).ToList();

            this.StandardScriptsRegistry = new StratisStandardScriptsRegistry();

            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x11bd504102b42b24680d7b4f9b9e9521adc1b690253494d108193cdfcdd2ef0b"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0x87e7f7df2fbe9fa56c627e59342e0d8142eb5e44c5e5769609059a60f17d0702"));

            this.RegisterRules(this.Consensus);
            this.RegisterMempoolRules(this.Consensus);
        }
    }
}