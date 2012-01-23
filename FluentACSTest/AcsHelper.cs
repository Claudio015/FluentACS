﻿namespace FluentACSTest
{
    using System.Linq;

    using FluentACS;
    using FluentACS.ManagementService;

    public static class AcsHelper
    {
        public static bool CheckIdentityProviderExists(AcsNamespaceDescription namespaceDesc, string idpDisplayName)
        {
            var acs = new ServiceManagementWrapper(namespaceDesc.Namespace, namespaceDesc.UserName, namespaceDesc.Password);
            var identityProviders = acs.RetrieveIdentityProviders();

            return identityProviders.Any(provider => provider.DisplayName == idpDisplayName);
        }

        public static bool CheckServiceIdentityExists(AcsNamespaceDescription namespaceDesc, string serviceIdentityName)
        {
            var acs = new ServiceManagementWrapper(namespaceDesc.Namespace, namespaceDesc.UserName, namespaceDesc.Password);
            var serviceIdentities = acs.RetrieveServiceIdentities();
            
            return serviceIdentities.Any(serviceIdentity => serviceIdentity.Name == serviceIdentityName);
        }

        public static bool CheckRelyingPartyExists(AcsNamespaceDescription namespaceDesc, string relyingPartyName)
        {
            var acs = new ServiceManagementWrapper(namespaceDesc.Namespace, namespaceDesc.UserName, namespaceDesc.Password);
            var relyingParties = acs.RetrieveRelyingParties();
            
            return relyingParties.Any(relyingParty => relyingParty.Name == relyingPartyName);
        }

        public static bool CheckRuleGroupExists(AcsNamespaceDescription namespaceDesc, string relyingParty, string ruleGroup)
        {
            var acs = new ServiceManagementWrapper(namespaceDesc.Namespace, namespaceDesc.UserName, namespaceDesc.Password);
            var relyingParties = acs.RetrieveRelyingParties();

            return relyingParties.Where(rp => rp.Name == relyingParty).Select(
                rp => rp.RelyingPartyRuleGroups.Any(rg => rg.RuleGroup.Name == ruleGroup)).FirstOrDefault();
        }

        public static bool CheckRuleGroupHasRules(AcsNamespaceDescription namespaceDesc, string relyingParty, string ruleGroup, int ruleCount)
        {
            var acs = new ServiceManagementWrapper(namespaceDesc.Namespace, namespaceDesc.UserName, namespaceDesc.Password);
            var rules = acs.RetrieveRules(ruleGroup);
            
            return (rules != null) && (rules.Count() == ruleCount);
        }

        public static bool CheckRuleGroupHasRule(AcsNamespaceDescription namespaceDesc, string relyingParty, string ruleGroup, string ruleDescription)
        {
            var acs = new ServiceManagementWrapper(namespaceDesc.Namespace, namespaceDesc.UserName, namespaceDesc.Password);
            var rules = acs.RetrieveRules(ruleGroup);

            return rules.Any(rule => rule.Description.Equals(ruleDescription));
        }

        public static bool CheckRelyingPartyHasKeys(AcsNamespaceDescription namespaceDesc, string relyingParty, int keyCount)
        {
            var acs = new ServiceManagementWrapper(namespaceDesc.Namespace, namespaceDesc.UserName, namespaceDesc.Password);
            var client = acs.CreateManagementServiceClient();

            var count = client.RelyingPartyKeys.Where(k => k.RelyingParty.Name.Equals(relyingParty)).Count();
            return count == keyCount;
        }
    }
}