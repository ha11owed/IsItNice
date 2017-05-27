using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ClientLogic.WinRT
{
    /// <summary>
    /// Container for a storage file.
    /// Allow most of the needed operations with image files.
    /// </summary>
    public class FileContainer
    {
        private StorageFile storageFile;

        public FileContainer(StorageFile storageFile)
        {
            this.storageFile = storageFile;
        }

        public static async Task<FileContainer> Load(string fileName)
        {
            StorageFile sf = null;
            FileContainer fc = null;
            try
            {
                sf = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            }
            catch (Exception e)
            {
                sf = null;
                Debug.WriteLine("Failed to load file: " + fileName + " ex: " + e);
            }
            if (sf != null)
            {
                fc = new FileContainer(sf);
            }
            return fc;
        }

        public static async Task<FileContainer> Save(string base64StringData, string fileName)
        {
            var data = Convert.FromBase64String(base64StringData);
            var sf = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
            await FileIO.WriteBytesAsync(sf, data);
            return new FileContainer(sf);
        }

        public static async Task<FileContainer> Save(byte[] data, string fileName)
        {
            var sf = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName);
            await FileIO.WriteBytesAsync(sf, data);
            return new FileContainer(sf);
        }

        public async Task Rename(string newFileName)
        {
            if (null != storageFile)
            {
                if (newFileName != storageFile.Name && !storageFile.Path.StartsWith(ApplicationData.Current.LocalFolder.Path))
                {
                    await storageFile.MoveAsync(ApplicationData.Current.LocalFolder, newFileName, NameCollisionOption.ReplaceExisting);
                }
            }
        }

        public async Task<byte[]> GetByteArray()
        {
            byte[] data = null;
            if (storageFile != null)
            {
                var stream = await storageFile.OpenReadAsync();

                var reader = new Windows.Storage.Streams.DataReader(stream);
                await reader.LoadAsync((uint)stream.Size);

                data = new byte[stream.Size];
                reader.ReadBytes(data);

                reader.Dispose();
                stream.Dispose();
            }
            return data;
        }

        public async Task<string> GetBase64String()
        {
            var data = await GetByteArray();
            string base64 = Convert.ToBase64String(data);
            return base64;
        }

        public string URI
        {
            get
            {
                if (storageFile == null) { return null; }
                else
                {
                    if (storageFile.Path.StartsWith(ApplicationData.Current.LocalFolder.Path))
                    {
                        return "ms-appdata:///local/" + storageFile.Name;
                    }
                    else
                    {
                        return "file://" + storageFile.Path;
                    }
                }
            }
        }
    }
}
