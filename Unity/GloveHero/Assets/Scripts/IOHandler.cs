using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class IOHandler : MonoBehaviour
{
    SerialPort data_stream; //arduino is connected to the port COM selsected by user, with a baud rate of 115200
    string receivedstring;
    public string portString;
    Thread readingThread;
    Thread writingThread;


    //flags to see if the thread is doing something
    private bool readFlag = false;
    private bool writeFlag = false;

    [SerializeField] bool isMainMenu = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!isMainMenu)
        {
            SerialReading();
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
        //StartCoroutine(SerialDataReading());
        //portString = receivedstring;
        //data_stream.BaseStream.Flush();
        // string[] data = receivedstring.Split(',');
    }

    private void OnDestroy()
    {
        data_stream.Close();

        if (readFlag)
        {
            readingThread.Abort();
            readFlag = false;
        }

        KillWriting();
        
    }

    public void SerialReading()
    {
        data_stream = new SerialPort("COM" + PlayerPrefs.GetString("COM"), 19200);

        data_stream.Open();

        readingThread = new Thread(new ThreadStart(GetArduino));
        readingThread.Start();

       // readingThread.Join();
    }

    public void SerialWriting(object msg)
    {
        writingThread = new Thread(WriteToArduino);
        writingThread.Start(msg);
    }

    private void WriteToArduino(object obj)
    {
        while (writingThread.IsAlive)
        {
            writeFlag = false;
            data_stream.Write(obj.ToString());
            writeFlag = true;
        }
    }

    public void KillWriting()
    {
        data_stream.BaseStream.Flush();
        if (writeFlag)
        {
            writingThread.Abort();
            writeFlag = false;
        }
    }

    private void GetArduino()
    {
        while (readingThread.IsAlive)
        {
            readFlag = false;
            receivedstring = data_stream.ReadLine();
            portString = receivedstring;
            //Debug.Log(portString);
            readFlag = true;
        }
    }


}
