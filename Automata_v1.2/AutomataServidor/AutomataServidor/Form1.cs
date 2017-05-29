using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;


namespace AutomataServidor
{
    public partial class Form1 : Form
    {
        int[] contarN = new int[2];//numero de clientes que admite
        int[] contarS = new int[2];
        string[] clientes = new string[2];
        int cont = 0;

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>();
        byte[] buffer = new byte[2048];

        public Form1()
        {
            InitializeComponent();
        }       
        
        public void AceptarPeticion(IAsyncResult estadoAsincrono)
        {
            CheckForIllegalCrossThreadCalls = false;//Desactiva la comprobacion de acceso de varios subprocesos
            Socket cliente;

            try
            {
                cliente = serverSocket.EndAccept(estadoAsincrono);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            clientSockets.Add(cliente);
            cliente.BeginReceive(buffer, 0, 2048, 0, ProcesarMensaje, cliente);
            listBox1.Items.Add("Se ha conectado un cliente");

            serverSocket.BeginAccept(AceptarPeticion, null);
        }

        public void ProcesarMensaje(IAsyncResult AR)
        {

            GenerarCadenas(); 
            string cadenas = "";
            using (StreamReader leerArchivo = new StreamReader("cadenas.txt"))
            {
                string line;
                while ((line = leerArchivo.ReadLine()) != null)
                {
                    cadenas += line;
                }
            }

            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                listBox1.Items.Add("Se ha desconectado el cliente");
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] mensaje = new byte[received];
            Array.Copy(buffer, mensaje, received);
            string host = Encoding.ASCII.GetString(mensaje);
            string[] host1 = host.Split(',');
            
            listBox1.Items.Add("El cliente es : " + host1[0]);

            if(host.Contains(",") == true )
            {
                //no se porque marca error
                //pero que lo que se tiene que hacer es que agrege 
                //el nombre de los clientes a un array para utlizarlos en la grafica
                //clientes[cont] = host1[0].ToString();
                contarS[cont] = Int32.Parse(host1[1]);
                contarN[cont] = Int32.Parse(host1[2]);
                cont++;
            }
            
            if (host != "")
            {
                listBox1.Items.Add("Enviando cadenas al cliente: " + host1[0]);
                byte[] cadena = Encoding.ASCII.GetBytes(cadenas);
                current.Send(cadena);
            }

            current.BeginReceive(buffer, 0, 2048, 0, ProcesarMensaje, current);
        }

        public void GenerarCadenas()
        {
            StreamWriter escribirCadena = new StreamWriter("cadenas.txt");
            Random cantidadCadenas = new Random();
            Random elemnetosCadena = new Random();
            Random tamanoCadena = new Random();

            int cantidad = cantidadCadenas.Next(2, 100);

            for (int i = 0; i < cantidad; i++)
            {
                int tamano = tamanoCadena.Next(2, 20);
                for (int j = 0; j < tamano; j++)
                {
                    escribirCadena.Write(elemnetosCadena.Next(2));
                }
                escribirCadena.WriteLine("&");
            }
            escribirCadena.Close();
        }        

        private void button1_Click_1(object sender, EventArgs e)
        {
            listBox1.Items.Add("Servidor Corriendo");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(textBox1.Text)));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(AceptarPeticion, null);
        }

        public void Graficar()
        {
            String[] datos = { "Validos", "Invalidos" };

            chart1.Titles.Add("Resultado de cadenas evaluadas por clientes");
            
            Series serie = chart1.Series.Add("Validos");
            Series serie1 = chart1.Series.Add("inValidos");
            for (int i = 0; i < contarS.Length; i++)
            {
                //en serie.label tendria que indicar el numero que correspodiente
                //pero en todas utliza el ultimo elemento que entra
                //se tiene que areglar
                serie.Label = contarS[i].ToString();
                serie1.Label = contarN[i].ToString();                
                //chart1.Series["Validos"].Points[i+1].AxisLabel = "First";
                serie.Points.Add(contarS[i]);
                serie1.Points.Add(contarN[i]);
            }

            //aun no se como poner el nombre de los clientes en la grafica
            serie.Points[1].AxisLabel = "yo";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Graficar();
        }
    }
}