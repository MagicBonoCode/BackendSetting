using UnityEngine;

namespace BackEnd
{
    public partial class BackendBaseManager : MonoBehaviour
    {
        private static BackendBaseManager _instance;
        public static BackendBaseManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindFirstObjectByType<BackendBaseManager>();
                    if(_instance == null)
                    {
                        var obj = new GameObject(nameof(BackendBaseManager));
                        _instance = obj.AddComponent<BackendBaseManager>();
                    }
                }

                return _instance;
            }
        }

        /// <summary>치명적인 에러 발생 여부</summary>
        private bool _isErrorOccured = false;

        private void Awake()
        {
            if(_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if(_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            var initializeBro = Backend.Initialize();

            if(initializeBro.IsSuccess())
            {
                Debug.Log($"뒤끝 초기화 성공했습니다.");

                CreateSendQueueMgr();
                SetErrorHandler();
                UpdateBackend();
            }
            else
            {
                Debug.LogError($"뒤끝 초기화 실패 : {initializeBro}");
            }
        }

        /// <summary>SendQueue 매니저 생성</summary>
        /// *SendQueue는 함수 호출 시 바로 호출하지 않고 큐에 적재한 후 순차적으로 함수를 호출하는 방식입니다.
        private void CreateSendQueueMgr()
        {
            var obj = new GameObject();
            obj.name = "SendQueueMgr";
            obj.transform.SetParent(transform);
            obj.AddComponent<SendQueueMgr>();
        }

        private void UpdateBackend()
        {
            // 데이터
            StartCoroutine(CoUpdateGameDataTransaction());

            // 우편
            StartCoroutine(CoUpdatePost());
        }

        /// <summary>로그 유지 기간</summary>
        private const int LOG_EXPIRATION_DAYS = 7;

        /// <summary>뒤끝 에러 발생 시 게임로그 전송</summary>
        public void SendBugReport(string className, string functionName, string errorInfo, int repeatCount = 3)
        {
            // 로그 보내기 실패 시 재귀함수를 통해 최대 'repeatCount'번까지 호출을 시도합니다.
            if(repeatCount <= 0)
            {
                return;
            }

            Param param = new Param();
            param.Add("className", className);
            param.Add("functionName", functionName);
            param.Add("errorPath", errorInfo);

            Backend.GameLog.InsertLogV2("error", param, LOG_EXPIRATION_DAYS, callback =>
            {
                // 에러 발생 시 재귀호출
                if(callback.IsSuccess() == false)
                {
                    SendBugReport(className, functionName, errorInfo, repeatCount - 1);
                }
            });
        }


        #region Handler

        private const string LOG_ERROR_MAINTENANCE = "서버 점검중 입니다.";
        private const string LOG_ERROR_TOOMANYREQUEST = "과도한 호출이 발생하였습니다.";
        private const string LOG_ERROR_OTHERDEVICELOGIN = "다른 기기로부터 로그인 요청이 발생했습니다. 연결이 중단됩니다.";
        private const string LOG_ERROR_DEVICEBLOCK = "차단된 계정으로 로그인을 시도했습니다.";

        /// <summary>뒤끝 에러 핸들러 설정</summary>
        private void SetErrorHandler()
        {
            Backend.ErrorHandler.OnMaintenanceError = () =>
            {
                Debug.Log(LOG_ERROR_MAINTENANCE);
            };

            Backend.ErrorHandler.OnTooManyRequestError = () =>
            {
                Debug.Log(LOG_ERROR_TOOMANYREQUEST);
            };

            Backend.ErrorHandler.OnTooManyRequestByLocalError = () =>
            {
                Debug.Log(LOG_ERROR_TOOMANYREQUEST);
            };

            Backend.ErrorHandler.OnOtherDeviceLoginDetectedError = () =>
            {
                Debug.Log(LOG_ERROR_OTHERDEVICELOGIN);
            };

            Backend.ErrorHandler.OnDeviceBlockError = () =>
            {
                Debug.Log(LOG_ERROR_DEVICEBLOCK);
            };
        }

        public bool BackendHandleResult(BackendReturnObject bro)
        {
            if(bro.IsSuccess())
            {
                Debug.Log($"뒤끝 베이스 서버와 통신에 성공했습니다.");
            }
            else
            {
                Debug.LogError($"뒤끝 베이스 서버와 통신 중 에러가 발생했습니다 : {bro}");
            }

            return bro.IsSuccess();
        }

        #endregion
    }
}
