using System;
using System.Device.Gpio;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {            
            //string serverIp = "10.141.131.82";  // 서버 IP 주소 (1호기)
            string serverIp = "10.141.132.112";  // 서버 IP 주소 (2호기)
            //string serverIp = "10.141.131.53";  // 서버 IP 주소 (3호기)

            int port = 8000;                    // 서버 포트 번호
            int retryInterval = 5000;           // 연결 재시도 간격 (5초)
            int sendInterval = 1000;            // 메시지 전송 간격 (1초)
            bool isConnected = false;

            // 1호기 ////////////////////////////////////////////////////
            //string strEquipmentName = "K2022-1101253";  // DUFCUD1
            //string strEquipmentName = "K2023-1100445";  // DUFCUD2
            //string strEquipmentName = "K2023-1100333";  // DUFCUD3
            //string strEquipmentName = "K2023-1100334";  // DUFCUD4
            //string strEquipmentName = "K2023-1100292";  // DUFCUD5
            /////////////////////////////////////////////////////////////
            ///
            // 2호기 ////////////////////////////////////////////////////
            string strEquipmentName = "K2020-1101206";  // DFCUD3
            //string strEquipmentName = "K2022-1100519";  // DFCUD4
            //string strEquipmentName = "K2022-1100535";  // DFCUD5            
            /////////////////////////////////////////////////////////////
            ///
            // 3호기 ////////////////////////////////////////////////////
            //string strEquipmentName = "K2021-1100673";  // DFCUD1
            //string strEquipmentName = "K2021-1100373";  // DFCUD2            
            /////////////////////////////////////////////////////////////

            // 통신 활성 여부
            string sConn = "0";

            // GPIO 핀 번호 설정
            int pinRed = 17;
            int pinYellow = 27;
            int pinGreen = 22;

            // GPIO 컨트롤러 초기화
            using (GpioController controller = new GpioController())
            {
                // GPIO 핀을 입력 모드로 설정
                controller.OpenPin(pinRed, PinMode.Input);
                controller.OpenPin(pinYellow, PinMode.Input);
                controller.OpenPin(pinGreen, PinMode.Input);                

                // 서버와 연결 시도
                while (!isConnected)
                {
                    try
                    {
                        using (TcpClient client = new TcpClient(serverIp, port))
                        {
                            Console.WriteLine("서버에 연결 성공!");
                            // 네트워크 스트림 가져오기
                            NetworkStream stream = client.GetStream();
                            // 서버와의 연결 유지 및 데이터 전송 loop
                            while (true)
                            {
                                // 통신 여부                                
                                if (sConn == "0")
                                    sConn = "1";
                                else
                                    sConn = "0";

                                // GPIO 핀에서 값 읽기                                
                                bool pinRedState = controller.Read(pinRed) == PinValue.High;
                                bool pinYellowState = controller.Read(pinYellow) == PinValue.High;
                                bool pinGreenState = controller.Read(pinGreen) == PinValue.High;                                
                                // 입력 상태를 서버로 전송할 문자열로 변환
                                string message = $"{strEquipmentName},{pinRedState},{pinYellowState},{pinGreenState},{sConn}";
                                // Unicode 인코딩하여 서버로 전송
                                byte[] data = Encoding.Unicode.GetBytes(message);
                                stream.Write(data, 0, data.Length);
                                Console.WriteLine("서버에 메시지 전송 : " + message);
                                // 1초 대기
                                Thread.Sleep(sendInterval);
                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        // 서버와 연결 실패 시 예외 처리
                        Console.WriteLine($"서버와 연결 실패 : {ex.Message}");
                        Console.WriteLine($"5초 후에 다시 시도합니다...");
                        Thread.Sleep(retryInterval);    // 5초 후 재시도
                    }
                    catch (Exception ex)
                    {
                        // 기타 예외 처리
                        Console.WriteLine($"오류 발생 : {ex.Message}");
                        Thread.Sleep(retryInterval);    // 5초 후 재시도
                    }
                }
            }
        }
    }
}