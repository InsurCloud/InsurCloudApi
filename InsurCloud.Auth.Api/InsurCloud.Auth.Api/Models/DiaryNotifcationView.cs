using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InsurCloud.Auth.Api.Models
{
    public class DiaryNotifcationView
    {
        public string Title { get; set; }
        public string diaryItemId {get; set;}
        public Int32 minutesAgo { get; set; }
        public Int32 hoursAgo { get; set; }
        public string classType { get; set; }
    }
}