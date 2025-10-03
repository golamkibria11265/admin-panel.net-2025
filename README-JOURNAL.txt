CREATE PROCEDURE dbo.GenerateTrxJournal  
AS  
BEGIN  
    SET NOCOUNT ON;
   
    DECLARE @TrxId VARCHAR(50);
   
    DECLARE DB_CURSOR CURSOR FOR  
    SELECT TrxId  
    FROM [dbo].[TrxInfos]  
    WHERE (IsJournal = 0 OR IsJournal IS NULL)  
    AND TrxType IN (SELECT TrxType FROM [dbo].[TrxTypes] WHERE IsActive = 1)
    AND TrxStatus IN (
   		'Authorized',
   	    'Refunded',
   	    'PartiallyRefunded',
   	    'Reversed',
   	    'Canceled'
   	);

    OPEN DB_CURSOR;  
    FETCH NEXT FROM DB_CURSOR INTO @TrxId;  

    WHILE @@FETCH_STATUS = 0  
    BEGIN  
        PRINT @TrxId;
        INSERT INTO [dbo].[TrxJournal]
           ([TrxId]
           ,[TrxDate]
           ,[TrxType]
           ,[WalletUserId]
           ,[WalletName]
           ,[DebitAmount]
           ,[CreditAmount]
           ,[BalanceAfterTrx]
           ,[Description]
           ,[CreatedAt]
           ,[CreatedBy]
           ,[UpdatedAt]
           ,[UpdatedBy]
           ,[IsDeleted])
        
        SELECT 
            Trx.TrxId,
            Trx.InitiationDateTime AS [TrxDate],
            Trx.TrxType,
            U.Id AS WalletUserId,
            U.Name AS WalletName,
            0 AS DebitAmount,
			CASE 
			    WHEN Trx.SenderId IS NOT NULL AND 
			         (SELECT TrxType.FeePayee FROM TrxTypes TrxType WHERE TrxType.TrxType = Trx.TrxType) = 'S' 
			         AND Trx.TrxType <> 'Refund' 
			    THEN Trx.Amount + ISNULL(Trx.Fee, 0) 			    
			    WHEN Trx.SenderId IS NOT NULL AND 
			         (SELECT TrxType.FeePayee FROM TrxTypes TrxType WHERE TrxType.TrxType = 
			             (SELECT TrxType FROM [dbo].[TrxInfos] RevTrx WHERE RevTrx.TrxId = Trx.RefTrxId)) = 'R' 
			         AND Trx.TrxType = 'Refund' 
			    THEN Trx.Amount + ISNULL(Trx.Fee, 0) 		
			    ELSE Trx.Amount 
			END AS CreditAmount,
            0 AS BalanceAfterTrx,
            Trx.PurposeOfTrx AS Description,
            SYSDATETIMEOFFSET() AS CreatedAt,
            'system' AS CreatedBy,
            SYSDATETIMEOFFSET() AS UpdatedAt,
            'system' AS UpdatedBy,
            0 AS IsDeleted
        FROM [dbo].[TrxInfos] Trx
        JOIN [dbo].[AspNetUsers] U ON Trx.SenderId = U.Id 
        WHERE Trx.TrxStatus IN (
	   		'Authorized',
	   	    'Refunded',
	   	    'PartiallyRefunded',
	   	    'Reversed',
	   	    'Canceled'
        ) AND Trx.TrxId = @TrxId
       
       
       UNION ALL

        SELECT 
            Trx.TrxId,
            Trx.InitiationDateTime AS [TrxDate],
            Trx.TrxType,
            U.Id AS WalletUserId,
            U.Name AS WalletName,
			CASE 
			    WHEN Trx.ReceiverId IS NOT NULL AND 
			         (SELECT TrxType.FeePayee FROM TrxTypes TrxType WHERE TrxType.TrxType = Trx.TrxType) = 'R' 
			         AND Trx.TrxType <> 'Refund' 
			    THEN Trx.Amount - ISNULL(Trx.Fee, 0) 			
			    WHEN Trx.ReceiverId IS NOT NULL AND 
			         (SELECT TrxType.FeePayee FROM TrxTypes TrxType WHERE TrxType.TrxType = 
			             (SELECT TrxType FROM [dbo].[TrxInfos] RevTrx WHERE RevTrx.TrxId = Trx.RefTrxId)) = 'S' 
			         AND Trx.TrxType = 'Refund' 
			    THEN Trx.Amount - ISNULL(Trx.Fee, 0) 
			    ELSE Trx.Amount 
			END AS DebitAmount,
            0 AS CreditAmount,
            0 AS BalanceAfterTrx,
            Trx.PurposeOfTrx AS Description,
            SYSDATETIMEOFFSET() AS CreatedAt,
            'system' AS CreatedBy,
            SYSDATETIMEOFFSET() AS UpdatedAt,
            'system' AS UpdatedBy,
            0 AS IsDeleted
        FROM [dbo].[TrxInfos] Trx
        JOIN [dbo].[AspNetUsers] U ON Trx.ReceiverId = U.Id 
        WHERE Trx.TrxStatus IN (
	   		'Authorized',
	   	    'Refunded',
	   	    'PartiallyRefunded',
	   	    'Reversed',
	   	    'Canceled'
        ) AND Trx.TrxId = @TrxId
       
       UNION ALL
       
       SELECT 
            Trx.TrxId,
            Trx.InitiationDateTime AS [TrxDate],
            Trx.TrxType,
            U.Id AS WalletUserId,
            U.Name AS WalletName,
			CASE WHEN FS.FeePercentage>0 THEN FS.FeePercentage ELSE 0 END AS DebitAmount,
			CASE WHEN FS.FeePercentage<0 THEN FS.FeePercentage*(-1) ELSE 0 END AS CreditAmount,
            0 AS BalanceAfterTrx,
            Trx.PurposeOfTrx AS Description,
            SYSDATETIMEOFFSET() AS CreatedAt,
            'system' AS CreatedBy,
            SYSDATETIMEOFFSET() AS UpdatedAt,
            'system' AS UpdatedBy,
            0 AS IsDeleted
		from [dbo].[TrxInfos] Trx 
		JOIN [dbo].[TrxFeeSettlement] FS ON Trx.TrxId=FS.TrxId
		JOIN [dbo].[TrxStakeholder] SH on FS.StakeHolderId=SH.StakeholderId 
		JOIN [dbo].[AspNetUsers] U ON SH.WalletUserId =U.Id
        WHERE Trx.TrxStatus IN (
	   		'Authorized',
	   	    'Refunded',
	   	    'PartiallyRefunded',
	   	    'Reversed',
	   	    'Canceled'
        ) AND Trx.TrxId = @TrxId;

-- 		  Optionally update IsJournal to 1 after processing
--        UPDATE [dbo].[TrxInfos]  
--        SET IsJournal = 1  
--        WHERE TrxId = @TrxId;

        -- Fetch next record
        FETCH NEXT FROM DB_CURSOR INTO @TrxId;  
    END  
    -- Close and Deallocate Cursor
    CLOSE DB_CURSOR;  
    DEALLOCATE DB_CURSOR;  
END;
