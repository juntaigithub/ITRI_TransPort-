using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SemacV14;
using SemacV14.Entity;
using SemacV14.Service;
using System.Net.Sockets;
using System.Data;

namespace Transport
{
    public class Core
    {
        private static System.Timers.Timer _timer = new System.Timers.Timer();
        private static System.Timers.Timer _timerDelAllLog = new System.Timers.Timer();

        static string ChannelID;
        static int TerminalID;
        static string MacAddress;
        static int SyncTimeSEQ;
        static bool blTodayIsSyncOK;

        private string Msg;

        public static IRealTimeCallback RealTimeCallBack = null;

        public delegate void TerminalConnectHandler(SemacV14.Entity.TerminalEntity En);
        public delegate void ExecuteHandler(SemacV14.Service.ExecuteArgz Ea);

        private DataTable TerminalTable = null;
        private DataTable dtCardReader = null;
        private DataTable dtCardReaderSyncTime = null;

        private static System.Timers.Timer _timerCheckUser = new System.Timers.Timer();
        private DataTable dtCheckUserSyncTime = null;
        static bool blIsCheckUserSyncOK;

        public void Start() {
            try
            {
                EventLog.Write("{0}-{1}", "Start", "成功啟動");
                Console.WriteLine("Start");
                Console.WriteLine(string.Format("Set Port:{0}", StaticConstant.MM_Port));
                EventLog.Write("{0}-{1}", "Start", string.Format("Set Port:{0}", StaticConstant.MM_Port));
                EventLog.Write("{0}-{1}", "Start", string.Format("App Name:{0}", System.Diagnostics.Process.GetCurrentProcess().ProcessName));
                setData();
            }
            catch (Exception ex)
            {
                EventLog.Write("Error:{0}-{1}", "Start", ex.ToString());
                Console.WriteLine(ex.Message);
            }
        }
        public void Stop() {
            try
            {
                EventLog.Write("{0}-{1}", "Stop", "成功停止");
                Console.WriteLine("Stop");
            }
            catch (Exception ex)
            {
                EventLog.Write("Error:{0}-{1}", "Stop", ex.ToString());
                Console.WriteLine(ex.Message);
            }
        }

        private void setData()
        {
            blTodayIsSyncOK = false;
            blIsCheckUserSyncOK = false;

            ini();

            //設定Timer - 同步使用者及時間
            setTimerSyncUser(false);
            //設定Timer - 刪除已接收過的讀卡紀錄 
            setTimerDelAllSyncLog(false);
            //設定Timer - 差異更新
            setTimerCheckUser(true);

            getCardReader();
            getCardReaderSyncTime();
            getCheckUserSyncTime();

            BeginListen();

        }
        private void setTimerSyncUser(bool blEnabled)
        {
            _timer.Enabled = false;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
            _timer.Interval = 1000 * StaticConstant.MM_SyncUserSecond;
            _timer.Enabled = blEnabled;
        }
        private void setTimerDelAllSyncLog(bool blEnabled)
        {
            _timerDelAllLog.Enabled = false;
            _timerDelAllLog.Elapsed += new System.Timers.ElapsedEventHandler(_timerDelAllLog_Elapsed);
            _timerDelAllLog.Interval = 1000 * StaticConstant.MM_DelLogSecond;
            _timerDelAllLog.Enabled = blEnabled;
        }
        private void setTimerCheckUser(bool blEnabled)
        {
            _timerCheckUser.Enabled = false;
            _timerCheckUser.Elapsed += new System.Timers.ElapsedEventHandler(_timerCheckUser_Elapsed);
            _timerCheckUser.Interval = 1000 * StaticConstant.MM_SyncUserSecond;
            _timerCheckUser.Enabled = blEnabled;
        }
        private void ini()
        {
            ChannelID = "";
            TerminalID = 0;
            MacAddress = "";

            //Terminal List
            this.TerminalTable = new DataTable();
            string[] cols = new string[] {
                "ChannelID",
                "Status",
                "TerminalID",
                "IPAddress",
                "MacAddress",
                "SerialNo",
                "ModelName",
                "FirmwareVersion"
            };
            foreach (string s in cols)
            {
                DataColumn col = new DataColumn();
                col.ColumnName = s;
                this.TerminalTable.Columns.Add(col);
            }
        }
        private void getCardReader()
        {
            this.dtCardReader = BO.getCardReader();
        }
        private void getCardReaderSyncTime()
        {
            this.dtCardReaderSyncTime = BO.getCardReaderSyncTime();
        }
        private void getCheckUserSyncTime()
        {
            this.dtCheckUserSyncTime = BO.getCheckUserSyncTime();
        }

        #region SmartV14 Events
        private void BeginListen()
        {
            //Connected
            SemacV14.Service.CurrentService.OnConnected += this.OnConnected;
            //DisConnected
            SemacV14.Service.CurrentService.OnDisconnected += this.OnDisconnected;
            //Start TCP Listen
            SemacV14.Service.CurrentService.StartListen(ushort.Parse(StaticConstant.MM_Port));
            //Realtime Transaction
            SemacV14.Service.CurrentService.OnRealtimeTransaction += this.HandleRealtime;
            //OffLineLog Transaction
            SemacV14.Service.CurrentService.OffLineLogTransaction += this.HandleRealtime;
            Console.WriteLine(string.Format("ListenPort:{0}", SemacV14.Service.CurrentService.Port.ToString()));
        }
        //OnConnected
        private void OnConnected(SemacV14.Entity.TerminalEntity En)
        {
            EventLog.Write("{0}-{1}", "OnConnected", "Start");
            try
            {
                ChannelID = En.ChannelID;
                TerminalID = En.TerminalID;
                MacAddress = En.MacAddress;

                lock (this.TerminalTable)
                {
                    DataRow row = this.TerminalTable.NewRow();
                    row["ChannelID"] = En.ChannelID;            //Connection ID
                    row["Status"] = En.Status;                 //On Line status
                    row["TerminalID"] = En.TerminalID.ToString(); //TerminalID
                    row["IPAddress"] = En.IPAddress;            //IP Address
                    row["MacAddress"] = En.MacAddress;         //MAC Address
                    row["SerialNo"] = En.SerialNo;            //Serial No.
                    row["ModelName"] = En.ModelName;          //Model Name
                    row["FirmwareVersion"] = En.FirmwareVersion;        //Firmware Version

                    this.TerminalTable.Rows.Add(row);
                }

                EventLog.Write("{0}-{1}", "OnConnected", "Start-UpdateCardReaderState");
                int iResult = 0;//0.Insert ,1.Update
                DBO.CardReader dt = new DBO.CardReader();
                foreach (DataRow dr in dtCardReader.Rows)
                {
                    if (dr["TerminalID"].ToString() == En.TerminalID.ToString())
                    {
                        iResult = 1;
                        break;
                    }
                }
                dt.Status = En.Status.Equals("On Line")?"1":"0";
                dt.TerminalID = En.TerminalID.ToString();
                dt.IPAddress = En.IPAddress;
                dt.MacAddress = En.MacAddress;
                dt.SerialNo = En.SerialNo;
                dt.ModelName = En.ModelName;
                dt.FirmwareVersion = En.FirmwareVersion;
                dt.IsUse = "Y";
                if (iResult == 0)
                    BO.insertCardReader(dt);
                else
                    BO.updateCardReader(dt);

                Msg = string.Format("OnConnected,CardReader:{0},{1}", TerminalTable.Rows.Count.ToString(), En.ToMyString);
                EventLog.Write("{0}-{1}", "OnConnected", Msg);
                Console.WriteLine(Msg);

                _timer.Enabled = true;
            }
            catch (Exception ex) { EventLog.Write("Error:{0}-{1}", "OnConnected", ex.ToString()); }
        }

        //OnDisconnected
        private void OnDisconnected(SemacV14.Entity.TerminalEntity En)
        {
            EventLog.Write("{0}-{1}", "OnDisconnected", "OnDisconnected");
            try { 

                lock (TerminalTable)
                {
                    DBO.CardReader dt = new DBO.CardReader();
                    foreach (DataRow row in TerminalTable.Rows)
                    {
                        if (row["TerminalID"].ToString() == En.TerminalID.ToString())
                        {
                            TerminalTable.Rows.Remove(row);
                            dt.TerminalID = En.TerminalID.ToString();
                            dt.Status = dt.Status = En.Status.Equals("On Line") ? "1" : "0";
                            BO.updateCardReaderState(dt);
                            break;
                        }
                    }
                }
                Msg = string.Format("OnDisconnected,CardReader:{0},{1}", TerminalTable.Rows.Count.ToString(), En.ToMyString);
                EventLog.Write("{0}-{1}", "OnDisconnected", Msg);
                Console.WriteLine(Msg);

                _timer.Enabled = false;
            }
            catch (Exception ex) { EventLog.WriteError("{0}-{1}", "OnDisconnected", ex.ToString()); }
        }

        //Realtime
        private void HandleRealtime(SemacV14.Service.ExecuteArgz Ea)
        {
            try
            {
                if (!(Ea.EntityList == null))
                {
                    foreach (SemacV14.Entity.DoorLogEntity En in Ea.EntityList)
                    {
                        EventLog.Write("{0}-{1}", "HandleRealtime", En.ToMyString);
                        insertLog(En);
                        _timerDelAllLog.Enabled = false;
                        _timerDelAllLog.Enabled = true;
                    }
                }
            }
            catch (Exception ex) { EventLog.WriteError("{0}-{1}", "HandleRealtime", ex.ToString()); }
        }
        private void HandleEntity(SemacV14.Service.ExecuteArgz Ea)
        {
            string sCmd = "";
            try
            {
                sCmd = Ea.CommandType.ToString();
                switch (Ea.IsCompleted)
                {
                    case true:
                        if (!(Ea.CallbackEntity == null))
                        {
                            EventLog.Write("{0}-{1}", "HandleEntity.Ea.IsCompleted=true", "成功:" + sCmd + ":" + Ea.CallbackEntity.ToMyString);
                        }
                        else
                        {
                            if (!(Ea.EntityList == null))
                            {
                                EventLog.Write("{0}-{1}", "HandleEntity.Ea.IsCompleted=true", "成功" + sCmd + ":筆數:" + Ea.EntityList.Count.ToString());
                            }
                        }
                        break;
                    case false:
                        EventLog.Write("{0}-{1}", "HandleEntity.Ea.IsCompleted=false", "失敗:" + sCmd + ":" + Ea.ErrorMessage.ToString());
                        break;
                }
            }
            catch (Exception ex) { EventLog.WriteError("{0}-{1}", "HandleEntity", sCmd + ":" + ex.ToString()); }
        }
        #endregion

        private void insertLog(SemacV14.Entity.DoorLogEntity table)
        {
           BO.insertCardLog(table.CardNo
                , table.TerminalID.ToString()
                , table.EntryDate.ToString("yyyy/MM/dd HH:mm:ss")
                , table.EventAlarmCode
                , table.EventAlarmCodeInt32.ToString()
                , table.LogIndex.ToString());
        }
        private void SyncUserNew()
        {
            try {
                //檢查CardReaderSyncTime的時間範圍，是否符合可執行的時段
                if (!getIsSync()) return;            

                EventLog.Write("{0}-{1}", "SyncUserNew", "Start");

                foreach (DataRow drT in TerminalTable.Rows)
                {
                    ChannelID = drT["ChannelID"].ToString();
                    TerminalID = Convert.ToInt32(drT["TerminalID"].ToString());
                    MacAddress = drT["MacAddress"].ToString();

                    this.dtCardReader = BO.getCardReader(TerminalID.ToString());
                    DataTable dtSyncCardReaderUserLog = BO.getSyncCardReaderUserLog(TerminalID.ToString(),SyncTimeSEQ.ToString());

                    //判斷今天是否已同步完成
                    if (dtSyncCardReaderUserLog.Rows.Count == 0)
                    {
                        if (dtCardReader.Rows[0]["IsSyncAll"].ToString().Equals("1"))
                        {
                            DeleteAllUser();
                            BO.UpdateCardReaderIsSyncAll(TerminalID.ToString());
                        }

                        int addCount = 0, delCount = 0, AllCount=0;
                        DataTable dt = BO.getPersonCardToCardReader(TerminalID.ToString());
                        foreach (DataRow dr in dt.Rows)
                        {
                            User user = new User
                            {
                                UserID = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber),
                                EmployeeID = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber).ToString(),
                                OverWrite = true,
                                CardNo = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber).ToString(),
                                UserName = dr["Cn"].ToString(),
                                CheckExpire = true,
                                ExpiredFrom = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd HH:mm:ss")),
                                ExpiredTo = Convert.ToDateTime(DateTime.Now.AddDays(1000).ToString("yyyy/MM/dd HH:mm:ss")),
                                EnabledStatus = true,//!entry.Black && entry.ExpireOn.Value >= DateTime.Now,
                                UserType = 0,
                                Group01 = 1,
                                Group02 = 0,
                                Group03 = 0,
                                Group04 = 0,
                                BypassTimeZoneLevel = 1,
                                PersonalPassword = ""
                            };

                            int RCount = addCount + delCount;
                            if (dr["State"].ToString().Equals("D"))
                            {
                                if (GetOneUser(user.UserID) == 1)
                                {
                                    if (UserDeletionRequest(user.UserID) == 1)
                                        delCount++;
                                    else if (UserDeletionRequest(user.UserID) == 1)
                                        delCount++;
                                    else if (UserDeletionRequest(user.UserID) == 1)
                                        delCount++;
                                    else if (UserDeletionRequest(user.UserID) == 1)
                                        delCount++;
                                }
                                else
                                    delCount++;
                            }
                            else
                            {
                                if (AddUser(user))
                                    addCount++;
                                else if (AddUser(user))
                                    addCount++;
                                else if (AddUser(user))
                                    addCount++;
                                else if (AddUser(user))
                                    addCount++;
                            }

                            if (RCount < (delCount + addCount))
                            {
                                BO.sp_TFSyncUserSuccess(dr["SEQ"].ToString());
                            }
                        }

                        Msg = String.Format(@"已同步【白名單】,人員筆數:{0},更新筆數:{1},刪除筆數:{2},失敗筆數:{3},TerminalID={4}, MacAddress={5}"
                            , dt.Rows.Count.ToString(), addCount.ToString(), delCount.ToString()
                            , (dt.Rows.Count - addCount - delCount).ToString()
                            , TerminalID, MacAddress);
                        EventLog.Write("{0}-{1}", "SyncUser", Msg);
                        
                        BO.insertSyncUserLog(TerminalID.ToString(), Msg);

                        if ((addCount + delCount) == dt.Rows.Count)
                        {
                            BO.insertSyncCardReaderUserLog(TerminalID.ToString(),SyncTimeSEQ.ToString());

                            //查詢卡機人員筆數
                            QueryTheNumberOfAlreadyRegisteredUsers();

                            blTodayIsSyncOK = true;
                        }
                    }
                    else
                        blTodayIsSyncOK = true;
                }
                EventLog.Write("{0}-{1}", "SyncUserNew", "End-Sync User");
            }
            catch (Exception ex) { EventLog.WriteError("{0}-{1}", "SyncUserNew", ex.ToString()); }
        }
        //private void SyncUser()
        //{
        //    DataTable dt = BO.getPersonCard();
        //    DataTable dtSyncCardReaderUserLog = BO.getSyncCardReaderUserLog();

        //    foreach (DataRow drT in TerminalTable.Rows) 
        //    {
        //        ChannelID = drT["ChannelID"].ToString();
        //        TerminalID = Convert.ToInt32(drT["TerminalID"].ToString());
        //        MacAddress = drT["MacAddress"].ToString();

        //        //判斷今天是否已同步完成 0:未, 1:完成
        //        int iYN = 0;
        //        foreach (DataRow drCRUL in dtSyncCardReaderUserLog.Rows)
        //        {
        //            if (TerminalID.ToString().Equals(drCRUL["TerminalID"].ToString()))
        //            {
        //                Msg = String.Format("今日已同步【白名單】,TerminalID={0}, MacAddress={1}", TerminalID, MacAddress);
        //                EventLog.Write("{0}-{1}", "SyncUser", Msg);
        //                iYN = 1;
        //            }
        //        }

        //        if (iYN == 0) 
        //        {
        //            Msg = String.Format("DeleteAllUser,TerminalID={0}, MacAddress={1}", TerminalID, MacAddress);
        //            EventLog.Write("{0}-{1}", "SyncUser", Msg);
        //            DeleteAllUser();
        //            System.Threading.Thread.Sleep(1000 * 10);
        //            Msg = String.Format("AddUser-Start,TerminalID={0}, MacAddress={1}", TerminalID, MacAddress);
        //            EventLog.Write("{0}-{1}", "SyncUser", Msg);
        //            int iCount = 0;
        //            foreach (DataRow dr in dt.Rows)
        //            {
        //                try {
        //                    User user = new User
        //                    {
        //                        //UserID = Convert.ToUInt32(dr["Uid"].ToString()),
        //                        UserID = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber),
        //                        EmployeeID = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber).ToString(),
        //                        OverWrite = true,
        //                        CardNo = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber).ToString(),
        //                        UserName = dr["Cn"].ToString(),
        //                        CheckExpire = true,
        //                        ExpiredFrom = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd HH:mm:ss")),
        //                        ExpiredTo = Convert.ToDateTime(DateTime.Now.AddDays(30).ToString("yyyy/MM/dd HH:mm:ss")),
        //                        EnabledStatus = true,//!entry.Black && entry.ExpireOn.Value >= DateTime.Now,
        //                        UserType = 0,
        //                        Group01 = 1,
        //                        Group02 = 0,
        //                        Group03 = 0,
        //                        Group04 = 0,
        //                        BypassTimeZoneLevel = 1,
        //                        PersonalPassword = ""
        //                    };

        //                    if (GetOneUser(user.UserID) == 0)
        //                    {
        //                        if (!AddUser(user))
        //                            iCount++;
        //                        else if (!AddUser(user))
        //                            iCount++;
        //                        else if (!AddUser(user))
        //                            iCount++;
        //                        else if (!AddUser(user))
        //                            iCount++;
        //                    }
        //                    else
        //                        iCount++;

        //                    System.Threading.Thread.Sleep(500);
        //                }
        //                catch (Exception ex) { 
        //                    iCount++;
        //                    EventLog.WriteError("{0}-{1}", "SyncUser", ex.ToString()); }
        //            }
        //            Msg = String.Format("已同步【白名單】,人員筆數:{0},成功筆數:{1},失敗筆數:{2},TerminalID={3}, MacAddress={4}"
        //                , dt.Rows.Count.ToString(), (dt.Rows.Count - iCount).ToString(), iCount.ToString(), TerminalID, MacAddress);
        //            EventLog.Write("{0}-{1}", "SyncUser", Msg);
        //            BO.insertSyncUserLog(TerminalID.ToString(), Msg);
        //            if (iCount == 0)
        //            {
        //                BO.insertSyncCardReaderUserLog(TerminalID.ToString());

        //                //查詢卡機人員筆數
        //                QueryTheNumberOfAlreadyRegisteredUsers();
        //            }
        //        }
        //    }

        //    //查檢今日是否都已同步完成
        //    dtSyncCardReaderUserLog = BO.getSyncCardReaderUserLog();
        //    foreach (DataRow drT in TerminalTable.Rows)
        //    {
        //        ChannelID = drT["ChannelID"].ToString();
        //        TerminalID = Convert.ToInt32(drT["TerminalID"].ToString());
        //        MacAddress = drT["MacAddress"].ToString();

        //        //判斷今天是否已同步完成 0:未, 1:完成
        //        foreach (DataRow drCRUL in dtSyncCardReaderUserLog.Rows)
        //        {
        //            if (TerminalID.ToString().Equals(drCRUL["TerminalID"].ToString()))
        //                _timer.Enabled = false;
        //        }
        //    }
        //}

        #region 卡機指令

        public string DeleteAllUser()
        {
            try
            {
                if (!checkCardReader("DeleteAllUser"))
                    return "99,99";

                EventLog.Write("{0}-{1}", "DeleteAllUser", "Start");

                var re = new SemacV14.Request.CommonRequest(TerminalID, Define.CommandType.AllUsersDeletion);
                var Ea = new SemacV14.Service.ExecuteArgz(ChannelID, re);

                //Execute 
                SemacV14.Service.Actor Wrk = new Actor(Ea);
                Ea = Wrk.SendAndReceive(); //  'Send command,and return result

                // check the result
                if (Ea.IsCompleted == true)
                {
                    EventLog.Write("{0}-{1}", "DeleteAllUser", "Success");
                    return "1,1";
                }
                else
                {
                    EventLog.Write("{0}-{1}", "DeleteAllUser", "Fialure");
                    return "0," + Ea.ErrorMessage;
                }
                //SemacV14.Service.Actor act = new Actor(Ea);

                //act.OnEntityDataArrival += HandleEntity;
                //act.Send();

                //return true;
            }
            catch (Exception ex)
            {
                Msg = String.Format("刪除所有使用者發生錯誤:{0},TerminalID={1}, ChannelID={2}"
                    , ex.Message, TerminalID, ChannelID);
                EventLog.WriteError("{0}-{1}", "DeleteAllUser", Msg);
                return "99,99";
            }
        }
        public bool AddUser(User _user)
        {
            try
            {
                if (!checkCardReader("AddUser"))
                    return false;

                //EventLog.Write("{0}-{1}", "AddUser", "Start:" + _user.UserID + "-" + _user.UserName);

                var re = new SemacV14.Request.RegisterModifyUserDataRequest(TerminalID);

                re.UserID = _user.UserID;
                re.EmployeeID = _user.EmployeeID;
                re.OverWrite = _user.OverWrite;
                re.CardNo = _user.CardNo;
                re.UserName = _user.UserName;
                re.CheckExpire = _user.CheckExpire;
                re.ExpiredFrom = _user.ExpiredFrom;
                re.ExpiredTo = _user.ExpiredTo;
                re.EnabledStatus = _user.EnabledStatus;
                re.UserType = (Define.UserType)_user.UserType;
                re.Group01 = _user.Group01;
                re.Group02 = _user.Group02;
                re.Group03 = _user.Group03;
                re.Group04 = _user.Group04;
                re.BypassTimeZoneLevel = (Define.BypassTimeZoneLevel)_user.BypassTimeZoneLevel;
                re.PersonalPassword = _user.PersonalPassword;

                SemacV14.Service.ExecuteArgz Ea = new SemacV14.Service.ExecuteArgz(ChannelID, re);

                //Execute 
                SemacV14.Service.Actor Wrk = new Actor(Ea);
                Ea = Wrk.SendAndReceive(); //  'Send command,and return result

                // check the result
                if (Ea.IsCompleted == true)
                {
                    //EventLog.Write("{0}-{1}", "AddUser", "Success");
                    return true;
                }
                else
                {
                    EventLog.Write("{0}-{1}", "AddUser", "Failure:" + _user.UserID + "-" + _user.UserName);
                    Msg = String.Format("加入使用者({0})發生錯誤:{1},TerminalID={2}, MacAddress={3}"
                                        , _user.UserID, Ea.ErrorMessage, TerminalID, MacAddress);
                    EventLog.Write("{0}-{1}", "AddUser", Msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Msg = String.Format("加入使用者({0})發生錯誤:{1},TerminalID={2}, MacAddress={3}"
                    , _user.UserID, ex.Message, TerminalID, MacAddress);
                EventLog.WriteError("{0}-{1}", "AddUser", Msg);
                return false;
            }
        }
        public string QueryTheNumberOfAlreadyRegisteredUsers()
        {
            try
            {
                if (!checkCardReader("QueryTheNumberOfAlreadyRegisteredUsers"))
                    return "99,99";

                EventLog.Write("{0}-{1}", "QueryTheNumberOfAlreadyRegisteredUsers", "Start");

                var re = new SemacV14.Request.CommonRequest(TerminalID, Define.CommandType.QueryTheNumberOfAlreadyRegisteredUsers);
                var Ea = new SemacV14.Service.ExecuteArgz(ChannelID, re);

                //Execute 
                SemacV14.Service.Actor Wrk = new Actor(Ea);
                Ea = Wrk.SendAndReceive(); //  'Send command,and return result

                // check the result
                if (Ea.IsCompleted == true)
                {
                    EventLog.Write("{0}-{1}", "QueryTheNumberOfAlreadyRegisteredUsers", "Success");

                    //Msg = String.Format("筆數:{0},TerminalID={1}, ChannelID={2}", Ea.EntityList.Count.ToString(), TerminalID, ChannelID);
                    //EventLog.Write("{0}-{1}", "QueryTheNumberOfAlreadyRegisteredUsers", Msg);

                    SemacV14.Entity.QueryTheNumberOfAlreadyRegisteredUsersEntity En = (SemacV14.Entity.QueryTheNumberOfAlreadyRegisteredUsersEntity)Ea.CallbackEntity;

                    BO.UpdateCardReaderUserCount(TerminalID.ToString(), En.RegisteredCount.ToString());

                    return "1," + En.RegisteredCount.ToString();
                }
                else
                {
                    EventLog.Write("{0}-{1}", "QueryTheNumberOfAlreadyRegisteredUsers", "Failure");
                    Msg = String.Format("ErrorMsg:{0},TerminalID={1}, ChannelID={2}", Ea.ErrorMessage, TerminalID, ChannelID);
                    EventLog.Write("{0}-{1}", "QueryTheNumberOfAlreadyRegisteredUsers", Msg);
                    return "0,"+ Ea.ErrorMessage;
                }
                //SemacV14.Service.Actor act = new Actor(Ea);

                //act.OnEntityDataArrival += HandleEntity;
                //act.Send();

                //return true;
            }
            catch (Exception ex)
            {
                Msg = String.Format("ErrorMsg:{0},TerminalID={1}, ChannelID={2}", ex.Message, TerminalID, ChannelID);
                EventLog.WriteError("{0}-{1}", "QueryTheNumberOfAlreadyRegisteredUsers", Msg);
                return "99,99";
            }
        }        
        private int GetOneUser(UInt32 UserID)
        {
            try
            {
                if (!checkCardReader("GetOneUser"))
                return 99;

                SemacV14.Request.GetUserDataRequest Re = new SemacV14.Request.GetUserDataRequest(TerminalID, UserID);
                SemacV14.Service.ExecuteArgz Ea = new SemacV14.Service.ExecuteArgz(ChannelID, Re);

                //Execute 
                SemacV14.Service.Actor Wrk = new Actor(Ea);
                Ea = Wrk.SendAndReceive(); //  'Send command,and return result

                // check the result
                if (Ea.IsCompleted == true)
                {
                    SemacV14.Entity.GetUserDataEntity En = (SemacV14.Entity.GetUserDataEntity)Ea.CallbackEntity;
                    //MessageBox.Show(En.UserName);
                    //MessageBox.Show(En.CardNo );                          
                    return 1;
                }
                else
                {
                    //MessageBox.Show(Ea.ErrorMessage);  // Failed
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Msg = String.Format("UserID:{0},ErrorMsg:{1},TerminalID={2}, ChannelID={3}", UserID.ToString(), ex.Message, TerminalID, ChannelID);
                EventLog.WriteError("{0}-{1}", "GetOneUser", Msg);
                return 99;
            }
        }
        private int UserDeletionRequest(UInt32 UserID)
        {
            try
            {
                if (!checkCardReader("UserDeletionRequest"))
                    return 99;

                //EventLog.Write("{0}-{1}", "UserDeletionRequest", "Start");

                SemacV14.Request.UserDeletionRequest Re = new SemacV14.Request.UserDeletionRequest(TerminalID, UserID);
                SemacV14.Service.ExecuteArgz Ea = new SemacV14.Service.ExecuteArgz(ChannelID, Re);

                //Execute 
                SemacV14.Service.Actor Wrk = new Actor(Ea);
                Wrk.TimeoutMinisecond = 10 * 60 * 1000;  //those cmd may need more time,but in default , timeout checking was 10 seconds.
                Ea = Wrk.SendAndReceive(); //  'Send command,and return result

                // check the result
                if (Ea.IsCompleted == true)
                {
                    //EventLog.Write("{0}-{1}", "UserDeletionRequest", "Success");
                    return 1;
                }
                else
                {
                    EventLog.Write("{0}-{1}", "UserDeletionRequest", "Failure:" + UserID);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Msg = String.Format("刪除人員發生錯誤:{0},TerminalID={1}, ChannelID={2}"
                    , ex.Message, TerminalID, ChannelID);
                EventLog.WriteError("{0}-{1}", "UserDeletionRequest", Msg);
                return 99;
            }
        }
        public int  SetNewTimeAndDate()
        {
            EventLog.Write("{0}-{1}", "SetNewTimeAndDate", "Start-Sync DateTime");
            try
            {
                if (!checkCardReader("SetNewTimeAndDate"))
                    return 99;

                SemacV14.Request.SetNewTimeAndDateRequest Re = new SemacV14.Request.SetNewTimeAndDateRequest(TerminalID, DateTime.Now);
                SemacV14.Service.ExecuteArgz Ea = new SemacV14.Service.ExecuteArgz(ChannelID, Re);

                //Execute 
                SemacV14.Service.Actor Wrk = new Actor(Ea);
                Ea = Wrk.SendAndReceive(); //  'Send command,and return result

                // check the result
                if (Ea.IsCompleted == true)
                {
                    EventLog.Write("{0}-{1}", "SetNewTimeAndDate", "Sync DateTime Success");
                    return 1;
                }
                else
                {
                    EventLog.Write("{0}-{1}", "SetNewTimeAndDate", "Sync DateTime Failure");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Msg = String.Format("同步時間發生錯誤:{0},TerminalID={1}, ChannelID={2}"
                    , ex.Message, TerminalID, ChannelID);
                EventLog.WriteError("{0}-{1}", "SetNewTimeAndDate", Msg);
                return 99;
            }


            //SemacV14.Define.CommandType cmd = SemacV14.Define.CommandType.SetNewTimeAndDate;
            //if (cmd <= 0) return;

            //DataTable dtSyncCardReaderUserLog = BO.getSyncCardReaderUserLog();

            //foreach (DataRow drT in TerminalTable.Rows)
            //{
            //    ChannelID = drT["ChannelID"].ToString();
            //    TerminalID = Convert.ToInt32(drT["TerminalID"].ToString());

            //    //判斷今天是否已同步完成 0:未, 1:完成
            //    int iYN = 0;
            //    foreach (DataRow drCRUL in dtSyncCardReaderUserLog.Rows)
            //    {
            //        if (TerminalID.Equals(drCRUL["TerminalID"].ToString()))
            //            iYN = 1;
            //    }

            //    if (iYN == 0)
            //    {
            //        var re = new SemacV14.Request.SetNewTimeAndDateRequest(TerminalID, DateTime.Now);

            //        var Ea = new SemacV14.Service.ExecuteArgz(ChannelID, re);

            //        if (Ea == null) return;
            //        SemacV14.Service.Actor act = new Actor(Ea);
            //        act.TimeoutMinisecond = 10 * 60 * 1000;  //those cmd may need more time,but in default , timeout checking was 10 seconds.

            //        act.OnEntityDataArrival += this.HandleEntity;
            //        act.Send();
            //    }
            //}
        }
        /// <summary>刪除已接收過的讀卡紀錄</summary>
        public string DeletingAllEntryExitLog()
        {
            try
            {
                if (!checkCardReader("DeletingAllEntryExitLog"))
                    return "99,99";

                EventLog.Write("{0}-{1}", "DeletingAllEntryExitLog", "Start");

                var re = new SemacV14.Request.CommonRequest(TerminalID, Define.CommandType.DeletingAllEntryExitLog);
                var Ea = new SemacV14.Service.ExecuteArgz(ChannelID, re);

                //Execute 
                SemacV14.Service.Actor Wrk = new Actor(Ea);
                Ea = Wrk.SendAndReceive(); //  'Send command,and return result

                // check the result
                if (Ea.IsCompleted == true)
                {
                    EventLog.Write("{0}-{1}", "DeletingAllEntryExitLog", "Success");

                    return "1,1";
                }
                else
                {
                    EventLog.Write("{0}-{1}", "DeletingAllEntryExitLog", "Failure");
                    Msg = String.Format("ErrorMsg:{0},TerminalID={1}, ChannelID={2}", Ea.ErrorMessage, TerminalID, ChannelID);
                    EventLog.Write("{0}-{1}", "DeletingAllEntryExitLog", Msg);
                    return "0," + Ea.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                Msg = String.Format("ErrorMsg:{0},TerminalID={1}, ChannelID={2}", ex.Message, TerminalID, ChannelID);
                EventLog.WriteError("{0}-{1}", "DeletingAllEntryExitLog", Msg);
                return "99,99";
            }
        }

        #endregion

        private bool checkCardReader(string functionName)
        {
            bool blnResult = true;
            if (TerminalID < 1 || ChannelID.Length < 1)
            {
                Msg = String.Format("讀卡機狀態錯誤:TerminalID={0}, ChannelID={1}"
                    , TerminalID, ChannelID);
                EventLog.WriteError("{0}-{1}", functionName, Msg);
                blnResult = false;
            }
            bool bStatus = SemacV14.Service.CurrentService.get_IsAlive(ChannelID);
            if (bStatus == false)
            {
                Msg = String.Format("讀卡機狀態錯誤:TerminalID={0}, ChannelID={1}, Status={2}"
                    , TerminalID, ChannelID, bStatus);
                EventLog.WriteError("{0}-{1}", functionName, Msg);
                blnResult = false;
            }
            return blnResult;
        }

        #region Timer
        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //檢查CardReaderSyncTime的時間範圍，是否符合可執行的時段
            if (!getIsSync()) { blTodayIsSyncOK = false; return; }
            if (blTodayIsSyncOK) return;

            _timer.Enabled = false;
            if (StaticConstant.MM_IsSyncDT)
            {
                SetNewTimeAndDate();
                System.Threading.Thread.Sleep(10 * 1000);
            }

            if (StaticConstant.MM_IsSyncUser) {
                SyncUserNew();
            }
            _timer.Enabled = true;
        }
        private void _timerDelAllLog_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerDelAllLog.Enabled = false;
            DeletingAllEntryExitLog();
        }


        #endregion

        /// <summary>檢查CardReaderSyncTime的時間範圍，是否符合可執行的時段</summary>
        private bool getIsSync()
        { 
            SyncTimeSEQ = 0;
            bool blResult = false;
            foreach (DataRow dr in dtCardReaderSyncTime.Rows)
            {
                string StartTime,EndTime;
                int intS1,intS2;
                StartTime = dr["StartTime"].ToString();
                EndTime = dr["EndTime"].ToString();
                DateTime dt1=Convert.ToDateTime(StartTime); 
                DateTime dt2=Convert.ToDateTime(EndTime); 
                DateTime dt3=DateTime.Now; 

                intS1=0;
                intS2=0;
                if(DateTime.Compare(dt3,dt1)>=0) 
                    intS1=1;
                if(DateTime.Compare(dt2,dt3)>=0) 
                    intS2=1;

                if (intS1 == 1 && intS2 == 1)
                {
                    blResult = true;
                    SyncTimeSEQ = Convert.ToInt16(dr["SEQ"].ToString());
                }
            }
            return blResult;
        }

        #region CheckUser
        /// <summary>
        /// CheckUser Timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timerCheckUser_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //_timerCheckUser.Enabled = false;
            //忙線中
            if (blIsCheckUserSyncOK)
                return;

            blIsCheckUserSyncOK = true;
            //不在時限內，則不執行
            if (!getIsSyncCheckUser())
            {
                EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "不在時間區間內 return");
                Console.WriteLine("不在時間區間內 return");
                blIsCheckUserSyncOK = false;
                return;
            }
            //卡機基本資料-最後同步時間 等於 目前日期，則不執行
            //卡機基本資料-人員名單數量 等於 人員基本資料數量，則不執行
            if (BO.isReturn(TerminalID.ToString()))
            {
                EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "不符合規則 return");
                Console.WriteLine("不符合規則 return");
                blIsCheckUserSyncOK = false;
                return;
            }

            EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "=== _timerCheckUser Start ===");
            Console.WriteLine("=== _timerCheckUser Start ===");

            //清除資料CheckUserList
            EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "1.清除資料CheckUserList");
            Console.WriteLine("1.清除資料CheckUserList");
            BO.delCheckUserList(TerminalID.ToString());

            EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "2.取得卡機人員筆數");
            Console.WriteLine("2.取得卡機人員筆數");
            int iState = 0;
            //取得卡機人員筆數
            iState = RetrievingUserIDList();
            if (iState == 0) iState = RetrievingUserIDList();
            if (iState == 0) iState = RetrievingUserIDList();
            if (iState != 1)
            {
                EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "取得卡機人員筆數失敗 return");
                Console.WriteLine("取得卡機人員筆數失敗 return");
                blIsCheckUserSyncOK = false;
                return;
            }

            //比對人員名單
            EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "3.比對人員名單");
            Console.WriteLine("3.比對人員名單");
            BO.CompareUserList(TerminalID.ToString());

            //同步時間
            EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "4.同步時間");
            Console.WriteLine("4.同步時間");
            SetNewTimeAndDate();
            System.Threading.Thread.Sleep(10 * 1000);

            //差異更新
            EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "5.差異更新");
            Console.WriteLine("5.差異更新");
            SyncCRUser();
            System.Threading.Thread.Sleep(10 * 1000);

            //查詢卡機人員筆數
            EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "6.查詢卡機人員筆數");
            Console.WriteLine("6.查詢卡機人員筆數");
            QueryTheNumberOfAlreadyRegisteredUsers();

            EventLog.WriteCU("{0}-{1}", "_timerCheckUser_Elapsed", "=== _timerCheckUser End ===");
            Console.WriteLine("=== _timerCheckUser End ===");
            //_timerCheckUser.Enabled = true;
            blIsCheckUserSyncOK = false;
        }
        private bool getIsSyncCheckUser()
        {
            bool blResult = false;
            foreach (DataRow dr in dtCheckUserSyncTime.Rows)
            {
                string StartTime, EndTime;
                int intS1, intS2;
                StartTime = dr["StartTime"].ToString();
                EndTime = dr["EndTime"].ToString();
                DateTime dt1 = Convert.ToDateTime(StartTime);
                DateTime dt2 = Convert.ToDateTime(EndTime);
                DateTime dt3 = DateTime.Now;

                intS1 = 0;
                intS2 = 0;
                if (DateTime.Compare(dt3, dt1) >= 0)
                    intS1 = 1;
                if (DateTime.Compare(dt2, dt3) >= 0)
                    intS2 = 1;

                if (intS1 == 1 && intS2 == 1)
                {
                    blResult = true;
                }
            }
            return blResult;
        }

        /// <summary>
        /// 取得卡機內 UserID List
        /// </summary>
        /// <returns></returns>
        private int RetrievingUserIDList()
        {
            try
            {
                if (!checkCardReader("RetrievingUserIDList"))
                    return 99;

                var re = new SemacV14.Request.CommonRequest(TerminalID, Define.CommandType.RetrievingUserIDList);
                var Ea = new SemacV14.Service.ExecuteArgz(ChannelID, re);

                //Execute 
                SemacV14.Service.Actor Wrk = new Actor(Ea);
                Wrk.TimeoutMinisecond = 10 * 60 * 1000;  //those cmd may need more time,but in default , timeout checking was 10 seconds.
                Ea = Wrk.SendAndReceive(); //  'Send command,and return result

                // check the result
                if (Ea.IsCompleted == true)
                {
                    EventLog.Write("{0}-{1}", "RetrievingUserIDList", "Success");
                    SemacV14.Entity.RetrievingUserIDListEntity En = (SemacV14.Entity.RetrievingUserIDListEntity)Ea.CallbackEntity;
                    
                    BO.insertCheckUserLog(TerminalID.ToString(),En.Count.ToString());
                    string SEQ = BO.getCheckUserLogSEQ(TerminalID.ToString());

                    int iCount = 0;
                    foreach (uint Item in En.Items)
                    {
                        iCount++;
                        Console.WriteLine(iCount.ToString() + "-" + String.Format("{0:X}", Convert.ToInt64(Item.ToString())));
                        BO.insertCheckUserList(TerminalID.ToString(), String.Format("{0:X}", Convert.ToInt64(Item.ToString())));
                        BO.insertCheckUserListTemp(SEQ, TerminalID.ToString(), String.Format("{0:X}", Convert.ToInt64(Item.ToString())));
                    }
                    return 1;
                }
                else
                {
                    EventLog.WriteCU("{0}-{1}", "RetrievingUserIDList.Ea.IsCompleted=false", "失敗:" + Ea.ErrorMessage.ToString());
                     //EventLog.Write("{0}-{1}", "RetrievingUserIDList", "Failure");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Msg = String.Format("ErrorMsg:{0},TerminalID={1}, ChannelID={2}", ex.Message, TerminalID, ChannelID);
                EventLog.WriteCU("{0}-{1}", "RetrievingUserIDList", Msg);
                return 99;
            }
        }
        /// <summary>
        /// 差異更新
        /// </summary>
        private void SyncCRUser() 
        {
            blIsCheckUserSyncOK = true;
            try
            {
                EventLog.WriteCU("{0}-{1}", "SyncCRUser", "Start");

                DataTable dt = BO.getCheckUserToCR(TerminalID.ToString());
                foreach (DataRow dr in dt.Rows)
                {
                    User user = new User
                    {
                        UserID = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber),
                        EmployeeID = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber).ToString(),
                        OverWrite = true,
                        CardNo = UInt32.Parse(dr["HexCardOrderNo"].ToString(), System.Globalization.NumberStyles.HexNumber).ToString(),
                        UserName = dr["Cn"].ToString(),
                        CheckExpire = true,
                        ExpiredFrom = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd HH:mm:ss")),
                        ExpiredTo = Convert.ToDateTime(DateTime.Now.AddDays(1000).ToString("yyyy/MM/dd HH:mm:ss")),
                        EnabledStatus = true,//!entry.Black && entry.ExpireOn.Value >= DateTime.Now,
                        UserType = 0,
                        Group01 = 1,
                        Group02 = 0,
                        Group03 = 0,
                        Group04 = 0,
                        BypassTimeZoneLevel = 1,
                        PersonalPassword = ""
                    };
                    if (dr["State"].ToString().Equals("D"))
                    {
                        UserDeletionRequest(user.UserID);
                    }
                    else
                    {
                        AddUser(user);
                    }
                }
                EventLog.WriteCU("{0}-{1}", "SyncCRUser", "End-Sync CRUser");
            }
            catch (Exception ex) { EventLog.WriteCU("{0}-{1}", "SyncCRUser", ex.ToString()); }

            blIsCheckUserSyncOK = false;
        }
        #endregion
    }
}
