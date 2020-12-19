using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace NodeRegister
{
    class Program
    {
        static void Main(string[] args)
        {
            // 0.删除旧版本
            string tag;
            string tagValue;

            tag = "JAVA_HOME";
            tagValue = SysEnvironment.GetSysEnvironmentByName(tag);

            // The local machine. delete
            Environment.SetEnvironmentVariable(tag, null,
                                               EnvironmentVariableTarget.Machine);

            SysEnvironment.SetPathDelete(@"%JAVA_HOME%\bin");
            SysEnvironment.SetPathDelete(@"%JAVA_HOME%\jre\bin");

            SysEnvironment.SetPathDelete(tagValue + @"\bin");
            SysEnvironment.SetPathDelete(tagValue + @"\jre\bin");


            
            tag = "CLASSPATH";
            tagValue = SysEnvironment.GetSysEnvironmentByName(tag);

            // The local machine. delete
            Environment.SetEnvironmentVariable(tag, null,
                                               EnvironmentVariableTarget.Machine);


            // 刷新设定
            // System.Console.WriteLine("updating...");
            // RefreshEnvironment();

            //-----------------------------------------------------------------------------------------
            string temp = "";

            // 1.新建两个目录
            // 无

            // 2.设环境变量
            temp = System.Environment.CurrentDirectory;
            SysEnvironment.SetSysEnvironment("JAVA_HOME", temp);

            temp = @".;%JAVA_HOME%\lib;%JAVA_HOME%\lib\tools.jar";
            temp = temp.Replace("%JAVA_HOME%", System.Environment.CurrentDirectory);
            SysEnvironment.SetSysEnvironment("CLASSPATH", temp);

            // 3.设定PATH
            temp = @"%JAVA_HOME%\bin";
            temp = temp.Replace("%JAVA_HOME%", System.Environment.CurrentDirectory);
            SysEnvironment.SetPathAfter(temp);

            temp = @"%JAVA_HOME%\jre\bin";
            temp = temp.Replace("%JAVA_HOME%", System.Environment.CurrentDirectory);
            SysEnvironment.SetPathAfter(temp);

            // 4.刷新设定
            System.Console.WriteLine("Please wait...");
            RefreshEnvironment();

            // 5.执行其他设置(shell，配置文件等)
            // 无

            // 6.成功
            System.Console.WriteLine("");
            System.Console.WriteLine("Mission complete!");

            Console.ReadKey();

        }

        public static void writeSetting(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            // ----------------------------------------------------------------------------------------------
            sw.WriteLine("npm config set cache \""+ System.Environment.CurrentDirectory + "\\node_cache\"");
            sw.WriteLine("REM npm install -g cnpm --registry=https://registry.npm.taobao.org");
            sw.WriteLine("pause");
            // ----------------------------------------------------------------------------------------------
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }

        private static void callBat(string batName)
        {
            Process proc = null;
            try
            {
                string targetDir = string.Format(System.Environment.CurrentDirectory);//这是bat存放的目录
                proc = new Process();
                proc.StartInfo.WorkingDirectory = targetDir;
                proc.StartInfo.FileName = batName;//bat文件名称
                // proc.StartInfo.Arguments = string.Format("10");//this is argument
                // proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//这里设置DOS窗口不显示，经实践可行
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }
        }


        public static void RefreshEnvironment()
        {
            // The local machine. insert
            Environment.SetEnvironmentVariable("GAOXZ", "DAHAOREN!",
                                               EnvironmentVariableTarget.Machine);
            // The local machine. delete
            Environment.SetEnvironmentVariable("GAOXZ", null,
                                               EnvironmentVariableTarget.Machine);
        }

    }

    class SysEnvironment
    {
        /// <summary>
        /// 获取系统环境变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSysEnvironmentByName(string name)
        {
            string result = string.Empty;
            try
            {
                result = OpenSysEnvironment().GetValue(name).ToString();//读取
            }
            catch (Exception)
            {

                return string.Empty;
            }
            return result;

        }

        /// <summary>
        /// 打开系统环境变量注册表
        /// </summary>
        /// <returns>RegistryKey</returns>
        private static RegistryKey OpenSysEnvironment()
        {
            RegistryKey regLocalMachine = Registry.LocalMachine;
            RegistryKey regSYSTEM = regLocalMachine.OpenSubKey("SYSTEM", true);//打开HKEY_LOCAL_MACHINE下的SYSTEM 
            RegistryKey regControlSet001 = regSYSTEM.OpenSubKey("ControlSet001", true);//打开ControlSet001 
            RegistryKey regControl = regControlSet001.OpenSubKey("Control", true);//打开Control 
            RegistryKey regManager = regControl.OpenSubKey("Session Manager", true);//打开Control 

            RegistryKey regEnvironment = regManager.OpenSubKey("Environment", true);
            return regEnvironment;
        }

        /// <summary>
        /// 设置系统环境变量
        /// </summary>
        /// <param name="name">变量名</param>
        /// <param name="strValue">值</param>
        public static void SetSysEnvironment(string name, string strValue)
        {
            // System.Console.Write(name + "←" + strValue);
            OpenSysEnvironment().SetValue(name, strValue);

        }

        /// <summary>
        /// 检测系统环境变量是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckSysEnvironmentExist(string name)
        {
            if (!string.IsNullOrEmpty(GetSysEnvironmentByName(name)))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 添加到PATH环境变量（会检测路径是否存在，存在就不重复）
        /// </summary>
        /// <param name="strPath"></param>
        public static void SetPathAfter(string strHome)
        {
            // System.Console.Write("Path" + "←" + strHome);

            string pathlist;
            pathlist = GetSysEnvironmentByName("PATH");
            //检测是否以;结尾
            if (pathlist.Substring(pathlist.Length - 1, 1) != ";")
            {
                SetSysEnvironment("PATH", pathlist + ";");
                pathlist = GetSysEnvironmentByName("PATH");
            }
            string[] list = pathlist.Split(';');
            bool isPathExist = false;

            foreach (string item in list)
            {
                if (item == strHome)
                    isPathExist = true;
            }
            if (!isPathExist)
            {
                SetSysEnvironment("PATH", pathlist + strHome + ";");
            }

        }

        public static void SetPathBefore(string strHome)
        {
            string pathlist;
            pathlist = GetSysEnvironmentByName("PATH");
            string[] list = pathlist.Split(';');
            bool isPathExist = false;

            foreach (string item in list)
            {
                if (item == strHome)
                    isPathExist = true;
            }
            if (!isPathExist)
            {
                SetSysEnvironment("PATH", strHome + ";" + pathlist);
            }

        }

        public static void SetPathDelete(string strHome)
        {
            string pathlist;
            pathlist = GetSysEnvironmentByName("PATH");
            string pathlist_update = "";
            string[] list = pathlist.Split(';');
            bool isPathExist = false;

            foreach (string item in list)
            {
                if (item == strHome)
                {
                    System.Console.WriteLine("DELETE <path>:" + strHome);
                    isPathExist = true;
                }else {
                    pathlist_update = pathlist_update + ";" + item;
                }
            }

            if (isPathExist == true)
            {
                pathlist_update = pathlist_update.Substring(1);
                SetSysEnvironment("PATH", pathlist_update);
            }
            else {
                // System.Console.WriteLine("no [" + strHome + "] exists.");
            }

        }

        public static void SetPath(string strHome)
        {
            string pathlist;
            pathlist = GetSysEnvironmentByName("PATH");
            string[] list = pathlist.Split(';');
            bool isPathExist = false;

            foreach (string item in list)
            {
                if (item == strHome)
                    isPathExist = true;
            }
            if (!isPathExist)
            {
                SetSysEnvironment("PATH", pathlist + strHome + ";");

            }

        }
    }

    class SetSysEnvironmentVariable
    {
        [DllImport("Kernel32.DLL ", SetLastError = true)]
        public static extern bool SetEnvironmentVariable(string lpName, string lpValue);

        public static void SetPath(string pathValue)
        {
            string pathlist;
            pathlist = SysEnvironment.GetSysEnvironmentByName("PATH");
            string[] list = pathlist.Split(';');
            bool isPathExist = false;

            foreach (string item in list)
            {
                if (item == pathValue)
                    isPathExist = true;
            }
            if (!isPathExist)
            {
                SetEnvironmentVariable("PATH", pathlist + pathValue + ";");

            }

        }
    }
}
