﻿namespace FluentACS.Specs
{
    using System;
    using System.Collections.Generic;

    using FluentACS.Commands;
    using FluentACS.ManagementService;

    public class RelyingPartySpec : BaseSpec
    {
        private readonly IList<string> allowedIdentityProviders = new List<string>();

        private readonly IList<string> linkedRuleGroups = new List<string>();

        private byte[] encryptionCert;

        private string name;

        private string realmAddress;

        private string replyAddress;

        private bool shouldRemoveRelatedRuleGroups;

        private SigningCertificateSpec signingCert;

        private byte[] symmetricKey;

        private int tokenLifetime;

        private TokenType tokenType;

        public RelyingPartySpec(List<ICommand> commands)
            : base(commands)
        {
        }

        public RelyingPartySpec AddRuleGroup(Action<RuleGroupSpec> configAction)
        {
            var cmds = new List<ICommand>();
            var spec = new RuleGroupSpec(cmds, this.Name());
            configAction(spec);

            this.Commands.Add(new AddRuleGroupCommand(spec));
            this.Commands.AddRange(cmds);

            return this;
        }

        public RelyingPartySpec AllowIdentityProvider(string identityProvider)
        {
            this.allowedIdentityProviders.Add(identityProvider);
            return this;
        }

        public RelyingPartySpec EncryptionCertificate(byte[] encryptionCert)
        {
            this.encryptionCert = encryptionCert;
            return this;
        }

        public RelyingPartySpec LinkToRuleGroup(string ruleGroupName)
        {
            this.linkedRuleGroups.Add(ruleGroupName);
            return this;
        }

        public RelyingPartySpec Name(string name)
        {
            this.name = name;
            return this;
        }

        public RelyingPartySpec RealmAddress(string realmAddress)
        {
            this.realmAddress = realmAddress;
            return this;
        }

        public RelyingPartySpec RemoveRelatedRuleGroups()
        {
            this.shouldRemoveRelatedRuleGroups = true;
            return this;
        }

        public RelyingPartySpec ReplyAddress(string replyAddress)
        {
            this.replyAddress = replyAddress;
            return this;
        }

        public RelyingPartySpec SamlToken()
        {
            this.tokenType = TokenType.SAML_2_0;
            return this;
        }

        public RelyingPartySpec SigningCertificate(Action<SigningCertificateSpec> configAction)
        {
            this.signingCert = new SigningCertificateSpec();
            configAction(this.signingCert);
            return this;
        }

        public RelyingPartySpec SwtToken()
        {
            this.tokenType = TokenType.SWT;
            return this;
        }

        public RelyingPartySpec SymmetricKey(byte[] symmetricKey)
        {
            this.symmetricKey = symmetricKey;
            return this;
        }

        public RelyingPartySpec TokenLifetime(int tokenLifetime)
        {
            this.tokenLifetime = tokenLifetime;
            return this;
        }

        internal IEnumerable<string> AllowedIdentityProviders()
        {
            return this.allowedIdentityProviders;
        }

        internal byte[] EncryptionCertificate()
        {
            return this.encryptionCert;
        }

        internal TokenType GetTokenType()
        {
            return this.tokenType;
        }

        internal IEnumerable<string> LinkedRuleGroups()
        {
            return this.linkedRuleGroups;
        }

        internal string Name()
        {
            return this.name;
        }

        internal string RealmAddress()
        {
            return this.realmAddress;
        }

        internal string ReplyAddress()
        {
            return this.replyAddress;
        }

        internal bool ShouldRemoveRelatedRuleGroups()
        {
            return this.shouldRemoveRelatedRuleGroups;
        }

        internal SigningCertificateSpec SigningCertificate()
        {
            return this.signingCert;
        }

        internal byte[] SymmetricKey()
        {
            return this.symmetricKey;
        }

        internal TokenType Token()
        {
            return this.tokenType;
        }

        internal int TokenLifetime()
        {
            return this.tokenLifetime;
        }
    }
}