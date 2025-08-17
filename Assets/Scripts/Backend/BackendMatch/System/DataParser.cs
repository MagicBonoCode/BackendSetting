using System.Text;
using UnityEngine;

namespace BackEnd
{
    public class DataParser : MonoBehaviour
    {
        #region Json

        public static T ReadJsonData<T>(byte[] buf)
        {
            var strByte = Encoding.Default.GetString(buf);
            //byte 배열을 string으로 변환
            return JsonUtility.FromJson<T>(strByte);
        }

        public static byte[] DataToJsonData<T>(T obj)
        {
            var jsonData = JsonUtility.ToJson(obj);
            //string을 byte 배열로 변환
            return Encoding.UTF8.GetBytes(jsonData);
        }

        #endregion
    }
}
