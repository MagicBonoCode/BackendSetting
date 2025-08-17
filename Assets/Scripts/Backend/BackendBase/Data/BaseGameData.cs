using LitJson;
using System;
using UnityEngine;

namespace BackEnd
{
    /// <summary>
    /// 뒤끝에서 관리하는 게임 데이터
    /// </summary>
    public abstract class BaseGameData
    {
        /// <summary>게임 데이터의 고유 식별자</summary>
        public string InData { get; private set; }
        /// <summary>데이터가 변경되었는지 여부</summary>
        public bool IsChangedBackendGameData { get; private set; }

        /// <summary>뒤끝에 등록된 게임 데이터 테이블 이름</summary>
        public abstract string GetTableName();
        /// <summary>뒤끝에 등록된 게임 데이터 테이블에 삽입할 데이터</summary>
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
            // 게임 데이터 요청
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
                        // 로드할 값이 없다면 초기화 후 삽입
                        InsertBackendGameData(action);
                        return;
                    }
                    else
                    {
                        InData = bro.FlattenRows()[0]["inDate"].ToString();

                        // 서버에서 로드된 데이터를 로컬 데이터로 설정
                        SetServerDataToLocal(bro.FlattenRows()[0]);
                    }
                }
                catch(Exception e)
                {
                    string className = GetType().Name;
                    string errorInfo = e.Message;

                    Debug.LogError($"[{className}] 게임 데이터 불러오기 실패 : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(bro.IsSuccess());
                }
            });
        }

        /// <summary>최초 로드할 값이 없다면 로컬에서 초기화 후 뒤끝에 삽입</summary>
        private void InsertBackendGameData(Action<bool> action = null)
        {
            // 데이터 초기화
            InitializeData();

            // 게임 정보 삽입 함수
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

                    Debug.LogError($"[{className}] 게임 데이터 삽입 실패 : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(true);
                }
            });
        }

        public void UpdateBackendGameData(Action<BackendReturnObject> action = null)
        {
            // 게임 정보 업데이트
            SendQueue.Enqueue(Backend.GameData.UpdateV2, GetTableName(), InData, Backend.UserInDate, GetParam(), bro =>
            {
                action?.Invoke(bro);
            });
        }

        /// <summary>테이블에 업데이트할 데이터를 트랜잭션으로 만들어 반환</summary>
        public TransactionValue GetTransactionUpdateValue()
        {
            return TransactionValue.SetUpdateV2(GetTableName(), InData, Backend.UserInDate, GetParam());
        }
    }
}
