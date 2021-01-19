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
    public interface IApiClient
    {
        MonitoringService.MonitoringServiceClient Monitoring { get; }

        Assets.AssetsClient Assets { get; }

        Accounts.AccountsClient Accounts { get; }

        Blockchains.BlockchainsClient Blockchains { get; }

        BrokerAccounts.BrokerAccountsClient BrokerAccounts { get; }

        Vaults.VaultsClient Vaults { get; }

        Deposits.DepositsClient Deposits { get; }

        Withdrawals.WithdrawalsClient Withdrawals { get; }

        Addresses.AddressesClient Addresses { get; }
    }
}
