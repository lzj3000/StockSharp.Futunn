using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace StockSharp.Futunn.component
{
    public class FutunnResources
    {
        public void ReleaseFTAPIChannel()
        {
            string filename = "FTAPIChannel.dll";
            string path = Environment.CurrentDirectory + "/" + filename;
            if (!File.Exists(path))
            {
                string fullname = "";
                if (Environment.Is64BitOperatingSystem)
                    fullname = "lib/x64/" + filename;
                else
                    fullname = "lib/x32/" + filename;
                using (Stream stream = GetResourceStream(fullname))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        stream.CopyTo(fs);
                        fs.Flush();
                    }
                }
            }
        }
        public void LoadFTAPI()
        {
            var type = Type.GetType("Futu.OpenApi.FTAPI");
            if (type == null) {
                using (Stream stream = GetResourceStream("FTAPI4Net.dll"))
                {
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    Assembly.Load(bytes);
                }
            }
        }
        private Stream GetResourceStream(string name)
        {
            var asmHolder = Assembly.GetExecutingAssembly();
         
            return asmHolder.GetManifestResourceStream($"{asmHolder.GetName().Name}.{Path.GetFileName(name)}");
        }
    }
}
