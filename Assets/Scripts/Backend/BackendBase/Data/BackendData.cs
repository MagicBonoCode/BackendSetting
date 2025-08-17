using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BackEnd
{
    /// <summary>�ڳ� �ֿܼ� ���ε��� ��Ʈ(������ ��Ʈ) ������</summary>
    public class BackendChart
    {
        /// <summary>��ü ��Ʈ�� ���� ID�� �����ϴ� Dictionary</summary>
        private readonly Dictionary<string, string> _allChartIDDict = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> AllChartIDDict => _allChartIDDict;

        public void LoadAllChartID(Action<bool> action = null)
        {
            // ��Ʈ ����Ʈ �ҷ�����
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
                            Debug.LogWarning($"������ ��Ʈ Ű ���� �����մϴ� : {chartName} - {selectedChartFileId}");
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

                    Debug.LogError($"[{className}] ��Ʈ ID �ҷ����� ���� : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(bro.IsSuccess());
                }
            });
        }
    }

    /// <summary>�ڳ� �ֿܼ��� �����ϴ� ���� ������</summary>
    public class BackendGameData
    {
        private readonly Dictionary<string, BaseGameData> _allGameDataDict = new Dictionary<string, BaseGameData>();
        public IReadOnlyDictionary<string, BaseGameData> AllGameDataDict => _allGameDataDict;

        public BackendGameData()
        {
            // TODO : ���� ������ �߰�
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

            // ������Ʈ�� �����Ͱ� �ִ��� Ȯ��
            List<BaseGameData> changedGameDataList = new List<BaseGameData>();
            foreach (BaseGameData gameData in GameData.AllGameDataDict.Values)
            {
                if (gameData.IsChangedBackendGameData)
                {
                    info += gameData.GetTableName() + "\n";
                    changedGameDataList.Add(gameData);
                }
            }

            // ������Ʈ�� ����� �������� ����
            if (changedGameDataList.Count == 0)
            {
                return;
            }

            // ������Ʈ ����� �ϳ���� �ش� ���̺� ������Ʈ
            if (changedGameDataList.Count == 1)
            {
                foreach (BaseGameData gameData in changedGameDataList)
                {
                    gameData.UpdateBackendGameData(bro =>
                    {
                        if(bro.IsSuccess())
                        {
                            // ������Ʈ�� ���������� �Ϸ�Ǹ� ����� ������ �÷��׸� �ʱ�ȭ
                            gameData.ResetIsChangedBackendGameData();

                            Debug.Log($"���� ������ ������Ʈ ���� : {gameData.GetTableName()}\n������Ʈ ���̺� : {gameData.GetTableName()}");
                        }
                        else
                        {
                            SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), bro.ToString() + "\n" + info);

                            Debug.LogError($"���� ������ ������Ʈ ���� : {bro}\n������Ʈ ���̺� : {gameData.GetTableName()}");
                        }
                    });
                }
            }
            // 2�� �̻��̶�� Ʈ����ǿ� ��� ������Ʈ
            // NOTE: 10�� �̻��̸� Ʈ����� ������ �� �ֽ��ϴ�.
            else
            {
                List<TransactionValue> transactionList = new List<TransactionValue>();

                // ����� �����͸�ŭ Ʈ����� �߰�
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

                        Debug.Log($"���� ������ Ʈ����� ������Ʈ ���� : {info}");
                    }
                    else
                    {
                        SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), bro.ToString() + "\n" + info);

                        Debug.LogError($"���� ������ Ʈ����� ������Ʈ ���� : {bro}\n������Ʈ ���̺� : {info}");
                    }
                });
            }
        }
    }
}
