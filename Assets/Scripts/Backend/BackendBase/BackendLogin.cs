using System;
using UnityEngine;

namespace BackEnd
{
    public partial class BackendBaseManager : MonoBehaviour
    {
        /// <summary>
        /// 토큰을 이용한 로그인
        /// </summary>
        /// <param name="action">결과 콜백</param>
        public void LoginWithTheBackendToken(Action<bool> action = null)
        {
            var bro = Backend.BMember.LoginWithTheBackendToken();
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// 게스트 로그인
        /// </summary>
        /// <param name="action">결과 콜백</param>
        public void GuestLogin(Action<bool> action = null)
        {
            var bro = Backend.BMember.GuestLogin();
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// 토큰을 갱신
        /// </summary>
        /// <param name="action">결과 콜백</param>
        public void RefreshTheBackendToken(Action<bool> action = null)
        {
            var bro = Backend.BMember.RefreshTheBackendToken();
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// 토큰 여부 확인
        /// </summary>
        /// <param name="action">결과 콜백</param>
        public void IsAccessTokenAlive(Action<bool> action = null)
        {
            var bro = Backend.BMember.IsAccessTokenAlive();
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// 닉네임 갱신
        /// </summary>
        /// <param name="nickname">닉네임</param>
        /// <param name="action">결과 콜백</param>
        public void UpdateNickName(string nickname, Action<bool> action = null)
        {
            var bro = Backend.BMember.UpdateNickname(nickname);
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// 닉네임 중복 확인
        /// </summary>
        /// <param name="nickname">닉네임</param>
        /// <param name="action">결과 콜백</param>
        public void CheckNicknameDuplication(string nickname, Action<bool> action = null)
        {
            var bro = Backend.BMember.CheckNicknameDuplication(nickname);
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// 로컬에 저장된 게스트 ID를 참조
        /// </summary>
        /// <returns>게스트 ID</returns>
        public string GetGuestID()
        {
            var bro = Backend.BMember.GetGuestID();
            return bro.ToString();
        }
    }
}
