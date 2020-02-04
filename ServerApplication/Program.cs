using EasyModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApplication
{

    class Program
    {
        private ModbusServer modbusServer;

        static void Main(string[] args)
        {
            Program application = new Program();
            application.startServer();
        }

        public void startServer()
        {
            modbusServer = new ModbusServer();
            modbusServer.UnitIdentifier = 255;

            modbusServer.holdingRegisters[1012] = 1100;
            modbusServer.holdingRegisters[1022] = 1200;
            modbusServer.holdingRegisters[1032] = 1300;
            modbusServer.holdingRegisters[1042] = 1400;
            modbusServer.holdingRegisters[1052] = 1500;

            modbusServer.HoldingRegistersChanged += holdingRegistersChanged;
            modbusServer.NumberOfConnectedClientsChanged += ModbusServer_NumberOfConnectedClientsChanged;

            modbusServer.Listen();

            Console.WriteLine("ModbusTCP server started");

            while (true)
            {
                try
                {
                    var read = Console.ReadLine().Trim();
                    if (read.Contains(" "))
                    {
                        var addressAndValue = read.Split(' ');
                        var address = int.Parse(addressAndValue[0]);
                        var value = short.Parse(addressAndValue[1]);
                        modbusServer.holdingRegisters[address] = value;
                    }
                    else if (read == "q4" || read == "p4")
                    {
                        for (int i = 0; i < modbusServer.holdingRegisters.localArray.Length; i++)
                            if (modbusServer.holdingRegisters[i] != 0)
                                PrintHoldingRegister(i);
                    }
                    else if (read == "exit" || read == "quit")
                        break;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
            modbusServer.StopListening();
        }

        private int previousNumberOfConnections = -1;
        private void ModbusServer_NumberOfConnectedClientsChanged()
        {
            if (modbusServer.NumberOfConnections != previousNumberOfConnections)
            {
                previousNumberOfConnections = modbusServer.NumberOfConnections;
                Console.WriteLine($"Connected clients: {modbusServer.NumberOfConnections}");
            }
        }

        public void holdingRegistersChanged(int startingAddress, int quantity)
        {
            var endAddress = startingAddress + quantity - 1;
            if (quantity == 1)
                Console.WriteLine($"Changed {startingAddress}");
            else
                Console.WriteLine($"Changed {startingAddress} to {endAddress}");

            for(int i = startingAddress; i <= endAddress; i++)
                PrintHoldingRegister(i);
        }

        private void PrintHoldingRegister(int i) => Console.WriteLine($"    {400_000 + i} = {modbusServer.holdingRegisters[i]}");
    }
}
