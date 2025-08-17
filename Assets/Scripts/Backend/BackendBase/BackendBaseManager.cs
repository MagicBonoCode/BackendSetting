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

        /// <summary>ġ������ ���� �߻� ����</summary>
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
                Debug.Log($"�ڳ� �ʱ�ȭ �����߽��ϴ�.");

                CreateSendQueueMgr();
                SetErrorHandler();
                UpdateBackend();
            }
            else
            {
                Debug.LogError($"�ڳ� �ʱ�ȭ ���� : {initializeBro}");
            }
        }

        /// <summary>SendQueue �Ŵ��� ����</summary>
        /// *SendQueue�� �Լ� ȣ�� �� �ٷ� ȣ������ �ʰ� ť�� ������ �� ���������� �Լ��� ȣ���ϴ� ����Դϴ�.
        private void CreateSendQueueMgr()
        {
            var obj = new GameObject();
            obj.name = "SendQueueMgr";
            obj.transform.SetParent(transform);
            obj.AddComponent<SendQueueMgr>();
        }

        private void UpdateBackend()
        {
            // ������
            StartCoroutine(CoUpdateGameDataTransaction());

            // ����
            StartCoroutine(CoUpdatePost());
        }

        /// <summary>�α� ���� �Ⱓ</summary>
        private const int LOG_EXPIRATION_DAYS = 7;

        /// <summary>�ڳ� ���� �߻� �� ���ӷα� ����</summary>
        public void SendBugReport(string className, string functionName, string errorInfo, int repeatCount = 3)
        {
            // �α� ������ ���� �� ����Լ��� ���� �ִ� 'repeatCount'������ ȣ���� �õ��մϴ�.
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
                // ���� �߻� �� ���ȣ��
                if(callback.IsSuccess() == false)
                {
                    SendBugReport(className, functionName, errorInfo, repeatCount - 1);
                }
            });
        }


        #region Handler

        private const string LOG_ERROR_MAINTENANCE = "���� ������ �Դϴ�.";
        private const string LOG_ERROR_TOOMANYREQUEST = "������ ȣ���� �߻��Ͽ����ϴ�.";
        private const string LOG_ERROR_OTHERDEVICELOGIN = "�ٸ� ���κ��� �α��� ��û�� �߻��߽��ϴ�. ������ �ߴܵ˴ϴ�.";
        private const string LOG_ERROR_DEVICEBLOCK = "���ܵ� �������� �α����� �õ��߽��ϴ�.";

        /// <summary>�ڳ� ���� �ڵ鷯 ����</summary>
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
                Debug.Log($"�ڳ� ���̽� ������ ��ſ� �����߽��ϴ�.");
            }
            else
            {
                Debug.LogError($"�ڳ� ���̽� ������ ��� �� ������ �߻��߽��ϴ� : {bro}");
            }

            return bro.IsSuccess();
        }

        #endregion
    }
}
