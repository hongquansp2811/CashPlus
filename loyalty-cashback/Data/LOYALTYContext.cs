using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LOYALTY.Data
{
    public class LOYALTYContext : DbContext
    {
        public LOYALTYContext(DbContextOptions<LOYALTYContext> options) : base(options)
        {
        }

        // Authen
        public DbSet<Function> Functions { get; set; }
        public DbSet<Action1> Actions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<UserGroupPermission> UserGroupPermissions { get; set; }

        // MasterData
        public DbSet<AppVersion> AppVersions { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogCategory> BlogCategorys { get; set; }
        public DbSet<OtherList> OtherLists { get; set; }
        public DbSet<OtherListType> OtherListTypes { get; set; }
        public DbSet<ProductGroup> ProductGroups { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<ProductLabel> ProductLabels { get; set; }
        public DbSet<CustomerRank> CustomerRanks { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StaticPage> StaticPages { get; set; }

        public DbSet<Offending_Words> offending_Words { get; set; }

        // Common
        public DbSet<Logging> Loggings { get; set; }
        public DbSet<OTPTransaction> OTPTransactions { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }

        // Configuration
        public DbSet<AccumulatePointConfig> AccumulatePointConfigs { get; set; }
        public DbSet<AccumulatePointConfigDetail> AccumulatePointConfigDetails { get; set; }
        public DbSet<AffiliateConfig> AffiliateConfigs { get; set; }
        public DbSet<AffiliateConfigDetail> AffiliateConfigDetails { get; set; }
        public DbSet<BonusPointConfig> BonusPointConfigs { get; set; }
        public DbSet<ConfigHistory> ConfigHistorys { get; set; }
        public DbSet<CustomerRankConfig> CustomerRankConfigs { get; set; }
        public DbSet<EstimateApprovePoint> EstimateApprovePoints { get; set; }
        public DbSet<EstimateApprovePointDetail> EstimateApprovePointDetails { get; set; }
        public DbSet<ExchangePointPackConfig> ExchangePointPackConfigs { get; set; }
        public DbSet<LoadPointPackConfig> LoadPointPackConfigs { get; set; }
        public DbSet<RatingConfig> RatingConfigs { get; set; }
        public DbSet<Settings> Settingses { get; set; }
        public DbSet<AppSuggestSearch> AppSuggestSearchs { get; set; }

        public DbSet<NotiConfig> NotiConfigs { get; set; }

        // Business
        public DbSet<AccumulatePointOrder> AccumulatePointOrders { get; set; }
        public DbSet<AccumulatePointOrderDetail> AccumulatePointOrderDetails { get; set; }
        public DbSet<AccumulatePointOrderRating> AccumulatePointOrderRatings { get; set; }
        public DbSet<AccumulatePointOrderAffiliate> AccumulatePointOrderAffiliates { get; set; }
        public DbSet<AddPointOrder> AddPointOrders { get; set; }
        public DbSet<ComplainInfo> ComplainInfos { get; set; }
        public DbSet<AccumulatePointOrderComplain> AccumulatePointOrderComplains { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<ChangePointOrder> ChangePointOrders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerBankAccount> CustomerBankAccounts { get; set; }
        public DbSet<CustomerFakeBank> CustomerFakeBanks { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<PartnerDocument> PartnerDocuments { get; set; }
        public DbSet<PartnerFavourite> PartnerFavourites { get; set; }
        public DbSet<PartnerBag> PartnerBags { get; set; }
        public DbSet<PartnerContract> PartnerContracts { get; set; }
        public DbSet<PartnerOrder> PartnerOrders { get; set; }
        public DbSet<PartnerOrderDetail> PartnerOrderDetails { get; set; }
        public DbSet<RecallPointOrder> RecallPointOrders { get; set; }

        // Report
        public DbSet<CustomerPointHistory> CustomerPointHistorys { get; set; }
        public DbSet<PartnerPointHistory> PartnerPointHistorys { get; set; }
        public DbSet<SystemPointHistory> SystemPointHistorys { get; set; }
        public DbSet<SysAmountHistory> SysAmountHistorys { get; set; }
        public DbSet<SysAmountSummary> SysAmountSummarys { get; set; }

        // Payment Gate
        public DbSet<BKVATransaction> BKVATransactions { get; set; }
        public DbSet<BaoKimTransaction> BaoKimTransactions { get; set; }

        // Schedule
        public DbSet<ScheduleJobs> ScheduleJobss { get; set; }

        //SMSHistory
        public DbSet<SMSHistory> SMSHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Authen
            modelBuilder.Entity<Function>().ToTable("Function").HasKey(v => v.id);
            modelBuilder.Entity<Action1>().ToTable("Action").HasKey(v => v.id);
            modelBuilder.Entity<User>().ToTable("User").HasKey(v => v.id);
            modelBuilder.Entity<UserGroup>().ToTable("UserGroup").HasKey(v => v.id);
            modelBuilder.Entity<UserPermission>().ToTable("UserPermission").HasKey(v => v.id);
            modelBuilder.Entity<UserGroupPermission>().ToTable("UserGroupPermission").HasKey(v => v.id);

            // Common
            modelBuilder.Entity<Logging>().ToTable("Logging").HasKey(v => v.id);
            modelBuilder.Entity<UserNotification>().ToTable("UserNotification").HasKey(v => new { v.notification_id, v.user_id });
            modelBuilder.Entity<OTPTransaction>().ToTable("OTPTransaction").HasKey(v => v.otp_code);

            // Master Data
            modelBuilder.Entity<AppVersion>().ToTable("AppVersion").HasKey(v => v.id);
            modelBuilder.Entity<Bank>().ToTable("Bank").HasKey(v => v.id);
            modelBuilder.Entity<Banner>().ToTable("Banner").HasKey(v => v.id);
            modelBuilder.Entity<Blog>().ToTable("Blog").HasKey(v => v.id);
            modelBuilder.Entity<BlogCategory>().ToTable("BlogCategory").HasKey(v => v.id);
            modelBuilder.Entity<OtherList>().ToTable("OtherList").HasKey(v => v.id);
            modelBuilder.Entity<OtherListType>().ToTable("OtherListType").HasKey(v => v.id);
            modelBuilder.Entity<Province>().ToTable("Province").HasKey(v => v.id);
            modelBuilder.Entity<ProductGroup>().ToTable("ProductGroup").HasKey(v => v.id);
            modelBuilder.Entity<Product>().ToTable("Product").HasKey(v => v.id);
            modelBuilder.Entity<CustomerRank>().ToTable("CustomerRank").HasKey(v => v.id);
            modelBuilder.Entity<ProductImage>().ToTable("ProductImage").HasKey(v => v.id);
            modelBuilder.Entity<ProductLabel>().ToTable("ProductLabel").HasKey(v => v.id);
            modelBuilder.Entity<ServiceType>().ToTable("ServiceType").HasKey(v => v.id);
            modelBuilder.Entity<NotificationType>().ToTable("NotificationType").HasKey(v => v.id);
            modelBuilder.Entity<Notification>().ToTable("Notification").HasKey(v => v.id);
            modelBuilder.Entity<StaticPage>().ToTable("StaticPage").HasKey(v => v.id);

            // Configuration
            modelBuilder.Entity<AccumulatePointConfig>().ToTable("AccumulatePointConfig").HasKey(v => v.id);
            modelBuilder.Entity<AccumulatePointConfigDetail>().ToTable("AccumulatePointConfigDetail").HasKey(v => v.id);
            modelBuilder.Entity<AffiliateConfig>().ToTable("AffiliateConfig").HasKey(v => v.id);
            modelBuilder.Entity<AffiliateConfigDetail>().ToTable("AffiliateConfigDetail").HasKey(v => v.id);
            modelBuilder.Entity<BonusPointConfig>().ToTable("BonusPointConfig").HasKey(v => v.id);
            modelBuilder.Entity<ConfigHistory>().ToTable("ConfigHistory").HasKey(v => v.id);
            modelBuilder.Entity<CustomerRankConfig>().ToTable("CustomerRankConfig").HasKey(v => v.id);
            modelBuilder.Entity<EstimateApprovePoint>().ToTable("EstimateApprovePoint").HasKey(v => v.id);
            modelBuilder.Entity<EstimateApprovePointDetail>().ToTable("EstimateApprovePointDetail").HasKey(v => v.id);
            modelBuilder.Entity<ExchangePointPackConfig>().ToTable("ExchangePointPackConfig").HasKey(v => v.id);
            modelBuilder.Entity<LoadPointPackConfig>().ToTable("LoadPointPackConfig").HasKey(v => v.id);
            modelBuilder.Entity<RatingConfig>().ToTable("RatingConfig").HasKey(v => v.id);
            modelBuilder.Entity<Settings>().ToTable("Settings").HasKey(v => v.id);
            modelBuilder.Entity<NotiConfig>().ToTable("NotiConfig").HasKey(v => v.id);
            modelBuilder.Entity<AppSuggestSearch>().ToTable("AppSuggestSearch").HasKey(v => v.id);

            // Business
            modelBuilder.Entity<AccumulatePointOrder>().ToTable("AccumulatePointOrder").HasKey(v => v.id);
            modelBuilder.Entity<AccumulatePointOrderRating>().ToTable("AccumulatePointOrderRating").HasKey(v => v.id);
            modelBuilder.Entity<AccumulatePointOrderDetail>().ToTable("AccumulatePointOrderDetail").HasKey(v => v.id);
            modelBuilder.Entity<AccumulatePointOrderAffiliate>().ToTable("AccumulatePointOrderAffiliate").HasKey(v => v.id);
            modelBuilder.Entity<AddPointOrder>().ToTable("AddPointOrder").HasKey(v => v.id);
            modelBuilder.Entity<ComplainInfo>().ToTable("ComplainInfo").HasKey(v => v.id);
            modelBuilder.Entity<AccumulatePointOrderComplain>().ToTable("AccumulatePointOrderComplain").HasKey(v => v.id);
            modelBuilder.Entity<BankAccount>().ToTable("BankAccount").HasKey(v => v.id);
            modelBuilder.Entity<ChangePointOrder>().ToTable("ChangePointOrder").HasKey(v => v.id);
            modelBuilder.Entity<Customer>().ToTable("Customer").HasKey(v => v.id);
            modelBuilder.Entity<CustomerBankAccount>().ToTable("CustomerBankAccount").HasKey(v => v.id);
            modelBuilder.Entity<CustomerFakeBank>().ToTable("CustomerFakeBank").HasKey(v => v.id);
            modelBuilder.Entity<Partner>().ToTable("Partner").HasKey(v => v.id);
            modelBuilder.Entity<PartnerBag>().ToTable("PartnerBag").HasKey(v => v.id);
            modelBuilder.Entity<PartnerDocument>().ToTable("PartnerDocument").HasKey(v => v.id);
            modelBuilder.Entity<PartnerFavourite>().ToTable("PartnerFavourite").HasKey(v => new { v.customer_id, v.partner_id });
            modelBuilder.Entity<PartnerContract>().ToTable("PartnerContract").HasKey(v => v.id);
            modelBuilder.Entity<PartnerOrder>().ToTable("PartnerOrder").HasKey(v => v.id);
            modelBuilder.Entity<PartnerOrderDetail>().ToTable("PartnerOrderDetail").HasKey(v => v.id);
            modelBuilder.Entity<RecallPointOrder>().ToTable("RecallPointOrder").HasKey(v => v.id);

            // Report
            modelBuilder.Entity<CustomerPointHistory>().ToTable("CustomerPointHistory").HasKey(v => v.id);
            modelBuilder.Entity<PartnerPointHistory>().ToTable("PartnerPointHistory").HasKey(v => v.id);
            modelBuilder.Entity<SystemPointHistory>().ToTable("SystemPointHistory").HasKey(v => v.id);
            modelBuilder.Entity<SysAmountHistory>().ToTable("SysAmountHistory").HasKey(v => v.id);
            modelBuilder.Entity<SysAmountSummary>().ToTable("SysAmountSummary").HasKey(v => v.id);

            // Payment Gate
            modelBuilder.Entity<BKVATransaction>().ToTable("BKVATransaction").HasKey(v => v.RequestId);
            modelBuilder.Entity<BaoKimTransaction>().ToTable("BaoKimTransaction").HasKey(v => v.id);

            // Schedule Jobs
            modelBuilder.Entity<ScheduleJobs>().ToTable("ScheduleJobs").HasKey(v => v.id);

            modelBuilder.Entity<SMSHistory>().ToTable("SMSHistory").HasKey(v => v.id);
        }
    }
}
