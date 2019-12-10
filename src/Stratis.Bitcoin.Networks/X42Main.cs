using System;
using System.Collections.Generic;
using System.Linq;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using Stratis.Bitcoin.Features.Consensus.Rules.CommonRules;
using Stratis.Bitcoin.Features.Consensus.Rules.ProvenHeaderRules;
using Stratis.Bitcoin.Features.MemoryPool.Rules;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class X42Main : Network
    {
        /// <summary> x42 maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int x42MaxTimeOffsetSeconds = 25 * 60;

        /// <summary> x42 default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int x42DefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different x42 blockchains (x42Main, x42Test, x42RegTest). </summary>
        public const string x42RootFolderName = "x42";

        /// <summary> The default name used for the x42 configuration file. </summary>
        public const string x42DefaultConfigFilename = "x42.conf";

        public X42Main()
        {
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0x42;
            messageStart[1] = 0x66;
            messageStart[2] = 0x52;
            messageStart[3] = 0x03;
            uint magic = BitConverter.ToUInt32(messageStart, 0); //0x3526642

            this.Name = "x42Main";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = magic;
            this.DefaultPort = 52342;
            this.DefaultRPCPort = 52343;
            this.DefaultAPIPort = 42220;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = Money.Zero;
            this.FallbackFee = Money.Zero;
            this.MinRelayTxFee = Money.Zero;
            this.RootFolderName = x42RootFolderName;
            this.DefaultConfigFilename = x42DefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "x42";

            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1533106324;
            this.GenesisNonce = 246101626;
            this.GenesisBits = 0x1e0fffff;
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
                ruleChangeActivationThreshold: 1916, // 95% of 2016
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: null,
                maxMoney: Money.Coins(42 * 1000000),
                coinbaseMaturity: 50,
                premineHeight: 2,
                premineReward: Money.Coins(10.5m * 1000000),
                proofOfWorkReward: Money.Zero,
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                powTargetSpacing: TimeSpan.FromSeconds(60),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 523,
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(20),
                subsidyLimit: 400000,
                proofOfStakeRewardAfterSubsidyLimit: Money.Coins(2),
                lastProofOfStakeRewardHeight: 12155230
            );

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (75) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (125) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (75 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0x04ffe583707a96c1c2eb54af33a4b1dc6d9d8e09fea8c9a7b097ba88f0cb64c4"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) }, // Genisis
                { 2, new CheckpointInfo(new uint256("0x1a64847f52fce72763a9eaa99bed6a896556917cd16f491bbdec070b40514282"), new uint256("0xa55d7663540264e7ed1e7195ecd0050303187eaf9485edeec70806491b5a53d1")) }, // Premine
                { 523, new CheckpointInfo(new uint256("0x1ca01c02f5989a198433cbe83e0eb26d9166d6aaaa9c20d6b765d5bace7829f1"), new uint256("0xbf04ecd478d78d302aa65293dde85036954b76216b0812104315c8a5ad139525")) }, // Last POW Block
                { 20000, new CheckpointInfo(new uint256("0x79976dfc025e982239a0bd62099475e6abf839c73aba5805b5cbe4091744c09a"), new uint256("0x250690dd6f264565c5ce16d84d250d67eb940d084c253e4006cdba3091fd66b6")) },
                { 200000, new CheckpointInfo(new uint256("0xaa276a1c51c025ff1a21fd4b07bfa5d55effc173840e054dd851b20dbb1f2f17"), new uint256("0x63d4bc7b0272703e94ae79103970ea324dc85221e88a51c39a170744848c0cc7")) },
                { 300000, new CheckpointInfo(new uint256("0xff72e73ee8f87c0de9bf82c3bb758f4905c3e005493f2ed1faede7c120961750"), new uint256("0x2097fc9edfb8dfe287db45bbce820e168c50be32c9840b1cddf56b297011fc69")) },
                { 500000, new CheckpointInfo(new uint256("0x7f9a88ebb32f47090ec37a110c5a05c1162a604dfbfb69c8d492e771cdb63289"), new uint256("0x19db6890c5c934e883bc99eb197509b0a81f19faeefcf49fd7fa6dab83644bfb")) }
            };

            var encoder = new Bech32Encoder("bc");
            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            this.DNSSeeds = new List<DNSSeedData>
            {
                new DNSSeedData("mainnet1.x42seed.host", "mainnet1.x42seed.host"),
                new DNSSeedData("mainnetnode1.x42seed.host", "mainnetnode1.x42seed.host"),
                new DNSSeedData("tech.x42.cloud", "tech.x42.cloud"),
            };

            string[] seedNodes = { "34.255.35.42", "52.211.235.48", "52.210.106.220" };
            this.SeedNodes = ConvertToNetworkAddresses(seedNodes, this.DefaultPort).ToList();

            this.StandardScriptsRegistry = new StratisStandardScriptsRegistry();

            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0x04ffe583707a96c1c2eb54af33a4b1dc6d9d8e09fea8c9a7b097ba88f0cb64c4"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0x6e3439a32382f83dee4f94a6f8bdd38908bcf0c82ec09aba85c5321357f01f67"));

            this.RegisterRules(this.Consensus);
            this.RegisterMempoolRules(this.Consensus);
        }

        protected void RegisterRules(IConsensus consensus)
        {
            consensus.ConsensusRules
                .Register<HeaderTimeChecksRule>()
                .Register<HeaderTimeChecksPosRule>()
                .Register<StratisBugFixPosFutureDriftRule>()
                .Register<CheckDifficultyPosRule>()
                .Register<StratisHeaderVersionRule>()
                .Register<ProvenHeaderSizeRule>()
                .Register<ProvenHeaderCoinstakeRule>();

            consensus.ConsensusRules
                .Register<BlockMerkleRootRule>()
                .Register<PosBlockSignatureRepresentationRule>()
                .Register<PosBlockSignatureRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsPartialValidationRule>()
                .Register<PosTimeMaskRule>()

                // rules that are inside the method ContextualCheckBlock
                .Register<TransactionLocktimeActivationRule>()
                .Register<CoinbaseHeightActivationRule>()
                .Register<WitnessCommitmentsRule>()
                .Register<BlockSizeRule>()

                // rules that are inside the method CheckBlock
                .Register<EnsureCoinbaseRule>()
                .Register<CheckPowTransactionRule>()
                .Register<CheckPosTransactionRule>()
                .Register<CheckSigOpsRule>()
                .Register<PosCoinstakeRule>();

            consensus.ConsensusRules
                .Register<SetActivationDeploymentsFullValidationRule>()

                .Register<CheckDifficultyHybridRule>()

                // rules that require the store to be loaded (coinview)
                .Register<LoadCoinviewRule>()
                .Register<TransactionDuplicationActivationRule>()
                .Register<PosCoinviewRule>() // implements BIP68, MaxSigOps and BlockReward calculation
                                             // Place the PosColdStakingRule after the PosCoinviewRule to ensure that all input scripts have been evaluated
                                             // and that the "IsColdCoinStake" flag would have been set by the OP_CHECKCOLDSTAKEVERIFY opcode if applicable.
                .Register<PosColdStakingRule>()
                .Register<SaveCoinviewRule>();
        }

        protected void RegisterMempoolRules(IConsensus consensus)
        {
            consensus.MempoolRules = new List<Type>()
            {
                typeof(CheckConflictsMempoolRule),
                typeof(CheckCoinViewMempoolRule),
                typeof(CreateMempoolEntryMempoolRule),
                typeof(CheckSigOpsMempoolRule),
                typeof(CheckFeeMempoolRule),
                typeof(CheckRateLimitMempoolRule),
                typeof(CheckAncestorsMempoolRule),
                typeof(CheckReplacementMempoolRule),
                typeof(CheckAllInputsMempoolRule)
            };
        }

        protected static Block CreateX42GenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            string pszTimestamp = "On Emancipation Day, we are fighting to maintain our democratic freedom at various levels - https://www.stabroeknews.com/2018/opinion/letters/08/01/on-emancipation-day-we-are-fighting-to-maintain-our-democratic-freedom-at-various-levels/ | popó & lita - 6F3582CC2B720980C936D95A2E07F809";

            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 1;
            txNew.Time = nTime;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(0), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)42 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(pszTimestamp)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });
            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = nVersion;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();
            return genesis;
        }
    }
}