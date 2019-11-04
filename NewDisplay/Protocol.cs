using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace NewDisplay
{
    class Protocol
    {
        #region HEAD
        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public class HEAD
        {
            public byte STX;       // 0x02 1 Byte

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Bid_no = new byte[2];   // 100  2 Byte, 단말기ID
            

            public byte Opcode;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Length = new byte[2];   // 데이터길이
        }
        #endregion

        #region Tail
        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public class TAIL
        {
            public byte Checksum;           //에러체크(Head~Tail XOR)
            public byte ETX;                //종료부호(0x03)
        }
        #endregion

        #region DATE
        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public class DATE
        {

            public short yyyy;
            public byte MM;
            public byte dd;
        }
        #endregion

        #region TIME
        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public class TIME
        {
            public byte hh;
            public byte mm;
            public byte ss;
        }
        #endregion

        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        #region Info
        public class Info
        {
            public TIME FrameSend = new TIME(); // 프레임 전송 시점 제공 시간

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] bus_info_count = new byte[2];        // 운행 정보 전체 개수
        }
        #endregion

        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        #region Send
        public class Send
        {
            public TIME sendTime = new TIME();
            public int node_info; // 노선보 관리 번호
            public int stop_info; // 지점 정보 관리 번호
            public int node_stop_info; // 노선 경유 정보 관리 번호
            public int message_ver;     // 홍보 메시지
            public int image_ver;   // 홍보 이미지
            public int video_ver;   // 동영상 버전

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] version;  // 프로그램버전 20 byte
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Reserved; // 4 

        }
        #endregion

        #region Recieve
        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public class Receive
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] nodeNum = new byte[2];               // 000-0에서 - 앞자리 노선 번호
            public byte nodeBehind;            // 000-0에서 - 뒷자리 노선 번호
            public byte nodePart;               // 노선 구분..0x01 : 시점출발 - > 종점도착, 0x02 : 종점출발 -> 시점도착, 0x03 : 시점출발->회차지경유->종점도착
            public byte nodeStat;               // 노선 형태 0:일반, 1~8:지원, 9:비상 

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] stop_id = new byte[2];               // 정류소ID
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] rest_stop = new byte[2];              // 남은 정류소 수
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] expected_arrival = new byte[2];      // 도착 예정시간, 
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] bus_stat = new byte[2];              // 버스 유형
            public byte info_stat;              // 정보 유형 0x00 일반버스, 0x01 지상버스, 0x02 일반버스지체, 0x03 저상 버스지체.0x04 일반버스부분지체 0x05저상버스부분지체
                                                // 0x10 버스진입상태(삭제요청), 0x20 기점출발정보
            public byte emer_stat;              // 비상 구분 0정상 1: 비상
            public byte bus_sort;               // 버스 구분
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] vehicle_id = new byte[2];            // 버스ID
        }
        #endregion

    }
}
