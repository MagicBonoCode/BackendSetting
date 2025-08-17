using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BackEnd
{
    public class BackendPost
    {
        private readonly Dictionary<string, BasePost> _allPostDict = new Dictionary<string, BasePost>();
        public IReadOnlyDictionary<string, BasePost> AllPostDict => _allPostDict;

        public void LoadPostList(PostType postType, Action<BackendReturnObject> action = null)
        {
            SendQueue.Enqueue(Backend.UPost.GetPostList, postType, bro =>
            {
                try
                {
                    Debug.Log($"Backend.UPost.GetPostList({postType}) : {bro}");

                    if(!bro.IsSuccess())
                    {
                        throw new Exception(bro.ToString());
                    }

                    JsonData jsonData = bro.GetReturnValuetoJSON()["postList"];
                    for(int i = 0; i < jsonData.Count; i++)
                    {
                        // 새로운 우편인	경우에만 추가
                        if(!_allPostDict.ContainsKey(jsonData[i]["inDate"].ToString()))
                        {
                            BasePost post = new BasePost(postType, jsonData[i]);
                            _allPostDict.Add(post.InDate, post);
                        }
                    }
                }
                catch(Exception e)
                {
                    string errorInfo = e.Message;
                 
                    Debug.LogError($"우편 목록 불러오기 실패 : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(bro);
                }
            });
        }

        public void ReceivePost(string postKey)
        {
            if(_allPostDict.ContainsKey(postKey))
            {
                _allPostDict[postKey].Receive((isSuccess) =>
                {
                    if(!isSuccess)
                    {
                        return;
                    }

                    RemovePost(postKey);
                });
            }
        }

        public void RemovePost(string inDate)
        {
            if(_allPostDict.ContainsKey(inDate))
            {
                _allPostDict.Remove(inDate);
            }
        }
    }

    public partial class BackendBaseManager : MonoBehaviour
    {
        public BackendPost BackendPost { get; private set; } = new BackendPost();

        private const float POST_UPDATE_TICK = 60.0f;

        private IEnumerator CoUpdatePost()
        {
            while(!_isErrorOccured)
            {
                UpdatePost();

                yield return new WaitForSeconds(POST_UPDATE_TICK);
            }
        }

        private void UpdatePost()
        {
            // 우편 갯수 확인
            int postCount = BackendPost.AllPostDict.Count;

            BackendPost.LoadPostList(PostType.Admin, bro =>
            {
                if(bro.IsSuccess())
                {
                    // 우편 갯수가 변경된 경우에만 갱신
                }
                else
                {
                    SendBugReport(GetType().Name, MethodBase.GetCurrentMethod()?.ToString(), bro.ToString());

                    Debug.LogError($"우편 목록 갱신 실패 : {bro}");
                }
            });
        }

    }
}
