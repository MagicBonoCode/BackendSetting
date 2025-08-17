using LitJson;
using System;
using UnityEngine;

namespace BackEnd
{
    /// <summary>
    /// �ڳ��� ��ϵ� ��Ʈ(������ ��Ʈ)
    /// </summary>
    /// <typeparam name="T">������ Ÿ��</typeparam>
    public abstract class BaseChart<T>
    {
        /// <summary>��Ʈ �������� ���� �ĺ���</summary>
        public abstract string GetChartFileName();

        protected abstract void InitializeData(JsonData jsonData);

        public void LoadChartData(Action<bool> action = null)
        {
            string chartFileName = GetChartFileName();
            string chartId = BackendBaseManager.Instance.Chart.AllChartIDDict[chartFileName];

            // ��Ʈ ������ ��û
            SendQueue.Enqueue(Backend.Chart.GetChartContents, chartId, bro =>
            {
                try
                {
                    Debug.Log($"Backend.Chart.GetChartContents({chartId}) : {bro}");

                    if(!bro.IsSuccess())
                    {
                        throw new Exception(bro.ToString());
                    }

                    JsonData jsonData = bro.FlattenRows();
                    InitializeData(jsonData);
                }
                catch(Exception e)
                {
                    string className = GetType().Name;
                    string errorInfo = e.Message;

                    Debug.LogError($"[{className}] ��Ʈ ������ �ҷ����� ���� : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(bro.IsSuccess());
                }
            });
        }
    }
}
