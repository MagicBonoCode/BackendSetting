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
             * # 매칭 흐름 (Matchmaking Phase)
             * ------------------------------------------- *
             * - JoinMatchMakingServer     : 매칭 서버 접속 요청
             * - OnJoinMatchMakingServer   : 접속 성공 콜백
             * - GetMatchList              : 매칭 리스트 조회
             * - CreateMatchRoom           : 매칭 대기방 생성
             * - OnMatchMakingRoomCreate   : 대기방 생성 확인
             * - RequestMatchMaking        : 매칭 신청
             * - OnMatchMakingResponse     : 매칭 응답 수신
             * - JoinGameServer            : 인게임 서버 접속
             * =========================================== */

            /* =========================================== *
             * # 인게임 흐름 (In-Game Phase)
             * ------------------------------------------- *
             * - JoinGameServer            : 인게임 서버 접속 요청
             * - OnSessionJoinInServer     : 접속 성공 콜백
             * - JoinGameRoom              : 게임방 입장
             * - OnSessionListInServer     : 게임방 입장 유저 정보 수신(입장 시 최초 1회)
             * - OnMatchInGameAccess       : 게임방 개별 유저 입장 정보 수신
             * - OnMatchInGameStart        : 게임 시작 콜백
             * - SendDataToInGameRoom      : 실시간 데이터 전송
             * - OnMatchRelay              : 실시간 데이터 수신
             * - MatchEnd                  : 게임 종료 처리
             * - OnMatchResult             : 결과 수신 콜백
             * =========================================== */

            // 매치 서버에서 오는 이벤트 처리 핸들러
            MatchHandler();
            // 인게임 서버에서 오는 이벤트 처리 핸들러
            InGameHandler();
        }

        private void Update()
        {
            if(_isJoinedMatchMakingServer)
            {
                // 클라이언트와 매치 서버 간의 메시지 송수신을 담당합니다.
                // - 서버로부터 수신된 데이터는 SDK에서 처리 후 관련 이벤트를 트리거합니다.
                // - 안정적인 실시간 통신을 위해 Poll() 함수는 프레임마다 반복적으로 호출되어야 합니다.
                Backend.Match.Poll();
            }
        }

        #region Match

        /// <summary>뒤끝 콘솔 매칭 정보 리스트</summary>
        public List<MatchInfo> MatchInfoList { get; private set; } = new List<MatchInfo>();

        /// <summary>매치 서버 접속 확인</summary>
        private bool _isJoinedMatchMakingServer = false;
        /// <summary>대기방 생성 확인</summary>
        private bool _isCreatedMatchMakingRoom = false;
        /// <summary>매치 성공 응답 확인</summary>
        private bool _isRespondedMatchMaking = false;

        /// <summary>매칭 정보 인덱스</summary>
        private int _matchInfoIndex = 0;

        private void MatchHandler()
        {
            // 매칭 서버 접속 이벤트 핸들러
            Backend.Match.OnJoinMatchMakingServer += (args) =>
            {
                if(args.ErrInfo == ErrorInfo.Success)
                {
                    Debug.Log($"매칭 서버에 성공적으로 접속했습니다.");

                    _isJoinedMatchMakingServer = true;
                }
                else
                {
                    Debug.LogError($"매칭 서버 접속 실패 : {args.ErrInfo}");

                    _isJoinedMatchMakingServer = false;
                }
            };

            // 대기방을 생성 이벤트 핸들러
            Backend.Match.OnMatchMakingRoomCreate += (args) =>
            {
                if(args.ErrInfo == ErrorCode.Success)
                {
                    Debug.Log($"매칭 대기방이 성공적으로 생성되었습니다.");

                    _isCreatedMatchMakingRoom = true;
                }
                else
                {
                    Debug.Log($"매칭 대기방 생성 실패 : {args.ErrInfo}");

                    _isCreatedMatchMakingRoom = false;
                }
            };

            // 매칭 성사 이벤트 핸들러
            Backend.Match.OnMatchMakingResponse += (args) =>
            {
                if(args.ErrInfo == ErrorCode.Success)
                {
                    Debug.Log("매칭이 성공적으로 성사되었습니다.");

                    _isRespondedMatchMaking = true;
                }
                else
                {
                    Debug.Log("매칭 실패: " + args.ErrInfo);

                    _isRespondedMatchMaking = false;
                }
            };
        }

        public void JoinMatchMakingServer()
        {
            ErrorInfo errorInfo;
            if(Backend.Match.JoinMatchMakingServer(out errorInfo))
            {
                Debug.Log("매칭 서버 접속 요청이 성공적으로 처리되었습니다." + errorInfo);
            }
            else
            {
                Debug.LogError("매칭 서버 접속 요청 실패: " + errorInfo);
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
                    Debug.Log("매칭 목록을 성공적으로 가져왔습니다.");
                }
                else
                {
                    Debug.LogError("매칭 목록 가져오기 실패: " + bro);
                }

                action?.Invoke(bro.IsSuccess());
            });
        }

        public void CreateMatchRoom()
        {
            Debug.Log("매칭 대기방을 생성을 요청합니다.");
            
            Backend.Match.CreateMatchRoom();
        }

        public void RequestMatchMaking()
        {
            Debug.Log("매칭 신청을 요청합니다.");
            
            Backend.Match.RequestMatchMaking(MatchInfoList[_matchInfoIndex].MatchType, MatchInfoList[_matchInfoIndex].MatchModeType, MatchInfoList[_matchInfoIndex].InDate);
        }

        public void JoinGameServer(MatchMakingResponseEventArgs args)
        {
            ErrorInfo errorInfo;
            if(Backend.Match.JoinGameServer(args.RoomInfo.m_inGameServerEndPoint.m_address, args.RoomInfo.m_inGameServerEndPoint.m_port, false, out errorInfo))
            {
                Debug.Log("인게임 서버 접속 요청이 성공적으로 처리되었습니다.");

                _inGameRoomToekn = args.RoomInfo.m_inGameRoomToken;
            }
            else
            {
                Debug.LogError("인게임 서버 접속 요청 실패: " + errorInfo);
            }
        }

        #endregion

        #region InGame

        /// <summary>인게임 참여 유저들 세션 리스트</summary>
        public List<SessionId> SessionIdList { get; private set; } = new List<SessionId>();
        /// <summary>호스트 세션</summary>
        public SessionId HostSessionId { get; private set; }
        /// <summary>인게임 결과 정보</summary>
        public MatchGameResult MatchGameResult { get; private set; }

        /// <summary>인게임 서버 접속 확인</summary>
        private bool _isJoinedInGameServer = false;

        /// <summary>게임방 룸 토큰</summary>
        private string _inGameRoomToekn = string.Empty;

        private void InGameHandler()
        {
            // 인게임 서버 접속 이벤트 핸들러
            Backend.Match.OnSessionJoinInServer += (args) =>
            {
                if(args.ErrInfo == ErrorInfo.Success)
                {
                    Debug.Log("인게임 서버에 성공적으로 접속했습니다.");

                    _isJoinedInGameServer = true;
                }
                else
                {
                    Debug.LogError("인게임 서버 접속 실패: " + args.ErrInfo);

                    _isJoinedInGameServer = false;
                }
            };

            // 게임방 접속 성공 이벤트 핸들러(입장한 유저에게만 최초 1회 호출)
            Backend.Match.OnSessionListInServer += (args) =>
            {
                /* =========================================== *
                 * # 이벤트 호출 규칙
                 * ------------------------------------------- *
                 * 현재 방에 접속해 있던 유저 + 방금 방에 접속한 유저의 정보가 GameRecords에 포함되어 있습니다.
                 * - A, B, C 유저가 매칭되었습니다.
                 * - 게임방에 아무도 입장하지 않은 상황에서 A가 방에 접속하면 A의 GameRecord만 존재하는 OnSessionListInServer 이벤트가 A에게 호출됩니다.
                 * - 이후(A가 방에 있는 상황) B가 방에 접속하면 A, B의 GameRecord가 존재하는 OnSessionListInServer 이벤트가 B에게 호출됩니다.
                 * - 마지막으로 C가 방에 접속하면 A, B, C의 GameRecord가 존재하는 OnSessionListInServer 이벤트가 C에게 호출됩니다.
                 * =========================================== */

            };

            // 게임방 접속 유저 정보 수신 이벤트 핸들러
            Backend.Match.OnMatchInGameAccess += (args) =>
            {
                /* =========================================== *
                 * # 이벤트 호출 규칙
                 * ------------------------------------------- *
                 * 입장한 유저의 정보가 GameRecord에 포함되어 있습니다.
                 * - A, B, C의 유저가 있을 때
                 * - A가 방에 접속하면 A의 GameRecord가 포함된 OnMatchInGameAccess 이벤트 핸들러가 호출됩니다.
                 * - 방에 A가 접속해있고, B가 접속했을 때 A와 B는 B의 GameRecord가 포함된 OnMatchInGameAccess 이벤트 핸들러를 호출 받습니다.
                 * - 이후 C가 방에 입장하면 A, B, C는 각각 C의 GameRecord가 포함된 OnMatchInGameAccess 이벤트 핸들러를 호출 받습니다.
                 * =========================================== */

            };

            // 게임 시작 이벤트 핸들러
            Backend.Match.OnMatchInGameStart += () =>
            {
                Debug.Log("게임이 시작되었습니다.");
            };

            // 인게임 데이터 수신 이벤트 핸들러
            // 서버는 단순 브로드캐스팅만 지원 (서버에서 어떠한 연산도 수행하지 않음 => 중계 서버)
            Backend.Match.OnMatchRelay += (args) =>
            {
                Debug.Log("인게임 데이터 수신: " + args.ErrInfo);

                byte[] binaryData = args.BinaryUserData;
            };

            // 게임 종료 이벤트 핸들러
            Backend.Match.OnMatchResult += (args) =>
            {
                if(args.ErrInfo == ErrorCode.Success)
                {
                    Debug.Log("게임 종료 처리 성공: " + args.ErrInfo);
                }
                else
                {
                    Debug.LogError("게임 종료 처리 실패: " + args.ErrInfo);
                }
            };
        }

        public void JoinGameRoom()
        {
            Debug.Log("인게임 방에 입장 요청을 합니다.");

            Backend.Match.JoinGameRoom(_inGameRoomToekn);
        }

        public void SendDataToInGameRoom<T>(T msg)
        {
            Debug.Log("인게임 데이터를 전송을 요청 합니다.");

            byte[] binaryData = DataParser.DataToJsonData<T>(msg);
            Backend.Match.SendDataToInGameRoom(binaryData);
        }

        public void MatchEnd()
        {
            Debug.Log("게임을 종료합니다.");

            Backend.Match.MatchEnd(MatchGameResult);
        }

        #endregion
    }
}
