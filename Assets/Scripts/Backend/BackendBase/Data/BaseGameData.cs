using LitJson;
using System;
using UnityEngine;

namespace BackEnd
{
    /// <summary>
    /// �ڳ����� �����ϴ� ���� ������
    /// </summary>
    public abstract class BaseGameData
    {
        /// <summary>���� �������� ���� �ĺ���</summary>
        public string InData { get; private set; }
        /// <summary>�����Ͱ� ����Ǿ����� ����</summary>
        public bool IsChangedBackendGameData { get; private set; }

        /// <summary>�ڳ��� ��ϵ� ���� ������ ���̺� �̸�</summary>
        public abstract string GetTableName();
        /// <summary>�ڳ��� ��ϵ� ���� ������ ���̺� ������ ������</summary>
        public abstract Param GetParam();

        protected abstract void InitializeData();
        protected abstract void SetServerDataToLocal(JsonData gameDataJson);

        protected virtual void UpdateData()
        {
            IsChangedBackendGameData = true;
        }

        public void ResetIsChangedBackendGameData()
        {
            IsChangedBackendGameData = false;
        }

        public void LoadGameData(Action<bool> action = null)
        {
            // ���� ������ ��û
            SendQueue.Enqueue(Backend.GameData.GetMyData, GetTableName(), new Where(), bro =>
            {
                try
                {
                    Debug.Log($"Backend.GameData.GetMyData({GetTableName()}) : {bro}");

                    if(!bro.IsSuccess())
                    {
                        throw new Exception(bro.ToString());
                    }

                    if(bro.FlattenRows().Count == 0)
                    {
                        // �ε��� ���� ���ٸ� �ʱ�ȭ �� ����
                        InsertBackendGameData(action);
                        return;
                    }
                    else
                    {
                        InData = bro.FlattenRows()[0]["inDate"].ToString();

                        // �������� �ε�� �����͸� ���� �����ͷ� ����
                        SetServerDataToLocal(bro.FlattenRows()[0]);
                    }
                }
                catch(Exception e)
                {
                    string className = GetType().Name;
                    string errorInfo = e.Message;

                    Debug.LogError($"[{className}] ���� ������ �ҷ����� ���� : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(bro.IsSuccess());
                }
            });
        }

        /// <summary>���� �ε��� ���� ���ٸ� ���ÿ��� �ʱ�ȭ �� �ڳ��� ����</summary>
        private void InsertBackendGameData(Action<bool> action = null)
        {
            // ������ �ʱ�ȭ
            InitializeData();

            // ���� ���� ���� �Լ�
            SendQueue.Enqueue(Backend.GameData.Insert, GetTableName(), GetParam(), bro =>
            {
                try
                {
                    Debug.Log($"Backend.GameData.Insert({GetTableName()}) : {bro}");

                    if(!bro.IsSuccess())
                    {
                        throw new Exception(bro.ToString());
                    }

                    InData = bro.GetInDate();
                }
                catch(Exception e)
                {
                    string className = GetType().Name;
                    string errorInfo = e.Message;

                    Debug.LogError($"[{className}] ���� ������ ���� ���� : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(true);
                }
            });
        }

        public void UpdateBackendGameData(Action<BackendReturnObject> action = null)
        {
            // ���� ���� ������Ʈ
            SendQueue.Enqueue(Backend.GameData.UpdateV2, GetTableName(), InData, Backend.UserInDate, GetParam(), bro =>
            {
                action?.Invoke(bro);
            });
        }

        /// <summary>���̺� ������Ʈ�� �����͸� Ʈ��������� ����� ��ȯ</summary>
        public TransactionValue GetTransactionUpdateValue()
        {
            return TransactionValue.SetUpdateV2(GetTableName(), InData, Backend.UserInDate, GetParam());
        }
    }
}
