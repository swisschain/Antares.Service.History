using Swisschain.Sirius.Api.ApiClient.Common;
using Swisschain.Sirius.Api.ApiContract;
using Swisschain.Sirius.Api.ApiContract.Account;
using Swisschain.Sirius.Api.ApiContract.Address;
using Swisschain.Sirius.Api.ApiContract.Asset;
using Swisschain.Sirius.Api.ApiContract.Blockchain;
using Swisschain.Sirius.Api.ApiContract.BrokerAccount;
using Swisschain.Sirius.Api.ApiContract.Deposit;
using Swisschain.Sirius.Api.ApiContract.Monitoring;
using Swisschain.Sirius.Api.ApiContract.Vault;
using Swisschain.Sirius.Api.ApiContract.Withdrawal;

namespace Swisschain.Sirius.Api.ApiClient
{
    public class ApiClient : BaseGrpcClient, IApiClient
    {
        public ApiClient(string serverGrpcUrl, string apiKey) : base(serverGrpcUrl, apiKey)
        {
            Monitoring = new MonitoringService.MonitoringServiceClient(CallInvoker);
            Assets = new Assets.AssetsClient(CallInvoker);
            Accounts = new Accounts.AccountsClient(CallInvoker);
            Blockchains = new Blockchains.BlockchainsClient(CallInvoker);
            BrokerAccounts = new BrokerAccounts.BrokerAccountsClient(CallInvoker);
            Vaults = new Vaults.VaultsClient(CallInvoker);
            Deposits = new Deposits.DepositsClient(CallInvoker);
            Withdrawals = new Withdrawals.WithdrawalsClient(CallInvoker);
            Addresses = new Addresses.AddressesClient(CallInvoker);
        }

        public Addresses.AddressesClient Addresses { get; }

        public MonitoringService.MonitoringServiceClient Monitoring { get; }

        public Assets.AssetsClient Assets { get; }

        public Accounts.AccountsClient Accounts { get; }

        public Blockchains.BlockchainsClient Blockchains { get; }

        public BrokerAccounts.BrokerAccountsClient BrokerAccounts { get; }

        public Vaults.VaultsClient Vaults { get; }

        public Deposits.DepositsClient Deposits { get; }

        public Withdrawals.WithdrawalsClient Withdrawals { get; }
    }
}
