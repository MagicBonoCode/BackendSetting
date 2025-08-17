using BackEnd.Tcp;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BackEnd
{
    public class MatchInfo
    {
        public string Title { get; private set; }
        public string InDate { get; private set; }
        public MatchType MatchType { get; private set; }
        public MatchModeType MatchModeType { get; private set; }
        public string HeadCount { get; private set; }
        public bool IsSandBoxEnable { get; private set; }

        public MatchInfo(string title, string inDate, MatchType matchType, MatchModeType matchModeType, string headCount, bool isSandBoxEnable)
        {
            Title = title;
            InDate = inDate;
            MatchType = matchType;
            MatchModeType = matchModeType;
            HeadCount = headCount;
            IsSandBoxEnable = isSandBoxEnable;
        }
    }

    public class BackEndMatchManager : MonoBehaviour
    {
        private static BackEndMatchManager _instance;
        public static BackEndMatchManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindFirstObjectByType<BackEndMatchManager>();
                    if(_instance == null)
                    {
                        var obj = new GameObject(nameof(BackEndMatchManager));
                        _instance = obj.AddComponent<BackEndMatchManager>();
                    }
                }

                return _instance;
            }
        }

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
            /* =========================================== *
             * # ��Ī �帧 (Matchmaking Phase)
             * ------------------------------------------- *
             * - JoinMatchMakingServer     : ��Ī ���� ���� ��û
             * - OnJoinMatchMakingServer   : ���� ���� �ݹ�
             * - GetMatchList              : ��Ī ����Ʈ ��ȸ
             * - CreateMatchRoom           : ��Ī ���� ����
             * - OnMatchMakingRoomCreate   : ���� ���� Ȯ��
             * - RequestMatchMaking        : ��Ī ��û
             * - OnMatchMakingResponse     : ��Ī ���� ����
             * - JoinGameServer            : �ΰ��� ���� ����
             * =========================================== */

            /* =========================================== *
             * # �ΰ��� �帧 (In-Game Phase)
             * ------------------------------------------- *
             * - JoinGameServer            : �ΰ��� ���� ���� ��û
             * - OnSessionJoinInServer     : ���� ���� �ݹ�
             * - JoinGameRoom              : ���ӹ� ����
             * - OnSessionListInServer     : ���ӹ� ���� ���� ���� ����(���� �� ���� 1ȸ)
             * - OnMatchInGameAccess       : ���ӹ� ���� ���� ���� ���� ����
             * - OnMatchInGameStart        : ���� ���� �ݹ�
             * - SendDataToInGameRoom      : �ǽð� ������ ����
             * - OnMatchRelay              : �ǽð� ������ ����
             * - MatchEnd                  : ���� ���� ó��
             * - OnMatchResult             : ��� ���� �ݹ�
             * =========================================== */

            // ��ġ �������� ���� �̺�Ʈ ó�� �ڵ鷯
            MatchHandler();
            // �ΰ��� �������� ���� �̺�Ʈ ó�� �ڵ鷯
            InGameHandler();
        }

        private void Update()
        {
            if(_isJoinedMatchMakingServer)
            {
                // Ŭ���̾�Ʈ�� ��ġ ���� ���� �޽��� �ۼ����� ����մϴ�.
                // - �����κ��� ���ŵ� �����ʹ� SDK���� ó�� �� ���� �̺�Ʈ�� Ʈ�����մϴ�.
                // - �������� �ǽð� ����� ���� Poll() �Լ��� �����Ӹ��� �ݺ������� ȣ��Ǿ�� �մϴ�.
                Backend.Match.Poll();
            }
        }

        #region Match

        /// <summary>�ڳ� �ܼ� ��Ī ���� ����Ʈ</summary>
        public List<MatchInfo> MatchInfoList { get; private set; } = new List<MatchInfo>();

        /// <summary>��ġ ���� ���� Ȯ��</summary>
        private bool _isJoinedMatchMakingServer = false;
        /// <summary>���� ���� Ȯ��</summary>
        private bool _isCreatedMatchMakingRoom = false;
        /// <summary>��ġ ���� ���� Ȯ��</summary>
        private bool _isRespondedMatchMaking = false;

        /// <summary>��Ī ���� �ε���</summary>
        private int _matchInfoIndex = 0;

        private void MatchHandler()
        {
            // ��Ī ���� ���� �̺�Ʈ �ڵ鷯
            Backend.Match.OnJoinMatchMakingServer += (args) =>
            {
                if(args.ErrInfo == ErrorInfo.Success)
                {
                    Debug.Log($"��Ī ������ ���������� �����߽��ϴ�.");

                    _isJoinedMatchMakingServer = true;
                }
                else
                {
                    Debug.LogError($"��Ī ���� ���� ���� : {args.ErrInfo}");

                    _isJoinedMatchMakingServer = false;
                }
            };

            // ������ ���� �̺�Ʈ �ڵ鷯
            Backend.Match.OnMatchMakingRoomCreate += (args) =>
            {
                if(args.ErrInfo == ErrorCode.Success)
                {
                    Debug.Log($"��Ī ������ ���������� �����Ǿ����ϴ�.");

                    _isCreatedMatchMakingRoom = true;
                }
                else
                {
                    Debug.Log($"��Ī ���� ���� ���� : {args.ErrInfo}");

                    _isCreatedMatchMakingRoom = false;
                }
            };

            // ��Ī ���� �̺�Ʈ �ڵ鷯
            Backend.Match.OnMatchMakingResponse += (args) =>
            {
                if(args.ErrInfo == ErrorCode.Success)
                {
                    Debug.Log("��Ī�� ���������� ����Ǿ����ϴ�.");

                    _isRespondedMatchMaking = true;
                }
                else
                {
                    Debug.Log("��Ī ����: " + args.ErrInfo);

                    _isRespondedMatchMaking = false;
                }
            };
        }

        public void JoinMatchMakingServer()
        {
            ErrorInfo errorInfo;
            if(Backend.Match.JoinMatchMakingServer(out errorInfo))
            {
                Debug.Log("��Ī ���� ���� ��û�� ���������� ó���Ǿ����ϴ�." + errorInfo);
            }
            else
            {
                Debug.LogError("��Ī ���� ���� ��û ����: " + errorInfo);
            }
        }

        public void GetMatchList(Action<bool> action = null)
        {
            MatchInfoList.Clear();
            Backend.Match.GetMatchList(bro =>
            {
                foreach(LitJson.JsonData row in bro.Rows())
                {
                    MatchInfo matchInfo = new MatchInfo(
                        row["matchTitle"]["S"].ToString(),
                        row["inDate"]["S"].ToString(),
                        Enum.TryParse(row["matchType"]["S"].ToString(), true, out MatchType matchType) ? matchType : default,
                        Enum.TryParse(row["matchModeType"]["S"].ToString(), true, out MatchModeType matchModeType) ? matchModeType : default,
                        row["matchHeadCount"]["N"].ToString(),
                        row["enable_sandbox"]["BOOL"].ToString().Equals("True") ? true : false
                    );

                    MatchInfoList.Add(matchInfo);
                }

                if(bro.IsSuccess())
                {
                    Debug.Log("��Ī ����� ���������� �����Խ��ϴ�.");
                }
                else
                {
                    Debug.LogError("��Ī ��� �������� ����: " + bro);
                }

                action?.Invoke(bro.IsSuccess());
            });
        }

        public void CreateMatchRoom()
        {
            Debug.Log("��Ī ������ ������ ��û�մϴ�.");
            
            Backend.Match.CreateMatchRoom();
        }

        public void RequestMatchMaking()
        {
            Debug.Log("��Ī ��û�� ��û�մϴ�.");
            
            Backend.Match.RequestMatchMaking(MatchInfoList[_matchInfoIndex].MatchType, MatchInfoList[_matchInfoIndex].MatchModeType, MatchInfoList[_matchInfoIndex].InDate);
        }

        public void JoinGameServer(MatchMakingResponseEventArgs args)
        {
            ErrorInfo errorInfo;
            if(Backend.Match.JoinGameServer(args.RoomInfo.m_inGameServerEndPoint.m_address, args.RoomInfo.m_inGameServerEndPoint.m_port, false, out errorInfo))
            {
                Debug.Log("�ΰ��� ���� ���� ��û�� ���������� ó���Ǿ����ϴ�.");

                _inGameRoomToekn = args.RoomInfo.m_inGameRoomToken;
            }
            else
            {
                Debug.LogError("�ΰ��� ���� ���� ��û ����: " + errorInfo);
            }
        }

        #endregion

        #region InGame

        /// <summary>�ΰ��� ���� ������ ���� ����Ʈ</summary>
        public List<SessionId> SessionIdList { get; private set; } = new List<SessionId>();
        /// <summary>ȣ��Ʈ ����</summary>
        public SessionId HostSessionId { get; private set; }
        /// <summary>�ΰ��� ��� ����</summary>
        public MatchGameResult MatchGameResult { get; private set; }

        /// <summary>�ΰ��� ���� ���� Ȯ��</summary>
        private bool _isJoinedInGameServer = false;

        /// <summary>���ӹ� �� ��ū</summary>
        private string _inGameRoomToekn = string.Empty;

        private void InGameHandler()
        {
            // �ΰ��� ���� ���� �̺�Ʈ �ڵ鷯
            Backend.Match.OnSessionJoinInServer += (args) =>
            {
                if(args.ErrInfo == ErrorInfo.Success)
                {
                    Debug.Log("�ΰ��� ������ ���������� �����߽��ϴ�.");

                    _isJoinedInGameServer = true;
                }
                else
                {
                    Debug.LogError("�ΰ��� ���� ���� ����: " + args.ErrInfo);

                    _isJoinedInGameServer = false;
                }
            };

            // ���ӹ� ���� ���� �̺�Ʈ �ڵ鷯(������ �������Ը� ���� 1ȸ ȣ��)
            Backend.Match.OnSessionListInServer += (args) =>
            {
                /* =========================================== *
                 * # �̺�Ʈ ȣ�� ��Ģ
                 * ------------------------------------------- *
                 * ���� �濡 ������ �ִ� ���� + ��� �濡 ������ ������ ������ GameRecords�� ���ԵǾ� �ֽ��ϴ�.
                 * - A, B, C ������ ��Ī�Ǿ����ϴ�.
                 * - ���ӹ濡 �ƹ��� �������� ���� ��Ȳ���� A�� �濡 �����ϸ� A�� GameRecord�� �����ϴ� OnSessionListInServer �̺�Ʈ�� A���� ȣ��˴ϴ�.
                 * - ����(A�� �濡 �ִ� ��Ȳ) B�� �濡 �����ϸ� A, B�� GameRecord�� �����ϴ� OnSessionListInServer �̺�Ʈ�� B���� ȣ��˴ϴ�.
                 * - ���������� C�� �濡 �����ϸ� A, B, C�� GameRecord�� �����ϴ� OnSessionListInServer �̺�Ʈ�� C���� ȣ��˴ϴ�.
                 * =========================================== */

            };

            // ���ӹ� ���� ���� ���� ���� �̺�Ʈ �ڵ鷯
            Backend.Match.OnMatchInGameAccess += (args) =>
            {
                /* =========================================== *
                 * # �̺�Ʈ ȣ�� ��Ģ
                 * ------------------------------------------- *
                 * ������ ������ ������ GameRecord�� ���ԵǾ� �ֽ��ϴ�.
                 * - A, B, C�� ������ ���� ��
                 * - A�� �濡 �����ϸ� A�� GameRecord�� ���Ե� OnMatchInGameAccess �̺�Ʈ �ڵ鷯�� ȣ��˴ϴ�.
                 * - �濡 A�� �������ְ�, B�� �������� �� A�� B�� B�� GameRecord�� ���Ե� OnMatchInGameAccess �̺�Ʈ �ڵ鷯�� ȣ�� �޽��ϴ�.
                 * - ���� C�� �濡 �����ϸ� A, B, C�� ���� C�� GameRecord�� ���Ե� OnMatchInGameAccess �̺�Ʈ �ڵ鷯�� ȣ�� �޽��ϴ�.
                 * =========================================== */

            };

            // ���� ���� �̺�Ʈ �ڵ鷯
            Backend.Match.OnMatchInGameStart += () =>
            {
                Debug.Log("������ ���۵Ǿ����ϴ�.");
            };

            // �ΰ��� ������ ���� �̺�Ʈ �ڵ鷯
            // ������ �ܼ� ��ε�ĳ���ø� ���� (�������� ��� ���굵 �������� ���� => �߰� ����)
            Backend.Match.OnMatchRelay += (args) =>
            {
                Debug.Log("�ΰ��� ������ ����: " + args.ErrInfo);

                byte[] binaryData = args.BinaryUserData;
            };

            // ���� ���� �̺�Ʈ �ڵ鷯
            Backend.Match.OnMatchResult += (args) =>
            {
                if(args.ErrInfo == ErrorCode.Success)
                {
                    Debug.Log("���� ���� ó�� ����: " + args.ErrInfo);
                }
                else
                {
                    Debug.LogError("���� ���� ó�� ����: " + args.ErrInfo);
                }
            };
        }

        public void JoinGameRoom()
        {
            Debug.Log("�ΰ��� �濡 ���� ��û�� �մϴ�.");

            Backend.Match.JoinGameRoom(_inGameRoomToekn);
        }

        public void SendDataToInGameRoom<T>(T msg)
        {
            Debug.Log("�ΰ��� �����͸� ������ ��û �մϴ�.");

            byte[] binaryData = DataParser.DataToJsonData<T>(msg);
            Backend.Match.SendDataToInGameRoom(binaryData);
        }

        public void MatchEnd()
        {
            Debug.Log("������ �����մϴ�.");

            Backend.Match.MatchEnd(MatchGameResult);
        }

        #endregion
    }
}
