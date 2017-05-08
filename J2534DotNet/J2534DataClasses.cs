﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace J2534DotNet
{
    [StructLayout(LayoutKind.Explicit)]
    public class J2534Message
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public J2534PROTOCOL ProtocolID;
        [FieldOffset(4), MarshalAs(UnmanagedType.U4)]
        public J2534RXFLAG RxStatus;
        [FieldOffset(8), MarshalAs(UnmanagedType.U4)]
        public J2534TXFLAG TxFlags;
        [FieldOffset(12), MarshalAs(UnmanagedType.U4)]
        public uint Timestamp;
        [FieldOffset(16), MarshalAs(UnmanagedType.U4)]
        private int Datasize;
        [FieldOffset(20), MarshalAs(UnmanagedType.U4)]
        public uint ExtraDataIndex;
        [FieldOffset(24), MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4128)]
        private byte[] data;

        public J2534Message()
        {
            data = new byte[4128];
        }

        public J2534Message(J2534PROTOCOL ProtocolID, J2534TXFLAG TxFlags, List<byte> Data)
        {
            this.ProtocolID = ProtocolID;
            this.TxFlags = TxFlags;
            data = new byte[4128];
            this.Data = Data;
        }

        public List<byte> Data
        {
            get
            {
                return data.Take(Datasize).ToList();
            }
            set
            {
                if (value.Count > 4128)
                    throw new ArgumentException("Message data length greater than 4128");
                Array.Copy(value.ToArray(), data, value.Count);
                Datasize = value.Count;
            }
        }
    }

    public class PeriodicMsg
    {
        public J2534Message Message { get; set; }
        public int Interval { get; set; }
        internal int MessageID;
        public PeriodicMsg(J2534Message Message, int Interval)
        {
            this.Message = Message;
            this.Interval = Interval;
        }
    }

    public class MessageFilter
    {
        public J2534FILTER FilterType;
        public List<byte> Mask;
        public List<byte> Pattern;
        public List<byte> FlowControl;
        public J2534TXFLAG TxFlags;
        public int FilterId;

        public MessageFilter()
        {
            Mask = new List<byte>();
            Pattern = new List<byte>();
            FlowControl = new List<byte>();
            TxFlags = J2534TXFLAG.NONE;
        }

        public MessageFilter(COMMONFILTER FilterType, List<byte> Match)
        {
            Mask = new List<byte>();
            Pattern = new List<byte>();
            FlowControl = new List<byte>();
            TxFlags = J2534TXFLAG.NONE;

            switch (FilterType)
            {
                case COMMONFILTER.PASSALL:
                    PassAll();
                    break;
                case COMMONFILTER.PASS:
                    Pass(Match);
                    break;
                case COMMONFILTER.BLOCK:
                    Block(Match);
                    break;
                case COMMONFILTER.STANDARDISO15765:
                    StandardISO15765(Match);
                    break;
                case COMMONFILTER.NONE:
                    break;
            }
        }

        public void Clear()
        {
            Mask.Clear();
            Pattern.Clear();
            FlowControl.Clear();
        }

        public void PassAll()
        {
            Clear();
            Mask.Add(0x00);
            Pattern.Add(0x00);
            FilterType = J2534FILTER.PASS_FILTER;
        }

        public void Pass(List<byte> Match)
        {
            ExactMatch(Match);
            FilterType = J2534FILTER.PASS_FILTER;
        }

        public void Block(List<byte> Match)
        {
            ExactMatch(Match);
            FilterType = J2534FILTER.BLOCK_FILTER;
        }

        private void ExactMatch(List<byte> Match)
        {
            Clear();
            Mask = Enumerable.Repeat((byte)0xFF, Match.Count).ToList();
            Pattern = Match;
        }
        public void StandardISO15765(List<byte> SourceAddress)
        {
            //Should throw exception??
            if (SourceAddress.Count != 4)
                return;
            Clear();
            Mask.Add(0xFF);
            Mask.Add(0xFF);
            Mask.Add(0xFF);
            Mask.Add(0xFF);

            Pattern.AddRange(SourceAddress);
            Pattern[3] += 0x08;

            FlowControl.AddRange(SourceAddress);

            TxFlags = J2534TXFLAG.ISO15765_FRAME_PAD;
            FilterType = J2534FILTER.FLOW_CONTROL_FILTER;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public class SConfig
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public J2534PARAMETER Parameter;
        [FieldOffset(4), MarshalAs(UnmanagedType.U4)]
        public int Value;

        public SConfig(J2534PARAMETER Parameter, int Value)
        {
            this.Parameter = Parameter;
            this.Value = Value;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public class SConfigList
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public int NumOfParams;
        [FieldOffset(4)]
        public IntPtr pSConfig;

        public SConfigList(int NumOfParams, IntPtr Pointer)
        {
            this.NumOfParams = NumOfParams;
            this.pSConfig = Pointer;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public class SByteArray
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public int NumOfBytes;
        [FieldOffset(4)]
        public IntPtr Pointer;

        public SByteArray(int NumOfBytes, IntPtr Pointer)
        {
            this.NumOfBytes = NumOfBytes;
            this.Pointer = Pointer;
        }
    }

    //class to hold data reported from the Windows Registry about what J2534 Devices are installed
    public class J2534RegisteryEntry
    {
        public string Vendor { get; set; }
        public string Name { get; set; }
        public string FunctionLibrary { get; set; }
        public string ConfigApplication { get; set; }
        public int CANChannels { get; set; }
        public int ISO15765Channels { get; set; }
        public int J1850PWMChannels { get; set; }
        public int J1850VPWChannels { get; set; }
        public int ISO9141Channels { get; set; }
        public int ISO14230Channels { get; set; }
        public int SCI_A_ENGINEChannels { get; set; }
        public int SCI_A_TRANSChannels { get; set; }
        public int SCI_B_ENGINEChannels { get; set; }
        public int SCI_B_TRANSChannels { get; set; }

        public bool IsCANSupported
        {
            get { return (CANChannels > 0 ? true : false); }
        }

        public bool IsISO15765Supported
        {
            get { return (ISO15765Channels > 0 ? true : false); }
        }

        public bool IsJ1850PWMSupported
        {
            get { return (J1850PWMChannels > 0 ? true : false); }
        }

        public bool IsJ1850VPWSupported
        {
            get { return (J1850VPWChannels > 0 ? true : false); }
        }

        public bool IsISO9141Supported
        {
            get { return (ISO9141Channels > 0 ? true : false); }
        }

        public bool IsISO14230Supported
        {
            get { return (ISO14230Channels > 0 ? true : false); }
        }

        public bool IsSCI_A_ENGINESupported
        {
            get { return (SCI_A_ENGINEChannels > 0 ? true : false); }
        }

        public bool IsSCI_A_TRANSSupported
        {
            get { return (SCI_A_TRANSChannels > 0 ? true : false); }
        }

        public bool IsSCI_B_ENGINESupported
        {
            get { return (SCI_B_ENGINEChannels > 0 ? true : false); }
        }

        public bool IsSCI_B_TRANSSupported
        {
            get { return (SCI_B_TRANSChannels > 0 ? true : false); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}