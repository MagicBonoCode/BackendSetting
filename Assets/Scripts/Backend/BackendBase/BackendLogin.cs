using System;
using UnityEngine;

namespace BackEnd
{
    public partial class BackendBaseManager : MonoBehaviour
    {
        /// <summary>
        /// ��ū�� �̿��� �α���
        /// </summary>
        /// <param name="action">��� �ݹ�</param>
        public void LoginWithTheBackendToken(Action<bool> action = null)
        {
            var bro = Backend.BMember.LoginWithTheBackendToken();
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// �Խ�Ʈ �α���
        /// </summary>
        /// <param name="action">��� �ݹ�</param>
        public void GuestLogin(Action<bool> action = null)
        {
            var bro = Backend.BMember.GuestLogin();
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// ��ū�� ����
        /// </summary>
        /// <param name="action">��� �ݹ�</param>
        public void RefreshTheBackendToken(Action<bool> action = null)
        {
            var bro = Backend.BMember.RefreshTheBackendToken();
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// ��ū ���� Ȯ��
        /// </summary>
        /// <param name="action">��� �ݹ�</param>
        public void IsAccessTokenAlive(Action<bool> action = null)
        {
            var bro = Backend.BMember.IsAccessTokenAlive();
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// �г��� ����
        /// </summary>
        /// <param name="nickname">�г���</param>
        /// <param name="action">��� �ݹ�</param>
        public void UpdateNickName(string nickname, Action<bool> action = null)
        {
            var bro = Backend.BMember.UpdateNickname(nickname);
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// �г��� �ߺ� Ȯ��
        /// </summary>
        /// <param name="nickname">�г���</param>
        /// <param name="action">��� �ݹ�</param>
        public void CheckNicknameDuplication(string nickname, Action<bool> action = null)
        {
            var bro = Backend.BMember.CheckNicknameDuplication(nickname);
            bool isSuccess = BackendHandleResult(bro);
            action?.Invoke(isSuccess);
        }

        /// <summary>
        /// ���ÿ� ����� �Խ�Ʈ ID�� ����
        /// </summary>
        /// <returns>�Խ�Ʈ ID</returns>
        public string GetGuestID()
        {
            var bro = Backend.BMember.GetGuestID();
            return bro.ToString();
        }
    }
}
