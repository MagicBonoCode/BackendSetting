using LitJson;
using System;
using UnityEngine;

namespace BackEnd
{
    /// <summary>
    /// 뒤끝에 등록된 차트(데이터 시트)
    /// </summary>
    /// <typeparam name="T">데이터 타입</typeparam>
    public abstract class BaseChart<T>
    {
        /// <summary>차트 데이터의 고유 식별자</summary>
        public abstract string GetChartFileName();

        protected abstract void InitializeData(JsonData jsonData);

        public void LoadChartData(Action<bool> action = null)
        {
            string chartFileName = GetChartFileName();
            string chartId = BackendBaseManager.Instance.Chart.AllChartIDDict[chartFileName];

            // 차트 데이터 요청
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

                    Debug.LogError($"[{className}] 차트 데이터 불러오기 실패 : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(bro.IsSuccess());
                }
            });
        }
    }
}
