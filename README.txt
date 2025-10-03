ELASTIC SEARCH COMMANDS:
start elasticsearch: elasticsearch.bat
https://192.168.31.206:9200
username:elastic
password generation: elasticsearch-reset-password -u elastic
password:Go+emDmMPM6yvhoXuLnn
token generation: elasticsearch-create-enrollment-token -s kibana
token: eyJ2ZXIiOiI4LjE0LjAiLCJhZHIiOlsiMTkyLjE2OC4zMS4yMDY6OTIwMCJdLCJmZ3IiOiI3ODM4MjNjOTNiNGJiZDQ1MWE0N2I0NDU2MDFlODZmZTQxMTU5ZmI1NzE5NGI0MzE1YWJmOTI1YTI3NDY3ODljIiwia2V5IjoiOTlfbm01TUJNMF9xVk1kbnZCMHo6U2NEby16cGlUWWUtVUNSY0l4V2VldyJ9

KIBANA COMMANDS:
start kibana: kibana.bat
http://localhost:5601
username:elastic
password:Go+emDmMPM6yvhoXuLnn

DOT NET EF CORE INSTALLATION:
dotnet tool install --global dotnet-ef

SCAFOLDING STRING COMMAND:
dotnet ef dbcontext scaffold "Data Source=DESKTOP-DUAU982;Initial Catalog=DotNet;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Data\Entities -f

dotnet ef dbcontext scaffold "Data Source=DESKTOP-DUAU982;Initial Catalog=DotNet;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False" Microsoft.EntityFrameworkCore.SqlServer --context-dir Data --output-dir Data\Entities -f --table AspNetUserRoles

MIGRATION COMMAND:
dotnet ef migrations add InitialCreate --output-dir Data\Migrations
dotnet ef database update

COMMENT: This Code is to be added in DbContext During Migration
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("Data Source=DESKTOP-DUAU982;Initial Catalog=DotNet;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False");




TABLES FOR DEMO SCHEMA:

CREATE TABLE DotNet.dbo.ApMenu (
	Id int IDENTITY(1,1) NOT NULL,
	Url nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	Parent int NULL,
	Name nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	MenuOrder int NULL,
	IsAction int NULL,
	IsActive int NULL,
	[Image] nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CreatedBy int NULL,
	CreatedDate datetime2 NULL,
	UpdatedBy int NULL,
	UpdatedDate datetime2 NULL,
	CONSTRAINT PK_ApMenu PRIMARY KEY (Id)
);

CREATE TABLE DotNet.dbo.ApMenuAction (
	Id bigint IDENTITY(1,1) NOT NULL,
	MenuId bigint NOT NULL,
	Url nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	ActionName nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsActive int NULL,
	CreatedBy int NULL,
	CreatedDate datetime2 NULL,
	UpdatedBy int NULL,
	UpdatedDate datetime2 NULL,
	CONSTRAINT PK_ApMenuAction PRIMARY KEY (Id)
);

CREATE TABLE DotNet.dbo.ApOtpHistory (
	Id bigint IDENTITY(1,1) NOT NULL,
	UserId bigint NULL,
	IKey nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IValue nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	Otp int NULL,
	OtpCount int NULL,
	ExpireAt datetimeoffset NULL,
	Status nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CreatedBy bigint NULL,
	CreatedAt datetime2 NULL,
	UpdatedBy bigint NULL,
	UpdatedAt datetime2 NULL,
	Remarks nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CONSTRAINT PK_ApOtpHistory PRIMARY KEY (Id)
);

CREATE TABLE DotNet.dbo.ApPermission (
	Id int IDENTITY(1,1) NOT NULL,
	Name nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	MenuId int NULL,
	IsActive int NULL,
	CreatedBy int NULL,
	CreatedDate datetime2 NULL,
	UpdatedBy int NULL,
	UpdatedDate datetime2 NULL,
	CONSTRAINT PK_ApPermission PRIMARY KEY (Id)
);

CREATE TABLE DotNet.dbo.ApRole (
	Id int IDENTITY(1,1) NOT NULL,
	Name nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	Description nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsActive int NULL,
	CreatedBy int NULL,
	CreatedDate datetime2 NULL,
	UpdatedBy int NULL,
	UpdatedDate datetime2 NULL,
	Flag int NULL,
	CONSTRAINT PK_ApRole PRIMARY KEY (Id)
);

CREATE TABLE DotNet.dbo.ApRolePermission (
	Id int IDENTITY(1,1) NOT NULL,
	RoleId int NOT NULL,
	PermissionId int NOT NULL,
	IsActive int NULL,
	CreatedBy int NULL,
	CreatedDate datetime2 NULL,
	UpdatedBy int NULL,
	UpdatedDate datetime2 NULL,
	CONSTRAINT PK_ApRolePermission PRIMARY KEY (Id)
);

CREATE TABLE DotNet.dbo.ApUser (
	Id bigint IDENTITY(1,1) NOT NULL,
	UserId int NULL,
	UserName nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	FirstName nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	LastName nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	Mobile nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	Email nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	Password nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsPasswordReset bit NULL,
	[Type] tinyint NULL,
	Status tinyint NULL,
	MetaData nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	OtpRequest bit NULL,
	IsActiveDirectoryEnabled bit NOT NULL,
	IsActive int NULL,
	CreatedBy int NULL,
	CreatedDate datetime2 NULL,
	UpdatedBy int NULL,
	UpdatedDate datetime2 NULL,
	CONSTRAINT PK_ApUser PRIMARY KEY (Id)
);

CREATE TABLE DotNet.dbo.ApUserRole (
	Id int IDENTITY(1,1) NOT NULL,
	UserId bigint NOT NULL,
	RoleId int NOT NULL,
	IsActive int NULL,
	CreatedBy int NULL,
	CreatedDate datetime2 NULL,
	UpdatedBy int NULL,
	UpdatedDate datetime2 NULL,
	CONSTRAINT PK_ApUserRole PRIMARY KEY (Id)
);

CREATE TABLE DotNet.dbo.TrxJournal (
	JournalId bigint IDENTITY(1,1) NOT NULL,
	TrxId varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	TrxDate datetime NOT NULL,
	TrxType varchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	WalletUserId bigint NOT NULL,
	WalletName varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	DebitAmount decimal(18,4) DEFAULT 0 NULL,
	CreditAmount decimal(18,4) DEFAULT 0 NULL,
	BalanceAfterTrx decimal(18,4) DEFAULT 0 NULL,
	Description varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CreatedAt datetimeoffset NOT NULL,
	CreatedBy nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NULL,
	UpdatedBy nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsDeleted bit DEFAULT 0 NOT NULL,
	CONSTRAINT PK__TrJourna__250103E62B12A777 PRIMARY KEY (JournalId)
);

CREATE TABLE DotNet.dbo.TrxFeeSettlement (
    SettlementId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TrxId VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    TrxType VARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    StakeholderId BIGINT NULL,
    FeeFixedAmount DECIMAL(18,4) NULL,
    FeePercentage DECIMAL(18,4) NULL,
    IsPaid BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL,
    CreatedBy NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE DotNet.dbo.TrxStakeholder (
    StakeholderId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    StakeholderName NVARCHAR(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    StakeholderType NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    Description NVARCHAR(500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    BankCode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    WalletUserId NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL,
    CreatedBy NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    UpdatedAt DATETIMEOFFSET NULL,
    UpdatedBy NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE DotNet.dbo.TrxInfos (
    -- Transaction Identifiers
    TrxInfoId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TrxRefId NVARCHAR(200) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    TrxId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
    TrxType NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    TrxParentId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    TrxMode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    TrxChannel NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Transaction Details
    Amount DECIMAL(18,2) NOT NULL CHECK (Amount >= 0),  -- Should not be negative
    Fee DECIMAL(18,2) DEFAULT 0 NULL CHECK (Fee >= 0),  -- Should not be negative
    FeePercentage DECIMAL(18,2) DEFAULT 0 NOT NULL CHECK (FeePercentage >= 0),  -- Should not be negative
    IsFeeFixed BIT DEFAULT 0 NULL,
    FeeVatamount decimal(18,2) DEFAULT 0 NULL,
    FeeVatPercentage decimal(18,2) DEFAULT 0 NOT NULL,
    Cost DECIMAL(18,4) DEFAULT 0 NULL CHECK (Cost >= 0),  -- Should not be negative
    Revenue DECIMAL(18,4) DEFAULT 0 NULL CHECK (Revenue >= 0),  -- Should not be negative
    
    -- Sender & Receiver
    SenderId BIGINT NULL,
    ReceiverId BIGINT NULL,
    MerchantId BIGINT NULL,
    CustomerId BIGINT NULL,
    SenderBalanceAfterTrx DECIMAL(18,2) DEFAULT 0 NOT NULL CHECK (SenderBalanceAfterTrx >= 0),  -- Should not be negative
    ReceiverBalanceAfterTrx DECIMAL(18,2) DEFAULT 0 NOT NULL CHECK (ReceiverBalanceAfterTrx >= 0),  -- Should not be negative
    
    -- Status & Timings
    InitiationDateTime DATETIME2 NULL,
    CompletionDateTime DATETIME2 NULL,
    TrxStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    TrxExpiry DATETIME2 NULL, -- Expiration time (useful for temp holds)
    RetryCount INT DEFAULT 0, -- Tracks retry attempts
    IsDisputed BIT DEFAULT 0, -- Helps identify disputed transactions
    
    -- OTP Details
    OtpCode NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    OtpExpiry DATETIME2 NULL,
    OtpStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    OtpAttempts INT DEFAULT 0 NOT NULL,
    
    -- Routing & Reference Information
    CurrencyCode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    RefTrxId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    PurposeOfTrx NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    RouteId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Taxation & Charges
    TaxAmount DECIMAL(18,4) DEFAULT 0 NOT NULL CHECK (TaxAmount >= 0),  -- Should not be negative
    TaxPercentage DECIMAL(18,2) DEFAULT 0 NOT NULL CHECK (TaxPercentage >= 0),  -- Should not be negative
    ServiceCharge DECIMAL(18,4) DEFAULT 0 NOT NULL CHECK (ServiceCharge >= 0),  -- Should not be negative

    -- Refunds & Chargebacks
    IsRefunded BIT DEFAULT 0 NOT NULL,
    RefundAmount DECIMAL(18,2) DEFAULT 0 NOT NULL CHECK (RefundAmount >= 0),  -- Should not be negative
    RefundReferenceId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    ChargebackStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Currency Exchange
    ExchangeRate DECIMAL(18,6) NULL CHECK (ExchangeRate >= 0),  -- Should not be negative
    ConvertedAmount DECIMAL(18,2) NULL CHECK (ConvertedAmount >= 0),  -- Should not be negative
    OriginalCurrencyCode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Risk & Fraud Monitoring
    RiskScore DECIMAL(5,2) NULL CHECK (RiskScore >= 0),  -- Should not be negative
    IsFlaggedForReview BIT DEFAULT 0 NULL,
    FraudDetectionStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Loan & Credit Transactions
    IsLoanDisbursement BIT DEFAULT 0 NULL,
    LoanRepaymentId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CreditLimitUtilized DECIMAL(18,2) DEFAULT 0 NULL CHECK (CreditLimitUtilized >= 0),  -- Should not be negative

    -- Cashback, Discounts & Promotions
    CashbackAmount DECIMAL(18,2) DEFAULT 0 NULL CHECK (CashbackAmount >= 0),  -- Should not be negative
    DiscountAmount DECIMAL(18,2) DEFAULT 0 NULL CHECK (DiscountAmount >= 0),  -- Should not be negative
    PromoCodeUsed NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Multi-Party Transactions
    SplitPaymentId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    SplitPaymentRatio DECIMAL(5,2) DEFAULT 0 NULL CHECK (SplitPaymentRatio >= 0),  -- Should not be negative

    -- Authorization & Compliance
    AMLFlagged BIT DEFAULT 0 NOT NULL,
    KYCStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    ComplianceStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Settlement
    IsSettled BIT DEFAULT 0 NOT NULL,
    SettledAmount DECIMAL(18,2) DEFAULT 0 NOT NULL CHECK (SettledAmount >= 0),  -- Should not be negative
    SettlementReferenceId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    SettlementStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    SettlementDate DATETIME2 NULL,

    -- User Info
    CreatedAt DATETIMEOFFSET NOT NULL,
    CreatedBy NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL,
    UpdatedBy NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IsDeleted BIT NOT NULL,

    -- Additional Fields
    WalletUpdated BIT DEFAULT 0 NOT NULL,
    RefNoSendingBank NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    RefNoReceivingBank NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IDTPRefNo NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    Remarks NVARCHAR(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    ErrorMessage NVARCHAR(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    FailureReason NVARCHAR(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Regulatory & Compliance
    RegulatoryReportFlag BIT DEFAULT 0 NULL,
    SanctionsScreeningStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    CustomerRiskLevel NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Issuer Bank Details
    IssuerBankCode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IssuerAccountMasked NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IssuerResponseCode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IssuerResponseDescription NVARCHAR(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IssuerBankTrxId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    
    -- Acquirer Bank Details
    AcquirerBankCode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    AcquirerBankTrxId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    AcquirerResponseCode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    AcquirerResponseDescription NVARCHAR(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    BankResponseDateTime DATETIME2 NULL,

    -- Location & Device
    Longitude NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    Latitude NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    DeviceId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IpAddress NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,

    -- Payment Gateway Integration
    GatewayTransactionId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, -- Unique ID from the payment gateway
    GatewayResponseCode NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, -- Response code from the gateway
    GatewayResponseMessage NVARCHAR(250) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, -- Response message from the gateway
    GatewayRequestDateTime DATETIME2 NULL, -- Timestamp of the request sent to the gateway
    GatewayResponseDateTime DATETIME2 NULL, -- Timestamp of the response received from the gateway

    -- Reconciliation
    IsReconciled BIT DEFAULT 0 NOT NULL, -- Indicates if the transaction is reconciled
    ReconciliationDate DATETIME2 NULL, -- Date when the transaction was reconciled
    ReconciliationReferenceId NVARCHAR(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, -- Reference ID for reconciliation
    ReconciliationStatus NVARCHAR(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL, -- Status of reconciliation (e.g., Pending, Completed)

    -- Journal & Stakeholder Info
    IsJournal BIT DEFAULT 0 NULL,
    JournalCostReceiverId BIGINT NULL,
    JournalCostSenderId BIGINT NULL,
    
    -- Indexes
    INDEX IX_TrxInfos_TrxId NONCLUSTERED (TrxId)
);

ALTER TABLE DotNet.dbo.TrxInfos
ADD CONSTRAINT UQ_TrxInfos_TrxId UNIQUE (TrxId);


CREATE TABLE DotNet.dbo.TrxWallet (
	WalletId bigint IDENTITY(1,1) NOT NULL,
	AspNetUserId bigint NOT NULL,
	Version timestamp NOT NULL,
	EncWalletBalance nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	WalletBalance decimal(18,2) NOT NULL,
	CreatedAt datetimeoffset NOT NULL,
	CreatedBy nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	LastUpdatedAt datetimeoffset NOT NULL,
	LastUpdatedBy nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsDeleted bit DEFAULT 0 NOT NULL,
	CONSTRAINT PK__Wallet__84D4F90EBA0E34C1 PRIMARY KEY (WalletId)
);

CREATE TABLE DotNet.dbo.TrxTypeInfo (
	TrxTypeId bigint IDENTITY(1,1) NOT NULL,
	TrxType nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	TrxCategory nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	TrxDescription nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	TrxGroupId bigint NULL,
	IsCustomerInitiated bit NOT NULL,
	IsMerchantInitiated bit NOT NULL,
	FeePayee nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsOnSpotSettlement bit NOT NULL,
	IsPinRequired bit DEFAULT 0 NOT NULL,
	IsOtpRequired bit DEFAULT 0 NOT NULL,
	IsFaceVerificationRequired bit DEFAULT 0 NOT NULL,
	CurrencyCode nvarchar(10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsActive bit NOT NULL,
	IsDeleted bit DEFAULT 0 NOT NULL,
	CreatedAt datetimeoffset DEFAULT sysdatetimeoffset() NOT NULL,
	CreatedBy nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset DEFAULT sysdatetimeoffset() NOT NULL,
	UpdatedBy nvarchar(255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CONSTRAINT PK_TrxTypes PRIMARY KEY (TrxTypeId)
);

CREATE TABLE DotNet.dbo.TrxFeeDistributionConfig (
	TrxFeeDistributionConfigId bigint IDENTITY(1,1) NOT NULL,
	TrxTypeId bigint NULL,
	StakeholderName nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	StakeholderId bigint NOT NULL,
	FeeInPercentage decimal(18,2) NOT NULL,
	IsFeeApplied bit NOT NULL,
	IsActive bit NOT NULL,
	CreatedAt datetimeoffset NOT NULL,
	CreatedBy nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NOT NULL,
	UpdatedBy nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsDeleted bit NOT NULL,
	CONSTRAINT PK_TrxFeeDistributionConfig PRIMARY KEY (TrxFeeDistributionConfigId)
);

CREATE TABLE DotNet.dbo.TrxFeePackage (
	TrxFeePackageId bigint IDENTITY(1,1) NOT NULL,
	PackageName nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UserType nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsDefault bit NULL,
	IsActive bit NOT NULL,
	CreatedAt datetimeoffset NOT NULL,
	CreatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NOT NULL,
	UpdatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsDeleted bit NOT NULL,
	CONSTRAINT PK_TrxFeePackage PRIMARY KEY (TrxFeePackageId)
);

CREATE TABLE DotNet.dbo.TrxFeePackageDetail (
    TrxFeePackageDetailId bigint IDENTITY(1,1) NOT NULL,
    TrxFeePackageId bigint NOT NULL,
    TrxTypeId bigint NOT NULL,
    FeeSettlementWay nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IsSlabRequired bit NOT NULL,
    FeeAmountFixed decimal(18,2) NOT NULL,
    FeeAmountInPercentage decimal(18,2) NOT NULL,
    StaticMaximumFeeAmount decimal(18,2) NOT NULL,
    IsMaximumApplicable bit NOT NULL,
    VatDeductPercentOnFeeAmount decimal(18,2) DEFAULT 15 NULL,
    IsQtyFee bit NULL,
    StartDate datetime2 NOT NULL,
    EndDate datetime2 NOT NULL,
    IsActive bit NOT NULL,
    CreatedAt datetimeoffset NOT NULL,
    CreatedBy nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    UpdatedAt datetimeoffset NOT NULL,
    UpdatedBy nvarchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
    IsDeleted bit NULL,
    CONSTRAINT PK_TrxFeePackageDetail PRIMARY KEY (TrxFeePackageDetailId)
);

CREATE TABLE DotNet.dbo.TrxLimitPackage (
	TrxLimitPackageId bigint IDENTITY(1,1) NOT NULL,
	PackageName nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UserType nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	WalletHoldAmount decimal(18,2) DEFAULT 0.0 NOT NULL,
	IsDefault bit NOT NULL,
	IsActive bit NOT NULL,
	CreatedAt datetimeoffset NOT NULL,
	CreatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NOT NULL,
	UpdatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsDeleted bit NOT NULL,
	CONSTRAINT PK_TrxLimitPackage PRIMARY KEY (TrxLimitPackageId),
);

CREATE TABLE DotNet.dbo.TrxLimitPackageDetail (
	TrxLimitPackageDetailId bigint IDENTITY(1,1) NOT NULL,
	TrxLimitPackageId bigint NOT NULL,
	TrxTypeId bigint NOT NULL,
	MinTrxAmount decimal(18,2) NOT NULL,
	MaxTrxAmount decimal(18,2) NOT NULL,
	MaxTrxAmountPerDay decimal(18,2) NOT NULL,
	MaxTrxAmountPerMonth decimal(18,2) NOT NULL,
	MaxNumberOfTrxPerDay int NOT NULL,
	MaxNumberOfTrxPerMonth int NOT NULL,
	OtpRequiredAmount decimal(18,2) NULL,
	TrxGroup varchar(20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsActive bit NOT NULL,
	IsDeleted bit NOT NULL,
	CreatedAt datetimeoffset NOT NULL,
	CreatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NOT NULL,
	UpdatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CONSTRAINT PK_TrxLimitPackageDetail PRIMARY KEY (TrxLimitPackageDetailId)
);

CREATE TABLE DotNet.dbo.TrxFeeSlab (
	TrxFeeSlabId bigint IDENTITY(1,1) NOT NULL,
	TrxFeePackageDetailId bigint NOT NULL,
	Description varchar(200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	MinAmount numeric(18,4) NOT NULL,
	MaxAmount numeric(18,4) NOT NULL,
	SegmentAmount numeric(18,4) NOT NULL,
	FeeAmountFixed numeric(18,4) NOT NULL,
	FeeAmountInPercentage numeric(18,4) NOT NULL,
	StaticMaximumFeeAmount numeric(18,4) NOT NULL,
	IsMaximumApplicable bit NULL,
	VatDeductPercentOnFeeAmount decimal(18,2) NULL,
	IsActive bit NOT NULL,
	CreatedAt datetimeoffset NULL,
	CreatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NULL,
	UpdatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	IsDeleted bit NULL,
	CONSTRAINT PK_TrxFeeSlab PRIMARY KEY (TrxFeeSlabId)
);

CREATE TABLE DotNet.dbo.TrxLimitPerDay(
	Id bigint IDENTITY(1,1) NOT NULL,
	UserId bigint NOT NULL,
	[Day] date NULL,
	[Month] int DEFAULT 0 NULL,
	[year] int DEFAULT 0 NULL,
	TrxTypeId bigint NULL,
	NumberOfTrxPerDay int DEFAULT 0 NOT NULL,
	TrxAmountPerDay decimal(18,2) DEFAULT 0 NOT NULL,
	LastTrxId nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NOT NULL,
	UpdatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CONSTRAINT PK_TrxLimitPerDay PRIMARY KEY (Id)
);
CREATE TABLE DotNet.dbo.TrxLimitPerMonth (
	Id bigint IDENTITY(1,1) NOT NULL,
	UserId bigint NOT NULL,
	[Month] int DEFAULT 0 NOT NULL,
	[year] int DEFAULT 0 NOT NULL,
	TrxTypeId bigint NOT NULL,
	NumberOfTrxPerMonth int DEFAULT 0 NOT NULL,
	TrxAmountPerMonth decimal(18,2) DEFAULT 0 NOT NULL,
	LastTrxId nvarchar(50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NOT NULL,
	UpdatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CONSTRAINT PK_TrxLimitPerMonth PRIMARY KEY (Id)
);
CREATE TABLE DotNet.dbo.TrxDailyBalance (
	TrxDailyBalanceId bigint IDENTITY(1,1) NOT NULL,
	[Year] int NOT NULL,
	[Month] int NOT NULL,
	[Day] int NOT NULL,
	WalletUserId bigint NOT NULL,
	WalletName varchar(100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	OpeningBalance decimal(18,4) DEFAULT 0 NOT NULL,
	Debit decimal(18,4) DEFAULT 0 NOT NULL,
	Credit decimal(18,4) DEFAULT 0 NOT NULL,
	ClosingBalance decimal(18,4) DEFAULT 0 NOT NULL,
        IsDeleted bit NOT NULL,
	CreatedAt datetimeoffset NOT NULL,
	CreatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	UpdatedAt datetimeoffset NOT NULL,
	UpdatedBy nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	CONSTRAINT PK_TrxDailyBalance PRIMARY KEY (TrxDailyBalanceId)
);