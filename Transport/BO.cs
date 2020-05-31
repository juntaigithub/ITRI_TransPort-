using System;
using System.Data;
using System.Data.SqlClient;

namespace Transport
{
    public static class BO
    {
        public static DataTable getCardReader()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = "select * from CardReader Order by TerminalID";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static DataTable getCardReader(string TerminalID)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = "select * from CardReader Where TerminalID = @TerminalID";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static DataTable getCardReaderSyncTime()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = "select * from CardReaderSyncTime Order by SEQ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static DataTable getPersonCard()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = "select * from PersonCard Order by Uid";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static DataTable getPersonCardToCardReader(string TerminalID)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
SELECT *
FROM [dbo].[PersonCardToCardReader]
WHERE [TerminalID] = @TerminalID and SyncState=0
Order by SEQ ASC
";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static DataTable getSyncCardReaderUserLog()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = "SELECT * FROM SyncCardReaderUserLog WHERE CONVERT(CHAR(8),LastModify,112)=CONVERT(CHAR(8),GETDATE(),112)";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static DataTable getSyncCardReaderUserLog(string TerminalID,string SyncTimeSEQ)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
SELECT * 
FROM SyncCardReaderUserLog 
WHERE CONVERT(CHAR(8),LastModify,112)=CONVERT(CHAR(8),GETDATE(),112)
    And TerminalID = @TerminalID
    And SyncTimeSEQ = @SyncTimeSEQ
";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.Parameters.AddWithValue("@SyncTimeSEQ", SyncTimeSEQ);

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static void insertCardReader(DBO.CardReader dt)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
INSERT INTO [dbo].[CardReader]
        ([Status],[TerminalID],[IPAddress],[MacAddress]
        ,[SerialNo],[ModelName],[FirmwareVersion]
        ,[LastModify],[IsUse])
VALUES (@Status,@TerminalID,@IPAddress,@MacAddress
        ,@SerialNo,@ModelName,@FirmwareVersion
        ,Getdate(),@IsUse)
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@Status", dt.Status);
                    command.Parameters.AddWithValue("@TerminalID", dt.TerminalID);
                    command.Parameters.AddWithValue("@IPAddress", dt.IPAddress);
                    command.Parameters.AddWithValue("@MacAddress", dt.MacAddress);
                    command.Parameters.AddWithValue("@SerialNo", dt.SerialNo);
                    command.Parameters.AddWithValue("@ModelName", dt.ModelName);
                    command.Parameters.AddWithValue("@FirmwareVersion", dt.FirmwareVersion);
                    command.Parameters.AddWithValue("@IsUse", dt.IsUse);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void updateCardReader(DBO.CardReader dt)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
UPDATE [dbo].[CardReader]
   SET Status=@Status, IPAddress=@IPAddress
        ,MacAddress=@MacAddress, SerialNo=@SerialNo, ModelName=@ModelName
        ,FirmwareVersion=@FirmwareVersion
        ,LastModify=GETDATE(), IsUse=@IsUse
WHERE TerminalID = @TerminalID
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@Status", dt.Status);
                    command.Parameters.AddWithValue("@TerminalID", dt.TerminalID);
                    command.Parameters.AddWithValue("@IPAddress", dt.IPAddress);
                    command.Parameters.AddWithValue("@MacAddress", dt.MacAddress);
                    command.Parameters.AddWithValue("@SerialNo", dt.SerialNo);
                    command.Parameters.AddWithValue("@ModelName", dt.ModelName);
                    command.Parameters.AddWithValue("@FirmwareVersion", dt.FirmwareVersion);
                    command.Parameters.AddWithValue("@IsUse", dt.IsUse);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void insertCardLog(string CardNo, string TerminalID, string EntryDate, string EventAlarmCode, string EventAlarmCodeInt32, string LogIndex)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
INSERT INTO [dbo].[CardLog]([HexCardOrderNo],[CardNo],[TerminalID],[EntryDate]
    ,[EventAlarmCode],[EventAlarmCodeInt32]
    ,[LogIndex]
    ,[ModifyOn])
VALUES (@HexCardOrderNo,@CardNo,@TerminalID,@EntryDate
    ,@EventAlarmCode,@EventAlarmCodeInt32
    ,@LogIndex
    ,Getdate())
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@HexCardOrderNo", String.Format("{0:X}", Convert.ToInt64(CardNo)));
                    command.Parameters.AddWithValue("@CardNo", CardNo);
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.Parameters.AddWithValue("@EntryDate", EntryDate);
                    command.Parameters.AddWithValue("@EventAlarmCode", EventAlarmCode);
                    command.Parameters.AddWithValue("@EventAlarmCodeInt32", EventAlarmCodeInt32);
                    command.Parameters.AddWithValue("@LogIndex", LogIndex);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void insertSyncUserLog(string TerminalID, string LogText)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
Exec dbo.sp_SyncUserLog @TerminalID, @State;
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.Parameters.AddWithValue("@State", LogText);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void insertSyncCardReaderUserLog(string TerminalID)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
INSERT INTO [dbo].[SyncCardReaderUserLog]([TerminalID],[LastModify])
 VALUES (@TerminalID , Getdate())
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void insertSyncCardReaderUserLog(string TerminalID, string SyncTimeSEQ)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
INSERT INTO [dbo].[SyncCardReaderUserLog]([TerminalID],[SyncTimeSEQ],[LastModify])
 VALUES (@TerminalID,@SyncTimeSEQ , Getdate())
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.Parameters.AddWithValue("@SyncTimeSEQ", SyncTimeSEQ);
                    command.ExecuteNonQuery();
                }
            }
        }

        #region CardReader
        public static void updateCardReaderState(DBO.CardReader dt)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
UPDATE [dbo].[CardReader]
   SET Status=@Status,LastModify=GETDATE()
WHERE TerminalID = @TerminalID
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@Status", dt.Status);
                    command.Parameters.AddWithValue("@TerminalID", dt.TerminalID);
                    command.ExecuteNonQuery();
                }
            }

        }

        public static void UpdateCardReaderUserCount(string TerminalID, string UserCount)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
UPDATE [dbo].[CardReader]
SET UserCount = @UserCount , UserCountDT = getdate()
WHERE TerminalID = @TerminalID
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.Parameters.AddWithValue("@UserCount", UserCount);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void UpdateCardReaderIsSyncAll(string TerminalID)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
UPDATE [dbo].[CardReader]
SET IsSyncAll = 0
WHERE TerminalID = @TerminalID
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region sp_TFSyncUserSuccess
        public static void sp_TFSyncUserSuccess(string SEQ)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"Exec sp_TFSyncUserSuccess @SEQ ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@SEQ", SEQ);
                    command.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region CheckUser
        public static DataTable getCheckUserSyncTime()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = "select * from CheckUserSyncTime Order by SEQ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// 是否跳離Timer 0.不跳離,1.跳離
        /// </summary>
        /// <param name="TerminalID"></param>
        /// <returns></returns>
        public static bool isReturn(string TerminalID)
        {
            bool isReturn = false;
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
--if((select Count(*) from [dbo].[CheckUserSyncTime] where convert(char(5),getdate(),108) between StartTime and EndTime)>0)
--begin
--	--select 0 as Result
--end
--else
--begin
--	--不在時間區間內
--	select 1 as isReturn	--跳離
--end
	Declare @isUC int,@isUCDT int
	Set @isUC = case when isnull((select UserCount from [dbo].[CardReader] where TerminalID = @TerminalID),0) = 
			isnull((select Count(*) from [dbo].[PersonCard]),0) 
		then 1
		else 0
		end
	Set @isUCDT = case when isnull((select Convert(char(8),UserCountDT,112) from [dbo].[CardReader] where TerminalID = @TerminalID),0) = 
			Convert(char(8),GetDate(),112) 
		then 1
		else 0
		end 
	if(@isUC=1 and @isUCDT=1)
		select 1 as isReturn	--跳離
	else
		select 0 as isReturn	--不跳離
";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["isReturn"].ToString()=="1")
                            isReturn = true;
                    }
                }
            }
            return isReturn;
        }
        public static void CompareUserList(string TerminalID)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
	--清空[dbo].[CheckUserToCR]
	Delete From CheckUserToCR Where TerminalID = @TerminalID

	--新增
	INSERT INTO [dbo].[CheckUserToCR]([TerminalID],[State],[Uid],[HexCardOrderNo],[Cn])
	SELECT @TerminalID, 'C',fn1.[Uid],fn1.[HexCardOrderNo],fn1.[Cn]
	FROM [dbo].[PersonCard] fn1
		LEFT OUTER JOIN (SELECT * FROM [dbo].[CheckUserList] WHERE [TerminalID] = @TerminalID) fn2 on fn1.[HexCardOrderNo] = fn2.[HexCardOrderNo]
	WHERE isnull(fn2.[HexCardOrderNo],'')=''

	--刪除
	INSERT INTO [dbo].[CheckUserToCR]([TerminalID],[State],[Uid],[HexCardOrderNo],[Cn])
	SELECT @TerminalID, 'D','',fn1.[HexCardOrderNo],''
	FROM (SELECT * FROM [dbo].[CheckUserList] WHERE [TerminalID] = @TerminalID) fn1
		LEFT OUTER JOIN [dbo].[PersonCard] fn2 on fn1.[HexCardOrderNo] = fn2.[HexCardOrderNo]
	WHERE isnull(fn2.[HexCardOrderNo],'')=''
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static DataTable getCheckUserToCR(string TerminalID)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = "SELECT * FROM CheckUserToCR WHERE TerminalID=@TerminalID";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                }
            }
            return dt;
        }
        public static string getCheckUserLogSEQ(string TerminalID)
        {
            string SEQ = "0";
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
SELECT Max(SEQ) as SEQ
  FROM [dbo].[CheckUserLog]
  where TerminalID = @TerminalID
";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        SEQ = dr["SEQ"].ToString();
                    }
                }
            }
            return SEQ;
        }
        public static void insertCheckUserLog(string TerminalID, string CRUserCount)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
 INSERT INTO [dbo].[CheckUserLog]([TerminalID],[CreateDT],[UserCount],[CRUserCount])
 select TerminalID,getdate(),isnull((select Count(*) from [dbo].[PersonCard]),0) ,@CRUserCount 
 from [dbo].[CardReader] 
 where TerminalID = @TerminalID
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.Parameters.AddWithValue("@CRUserCount", CRUserCount);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void insertCheckUserList(string TerminalID, string HexCardOrderNo)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
INSERT INTO [dbo].[CheckUserList]([TerminalID],[HexCardOrderNo])
 VALUES (@TerminalID,@HexCardOrderNo)
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.Parameters.AddWithValue("@HexCardOrderNo", HexCardOrderNo);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static void delCheckUserList(string TerminalID)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
delete from [dbo].[CheckUserList] where TerminalID=@TerminalID
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void insertCheckUserListTemp(string CheckUserLogSEQ,string TerminalID, string HexCardOrderNo)
        {
            using (SqlConnection con = new SqlConnection(StaticConstant.MM_conn))
            {
                con.Open();
                string sql = @"
INSERT INTO [dbo].[CheckUserListTemp]([CheckUserLogSEQ],[TerminalID],[HexCardOrderNo])
 VALUES (@CheckUserLogSEQ,@TerminalID,@HexCardOrderNo)
 ";
                using (SqlCommand command = con.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@CheckUserLogSEQ", CheckUserLogSEQ);
                    command.Parameters.AddWithValue("@TerminalID", TerminalID);
                    command.Parameters.AddWithValue("@HexCardOrderNo", HexCardOrderNo);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}
