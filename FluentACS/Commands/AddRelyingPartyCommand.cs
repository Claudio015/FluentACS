﻿namespace FluentACS.Commands
{
    using System;
    using System.Data.Services.Client;
    using System.Linq;

    using FluentACS.Logging;
    using FluentACS.ManagementService;
    using FluentACS.Specs;

    public class AddRelyingPartyCommand : BaseCommand
    {
        private readonly RelyingPartySpec relyingPartySpec;

        public AddRelyingPartyCommand(RelyingPartySpec relyingPartySpec)
        {
            Guard.NotNull(() => relyingPartySpec, relyingPartySpec);

            this.relyingPartySpec = relyingPartySpec;
        }

        public override void Execute(object receiver, Action<LogInfo> logAction)
        {
            var acsWrapper = (ServiceManagementWrapper)receiver;
            var client = acsWrapper.CreateManagementServiceClient();

            var rpToRemove = acsWrapper.RetrieveRelyingParties().Where(rp => rp.Name.Equals(this.relyingPartySpec.Name())).SingleOrDefault();
            if (rpToRemove != null)
            {
                if (this.relyingPartySpec.ShouldRemoveRelatedRuleGroups())
                {
                    var pendingChanges = false;

                    foreach (var ruleGroup in rpToRemove.RelyingPartyRuleGroups)
                    {
                        this.LogMessage(logAction, string.Format("Removing Rule Group '{0}'", ruleGroup.RuleGroup.Name));
                        RemoveRuleGroup(client, ruleGroup.RuleGroup.Name);

                        pendingChanges = true;
                    }

                    if (pendingChanges)
                    {
                        client.SaveChanges(SaveChangesOptions.Batch);
                        this.LogSavingChangesMessage(logAction);
                    }
                }

                this.RemoveRelatedKeys(rpToRemove, client, logAction);

                this.LogMessage(logAction, string.Format("Removing Relying Party '{0}'", rpToRemove.Name));
                acsWrapper.RemoveRelyingParty(rpToRemove.Name);
                this.LogSavingChangesMessage(logAction);
            }

            this.AddRelyingParty(acsWrapper, logAction);

            this.LinkExistingRuleGroups(client, logAction);
        }

        private static void RemoveRuleGroup(ManagementService managementService, string ruleGroupName)
        {
            Guard.NotNullOrEmpty(() => ruleGroupName, ruleGroupName);

            var rgToRemove = managementService.RuleGroups.Where(
                rg => rg.Name.Equals(ruleGroupName)).Single();

            managementService.DeleteObject(rgToRemove);
        }

        private void RemoveRelatedKeys(RelyingParty rpToRemove, ManagementService client, Action<LogInfo> logAction)
        {
            var pendingChanges = false;

            foreach (var key in rpToRemove.RelyingPartyKeys)
            {
                RelyingPartyKey keyLocal = key;
                var keyToRemove = client.RelyingPartyKeys.Where(
                    k => k.DisplayName.Equals(keyLocal.DisplayName)).Single();

                this.LogMessage(logAction, string.Format("Removing Key '{0}'", keyLocal.DisplayName));
                client.DeleteObject(keyToRemove);

                pendingChanges = true;
            }

            if (pendingChanges)
            {
                client.SaveChanges(SaveChangesOptions.Batch);
                this.LogSavingChangesMessage(logAction);
            }
        }

        private void AddRelyingParty(ServiceManagementWrapper acsWrapper, Action<LogInfo> logAction)
        {
            var tokenLifetime = this.relyingPartySpec.TokenLifetime();

            byte[] signingCertBytes = null;
            string signingCertPassword = null;
            DateTime? signingCertStartDate = null;
            DateTime? signingCertEndDate = null;

            var signingCert = this.relyingPartySpec.SigningCertificate();
            if (signingCert != null)
            {
                signingCertBytes = signingCert.Bytes();
                signingCertPassword = signingCert.Password();
                signingCertStartDate = signingCert.StartDate();
                signingCertEndDate = signingCert.EndDate();
            }

            this.LogMessage(logAction, string.Format("Adding Relying Party '{0}'", this.relyingPartySpec.Name()));
            acsWrapper.AddRelyingPartyWithKey(
                this.relyingPartySpec.Name(),
                this.relyingPartySpec.RealmAddress(),
                this.relyingPartySpec.ReplyAddress(),
                this.relyingPartySpec.SymmetricKey(),
                this.relyingPartySpec.GetTokenType(),
                (tokenLifetime == 0 ? Constants.RelyingPartyTokenLifetime : tokenLifetime),
                signingCertBytes,
                signingCertPassword,
                signingCertStartDate,
                signingCertEndDate,
                this.relyingPartySpec.EncryptionCertificate(),
                string.Empty,
                this.relyingPartySpec.AllowedIdentityProviders().ToArray());
            this.LogSavingChangesMessage(logAction);
        }

        private void LinkExistingRuleGroups(ManagementService client, Action<LogInfo> logAction)
        {
            foreach (var linkedRuleGroup in this.relyingPartySpec.LinkedRuleGroups())
            {
                var @group = linkedRuleGroup;
                DataServiceCollection<RuleGroup> ruleGroups = new DataServiceCollection<RuleGroup>(client.RuleGroups);

                while (ruleGroups.Continuation != null)
                {
                    ruleGroups.Load(client.Execute<RuleGroup>(ruleGroups.Continuation));
                }

                foreach (var ruleGroup in ruleGroups.Where(rg => System.Text.RegularExpressions.Regex.IsMatch(rg.Name, group)))
                {
                    var relyingParty = client.RelyingParties.Where(rp => rp.Name.Equals(this.relyingPartySpec.Name())).Single();

                    var relyingPartyRuleGroup = new RelyingPartyRuleGroup
                    {
                        RuleGroupId = ruleGroup.Id,
                        RelyingParty = relyingParty
                    };

                    this.LogMessage(logAction, string.Format("Linking Relying Party '{0}' to Rule Group '{1}'", this.relyingPartySpec.Name(), ruleGroup.Name));
                    client.AddRelatedObject(relyingParty, "RelyingPartyRuleGroups", relyingPartyRuleGroup);
                }
            }

            if (this.relyingPartySpec.LinkedRuleGroups().Any())
            {
                client.SaveChanges(SaveChangesOptions.Batch);
                this.LogSavingChangesMessage(logAction);
            }
        }
    }
}