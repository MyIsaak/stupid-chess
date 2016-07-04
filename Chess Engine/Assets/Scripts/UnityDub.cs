using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

/*
 *  
 *  Simple Game Recording Script
 *  Made by @sfiq12
 * 
 *  The script has to be attached to the Camera you wish to record, otherwise it will not work
 *  
 *  The VirtualDub file location has to be specified
 *  The Start and End Recording buttons are customisable
 *  You can set the FPS of which the game will record
 *  FPS has to be converted to the specified FPS manually in VirtualDub
 */

public class UnityDub : MonoBehaviour
{

    private Texture2D texture;
    private byte[] buffer;

    public string virtualdubLocation;

    public string startRecording = "f1";
    public string stopRecording = "f2";
    private bool recording = false;

    private string location;
    private string prefix = "frame_";
    private List<string> frames;

    private int frameCount = 0;

    public int FPS = 30;

    // Use this for initialization
    void Start()
    {

        texture = new Texture2D(Screen.width, Screen.height);
        location = Application.dataPath + "/tmp/";

        if(!Directory.Exists(location))
        {
            Directory.CreateDirectory(location);
        }

        frames = new List<string>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(startRecording))
        {
            Time.captureFramerate = FPS;
            recording = true;
        }

        if(Input.GetKeyDown(stopRecording))
        {
            Time.captureFramerate = 0;
            recording = false;

        }

        if(!recording)
        {
            if (frames.Count > 0)
            {
                Process vdub = new Process();
                ProcessStartInfo attr = new ProcessStartInfo();
                attr.FileName = virtualdubLocation + "virtualdub.exe";
                attr.Arguments = "\"" + frames[0] + "\"";
                attr.RedirectStandardOutput = true;
                attr.UseShellExecute = false;
                vdub.StartInfo = attr;

                vdub.Start();
                string output = vdub.StandardOutput.ReadToEnd();
                vdub.WaitForExit();

                print(output);

                for (int i = 0; i < frames.Count; i++)
                {
                    File.Delete(frames[i]);
                }

                frames.Clear();
            }
        }
    }

    void OnPostRender()
    {
        if (recording)
        {
            string ss = location + prefix + frameCount.ToString("00000") + ".jpg";
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            buffer = texture.EncodeToJPG();

            File.WriteAllBytes(ss, buffer);

            frames.Add(ss);

            frameCount++;
        }
    }

    void OnApplicationQuit()
    {
        if(Directory.Exists(location))
        {
            Directory.Delete(location);
        }
    }
}
