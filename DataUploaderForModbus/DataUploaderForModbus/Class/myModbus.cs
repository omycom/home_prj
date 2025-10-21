using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Device;
using System.IO.Ports;
using System.Net.Sockets;

namespace LIB_Common
{
    public class myModbus_TCP : myModbus
    {
        private string _ip;
        private int _port;

        private TcpClient _tcp;

        public myModbus_TCP(string ip, int port = 502, int slaveNo = 1)
            : base(slaveNo)
        {
            _ip = ip;
            _port = port;
        }

        public override void Close()
        {
            if (_tcp != null)
                _tcp.Dispose();
        }

        public override void Open()
        {
            Close();

            _tcp = new TcpClient(_ip, _port);
        }

        protected override ModbusMaster GetMaster()
        {
            return ModbusIpMaster.CreateIp(_tcp);
        }
    }

    public class myModbus_RTU : myModbus
    {
        private string _comport;
        private SerialPort _serial_port;


        public myModbus_RTU(string comport, int slaveNo = 1)
            : base(slaveNo)
        {
            _comport = comport;

            SetSerialPort();
        }

        private void SetSerialPort()
        {
            try
            {
                _serial_port = new SerialPort(_comport);
                _serial_port.BaudRate = 9600;
                _serial_port.DataBits = 8;
                _serial_port.Parity = Parity.None;
                _serial_port.StopBits = StopBits.One;
            }
            catch
            {
                throw new Exception($@"기기에 접속 할 수 없습니다.");
            }
        }

        protected override ModbusMaster GetMaster()
        {
            return ModbusSerialMaster.CreateRtu(_serial_port);
        }

        public override void Open()
        {
            Close();

            _serial_port.Open();
        }

        public override void Close()
        {
            if (_serial_port != null)
                _serial_port.Dispose();
        }
    }

    public abstract class myModbus : myPLCBase
    {
        private int _slaveNo;

        public myModbus(int slaveNo)
        {
            _slaveNo = slaveNo;
        }

        protected abstract ModbusMaster GetMaster();

        public ushort[] ReadHoldingRegisters(int startAddress, int count)
        {
            ModbusMaster master = GetMaster();
            return master.ReadHoldingRegisters((byte)_slaveNo, (ushort)startAddress, (ushort)count);
        }

        public bool[] ReadCoils(int startAddress, int count)
        {
            ModbusMaster master = GetMaster();
            return master.ReadCoils((byte)_slaveNo, (ushort)startAddress, (ushort)count);
        }

        public override void Write(string plcAddress, string value16, bool isBitType = false)
        {
            ModbusMaster master = GetMaster();

            if (isBitType)
                master.WriteSingleCoil((byte)_slaveNo, Convert.ToUInt16(plcAddress), value16 == "1");
            else
                master.WriteSingleRegister((byte)_slaveNo, Convert.ToUInt16(plcAddress), (ushort)Convert.ToDecimal(value16));

        }
    }


}
