using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Transport
{
    public class User
    {
        private uint _userid;
        private string _employeeid;
        private bool _overwrite;
        private string _cardno;
        private string _username;
        private bool _checkexpire;
        private DateTime _expirefrom;
        private DateTime _expireto;
        private bool _enabledstatus;
        private int _usertype;
        private int _group01;
        private int _group02;
        private int _group03;
        private int _group04;
        private int _bypasstimezonelevel;
        private string _personalpassword;

        // Apply the DataMemberAttribute to the property.
        public uint UserID { get { return _userid; } set { _userid = value; } }

        public string EmployeeID { get { return _employeeid; } set { _employeeid = value; } }

        public bool OverWrite { get { return _overwrite; } set { _overwrite = value; } }

        public string CardNo { get { return _cardno; } set { _cardno = value; } }

        public string UserName { get { return _username; } set { _username = value; } }

        public bool CheckExpire { get { return _checkexpire; } set { _checkexpire = value; } }

        public DateTime ExpiredFrom { get { return _expirefrom; } set { _expirefrom = value; } }

        public DateTime ExpiredTo { get { return _expireto; } set { _expireto = value; } }

        public bool EnabledStatus { get { return _enabledstatus; } set { _enabledstatus = value; } }

        public int UserType { get { return _usertype; } set { _usertype = value; } }

        public int Group01 { get { return _group01; } set { _group01 = value; } }

        public int Group02 { get { return _group02; } set { _group02 = value; } }

        public int Group03 { get { return _group03; } set { _group03 = value; } }

        public int Group04 { get { return _group04; } set { _group04 = value; } }

        public int BypassTimeZoneLevel { get { return _bypasstimezonelevel; } set { _bypasstimezonelevel = value; } }

        public string PersonalPassword { get { return _personalpassword; } set { _personalpassword = value; } }
    }
    public class LogEntry
    {
        private string _CardNo;
        private int _DoorNo;
        private DateTime _EntryDate;
        private string _EventAlarmCode;
        private int _EventAlarmCodeInt32;
        private string _FunctionKey;
        private int _FunctionKeyInt32;
        private string _InOutIndication;
        private int _InOutIndicationInt32;
        private int _LogIndex;
        private int _TerminalID;
        private uint _UserID;
        private string _VerificationSource;
        private int _VerificationSourceInt32;

        // Apply the DataMemberAttribute to the property.
        public string CardNo { get { return _CardNo; } set { _CardNo = value; } }

        public int DoorNo { get { return _DoorNo; } set { _DoorNo = value; } }

        public DateTime EntryDate { get { return _EntryDate; } set { _EntryDate = value; } }

        public string EventAlarmCode { get { return _EventAlarmCode; } set { _EventAlarmCode = value; } }

        public int EventAlarmCodeInt32 { get { return _EventAlarmCodeInt32; } set { _EventAlarmCodeInt32 = value; } }

        public string FunctionKey { get { return _FunctionKey; } set { _FunctionKey = value; } }

        public int FunctionKeyInt32 { get { return _FunctionKeyInt32; } set { _FunctionKeyInt32 = value; } }

        public string InOutIndication { get { return _InOutIndication; } set { _InOutIndication = value; } }

        public int InOutIndicationInt32 { get { return _InOutIndicationInt32; } set { _InOutIndicationInt32 = value; } }

        public int LogIndex { get { return _LogIndex; } set { _LogIndex = value; } }

        public int TerminalID { get { return _TerminalID; } set { _TerminalID = value; } }

        public uint UserID { get { return _UserID; } set { _UserID = value; } }

        public string VerificationSource { get { return _VerificationSource; } set { _VerificationSource = value; } }

        public int VerificationSourceInt32 { get { return _VerificationSourceInt32; } set { _VerificationSourceInt32 = value; } }
    }
    public interface IRealTimeCallback
    {
        void OnHandleRealTime_In(LogEntry En);

        void OnHandleDataEntity_In(string Msg);
    }
}
