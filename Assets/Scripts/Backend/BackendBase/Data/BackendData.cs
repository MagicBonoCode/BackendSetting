using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BackEnd
{
    /// <summary>뒤끝 콘솔에 업로드한 차트(데이터 시트) 데이터</summary>
    public class BackendChart
    {
        /// <summary>전체 차트의 파일 ID를 관리하는 Dictionary</summary>
        private readonly Dictionary<string, string> _allChartIDDict = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> AllChartIDDict => _allChartIDDict;

        public void LoadAllChartID(Action<bool> action = null)
        {
            // 차트 리스트 불러오기
            SendQueue.Enqueue(Backend.Chart.GetChartList, bro =>
            {
                try
                {
                    Debug.Log($"Backend.Chart.GetChartList : {bro}");

                    if(!bro.IsSuccess())
                    {
                        throw new Exception(bro.ToString());
                    }

                    JsonData jsonData = bro.FlattenRows();
                    for(int i = 0; i < jsonData.Count; i++)
                    {
                        string chartName = jsonData[i]["chartName"].ToString();
                        string selectedChartFileId = jsonData[i]["selectedChartFileId"].ToString();

                        if(_allChartIDDict.ContainsKey(chartName))
                        {
                            Debug.LogWarning($"동일한 차트 키 값이 존재합니다 : {chartName} - {selectedChartFileId}");
                        }
                        else
                        {
                            _allChartIDDict.Add(chartName, selectedChartFileId);
                        }
                    }
                }
                catch(Exception e)
                {
                    string className = GetType().Name;
                    string errorInfo = e.Message;

                    Debug.LogError($"[{className}] 차트 ID 불러오기 실패 : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(bro.IsSuccess());
                }
            });
        }
    }

    /// <summary>뒤끝 콘솔에서 관리하는 게임 데이터</summary>
    public class BackendGameData
    {
        private readonly Dictionary<string, BaseGameData> _allGameDataDict = new Dictionary<string, BaseGameData>();
        public IReadOnlyDictionary<string, BaseGameData> AllGameDataDict => _allGameDataDict;

        public BackendGameData()
        {
            // TODO : 게임 데이터 추가
        }
    }

    public partial class BackendBaseManager : MonoBehaviour
    { 
        public BackendChart Chart { get; private set; } = new BackendChart();
        public BackendGameData GameData { get; private set; } = new BackendGameData();

        private const float DATA_UPDATE_TICK = 60.0f;

        private IEnumerator CoUpdateGameDataTransaction()
        {
            while(!_isErrorOccured)
            {
                UpdateAllGameData();

                yield return new WaitForSeconds(DATA_UPDATE_TICK);
            }
        }

        private void UpdateAllGameData()
        {
            string info = string.Empty;

            // 업데이트할 데이터가 있는지 확인
            List<BaseGameData> changedGameDataList = new List<BaseGameData>();
            foreach (BaseGameData gameData in GameData.AllGameDataDict.Values)
            {
                if (gameData.IsChangedBackendGameData)
                {
                    info += gameData.GetTableName() + "\n";
                    changedGameDataList.Add(gameData);
                }
            }

            // 업데이트할 목록이 존재하지 않음
            if (changedGameDataList.Count == 0)
            {
                return;
            }

            // 업데이트 목록이 하나라면 해당 테이블만 업데이트
            if (changedGameDataList.Count == 1)
            {
                foreach (BaseGameData gameData in changedGameDataList)
                {
                    gameData.UpdateBackendGameData(bro =>
                    {
                        if(bro.IsSuccess())
                        {
                            // 업데이트가 성공적으로 완료되면 변경된 데이터 플래그를 초기화
                            gameData.ResetIsChangedBackendGameData();

                            Debug.Log($"게임 데이터 업데이트 성공 : {gameData.GetTableName()}\n업데이트 테이블 : {gameData.GetTableName()}");
                        }
                        else
                        {
                            SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), bro.ToString() + "\n" + info);

                            Debug.LogError($"게임 데이터 업데이트 실패 : {bro}\n업데이트 테이블 : {gameData.GetTableName()}");
                        }
                    });
                }
            }
            // 2개 이상이라면 트랜잭션에 묶어서 업데이트
            // NOTE: 10개 이상이면 트랜잭션 실패할 수 있습니다.
            else
            {
                List<TransactionValue> transactionList = new List<TransactionValue>();

                // 변경된 데이터만큼 트랜잭션 추가
                foreach (BaseGameData gameData in changedGameDataList)
                {
                    transactionList.Add(gameData.GetTransactionUpdateValue());
                }

                SendQueue.Enqueue(Backend.GameData.TransactionWriteV2, transactionList, bro =>
                {
                    Debug.Log($"Backend.BMember.TransactionWriteV2 : {bro}");

                    if (bro.IsSuccess())
                    {
                        foreach (BaseGameData gameData in changedGameDataList)
                        {
                            gameData.ResetIsChangedBackendGameData();
                        }

                        Debug.Log($"게임 데이터 트랜잭션 업데이트 성공 : {info}");
                    }
                    else
                    {
                        SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), bro.ToString() + "\n" + info);

                        Debug.LogError($"게임 데이터 트랜잭션 업데이트 실패 : {bro}\n업데이트 테이블 : {info}");
                    }
                });
            }
        }
    }
}
