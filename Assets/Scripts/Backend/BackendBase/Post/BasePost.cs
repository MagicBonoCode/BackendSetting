using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BackEnd
{
    public enum PostChartType
    {
    }

    public class PostChart
    {
        public int Count { get; private set; }
        public PostChartType PostChartType { get; private set; }
        public int TemplateID { get; private set; }
        public string Name { get; private set; }

        public PostChart(JsonData jsonData)
        {
            Count = int.Parse(jsonData["itemCount"].ToString());

            string chartName = jsonData["chartName"].ToString();
            if(!Enum.TryParse(chartName, out PostChartType postChartType))
            {
                Debug.LogError($"PostChartType을 찾을 수 없습니다 : {chartName}");
                return;
            }
            PostChartType = postChartType;

            switch(PostChartType)
            {
                // 차트 타입에 따라 처리
            }
        }

        public void Receive()
        {
            switch(PostChartType)
            {
                // 차트 타입에 따라 처리
            }
        }
    }

    public class BasePost
    {
        public PostType PostType { get; private set; }
        public string InDate { get; private set; }
        public string Title { get; private set; }
        public string Content { get; private set; }
        public DateTime ExpirationDate { get; private set; }

        public List<PostChart> PostChartList { get; private set; } = new List<PostChart>();

        public BasePost(PostType postType, JsonData jsonData)
        {
            PostType = postType;
            InDate = jsonData["inDate"].ToString();
            Title = jsonData["title"].ToString();
            Content = jsonData["content"].ToString();
            ExpirationDate = DateTime.Parse(jsonData["expirationDate"].ToString());

            if(jsonData["items"].Count > 0)
            {
                for(int index = 0; index < jsonData["items"].Count; index++)
                {
                    PostChart postChart = new PostChart(jsonData["items"][index]);
                    PostChartList.Add(postChart);
                }
            }
        }

        public void Receive(Action<bool> action = null)
        {
            SendQueue.Enqueue(Backend.UPost.ReceivePostItem, PostType, InDate, bro =>
            {
                try
                {
                    Debug.Log($"Backend.UPost.ReceivePostItem({PostType}, {InDate}) : {bro}");

                    if(!bro.IsSuccess())
                    {
                        throw new Exception(bro.ToString());
                    }

                    foreach(PostChart postChart in PostChartList)
                    {
                        postChart.Receive();
                    }
                }
                catch(Exception e)
                {
                    string errorInfo = e.Message;

                    Debug.LogError($"우편 수신 실패 : {errorInfo}");
                }
                finally
                {
                    action?.Invoke(bro.IsSuccess());
                }
            });
        }
    }
}
